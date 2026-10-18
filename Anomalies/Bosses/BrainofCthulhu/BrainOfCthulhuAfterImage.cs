namespace Anomalies.Bosses.BrainofCthulhu;

public sealed class BrainOfCthulhuAfterImage : PathAfterimageParticle
{
    public BrainOfCthulhuAfterImage(BezierCurve path, float rotation, Vector2 scale, int lifeTime, Rectangle frame, float startFadeRatio = 0f) : base(Main.dedServ ? null : TextureAssets.Npc[NPCID.BrainofCthulhu].Value, path.GetPoints(lifeTime), rotation, scale, frame, startFadeRatio)
    {
    }
}
