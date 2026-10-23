using Anomalies.Assets.Effects;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 两枚标记统一排序与尺寸，避免 GlobalNPC 钩子顺序改变叠色结果。
public sealed class EventideReunionMarkDrawing : AnomalyGlobalNPCBehavior
{
    internal static float SamsaraRadius(NPC npc)
        => MathHelper.Clamp(Math.Max(npc.width, npc.height) * 0.55f, 38f, 88f);

    internal static float SamsaraVisibility(NPC npc)
    {
        int time = npc.Anomaly.EventideReunion_SamsaraVisualTime;
        return time <= 0 ? 0f : MathHelper.SmoothStep(0f, 1f,
            Math.Min((EventideReunionSamsaraRecord.Duration - time + 1) / 12f, time / 18f));
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ)
            return;
        float death = MathHelper.SmoothStep(0f, 1f, npc.Anomaly.EventideReunion_DeathMarkProgress);
        int time = npc.Anomaly.EventideReunion_SamsaraVisualTime;
        float samsara = SamsaraVisibility(npc);
        if (death <= 0.001f && samsara <= 0.001f)
            return;
        Vector2 center = npc.Center + new Vector2(0f, npc.gfxOffY);
        spriteBatch.End();
        try
        {
            if (samsara > 0.001f)
            {
                float radius = SamsaraRadius(npc);
                DrawSamsara(center, radius * MathHelper.Lerp(0.78f, 1f, samsara)
                    * MathHelper.Lerp(1f, 1.15f, death), screenPos, samsara,
                    time / (float)EventideReunionSamsaraRecord.Duration, death);
            }
            if (death > 0.001f)
            {
                float radius = MathHelper.Clamp(Math.Max(npc.width, npc.height) * 0.55f, 30f, 70f);
                DrawDeath(center, radius * death * MathHelper.Lerp(1f, 0.8f, samsara), screenPos, samsara);
            }
        }
        finally
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    internal static void DrawDeath(Vector2 center, float radius, Vector2 screenPosition, float companion = 0f)
    {
        if (AnomalyEffects.Primitives.EventideDeathMark is not { } eventideDeathMarkShader)
            return;
        DrawQuad(eventideDeathMarkShader.Asset.Value, center, radius, screenPosition, 1f, 1f, companion);
    }

    internal static void DrawSamsara(Vector2 center, float radius, Vector2 screenPosition,
        float opacity, float remaining, float companion)
    {
        if (AnomalyEffects.Primitives.EventideSamsaraMark is not { } eventideSamsaraMarkShader)
            return;
        DrawQuad(eventideSamsaraMarkShader.Asset.Value, center, radius, screenPosition, opacity, remaining, companion);
    }

    // 调用前结束 SpriteBatch；所有 shader 参数逐次设置，防止单标记/双标记之间串状态。
    private static void DrawQuad(Effect effect, Vector2 center, float radius, Vector2 screenPosition,
        float opacity, float remaining, float companion)
    {
        VertexPositionColorTexture[] vertices =
        [
            new(new Vector3(center + new Vector2(-radius, -radius), 0f), Color.White, Vector2.Zero),
            new(new Vector3(center + new Vector2(radius, -radius), 0f), Color.White, Vector2.UnitX),
            new(new Vector3(center + new Vector2(-radius, radius), 0f), Color.White, Vector2.UnitY),
            new(new Vector3(center + new Vector2(radius, radius), 0f), Color.White, Vector2.One)
        ];
        GraphicsDevice device = Main.instance.GraphicsDevice;
        BlendState blend = device.BlendState;
        DepthStencilState depth = device.DepthStencilState;
        RasterizerState rasterizer = device.RasterizerState;
        try
        {
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            effect.Parameters["TransformMatrix"].SetValue(Matrix.CreateTranslation(-screenPosition.X, -screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f));
            effect.Parameters["Time"].SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["Opacity"]?.SetValue(opacity);
            effect.Parameters["Remaining"]?.SetValue(remaining);
            effect.Parameters["CompanionPresence"].SetValue(companion);
            effect.CurrentTechnique.Passes["Pass0"].Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
        }
        finally
        {
            device.BlendState = blend;
            device.DepthStencilState = depth;
            device.RasterizerState = rasterizer;
        }
    }
}
