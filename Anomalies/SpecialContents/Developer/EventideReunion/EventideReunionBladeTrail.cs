using Anomalies.Assets.Effects;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 调用前结束 SpriteBatch；所有刀光共用上挑的材质及径向 UV。
internal static class EventideReunionBladeTrail
{
    public static void DrawArc(Vector2 grip, float head, float tail, float opacity, float time, Func<float, Vector2> sampleVector, bool deathPalette = false, bool samsaraPalette = false)
    {
        if (head <= tail || opacity <= 0f)
            return;
        const int Samples = 96;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Samples * 2];
        for (int i = 0; i < Samples; i++)
        {
            float factor = i / (Samples - 1f);
            Vector2 vector = sampleVector(MathHelper.Lerp(tail, head, factor));
            Color tint = new(1f, 1f, 1f, MathF.Sin(factor * MathHelper.PiOver2) * opacity);
            vertices[i * 2] = new(new Vector3(grip + vector * 0.23f, 0f), tint, new Vector2(factor, 1f));
            vertices[i * 2 + 1] = new(new Vector3(grip + vector, 0f), tint, new Vector2(factor, 0f));
        }
        Draw(vertices, time, deathPalette, samsaraPalette);
    }


    public static void Draw(VertexPositionColorTexture[] vertices, float time, bool deathPalette = false, bool samsaraPalette = false)
    {
        if (AnomalyEffects.Primitives.EventideSweep is not { } eventideSweepShader)
            return;
        Effect trailEffect = eventideSweepShader.Asset.Value;
        GraphicsDevice device = Main.instance.GraphicsDevice;
        BlendState oldBlend = device.BlendState;
        DepthStencilState oldDepth = device.DepthStencilState;
        RasterizerState oldRasterizer = device.RasterizerState;
        SamplerState oldSampler = device.SamplerStates[0];
        try
        {
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            trailEffect.Parameters["TransformMatrix"].SetValue(Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f));
            trailEffect.Parameters["Shape"].SetValue(EventideReunionAssets.SpearTrail);
            trailEffect.Parameters["Time"].SetValue(time);
            // 每次绘制显式设置，避免共享 Effect 的紫色状态影响普通攻击和 Sun。
            trailEffect.Parameters["DeathPalette"].SetValue(deathPalette ? 1f : 0f);
            trailEffect.Parameters["SamsaraPalette"].SetValue(samsaraPalette ? 1f : 0f);
            trailEffect.CurrentTechnique.Passes["Pass0"].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
        finally
        {
            device.BlendState = oldBlend;
            device.DepthStencilState = oldDepth;
            device.RasterizerState = oldRasterizer;
            device.SamplerStates[0] = oldSampler;
        }
    }
}
