public static class MathHelper
{
    public static float Lerp(float start, float end, float amount)
    {
        return start + (end - start) * amount;
    }
}