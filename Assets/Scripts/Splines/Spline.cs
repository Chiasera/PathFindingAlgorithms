using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// Structure to hold information about the curve, including velocity and current position
public struct CurveInfo
{
    public Vector3 velocity;
    public Vector3 currentPosition;
}

// Abstract base class for a spline, inheriting from MonoBehaviour
public abstract class Spline : MonoBehaviour
{
    public GameObject splineKnotPrefab;
    protected CurveInfo curveInfo;
    public CurveInfo CurveInfo { get { return curveInfo; } }
    public List<SplineKnot> Knots;
    [SerializeField]
    [Range(0, 50)] // max of 10 spline points, could go further it needed
    protected float t;
    public float T { get { return t; } set { t = value; } }

    private float maxSegmentLength;
    public float MaxSegnmentLength { get { return maxSegmentLength; } set { maxSegmentLength = value; } }
    private float segmentLength;
    public float SegmentLength { get { return segmentLength; } }

    // Matrix for spline calculations
    protected Matrix4x4 Matrix;
    // Delegate for the input function
    protected delegate Vector4 InputFunction(float t);
    // Function to create a Vector4 based on the input parameter t
    public Vector4 tVector(float t)
    {
        return new Vector4(1, t, t * t, t * t * t);
    }

    // Function to create the derivative vector based on the input parameter t
    public Vector4 tVectorDerivative(float t)
    {
        return new Vector4(0, 1, 2 * t, 3 * t * t);
    }
    public Vector4 tVectorSecondDerivative(float t)
    {
        return new Vector4(0, 0, 2, 6*t);
    }
    // Abstract method to get the coefficient matrix, to be implemented in derived classes
    protected abstract Matrix4x4 CoefficientMatrix();
    //How many points ahead do I need to consider ?
    protected abstract int LookAheadCoefficient();

    // Abstract methods to be implemented in derived classes
    protected abstract Vector3 CubicSplineAtSegment(float t, int segment);
    public abstract Vector3 DerivativeAtSegment(float t, int segment);

    // Function to multiply two Vector4s
    protected float MultiplyVectors(Vector4 v1, Vector4 v2)
    {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z) + (v1.w * v2.w);
    }
    // Update is called once per frame
    void Update()
    {
        // Update the spline's current position and velocity based on the time parameter t
        for (int i = 0; i < Knots.Count - LookAheadCoefficient(); i++)
        {
            if (t >= i && t < i + 1)
            {
                segmentLength = (Knots[i].transform.position - Knots[i + 1].transform.position).magnitude;
                curveInfo.currentPosition = CubicSplineAtSegment(t, i);
                curveInfo.velocity = DerivativeAtSegment(t % Mathf.Max(i, 1), i);
            }
        }
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    // Computes the matrix operation for a given input function on parameter t
    protected Vector3 Compute(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, InputFunction tFunction)
    {
        Matrix = CoefficientMatrix();
        // Transpose the matrix to set p0, p1, p2, p3 as rows
        Matrix4x4 pointsMatrix = new Matrix4x4(p0, p1, p2, p3).transpose;

        Vector4 intermediate = new Vector4(
            MultiplyVectors(tFunction(t), Matrix.GetColumn(0)),
            MultiplyVectors(tFunction(t), Matrix.GetColumn(1)),
            MultiplyVectors(tFunction(t), Matrix.GetColumn(2)),
            MultiplyVectors(tFunction(t), Matrix.GetColumn(3))
            );
        Vector4 result = new Vector4(
             MultiplyVectors(intermediate, pointsMatrix.GetColumn(0)),
            MultiplyVectors(intermediate, pointsMatrix.GetColumn(1)),
            MultiplyVectors(intermediate, pointsMatrix.GetColumn(2)),
            MultiplyVectors(intermediate, pointsMatrix.GetColumn(3))
            );
        return new Vector3(result.x, result.y, result.z);
    }

    // Create the spline based on a dictionary of anchor points and the agent's position
    public void Create(Dictionary<Vector3, Vector3> anchors)
    {
        // Instantiate spline knots based on anchor points
        foreach (var point in anchors.Reverse())
        {
            if(this.GetType() == typeof(CatmullRomSpline))
            {
                if (Knots.Count == 0)
                {
                    SplineKnot firstKnot = Instantiate(splineKnotPrefab, point.Key, Quaternion.identity).GetComponent<SplineKnot>();
                    firstKnot.transform.position += Vector3.up * 1.5f;
                    Knots.Add(firstKnot);
                }
                else if (Knots.Count == anchors.Count)
                {
                    SplineKnot lastKnot = Instantiate(splineKnotPrefab, point.Key, Quaternion.identity).GetComponent<SplineKnot>();
                    lastKnot.transform.position += Vector3.up * 1.5f;
                    Knots.Add(lastKnot);
                }
            }           
            SplineKnot knot = Instantiate(splineKnotPrefab, point.Key, Quaternion.identity).GetComponent<SplineKnot>();
            Knots.Add(knot);
            knot.transform.position += Vector3.up * 1.5f;
            knot.transform.rotation *= Quaternion.FromToRotation(knot.transform.forward, -point.Value);
        }
        for (int i = 0; i < Knots.Count - LookAheadCoefficient(); i++)
        {
            float nextSegmentLength = (Knots[i].transform.position - Knots[i + 1].transform.position).magnitude;
            if (nextSegmentLength > maxSegmentLength)
            {
                maxSegmentLength = nextSegmentLength;
            }
        }
        curveInfo.currentPosition = Knots[0].transform.position;
    }

    // Draw gizmos in the Unity editor for visualization
    void OnDrawGizmos()
    {
        // Only draw if there are enough knots
        if (Knots.Count > 1)
        {
            // Draw the spline curve for visualization
            for (int i = 0; i < Knots.Count - LookAheadCoefficient(); i++)
            {
                for (float j = 0; j < 1; j += 0.1f)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(CubicSplineAtSegment(j, i), 0.05f);
                }
            }

            // Draw the current position and velocity on the curve
            int segment = (int)t;
            float u = t - segment;

            if (segment >= 0 && segment < Knots.Count - LookAheadCoefficient())
            {
                Gizmos.color = Color.cyan;
                Vector3 curvePositionGizmo = CubicSplineAtSegment(u, segment);
                Gizmos.DrawSphere(curvePositionGizmo, 0.5f);
                Gizmos.color = Color.magenta;
                Vector3 curveVelocityGizmo = DerivativeAtSegment(u, segment);
                Gizmos.DrawRay(curvePositionGizmo, curveVelocityGizmo);
            }
        }
    }
}
