using System;

[Serializable]
public struct Spring
{
    public int massA;
    public int massB;
    public float equilibriumDistance;

    public Spring(int _massA, int _massB, float _equilibriumDistance)
    {
        massA = _massA;
        massB = _massB;
        equilibriumDistance = _equilibriumDistance;
    }

    public bool ContainsMasses(int _massA, int _massB)
    {
        return ((massA == _massA) && (massB == _massB))
            || ((massA == _massB) && (massB == _massA));
    }
}
