using Terraria.UI;

namespace Anomalies.SpecialContents.Contributor.MeguminExplosion;

public sealed class MeguminAtmosphere : ModSystem
{
    public override bool IsLoadingEnabled(Mod mod) => AnomalySpecialContentToggler.MeguminExplosion;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // This overlay uses physical pixels. UI layers temporarily change screenWidth/screenHeight with UI scaling.
        layers.Insert(0, new LegacyGameInterfaceLayer("Anomalies: Explosion Atmosphere", DrawAtmosphere, InterfaceScaleType.None));
    }

    private static bool DrawAtmosphere()
    {
        if (Main.gameMenu) return true;
        float darkness = 0f;
        float flash = 0f;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            float distanceFade = MathHelper.Clamp(1f - Vector2.Distance(Main.LocalPlayer.Center, projectile.Center) / 2400f, 0f, 1f);
            if (projectile.ModProjectile is MeguminCasting cast)
                darkness = Math.Max(darkness, cast.Charge * 0.28f * distanceFade);
            if (projectile.ModProjectile is MeguminBlast blast)
            {
                darkness = Math.Max(darkness, blast.Opacity * 0.12f * distanceFade);
                flash = Math.Max(flash, MathHelper.Clamp(1f - Math.Abs(blast.Time - MeguminBlast.WarmupTime - 2f) / 7f,
                    0f, 1f) * 0.16f * blast.Opacity * distanceFade);
            }
        }
        if (darkness <= 0f && flash <= 0f) return true;
        using (Main.spriteBatch.Scope(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, RasterizerState.CullNone, transformMatrix: Matrix.Identity, effectIsNull: true))
        {
            Viewport viewport = Main.instance.GraphicsDevice.Viewport;
            Rectangle screen = new(0, 0, viewport.Width, viewport.Height);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, screen, new Rectangle(0, 0, 1, 1), Color.Black * darkness);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, screen, new Rectangle(0, 0, 1, 1), new Color(255, 235, 210) * flash);
        }
        return true;
    }
}
