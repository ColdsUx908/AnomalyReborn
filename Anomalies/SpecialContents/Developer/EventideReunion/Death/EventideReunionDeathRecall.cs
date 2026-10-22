namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionDeathRecall
{
    public const int Stagger = 10;
    public const int RaiseDuration = 8;
    public const int FlightDuration = 20;

    public static (float Raise, float Flight, float Scale) Pose(int age, int index)
    {
        float raise = MathHelper.Clamp((age - index * Stagger) / (float)RaiseDuration, 0f, 1f);
        float flight = MathHelper.Clamp((age - index * Stagger - RaiseDuration) / (float)FlightDuration, 0f, 1f);
        return (MathHelper.SmoothStep(0f, 1f, raise), flight, MathHelper.SmoothStep(1f, 0f, flight));
    }

    public static Vector2 Center(Vector2 start, Vector2 player, float flight) => Vector2.Lerp(start, player, flight * flight * flight);
}
