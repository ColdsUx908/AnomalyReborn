namespace Anomalies.SpecialContents.Developer.EventideReunion;

internal static class EventideReunionDeathSoulMotion
{
    // 玩家速度作为前馈项，额外速度随距离增长；高速奔跑也无法持续甩开灵魂。
    public static Vector2 Velocity(Vector2 offset, Vector2 playerVelocity)
    {
        float distance = offset.Length();
        if (distance < 0.001f)
            return playerVelocity;
        float closingSpeed = Math.Max(24f, Math.Max(playerVelocity.Length() + 16f, distance / 12f));
        return playerVelocity + offset / distance * Math.Min(distance, closingSpeed);
    }
    public static int Healing(int life, int maxLife, bool moonLeech) => moonLeech ? 0 : Math.Clamp(maxLife - life, 0, 5);
}
