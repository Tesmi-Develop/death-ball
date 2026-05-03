using Hypercube.Mathematics.Vectors;
using Shared.Data;

namespace Shared.Helpers;

public static class ComparisonStrategyHelper
{
    public static ComparisonStrategy GetStrategy(Type fieldType)
    {
        if (fieldType == typeof(float)) return ComparisonStrategy.RelaxedFloat;
        if (fieldType == typeof(double)) return ComparisonStrategy.RelaxedDouble;
        if (fieldType == typeof(Vector3)) return ComparisonStrategy.RelaxedVector3;
        if (fieldType == typeof(Vector2)) return ComparisonStrategy.RelaxedVector2;
    
        return ComparisonStrategy.Binary;
    }
}