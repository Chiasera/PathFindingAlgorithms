using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatmullRomSpline : Spline
{
    // Start is called before the first frame update
    public static Matrix4x4 Catmull_Rom_Matrix_Scaled = new Matrix4x4(
    new Vector4(0, -1, 2, -1) * 0.5f,
    new Vector4(2, 0, -5, 3) * 0.5f,
    new Vector4(0, 1, 4, -3) * 0.5f,
    new Vector4(0, 0, -1, 1) * 0.5f
    );

    private int lookAheadCoefficient = 3;

    protected override int LookAheadCoefficient()
    {
        return lookAheadCoefficient;
    }

    private void Awake()
    {
        lookAheadCoefficient = 3;
    }

    protected override Vector3 CubicSplineAtSegment(float t, int segment)
    {   
        return Compute(Knots[segment].transform.position, Knots[segment + 1].transform.position,
                    Knots[segment + 2].transform.position, Knots[segment + 3].transform.position, t % Mathf.Max(segment, 1), tVector);
    }

    protected override Matrix4x4 CoefficientMatrix()
    {
        return Catmull_Rom_Matrix_Scaled;
    }

    public override Vector3 DerivativeAtSegment(float t, int segment)
    {
        return Compute(Knots[segment].transform.position, Knots[segment + 1].transform.position, Knots[segment + 2].transform.position,
            Knots[segment + 3].transform.position, t, tVectorDerivative);
    }
}
