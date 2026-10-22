namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathSkill
{
    private void EmitParticles()
    {
        if (Main.dedServ)
            return;
        Player player = Projectile.Owner;
        UpdateAfterimages();
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
        Player player = Projectile.Owner;
        Texture2D weapon = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new(144f, 156f);
        DrawAfterimages(weapon, origin);
        if (Phase == DeathPhase.Slash)
        {
            Main.spriteBatch.End();
            EventideReunionDeathDrawing.DrawCrescent(player.MountedCenter, SlashAim, _SwingSign,
                Timer / (float)SlashDuration, 0.85f);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        Main.EntitySpriteDraw(weapon, player.MountedCenter - Main.screenPosition, null,
            Color.Lerp(lightColor, new Color(120, 90, 145), 0.45f), WeaponAngle + MathHelper.PiOver4,
            origin, WeaponVector.Length(), SpriteEffects.None);
        return false;
    }
}
