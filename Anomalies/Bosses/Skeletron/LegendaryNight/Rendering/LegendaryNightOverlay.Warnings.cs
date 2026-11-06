namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendaryNightOverlay
{
    private void DrawMechanic6ContractionWarning(SpriteBatch spriteBatch)
    {
        SkeletronLegendHead head = Phase2Arena.MasterBehavior;
        if (head is not { StellarMechanicsActive: true })
            return;
        int time = head.Phase2MusicElapsedTime;
        int age = time - SkeletronLegendHead.Mechanic6Timeline.StartMusicTime;
        if (age < 0 || time >= SkeletronLegendHead.Mechanic6Timeline.ExpandMusicTime)
            return;
        float innerRadius = SkeletronLegendHead.Mechanic6Parameters.ArenaTargetRadius;
        float outerRadius = Phase2Arena.Radius;
        if (outerRadius <= innerRadius)
            return;

        // 与金币审判共用红色预警的淡入和透明度；遮罩只覆盖收缩将扫过的圆环。
        float opacity = CoinJudgment.GetWarningOverlayOpacity(
            CoinJudgment.WarningDuration - CoinJudgment.DangerWarningDuration + age);
        if (opacity <= 0f)
            return;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        Vector2 center = Phase2Arena.Projectile.Center - Main.screenPosition;
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(
            Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
        using (lease.Scope(clearColor: Color.Transparent))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.DrawFromCenter(circle, center, null, Color.White, scale: outerRadius * 2f / circle.Width);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, TODrawUtils.EraseBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.DrawFromCenter(circle, center, null, Color.White, scale: innerRadius * 2f / circle.Width);
            spriteBatch.End();
        }
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        spriteBatch.Draw(lease.Target, Vector2.Zero, new Color(255, 120, 120) * opacity);
        spriteBatch.End();
    }

    private void DrawMechanic2HalfField(SpriteBatch spriteBatch)
    {
        SkeletronLegendHead head = Phase2Arena.MasterBehavior;
        if (head is not { Mechanic2Active: true } || head.Mechanic2HandPattern == 0)
            return;
        int age = head.Mechanic2Age;
        Texture2D circle = AnomalyTextures.Extra.BasicCircleLarge;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        // 第二次预警可能与第一次的冲击余光重叠，分别绘制，避免丢失预警最初的帧数。
        for (int strike = 0; strike < SkeletronLegendHead.Mechanic2Parameters.StrikeCount; strike++)
        {
            int relativeAge = age - SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime(strike);
            float opacity = SkeletronLegendHead.Mechanic2Parameters.HalfFieldOpacity(relativeAge);
            if (opacity <= 0f)
                continue;
            int side = head.Mechanic2StrikeSide(strike);
            Rectangle frame = new(side < 0 ? 0 : circle.Width / 2, 0, circle.Width / 2, circle.Height);
            Vector2 origin = new(side < 0 ? circle.Width / 2f : 0f, circle.Height / 2f);
            Color color = relativeAge < 0 ? Color.OrangeRed : Color.White;
            spriteBatch.Draw(circle, Phase2Arena.Projectile.Center - Main.screenPosition, frame, color * opacity,
                0f, origin, Phase2Arena.Radius * 2f / circle.Width, SpriteEffects.None, 0f);
        }
        spriteBatch.End();
    }
}