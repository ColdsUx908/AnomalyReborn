namespace Anomalies.SpecialContents.Developer.EventideReunion;

// NPC 死亡后的纯本地标记退场，不生成伤害实体，也不重新生成星芒。
public sealed class EventideReunionDeathMarkFade : Particle
{
    private readonly float _Radius;
    private readonly float _Companion;
    public override bool AutoLoadTexture => false;
    public override BlendState DrawBlendState => BlendState.AlphaBlend;

    public EventideReunionDeathMarkFade(Vector2 center, float radius, int lifetime, float companion = 0f)
    {
        Center = center;
        _Radius = radius;
        _Companion = companion;
        Lifetime = lifetime;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawOffset = default)
    {
        float scale = MathHelper.SmoothStep(1f, 0f, LifetimeCompletion);
        if (scale <= 0.001f)
            return false;
        spriteBatch.End();
        EventideReunionMarkDrawing.DrawDeath(Center + drawOffset, _Radius * scale, Main.screenPosition, _Companion * scale);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}
