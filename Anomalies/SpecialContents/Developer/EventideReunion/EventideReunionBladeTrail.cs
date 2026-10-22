namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 调用前结束 SpriteBatch；所有刀光共用上挑的材质及径向 UV。
internal static class EventideReunionBladeTrail
{
    [LoadTexture(EventideReunionHoldout.AssetPath + "SpearTrail")]
    private static Asset<Texture2D> _SpearTrail;
    private static Asset<Effect> _SweepEffect;

    public static void Draw(VertexPositionColorTexture[] vertices, float time)
    {
        _SweepEffect ??= ModContent.Request<Effect>("Anomalies/Assets/Effects/Primitives/EventideSweep", AssetRequestMode.ImmediateLoad);
        Effect trailEffect = _SweepEffect.Value;
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
            trailEffect.Parameters["Shape"].SetValue(_SpearTrail.Value);
            trailEffect.Parameters["Time"].SetValue(time);
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
