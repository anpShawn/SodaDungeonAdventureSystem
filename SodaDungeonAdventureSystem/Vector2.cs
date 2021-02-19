public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float inX, float inY)
    {
        x = inX;
        y = inY;
    }

    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new Vector2(a.x + b.x, a.y + b.y);
}