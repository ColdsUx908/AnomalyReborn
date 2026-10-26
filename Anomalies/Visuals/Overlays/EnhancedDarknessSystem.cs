using Anomalies.Assets.Effects;
using Terraria.Graphics.Effects;

namespace Anomalies.Visuals.Overlays;

public class EnhancedDarknessSystem : CustomOverlay, IContentLoader
{
    public class LightSource
    {
        public Texture2D Texture = _DefaultTexture;
        public float Scale = 1;
        public Vector2 VectorScale = Vector2.One;
        public Vector2 Center = Main.LocalPlayer.Center;
        public float Rotation = 0;
        public float Opacity = 1;
        public Color Color = Color.White;
        public int Lifetime = 1;
        public Rectangle? Frame = null;
        public LightSource() { }

        // This constructor only gives the most common arguments. Frame, Color, lifetime, etc. must be set in curly braces afterwards to prevent this constructor getting too unwieldy.
        public LightSource(Vector2? center = null, Texture2D texture = null, float scale = 1, float rotation = 0, Vector2? vectorScale = null, float opacity = 1)
        {
            this.Texture = texture ?? _DefaultTexture;
            this.Scale = scale;
            this.VectorScale = vectorScale ?? Vector2.One;
            this.Center = center ?? Main.LocalPlayer.Center;
            this.Rotation = rotation;
            this.Opacity = opacity;
        }
    }

    private static Texture2D _DefaultTexture;

    public static List<LightSource> Lights = [];

    public override void Update()
    {
        // Every frame, the light sources are determined by what was added on that frame only. Therefore, we decrement lifetime each frame.
        // Lifetime smooths out sources that aren't added every frame to prevent flicker.
        for (int i = 0; i < Lights.Count; i++)
        {
            LightSource item = Lights[i];
            item.Lifetime--;
            if (item.Lifetime <= 0)
            {
                Lights.Remove(item);
                i--;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
    {
        // This ensures that the shadows only draw in the world and at the appropriate render layer.
        if (Main.gameMenu || layer != RenderLayers.All)
            return;

        AnomalyPlayer anomalyPlayer = Main.LocalPlayer.Anomaly;
        if (anomalyPlayer.Visual_DarknessIntensity <= 0)
            return;

        _DefaultTexture ??= ParticleHandler.GetTexture<BloomParticle>();

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
                spriteBatch.Draw(item.Texture, item.Center - Main.screenPosition, item.Frame, item.Color * item.Opacity, item.Rotation, item.Frame is null ? item.Texture.Size() * 0.5f : item.Frame.Value.Size(), item.VectorScale * item.Scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        AnomalyEffects.Overlays.SolidColorMask.Data
            .UseColor(Color.Black)
            .UseOpacity(anomalyPlayer.Visual_DarknessIntensity)
            .Apply();
        spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, 0, 0);
        spriteBatch.End();
    }

    public static void AddLightSource(Vector2? center = null, Texture2D texture = null, float scale = 1f, float rotation = 0, Vector2? vectorScale = null, float opacity = 1) =>
        Lights.Add(new LightSource(center, texture, scale, rotation, vectorScale, opacity));

    public static void ChangeDarknessIntensity(Player player, Func<float, float> intensityModifier)
    {
        if (intensityModifier is null)
            return;
        AnomalyPlayer anomalyPlayer = player.Anomaly;
        anomalyPlayer.Visual_DarknessIntensity = intensityModifier(anomalyPlayer.Visual_DarknessIntensity);
    }

    void IContentLoader.PostSetupContent()
    {
        _DefaultTexture = ParticleHandler.GetTexture<BloomParticle>();
    }

    void IContentLoader.OnWorldUnload()
    {
        Lights.Clear();
    }
}