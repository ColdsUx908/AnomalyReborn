// Developed by ColdsUx

using Anomalies.Assets.Effects;
using Terraria.Graphics.Effects;
using Transoceanic.DataStructures.Rendering;

namespace Anomalies.Visuals;

public class EnhancedDarknessSystem : ModSystem, IContentLoader
{
    public class LightSource
    {
        public Texture2D texture = _DefaultTexture;
        public float scale = 1;
        public Vector2 vectorScale = Vector2.One;
        public Vector2 center = Main.LocalPlayer.Center;
        public float rotation = 0;
        public float opacity = 1;
        public Color color = Color.White;
        public int lifetime = 1;
        public Rectangle? frame = null;
        public LightSource() { }

        // This constructor only gives the most common arguments. Frame, Color, lifetime, etc. must be set in curly braces afterwards to prevent this constructor getting too unwieldy.
        public LightSource(Vector2? center = null, Texture2D texture = null, float scale = 1, float rotation = 0, Vector2? vectorScale = null, float opacity = 1)
        {
            this.texture = texture ?? _DefaultTexture;
            this.scale = scale;
            this.vectorScale = vectorScale ?? Vector2.One;
            this.center = center ?? Main.LocalPlayer.Center;
            this.rotation = rotation;
            this.opacity = opacity;
        }
    }

    private static Texture2D _DefaultTexture;

    public static List<LightSource> Lights = [];

    private static void DrawShadowOverlay(On_OverlayManager.orig_Draw orig, OverlayManager self, SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        orig(self, spriteBatch, layer, beginSpriteBatch);

        //This ensures that the shadows only draw
        //  - In the world
        //  - Right before UI is drawn (and right before the hideUI check), as that's where RenderLayers.All is drawn
        //  - 
        if (Main.gameMenu || layer != RenderLayers.All)
            return;

        AnomalyPlayer anomalyPlayer = Main.LocalPlayer.Anomaly;

        if (anomalyPlayer.DarknessIntensity <= 0)
            return;

        GraphicsDevice device = Main.instance.GraphicsDevice;
        using RenderTargetLease lease = RenderTargetPool.Shared.Rent(
            device,
            Main.screenWidth,
            Main.screenHeight,
            RenderTargetDescriptor.Default
        );

        using (lease.Scope(clearColor: Color.Black))
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);
            foreach (LightSource item in Lights)
                spriteBatch.Draw(item.texture, item.center - Main.screenPosition, item.frame, item.color * item.opacity, item.rotation, item.frame is null ? item.texture.Size() * 0.5f : item.frame.Value.Size(), item.vectorScale * item.scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        AnomalyEffects.LightingShader.Data
            .UseOpacity(anomalyPlayer.DarknessIntensity)
            .Apply();
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();
    }

    public override void PreUpdateEntities()
    {
        //Every frame, the light sources are determined by what was added on that frame only. Therefore, we reset the light list every frame.
        //Lifetime is provided to smooth out things that don't run every frame consistently to prevent flickering, such as the abyss torches.
        for (int i = 0; i < Lights.Count; i++)
        {
            LightSource item = Lights[i];
            item.lifetime--;
            if (item.lifetime <= 0)
            {
                Lights.Remove(item);
                i--;
            }
        }
    }

    public static void AddLightSource(Vector2? center = null, Texture2D texture = null, float scale = 1f, float rotation = 0, Vector2? vectorScale = null, float opacity = 1) =>
        Lights.Add(new LightSource(center, texture, scale, rotation, vectorScale, opacity));

    public static void ChangeDarknessIntensity(Player player, Func<float, float> intensityModifier)
    {
        if (intensityModifier is null)
            return;
        AnomalyPlayer anomalyPlayer = player.Anomaly;
        anomalyPlayer.DarknessIntensity = intensityModifier(anomalyPlayer.DarknessIntensity);
    }

    void IContentLoader.PostSetupContent()
    {
        On_OverlayManager.Draw += DrawShadowOverlay;
        _DefaultTexture = ParticleHandler.GetTexture<BloomParticle>();
    }

    void IContentLoader.OnWorldUnload()
    {
        Lights.Clear();
    }
}