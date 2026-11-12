using Anomalies.Assets.Effects;

namespace Anomalies.Bosses.Skeletron;

internal static class SkeletronPhase3Warnings
{
    internal static void Draw(SpriteBatch spriteBatch, SkeletronPhase3Arena arena)
    {
        if (Main.dedServ || !arena.IsActivated || !AnomalyClientConfig.Instance.EnableShaders
            || AnomalyEffects.Overlays.SkeletronPhase3Warning is not AnomalyEffects.MiscShaderContainer shader
            || arena.WarningOpacity <= 0f)
            return;

        Hexadecagon polygon = arena.ArenaPolygon;
        float age = arena.WarningAge;
        int direction = arena.WarningDirection;
        float startAngle = arena.WarningStartAngle;
        Effect effect = shader.Asset.Value;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
        projection.M41 -= 1f / Main.screenWidth;
        projection.M42 += 1f / Main.screenHeight;
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        switch (arena.WarningBehavior)
        {
            case SkeletronHead.Behavior.Phase3_GuardianRain:
                // 碎裂下坠雨痕，标出第一批雨幕的通道；入场后平滑退去。
                float rainOpacity = Envelope(age, SkeletronHead.Phase3Patterns.RainStart, 18f);
                if (rainOpacity > 0f)
                {
                    // 预警只标记首批雨幕的通道；淡出期间不跟随随后移动的雨幕偏移。
                    DrawPattern("Pass0", MathHelper.Pi, age, polygon.CircumRadius, 0f, 0f, rainOpacity);
                }
                break;
            case SkeletronHead.Behavior.Phase3_GuardianPatrol:
                // 五层断裂轨道提前显示缺口，少量沿转向流动的碎齿提示内快外慢。
                float patrolOpacity = Envelope(age, SkeletronHead.Phase3Patterns.PreparationTime, 24f);
                if (patrolOpacity > 0f)
                {
                    for (int layer = 0; layer < SkeletronHead.Phase3Patterns.PatrolLayers; layer++)
                    {
                        float flightAge = Math.Max(0f, age - SkeletronHead.Phase3Patterns.PreparationTime);
                        float radius = polygon.CircumRadius * SkeletronHead.Phase3Patterns.PatrolRadiusFactor(layer);
                        float gapAngle = direction * SkeletronHead.Phase3Patterns.PatrolSpeed(layer) * flightAge;
                        DrawPattern("Patrol", MathHelper.Pi, age, radius, gapAngle, SkeletronHead.Phase3Patterns.PatrolGapHalfAngle,
                            patrolOpacity, SkeletronHead.Phase3Patterns.PatrolSpeed(layer));
                    }
                }
                break;
            case SkeletronHead.Behavior.Phase3_BoneTide:
                for (int wave = 0; wave < SkeletronHead.Phase3Patterns.TideWaves; wave++)
                {
                    float waveAge = age - wave * SkeletronHead.Phase3Patterns.TideInterval;
                    float opacity = Envelope(waveAge, SkeletronHead.Phase3Patterns.PreparationTime, 16f);
                    if (opacity <= 0f)
                        continue;
                    float gapAngle = startAngle + direction * (wave * SkeletronHead.Phase3Patterns.TideGapStep
                        + waveAge * SkeletronHead.Phase3Patterns.TideAngularSpeed);
                    // 外侧旋涡骨痕展示当前波的三个缺口和将要内收的方向。
                    DrawPattern("Tide", SkeletronHead.Phase3Patterns.TideGapPeriod, waveAge, polygon.CircumRadius, gapAngle,
                        SkeletronHead.Phase3Patterns.TideGapHalfAngle, opacity, SkeletronHead.Phase3Patterns.TideAngularSpeed);
                }
                break;
        }
        spriteBatch.End();

        void DrawPattern(string passName, float gapPeriod, float patternAge, float radius, float gapAngle, float gapHalfAngle,
            float opacity, float angularSpeed = 0f)
        {
            // 每次绘制设置完整参数；全屏像素通过逆相机矩阵还原为世界坐标。
            // 三种模式编译为独立 Pass，避免 FNA 动态模式分支与 XNA 表现不同。
            // 不把多 Pass Effect 交给 SpriteBatch，否则它会依次绘制所有 Pass。
            // 不经 MiscShaderData.Apply，避免标准 uOpacity 被其默认透明度覆盖。
            effect
                .SetCustomParameter("uTransformMatrix", projection)
                .SetCustomParameter("uScreenPosition", Main.screenPosition)
                .SetCustomParameter("uInverseViewMatrix", Matrix.Invert(Main.GameViewMatrix.TransformationMatrix))
                .SetCustomParameter("uCenter", polygon.Center)
                .SetCustomParameter("uArenaRadius", polygon.CircumRadius)
                .SetCustomParameter("uRadius", radius)
                .SetCustomParameter("uGapAngle", gapAngle)
                .SetCustomParameter("uGapHalfAngle", gapHalfAngle)
                .SetCustomParameter("uGapPeriod", gapPeriod)
                .SetCustomParameter("uAge", patternAge)
                .SetCustomParameter("uDirection", (float)direction)
                .SetCustomParameter("uAngularSpeed", angularSpeed)
                .SetCustomParameter("uPreparationTime", (float)SkeletronHead.Phase3Patterns.PreparationTime)
                .SetCustomParameter("uRadialSpeed", SkeletronPhase3Guardian.TideSpeed)
                .SetCustomParameter("uOpacity", opacity * arena.Intensity * arena.WarningOpacity);
            effect.CurrentTechnique.Passes[passName].Apply();
            spriteBatch.Draw(TOTextures.FillerTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }
    }

    private static float Envelope(float age, float preparation, float fadeTime) => age < 0f ? 0f
        : MathHelper.Clamp(age / 12f, 0f, 1f) * MathHelper.Clamp((preparation + fadeTime - age) / fadeTime, 0f, 1f);
}
