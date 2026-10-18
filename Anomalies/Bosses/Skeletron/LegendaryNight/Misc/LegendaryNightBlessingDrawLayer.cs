namespace Anomalies.Bosses.Skeletron.LegendaryNight.Misc;

public sealed class LegendaryNightBlessingDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.active && !drawInfo.drawPlayer.dead && !drawInfo.drawPlayer.invis
        && drawInfo.shadow == 0f && drawInfo.drawPlayer.Anomaly.StarGodBlessingActive;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Texture2D bloom = ParticleHandler.GetTexture<BloomParticle>();
        Vector2 position = drawInfo.Center - Main.screenPosition;
        float pulse = 1f + 0.08f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
        // Alpha 为零的颜色在原版预乘混合中产生叠加光，不切换玩家绘制的 SpriteBatch。
        float opacity = drawInfo.drawPlayer.Anomaly.StarGodBlessingOpacity;
        Color outer = (LegendaryNightHandler.StarColor with { A = 0 }) * (0.5f * opacity);
        Color inner = (Color.White with { A = 0 }) * (0.5f * opacity);
        drawInfo.DrawDataCache.Add(new DrawData(bloom, position, null, outer, 0f,
            bloom.Size() * 0.5f, 140f * pulse / bloom.Width, SpriteEffects.None, 0));
        drawInfo.DrawDataCache.Add(new DrawData(bloom, position, null, inner, 0f,
            bloom.Size() * 0.5f, 70f * pulse / bloom.Width, SpriteEffects.None, 0));
    }
}
