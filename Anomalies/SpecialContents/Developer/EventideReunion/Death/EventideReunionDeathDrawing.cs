using Anomalies.Assets.Effects;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 自定义投影弧面；调用前结束 SpriteBatch，调用后由调用者恢复批次。
internal static class EventideReunionDeathDrawing
{
    public static void DrawSlash(Vector2 grip, Vector2 aim, float swingSign, float progress)
    {
        if (progress <= 0f)
            return;
        const int Samples = 96;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Samples * 2];
        float tail = Math.Max(0f, progress - 0.55f);
        for (int i = 0; i < Samples; i++)
        {
            float factor = i / (Samples - 1f);
            Vector2 vector = EventideReunionDeathGeometry.SlashVector(MathHelper.Lerp(tail, progress, factor), aim, swingSign);
            Color tint = new(1f, 1f, 1f, MathF.Sin(factor * MathHelper.PiOver2));
            vertices[i * 2] = new(new Vector3(grip + vector * 0.23f, 0f), tint, new Vector2(factor, 1f));
            vertices[i * 2 + 1] = new(new Vector3(grip + vector, 0f), tint, new Vector2(factor, 0f));
        }
        // 与其他攻击共用刀光材质、流纹和渲染，仅切换为紫色；星空弧面只用于飞行波。
        EventideReunionBladeTrail.Draw(vertices, Main.GlobalTimeWrappedHourly, deathPalette: true);
    }

    public static void DrawCrescent(Vector2 center, Vector2 aim, float swingSign, float progress, float opacity, float scale = 1f)
    {
        if (progress <= 0f || opacity <= 0f)
            return;
        const int Samples = 96;
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Samples * 2];
        float head = MathHelper.Clamp(progress, 0f, 1f);
        for (int i = 0; i < Samples; i++)
        {
            float factor = i / (Samples - 1f);
            Vector2 vector = EventideReunionDeathGeometry.SlashVector(head * factor, aim, swingSign) * scale;
            Color color = new(1f, 1f, 1f, opacity);
            vertices[i * 2] = new(new Vector3(center + vector, 0f), color, new Vector2(factor, 0f));
            vertices[i * 2 + 1] = new(new Vector3(center + vector * EventideReunionDeathGeometry.InnerRatio, 0f), color, new Vector2(factor, 1f));
        }
        if (AnomalyEffects.Primitives.EventideDeathSweep is not { } eventideDeathSweepShader)
            return;
        Effect effect = eventideDeathSweepShader.Asset.Value;
        effect.Parameters["Starfield"].SetValue(EventideReunionDeathAssets.Starfield);
        DrawMesh(effect, vertices, Main.screenPosition);
    }

    public static void DrawMark(Vector2 center, float radius, Vector2 screenPosition)
        => EventideReunionMarkDrawing.DrawDeath(center, radius, screenPosition);

    public static void DrawVignette(float opacity)
    {
        if (AnomalyEffects.Overlays.EventideDeathVignette is not { } eventideDeathVignetteShader)
            return;
        Effect effect = eventideDeathVignetteShader.Asset.Value;
        effect.Parameters["Resolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
        effect.Parameters["Opacity"].SetValue(opacity);
        VertexPositionColorTexture[] vertices =
        [
            new(Vector3.Zero, Color.White, Vector2.Zero),
            new(new Vector3(Main.screenWidth, 0f, 0f), Color.White, Vector2.UnitX),
            new(new Vector3(0f, Main.screenHeight, 0f), Color.White, Vector2.UnitY),
            new(new Vector3(Main.screenWidth, Main.screenHeight, 0f), Color.White, Vector2.One)
        ];
        DrawMesh(effect, vertices, Vector2.Zero, true);
    }

    private static void DrawMesh(Effect effect, VertexPositionColorTexture[] vertices, Vector2 screenPosition, bool screenSpace = false)
    {
        GraphicsDevice device = Main.instance.GraphicsDevice;
        BlendState blend = device.BlendState;
        DepthStencilState depth = device.DepthStencilState;
        RasterizerState rasterizer = device.RasterizerState;
        SamplerState sampler = device.SamplerStates[0];
        try
        {
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            device.SamplerStates[0] = SamplerState.LinearWrap;
            effect.Parameters["TransformMatrix"].SetValue((screenSpace ? Matrix.Identity
                : Matrix.CreateTranslation(-screenPosition.X, -screenPosition.Y, 0f) * Main.GameViewMatrix.TransformationMatrix)
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f));
            effect.Parameters["Time"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.CurrentTechnique.Passes["Pass0"].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
        }
        finally
        {
            device.BlendState = blend;
            device.DepthStencilState = depth;
            device.RasterizerState = rasterizer;
            device.SamplerStates[0] = sampler;
        }
    }
}
