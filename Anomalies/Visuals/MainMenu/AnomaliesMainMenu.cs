namespace Anomalies.Visuals.MainMenu;

public sealed class AnomaliesMainMenu : ModMenu
{
    private const string AssetPath = "Anomalies/Visuals/MainMenu/";

    [LoadTexture(AssetPath + "MenuBackground")]
    private static Asset<Texture2D> _Background;

    [LoadTexture(TOTextures.InvisibleTexturePath)]
    private static Asset<Texture2D> _InvisibleTexture;

    public override string DisplayName => "Anomalous Permafrost"; //异常冻土
    public override int Music => AnomaliesMusicHandler.PathwaytoReverie > 0 ? AnomaliesMusicHandler.PathwaytoReverie : MusicID.OtherworldlySnow;
    public override Asset<Texture2D> Logo => TextureAssets.Logo;
    public override Asset<Texture2D> SunTexture => _InvisibleTexture;
    public override Asset<Texture2D> MoonTexture => SunTexture;
    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<AnomaliesMenuBackground>();

    public override void OnSelected() => SetMenuLighting();

    public override void Update(bool isOnTitleScreen) => SetMenuLighting();

    private static void SetMenuLighting()
    {
        if (!Main.gameMenu)
            return;

        // The painted night sky is independent of vanilla's menu lighting.
        // Keep menu text bright instead of allowing the day/night cycle to tint it.
        new TerrariaTime(12, 0).Apply();
    }

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        Texture2D background = _Background.Value;
        float scale = Math.Max((float)Main.screenWidth / background.Width, (float)Main.screenHeight / background.Height);
        Vector2 offset = (new Vector2(Main.screenWidth, Main.screenHeight) - background.Size() * scale) * 0.5f;

        // PreDrawMenu converts Main.screenWidth/Height to UI coordinates.
        // Apply the matching UI matrix to cover the physical viewport.
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        spriteBatch.Draw(background, offset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        AnomalyStarVisual.Draw(spriteBatch, 1f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        // Vanilla's Journey's End logo uses two layers with Main's existing fade,
        // rotation and scale. MenuLoader only draws a single layer for mod menus.
        drawColor = Color.White;
        Rectangle source = new(0, 0, Logo.Width(), Logo.Height());
        Vector2 origin = Logo.Size() * 0.5f;
        spriteBatch.Draw(Logo.Value, logoDrawCenter, source, drawColor * (Main.LogoA / 255f), logoRotation, origin, logoScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(TextureAssets.Logo2.Value, logoDrawCenter, source, drawColor * (Main.LogoB / 255f), logoRotation, origin, logoScale, SpriteEffects.None, 0f);
        return false;
    }
}
