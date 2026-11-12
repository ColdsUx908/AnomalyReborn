using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    public override void BossHeadRotation(ref float rotation) =>
        rotation = IsSpinning ? NPC.rotation : 0f;

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawFinalCharge(spriteBatch);
        // Charge overlay fades using the synchronized telegraph clock.
        float beginTelegraphGateValue = ChargeGateValue - ChargeTelegraphTime;
        if (ChargeTelegraphTimer > beginTelegraphGateValue)
        {
            float colorScale = MathHelper.Clamp((ChargeTelegraphTimer - beginTelegraphGateValue) / ChargeTelegraphTime, 0f, 1f);
            Color telegraphColor = new Color(150, 150, 150, 0) * colorScale;
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 0; i < 2; i++)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY), NPC.frame,
                    telegraphColor, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            }
        }
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRectangle) =>
        ApplyBossBarShader(destinationRectangle);

    internal static void ApplyBossBarShader(Rectangle destinationRectangle)
    {
        AnomalyEffects.BossBars.Skeletron.Data
            .UseImage1(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRectangle.TopLeft())
            .SetCustomParameter("uStarIntensity", 0f)
            .Apply();
    }

    private void EmitHandProtectionDust()
    {
        Dust.NewDustAction(new Vector2(NPC.Center.X - 15f - NPC.velocity.X * 5f, NPC.position.Y + NPC.height - 2f) + new Vector2(30 / 2f, 10 / 2f), 30, 10, DustID.Blood, new Vector2(-NPC.velocity.X * 0.2f, 3f), dust =>
        {
            dust.alpha = 0;
            dust.scale *= 2f;
            dust.noGravity = true;
            dust.velocity.X *= 1.3f;
            dust.velocity.X += NPC.velocity.X * 0.4f;
            dust.velocity.Y += (2f + NPC.velocity.Y);
        });
        for (int j = 0; j < 2; j++)
        {
            Dust.NewDustAction(new Vector2(NPC.position.X, NPC.position.Y + 120f) + new Vector2(NPC.width / 2f, 60 / 2f), NPC.width, 60, DustID.Blood, new Vector2(NPC.velocity.X, NPC.velocity.Y), dust =>
            {
                dust.alpha = 0;
                dust.scale *= 2f;
                dust.noGravity = true;
                dust.velocity -= NPC.velocity;
                dust.velocity.Y += 5f;
            });
        }
    }
}
