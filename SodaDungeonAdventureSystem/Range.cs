public struct Range
{
    public int min;
    public int max;

    public Range(int inMin, int inMax)
    {
        min = inMin;
        max = inMax;
    }

    public bool Contains(int inVal)
    {
        return inVal >= min && inVal <= max;
    }
}
