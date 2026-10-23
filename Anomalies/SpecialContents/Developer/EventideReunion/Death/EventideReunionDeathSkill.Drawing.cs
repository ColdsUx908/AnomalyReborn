namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathSkill
{
    private void EmitParticles()
    {
        if (Main.dedServ)
            return;
        Player player = Projectile.Owner;
        UpdateAfterimages();
        if (!BlocksActions)
            return;
        if (Phase == DeathPhase.Slash)
            EmitSlashSparkles();
        Vector2 tip = player.MountedCenter + WeaponVector * WeaponTipDistance;
        if (Phase == DeathPhase.Charge || Phase == DeathPhase.Slash)
            Dust.NewDustPerfectAction(tip, DustID.Smoke, d =>
            {
                d.noGravity = true;
                d.color = new Color(30, 20, 40);
                d.velocity = new Vector2(0f, -2f);
                d.scale = 1.6f;
            });
        if (Phase == DeathPhase.Recovery && Timer % 3 == 0)
            Dust.NewDustPerfectAction(tip, DustID.Shadowflame, d =>
            {
                d.noGravity = true;
                d.velocity = (player.Center - tip) * 0.1f;
            });
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (!BlocksActions)
            return false;
        Player player = Projectile.Owner;
        Texture2D weapon = EventideReunionAssets.Weapon;
        Vector2 origin = new(144f, 156f);
        if (Phase == DeathPhase.Slash)
        {
            Main.spriteBatch.End();
            EventideReunionDeathDrawing.DrawSlash(player.MountedCenter, SlashAim, _SwingSign, Timer / (float)SlashDuration);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        Main.EntitySpriteDraw(weapon, player.MountedCenter - Main.screenPosition, null,
            Color.Lerp(lightColor, new Color(120, 90, 145), 0.45f), WeaponAngle + MathHelper.PiOver4,
            origin, WeaponVector.Length(), SpriteEffects.None);
        return false;
    }

    private void EmitSlashSparkles()
    {
        float head = MathHelper.Clamp(Timer / (float)SlashDuration, 0f, 1f);
        float tail = Math.Max(0f, (Timer - 1f) / SlashDuration);
        for (int i = 0; i < 3; i++)
        {
            float sample = MathHelper.Lerp(tail, head, (i + 0.5f) / 3f);
            Vector2 vector = EventideReunionDeathGeometry.SlashVector(sample, SlashAim, _SwingSign);
            Vector2 next = EventideReunionDeathGeometry.SlashVector(Math.Min(1f, sample + 0.015f), SlashAim, _SwingSign);
            Vector2 motion = (next - vector).SafeNormalize(SlashAim) * Main.rand.NextFloat(2f, 5f);
            Vector2 position = Projectile.Owner.MountedCenter + vector * Main.rand.NextFloat(0.8f, 1f);
            Color purple = Color.Lerp(new Color(125, 35, 240), new Color(225, 165, 255), Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new PointingParticle(position, motion, false, Main.rand.Next(14, 24),
                Main.rand.NextFloat(0.25f, 0.5f), purple));
            if (i == 0 && Timer % 2 == 0)
                ParticleHandler.SpawnParticle(new GenericSparkle(position, motion * 0.3f, new Color(245, 225, 255),
                    purple, 0.3f, 20, rotationSpeed: 0.025f, bloomScale: 1.2f));
        }
    }
}
