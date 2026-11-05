using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>ai[0]：命中音乐帧；ai[1]：最大半径；ai[2]：主人索引+1。</summary>
public sealed class LegendFinalShockwave : AnomalyModProjectile
{
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
        Projectile.timeLeft = LegendEnergySphere.CompanionExplosionDuration;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;

    // 已命中的冲击波独立完成扩张；主人此后死亡不能取消惩罚。
    private int Age => LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[2], out SkeletronLegendHead head, oneBased: true)
        ? head.Phase2MusicElapsedTime - (int)Projectile.ai[0]
        : LegendEnergySphere.CompanionExplosionDuration - Projectile.timeLeft;

    public override void AI()
    {
        int age = Age;
        if (age >= LegendEnergySphere.CompanionExplosionDuration)
        {
            Projectile.Kill();
            return;
        }
        if (age < 0 || Main.dedServ)
            return;
        if (age == 0)
        {
            SoundEngine.PlaySound(Meteor.ExplosionSound, Projectile.Center);
            Main.LocalPlayer.SetScreenshake(18f);
        }
        Player player = Main.LocalPlayer;
        float radius = Projectile.ai[1] * LegendEnergySphere.CompanionShockwaveProgress(age);
        if (player.active && !player.dead && !player.ghost && new Circle(Projectile.Center, radius).Collides(player.Hitbox))
            player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetText(
                "Mods.Anomalies." + LegendaryNightHandler.LocalizationCategory + ".TrialFailedDeath").ToNetworkText(player.name)), int.MaxValue, 0);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int age = Age;
        if (age < 0 || age >= LegendEnergySphere.CompanionExplosionDuration)
            return false;
        SpriteBatch spriteBatch = Main.spriteBatch;
        float maximumRadius = Projectile.ai[1];
        float progress = age / (float)(LegendEnergySphere.CompanionExplosionDuration - 1);
        float opacity = 1f - MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - 0.82f) / 0.18f, 0f, 1f));
        AnomalyEffects.MiscShaderContainer shader = AnomalyClientConfig.Instance.EnableShaders
            ? AnomalyEffects.Overlays.LegendCompanionExplosion : null;
        if (shader is not null)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            shader.Data.SetCustomParameter("uGlobalTime", age / 60f)
                .SetCustomParameter("uLifetimeRatio", progress)
                .SetCustomParameter("uYellowPalette", 1f)
                .SetCustomParameter("uExpansionRatio", LegendEnergySphere.CompanionShockwaveProgress(age))
                .SetCustomParameter("uNoiseTexture", AnomalyTextures.Noise.Perlin2)
                .SetCustomParameter("uTextureSize", new Vector2(maximumRadius * 2f))
                .Apply();
            Texture2D pixel = TOTextures.FillerTexture;
            spriteBatch.DrawFromCenter(pixel, Projectile.Center - Main.screenPosition, null,
                Color.White * opacity, scale: maximumRadius * 2f / pixel.Width);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        else
        {
            // 回退亮环与伤害扩张使用同一进度，线宽包含在半径上限内。
            const int Segments = 240;
            float radius = Math.Max(0f, LegendEnergySphere.CompanionShockwaveProgress(age) * maximumRadius - 10f);
            Texture2D pixel = TOTextures.FillerTexture;
            for (int i = 0; i < Segments; i++)
            {
                Vector2 start = Projectile.Center + (MathHelper.TwoPi * i / Segments).ToRotationVector2() * radius;
                Vector2 end = Projectile.Center + (MathHelper.TwoPi * (i + 1) / Segments).ToRotationVector2() * radius;
                Vector2 edge = end - start;
                Vector2 center = (start + end) * 0.5f - Main.screenPosition;
                spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, new Color(255, 200, 20, 0) * (opacity * 0.85f),
                    edge.ToRotation(), new Vector2(edge.Length() + 1f, 20f) / pixel.Size());
                spriteBatch.DrawFromCenter_VectorScale(pixel, center, null, new Color(255, 245, 170, 0) * opacity,
                    edge.ToRotation(), new Vector2(edge.Length() + 1f, 6f) / pixel.Size());
            }
        }
        return false;
    }
}