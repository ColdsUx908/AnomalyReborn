namespace Anomalies.Bosses.EyeofCthulhu;

public sealed partial class EyeofCthulhuArena
{
    /// <summary>
    /// 竞技场边缘的眼睛单体。
    /// <br/>随竞技场同步状态和高光预警，不传输绘制缓存或委托。
    /// </summary>
    public sealed class ArenaEye
    {
        public EyeofCthulhuArena Master;
        public int Index;
        public float Rotation;
        public bool ShouldUseCustomRotation;
        public int CustomRotationOriginIndex = -1;
        public bool ShouldFaceTarget;
        public int Timer;
        public int Timer2;
        public bool MasterPhase32;
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

            float targetRotation = (ShouldUseCustomRotation && CustomRotationOriginIndex >= 0 ? GetCustomRotation() : (float?)null)
                ?? (ShouldFaceTarget ? (Target.Center - Master.GetEyeCenter(Index)).ToRotation(MathHelper.Pi) : Master.GetEyeRotation(Index));
            float rotationSpeed = Math.Max(Master.RotationSpeed + 0.1f, 0.3f);
            EyeofCthulhuHandler.UpdateRotation(ref Rotation, targetRotation, rotationSpeed);

            foreach (EyeHighlight highlight in Highlights)
                highlight?.Update();

            Highlights.RemoveAll(hightlight => hightlight is null || hightlight.ShouldBeRemoved);
        }

        private float GetCustomRotation()
        {
            Vector2 originalVector = Master.GetEyeCenterDirection(CustomRotationOriginIndex) * (Master.Radius - 15f);
            int offset = (Index - CustomRotationOriginIndex + 32) % 32;
            Vector2 destination = MasterProjectile.Center + EyeofCthulhuHandler.EyeShapeHelper.GetVector(originalVector, TOMathUtils.PiOver16 * offset);
            return (destination - Center).ToRotation(MathHelper.Pi);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Rotation);
            writer.Write(ShouldUseCustomRotation);
            writer.Write(CustomRotationOriginIndex);
            writer.Write(ShouldFaceTarget);
            writer.Write(Timer);
            writer.Write(Timer2);
            writer.Write(MasterPhase32);
            writer.Write(Highlights.Count);
            foreach (EyeHighlight highlight in Highlights)
            {
                writer.Write(highlight.LifeTime);
                writer.Write(highlight.AnimationTime);
                writer.Write(highlight.FinalBorderWidth);
                writer.Write(highlight.FadeBeginTimer ?? -1);
                writer.Write(highlight.Timer);
            }
        }

        public static ArenaEye Read(BinaryReader reader, EyeofCthulhuArena master, int index)
        {
            ArenaEye eye = new(master, index)
            {
                Rotation = reader.ReadSingle(),
                ShouldUseCustomRotation = reader.ReadBoolean(),
                CustomRotationOriginIndex = reader.ReadInt32(),
                ShouldFaceTarget = reader.ReadBoolean(),
                Timer = reader.ReadInt32(),
                Timer2 = reader.ReadInt32(),
                MasterPhase32 = reader.ReadBoolean()
            };
            if (eye.CustomRotationOriginIndex is < -1 or >= 32)
                throw new InvalidDataException("Invalid eye rotation origin.");
            int count = reader.ReadInt32();
            if ((uint)count > 256)
                throw new InvalidDataException("Invalid eye highlight count.");
            for (int i = 0; i < count; i++)
            {
                int lifetime = reader.ReadInt32();
                int animationTime = reader.ReadInt32();
                float borderWidth = reader.ReadSingle();
                int fadeBegin = reader.ReadInt32();
                int timer = reader.ReadInt32();
                if (lifetime <= 0 || animationTime <= 0 || fadeBegin < -1)
                    throw new InvalidDataException("Invalid eye highlight.");
                eye.Highlights.Add(new EyeHighlight(lifetime, animationTime, borderWidth)
                {
                    FadeBeginTimer = fadeBegin == -1 ? null : fadeBegin,
                    Timer = timer
                });
            }
            return eye;
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

