using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.EaterofWorlds;

// DoGDistortionMetaball and foreground layers, adapted from CalamityMod; Copyright Azafure, LLC.
public sealed class EaterDistortionSystem : ModSystem
{
    private sealed class DistortionParticle(Vector2 center, Vector2 velocity, float size, bool square, float rotation)
    {
        public Vector2 Center = center;
        public Vector2 Velocity = velocity;
        public float Size = size;
        public bool Square = square;
        public float Rotation = rotation;
    }
    private sealed class RiftCrack(Vector2[] points, float width)
    {
        public Vector2[] Points = points;
        public float Width = width;
        public int Timer;
    }
    private static readonly List<DistortionParticle> _Particles = [];
    private static readonly List<RiftCrack> _Cracks = [];
    // BasicEffect only supplies the solid-color primitive vertex transform; no external shader asset.
    private static BasicEffect _PrimitiveEffect;

    public override void Load()
    {
        if (!Main.dedServ)
            On_Main.DrawProjectiles += DrawDistortion;
    }
    public override void Unload()
    {
        Clear();
        BasicEffect effect = _PrimitiveEffect;
        _PrimitiveEffect = null;
        if (effect is not null)
            Main.QueueMainThreadAction(effect.Dispose);
    }
    public override void OnWorldUnload() => Clear();
    private static void Clear()
    {
        _Particles.Clear();
        _Cracks.Clear();
    }
    public static void SpawnParticle(Vector2 position, Vector2 velocity, float size, bool square = false, float rotation = 0f)
    {
        if (!Main.dedServ)
            _Particles.Add(new(position, velocity, size, square, rotation));
    }
    public static void SpawnCrack(Vector2 center, Vector2 velocity, float width)
    {
        if (Main.dedServ)
            return;
        Vector2[] points = new Vector2[25];
        for (int i = 0; i < points.Length; i++)
        {
            if (i == 0)
                points[i] = center;
            else if (i == 1)
                points[i] = center + velocity * Main.rand.NextFloat(0.8f, 1.2f) + Main.rand.NextVector2Circular(30f, 30f);
            else
            {
                float distance = Main.rand.NextFloat(25f, 50f) * (i == points.Length - 1 ? 0.5f : 1f);
                points[i] = points[i - 1] + (points[i - 1] - points[i - 2]).SafeNormalize(Vector2.UnitY)
                    .RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f) * i * 0.25f)) * distance;
            }
            if (Main.rand.NextBool(9))
                points[i] += Main.rand.NextVector2Circular(10f, 10f);
        }
        _Cracks.Add(new(points, width));
    }
    public override void PostUpdateEverything()
    {
        foreach (DistortionParticle particle in _Particles)
        {
            particle.Center += particle.Velocity;
            particle.Velocity *= 0.96f;
            particle.Size *= 0.91f;
        }
        _Particles.RemoveAll(particle => particle.Size <= 2f);
        foreach (RiftCrack crack in _Cracks)
            crack.Timer++;
        _Cracks.RemoveAll(crack => crack.Timer >= 30);
    }
    private static void DrawDistortion(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);
        if (_Particles.Count == 0 && _Cracks.Count == 0 || Main.gameMenu)
            return;
        SpriteBatch batch = Main.spriteBatch;
        GraphicsDevice device = Main.instance.GraphicsDevice;
        using RenderTargetLease mask = RenderTargetPool.Shared.Rent(device, Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
        using RenderTargetLease contents = RenderTargetPool.Shared.Rent(device, Main.screenWidth, Main.screenHeight, RenderTargetDescriptor.Default);
        using (mask.Scope(clearColor: Color.Transparent))
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (DistortionParticle particle in _Particles)
            {
                Texture2D texture = particle.Square ? TOTextures.FillerTexture : TOTextures.Extra.BasicCircle;
                batch.Draw(texture, particle.Center - Main.screenPosition, null, Color.White, particle.Rotation,
                    texture.Size() * 0.5f, Vector2.One * particle.Size / texture.Size(), SpriteEffects.None, 0f);
            }
            batch.End();
            DrawCracks(device);
        }
        if (AnomalyEffects.Overlays.EaterMetaballEdge is not { } edge
            || AnomalyEffects.Overlays.EaterDistortionFog is null || AnomalyEffects.Overlays.EaterDistortionWinds is null)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            batch.Draw(mask.Target, Vector2.Zero, EaterPortalAssets.Twilight * 0.65f);
            batch.End();
            return;
        }
        Texture previousTexture1 = device.Textures[1];
        Texture previousTexture2 = device.Textures[2];
        Texture previousTexture3 = device.Textures[3];
        SamplerState previousSampler1 = device.SamplerStates[1];
        SamplerState previousSampler2 = device.SamplerStates[2];
        SamplerState previousSampler3 = device.SamplerStates[3];
        using (contents.Scope(clearColor: Color.Transparent))
            DrawDistortionForeground();
        Effect shader = edge.Asset.Value;
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        shader.Parameters["screenSize"].SetValue(screenSize);
        shader.Parameters["layerSize"].SetValue(screenSize);
        shader.Parameters["layerOffset"].SetValue(Vector2.Zero);
        shader.Parameters["singleFrameScreenOffset"].SetValue(Vector2.Zero);
        shader.Parameters["layerColor"].SetValue(Vector4.One);
        shader.Parameters["edgeColor"].SetValue(new Color(136, 26, 186).ToVector4());
        device.Textures[1] = contents.Target;
        device.SamplerStates[1] = SamplerState.LinearWrap;
        batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, shader, Matrix.Identity);
        batch.Draw(mask.Target, Vector2.Zero, Color.White);
        batch.End();
        device.Textures[1] = previousTexture1;
        device.Textures[2] = previousTexture2;
        device.Textures[3] = previousTexture3;
        device.SamplerStates[1] = previousSampler1;
        device.SamplerStates[2] = previousSampler2;
        device.SamplerStates[3] = previousSampler3;
    }

    private static void DrawCracks(GraphicsDevice device)
    {
        if (_Cracks.Count == 0)
            return;
        _PrimitiveEffect ??= new BasicEffect(device) { VertexColorEnabled = true, TextureEnabled = false, LightingEnabled = false };
        _PrimitiveEffect.World = Matrix.Identity;
        _PrimitiveEffect.View = Main.GameViewMatrix.TransformationMatrix;
        _PrimitiveEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
        device.RasterizerState = RasterizerState.CullNone;
        device.DepthStencilState = DepthStencilState.None;
        device.BlendState = BlendState.AlphaBlend;
        foreach (RiftCrack crack in _Cracks)
        {
            int count = crack.Points.Length;
            float[] distances = new float[count];
            Vector2[] normals = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    distances[i] = distances[i - 1] + Vector2.Distance(crack.Points[i], crack.Points[i - 1]);
                Vector2 tangent = i == 0 ? crack.Points[1] - crack.Points[0]
                    : i == count - 1 ? crack.Points[i] - crack.Points[i - 1] : crack.Points[i + 1] - crack.Points[i - 1];
                tangent = tangent.SafeNormalize(Vector2.UnitX);
                normals[i] = new Vector2(-tangent.Y, tangent.X);
            }
            VertexPositionColor[] vertices = new VertexPositionColor[count * 2];
            for (int i = 0; i < count; i++)
            {
                Vector2 normal = i == 0 || i == count - 1 ? normals[i]
                    : (normals[i - 1] + normals[i] + normals[i + 1]).SafeNormalize(normals[i]);
                float completion = distances[i] / Math.Max(0.001f, distances[count - 1]);
                Vector2 offset = normal * crack.Width * (1f - crack.Timer / 30f) * (1f - completion);
                Vector2 center = crack.Points[i] - Main.screenPosition;
                vertices[i * 2] = new(new Vector3(center - offset, 0f), Color.White);
                vertices[i * 2 + 1] = new(new Vector3(center + offset, 0f), Color.White);
            }
            _PrimitiveEffect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
    }

    private static void DrawDistortionForeground()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

        // Draw a black tile for the same reasoning explained above for the background drawing.
        Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        
        // Draw a layer of rolling clouds in the background.
        DrawDistortionClouds_Foreground();

        // Draw the winds effect in front of everything.
        DrawDistortionWinds_Foreground();

        Main.spriteBatch.End();
    }

    private static void DrawDistortionWinds_Foreground()
    {
        Effect windsShader = AnomalyEffects.Overlays.EaterDistortionWinds.Asset.Value;

        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        /* Screen UVs keep the original 16:9 scale at any viewport. */
        Texture2D windsTexture = AnomalyTextures.Noise.Neuron;
        Texture2D highlightsTexture = AnomalyTextures.Noise.Sharp;
        Texture2D distortionTexture = AnomalyTextures.Noise.Melt2;
        Texture2D erosionTexture = AnomalyTextures.Noise.Pebble;

        windsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 1.075f);
        windsShader.Parameters["overallOpacity"].SetValue(0.8f);
        windsShader.Parameters["distortionStrength"].SetValue(0.3f);
        windsShader.Parameters["mainNoiseTextureScale"].SetValue(1.6f);
        windsShader.Parameters["distortionTextureScale"].SetValue(1.76f);
        windsShader.Parameters["erosionTextureScale"].SetValue(2f);
        windsShader.Parameters["erosionMin"].SetValue(0.5f);
        windsShader.Parameters["gradientPrecision"].SetValue(20f);

        windsShader.Parameters["aspectRatioCorrection"].SetValue(screenSize.X / screenSize.Y / (16f / 9f));
        windsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
        windsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / windsTexture.Size() * 0.025f);

        windsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.64f).ToVector3());
        windsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.32f).ToVector3());
        windsShader.Parameters["highlightsColor"].SetValue(Color.Fuchsia.ToVector3());

        Main.instance.GraphicsDevice.Textures[1] = highlightsTexture;
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

        Main.instance.GraphicsDevice.Textures[2] = distortionTexture;
        Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

        Main.instance.GraphicsDevice.Textures[3] = erosionTexture;
        Main.instance.GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

        // Fuchsia highlights.
        windsShader.CurrentTechnique.Passes["Pass0"].Apply();
        Main.spriteBatch.Draw(windsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        windsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.8f);
        windsShader.Parameters["overallOpacity"].SetValue(0.7f);
        windsShader.Parameters["distortionStrength"].SetValue(0.6f);
        windsShader.Parameters["mainNoiseTextureScale"].SetValue(1.24f);
        windsShader.Parameters["distortionTextureScale"].SetValue(0.46f);
        windsShader.Parameters["erosionTextureScale"].SetValue(3f);
        windsShader.Parameters["erosionMin"].SetValue(0.75f);
        windsShader.Parameters["highlightsColor"].SetValue(new Color(0, 221, 250).ToVector3());

        // Cyan highlights.
        windsShader.CurrentTechnique.Passes["Pass0"].Apply();
        Main.spriteBatch.Draw(windsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
    }

    private static void DrawDistortionClouds_Foreground()
    {
        Effect rollingCloudsShader = AnomalyEffects.Overlays.EaterDistortionFog.Asset.Value;
        Texture2D cloudsTexture = AnomalyTextures.Noise.Cloud;
        Texture2D erosionTexture = AnomalyTextures.Noise.Harsh;
        Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
        /* Screen UVs keep the original 16:9 scale at any viewport. */

        rollingCloudsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 2f);
        rollingCloudsShader.Parameters["overallOpacity"].SetValue(1f);
        rollingCloudsShader.Parameters["distortionStrength"].SetValue(0.12f);
        rollingCloudsShader.Parameters["mainNoiseTextureScale"].SetValue(1f);
        rollingCloudsShader.Parameters["distortionTextureScale"].SetValue(0.8f);
        rollingCloudsShader.Parameters["erosionTextureScale"].SetValue(0.26f);
        rollingCloudsShader.Parameters["erosionMin"].SetValue(0.06f);
        rollingCloudsShader.Parameters["gradientPrecision"].SetValue(20f);

        rollingCloudsShader.Parameters["aspectRatioCorrection"].SetValue(screenSize.X / screenSize.Y / (16f / 9f));
        rollingCloudsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
        rollingCloudsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.025f);

        rollingCloudsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, new(0, 221, 250), 0.25f).ToVector3());
        rollingCloudsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.Black, Color.Fuchsia, 0.25f).ToVector3());

        Main.instance.GraphicsDevice.Textures[1] = cloudsTexture;
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

        Main.instance.GraphicsDevice.Textures[2] = erosionTexture;
        Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

        rollingCloudsShader.CurrentTechnique.Passes["Pass0"].Apply();
        Main.spriteBatch.Draw(cloudsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
    }

}
