// Designed by ColdsUx

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed partial class EyeofCthulhuArena
{
    /// <summary>
    /// 竞技场边缘的眼睛单体。
    /// <br/>不同步。
    /// </summary>
    public sealed class ArenaEye
    {
        public EyeofCthulhuArena Master;
        public int Index;
        public float Rotation;
        public bool ShouldUseCustomRotation;
        public Func<ArenaEye, float> CustomFindRotationFunction;
        public bool ShouldFaceTarget;
        public int Timer;
        public int Timer2;
        public bool MasterPhase3_2;
        public List<EyeHighlight> Highlights = [];

        public Projectile MasterProjectile => Master.Projectile;
        public Player Target => Master.Master.PlayerTarget;
        public Vector2 Center => Master.GetEyeCenter(Index);

        public Vector2 OffsetCenter => Center + new PolarVector2(BloodlettingServant.ProjectileOffset, Rotation + MathHelper.Pi);

        public ArenaEye(EyeofCthulhuArena master, int index)
        {
            Master = master;
            Index = index;
        }

        public void AI()
        {
            Timer++;

            float targetRotation = (ShouldUseCustomRotation ? CustomFindRotationFunction?.Invoke(this) : null)
                ?? (ShouldFaceTarget ? (Target.Center - Master.GetEyeCenter(Index)).ToRotation(MathHelper.Pi) : Master.GetEyeRotation(Index));
            float rotationSpeed = Math.Max(Master.RealRotationSpeed + 0.1f, 0.3f);
            EyeofCthulhu_Handler.UpdateRotation(ref Rotation, targetRotation, rotationSpeed);

            foreach (EyeHighlight highlight in Highlights)
                highlight?.Update();

            Highlights.RemoveAll(hightlight => hightlight is null || hightlight.ShouldBeRemoved);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle frame, Color color)
        {
            Vector2 drawPosition = Master.GetEyeCenter(Index) - Main.screenPosition;
            Color lightColor = Color.Lerp(color, Color.White, 0.1f);
            foreach (EyeHighlight highlight in Highlights)
                TODrawUtils.DrawBorderTextureFromCenter(spriteBatch, texture, drawPosition, frame, lightColor, Rotation, MasterProjectile.scale, borderWidth: highlight.BorderWidth);
            spriteBatch.DrawFromCenter(texture, drawPosition, frame, color * Math.Clamp((float)Timer / EyeSpawnGateValue, 0f, 1f), Rotation, MasterProjectile.scale);
        }
    }

    public sealed class EyeHighlight
    {
        public int LifeTime;
        public int AnimationTime;
        public float FinalBorderWidth;
        public int? FadeBeginTimer;
        public int Timer;

        public float Ratio
        {
            get
            {
                float ratio;

                if (FadeBeginTimer.HasValue)
                {
                    int fadeBeginTimer = FadeBeginTimer.Value;
                    int startDist = Math.Min(fadeBeginTimer, LifeTime - fadeBeginTimer);
                    float startRatio = Math.Clamp((float)startDist / AnimationTime, 0f, 1f);
                    int elapsed = Timer - fadeBeginTimer;
                    float progress = Math.Clamp((float)elapsed / AnimationTime, 0f, 1f);
                    ratio = startRatio * 1f - progress;
                }
                else
                {
                    int dist = Math.Min(Timer, LifeTime - Timer);
                    ratio = Math.Clamp((float)dist / AnimationTime, 0f, 1f);
                }

                return TOMathUtils.Interpolation.QuadraticEaseInOut(ratio);
            }
        }
        public float Opacity => Ratio;
        public float BorderWidth => FinalBorderWidth * Ratio;

        public bool ShouldBeRemoved => Timer >= LifeTime || Timer - FadeBeginTimer >= AnimationTime;

        public EyeHighlight(int lifeTime, int animationTime, float finalBorderWidth)
        {
            LifeTime = lifeTime;
            AnimationTime = animationTime;
            FinalBorderWidth = finalBorderWidth;
        }

        public void BeginFade() => FadeBeginTimer = Timer;

        public void Update() => Timer++;
    }
}
