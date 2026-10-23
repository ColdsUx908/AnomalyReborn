using Anomalies.Assets.Effects;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 世界坐标连续顶点带；不再用一段段 SpriteBatch 像素矩形拼接因果线。
internal static class EventideReunionSamsaraThreadDrawing
{

    internal static void Draw(EventideReunionSamsaraTrail trail, Vector2 tip, float recall)
    {
        (int first, float fraction) = trail.RetractionStart(recall);
        if (first >= trail.Points.Count - 1)
            return;
        float extraLength = trail.Frozen ? 0f : Vector2.Distance(trail.Points[^1], tip);
        bool attachTip = extraLength > 0.05f;
        int count = trail.Points.Count - first + (attachTip ? 1 : 0);
        Vector2[] centers = new Vector2[count];
        float[] distances = new float[count];
        VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[count * 2];
        if (AnomalyEffects.Primitives.EventideSamsaraThread is not { } eventideSamsaraThreadShader)
            return;
        Effect effect = eventideSamsaraThreadShader.Asset.Value;
        GraphicsDevice device = Main.instance.GraphicsDevice;
        BlendState blend = device.BlendState;
        DepthStencilState depth = device.DepthStencilState;
        RasterizerState rasterizer = device.RasterizerState;
        try
        {
            effect.Parameters["SpearMode"].SetValue(0f);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            effect.Parameters["TransformMatrix"].SetValue(Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f));
            effect.Parameters["StartDistance"].SetValue(MathHelper.Lerp(trail.Distances[first], trail.Distances[first + 1], fraction));
            effect.Parameters["EndDistance"].SetValue(trail.Distances[^1] + extraLength);
            for (int strand = 0; strand < 6; strand++)
            {
                centers[0] = Vector2.Lerp(trail.StrandPoint(first, strand, extraLength),
                    trail.StrandPoint(first + 1, strand, extraLength), fraction);
                distances[0] = MathHelper.Lerp(trail.Distances[first], trail.Distances[first + 1], fraction);
                for (int i = first + 1; i < trail.Points.Count; i++)
                {
                    centers[i - first] = trail.StrandPoint(i, strand, extraLength);
                    distances[i - first] = trail.Distances[i];
                }
                if (attachTip)
                {
                    centers[^1] = tip;
                    distances[^1] = trail.Distances[^1] + extraLength;
                }
                Color color = Color.Lerp(new Color(45, 220, 235), new Color(210, 170, 255), strand / 5f);
                for (int i = 0; i < count; i++)
                {
                    // 裁剪端插值原带边缘；其余采样沿用原法线，不能因删掉前段而改变下一段宽度。
                    Vector2 normal = i == 0 ? Vector2.Lerp(StrandNormal(first), StrandNormal(first + 1), fraction)
                        : StrandNormal(first + i);
                    float fade = MathHelper.SmoothStep(0f, 1f, Math.Min(1f, distances[i] / 24f));
                    // 汇合点有六条线叠加，稍减强度以免矛尖变成纯白光团。
                    float fan = EventideReunionSamsaraTrail.FanOut(distances[^1] - distances[i]);
                    Color tint = color;
                    tint.A = (byte)(165f * fade * (0.45f + 0.55f * fan));
                    float u = distances[i];
                    vertices[i * 2] = new(new Vector3(centers[i] - normal * 6f, 0f), tint, new Vector2(u, 0f));
                    vertices[i * 2 + 1] = new(new Vector3(centers[i] + normal * 6f, 0f), tint, new Vector2(u, 1f));
                }
                Vector2 StrandNormal(int index)
                {
                    if (index >= trail.Points.Count)
                    {
                        Vector2 direction = (tip - trail.StrandPoint(trail.Points.Count - 1, strand, extraLength))
                            .SafeNormalize(Vector2.UnitX);
                        return new Vector2(-direction.Y, direction.X);
                    }
                    float distance = trail.Distances[index];
                    Vector2 next = attachTip && distance + 3f >= trail.Distances[^1] ? tip
                        : trail.StrandAtDistance(distance + 3f, strand, extraLength);
                    Vector2 previous = trail.StrandAtDistance(distance - 3f, strand, extraLength);
                    Vector2 tangent = (next - previous).SafeNormalize(Vector2.UnitX);
                    return new Vector2(-tangent.Y, tangent.X);
                }
                effect.CurrentTechnique.Passes["Pass0"].Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length - 2);
            }
        }
        finally
        {
            device.BlendState = blend;
            device.DepthStencilState = depth;
            device.RasterizerState = rasterizer;
        }
    }
}
