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
        public BehaviorCommand_ArenaEye MasterCommandReceiver;
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

            //执行命令
            switch (MasterCommandReceiver)
            {
                case BehaviorCommand_ArenaEye.ShootBlood_Charge:
                    ShootBlood_Charge();
                    break;
                case BehaviorCommand_ArenaEye.ShootBlood_EyeSpin:
                    ShootBlood_EyeSpin();
                    break;
                default:
                    Timer2 = 0;
                    break;
            }

            void ShootBlood_Charge()
            {
                bool shouldIncreaseTimer = true;

                ShouldFaceTarget = true;
                if (Timer2 == 0)
                    Highlights.Add(new EyeHighlight(EyeofCthulhu_Handler.NormalTeleportDuration + 10, 20, 10f));
                else if (Timer2 == EyeofCthulhu_Handler.NormalTeleportDuration)
                {
                    int particleAmount = 20;
                    for (int i = 0; i < particleAmount; i++)
                        EyeofCthulhu_Handler.SpawnOrbParticle(Center, Main.rand.NextFloat(3f, 4f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 0.8f));

                    Vector2 offsetCenter = OffsetCenter;
                    Vector2 velocity = (Target.Center - offsetCenter).ToCustomLength(15f);
                    Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, velocity, ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);
                    if (MasterPhase3_2)
                    {
                        Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, velocity * 0.5f, ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);
                        Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, velocity * 1.35f, ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);
                    }

                    shouldIncreaseTimer = false;
                    ShouldFaceTarget = false;

                    Timer2 = 0;
                    MasterCommandReceiver = BehaviorCommand_ArenaEye.None;
                }

                if (shouldIncreaseTimer)
                    Timer2++;
            }

            void ShootBlood_EyeSpin()
            {
                bool shouldIncreaseTimer = true;

                ShouldFaceTarget = true;

                int shootBloodGateValue = 25;
                if (Timer2 % shootBloodGateValue == 0)
                {
                    int num = Timer2 / shootBloodGateValue;

                    if (num > 0)
                    {
                        int particleAmount = 15;
                        for (int i = 0; i < particleAmount; i++)
                            EyeofCthulhu_Handler.SpawnOrbParticle(Center, Main.rand.NextFloat(3f, 4f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 0.8f));

                        Vector2 offsetCenter = OffsetCenter;
                        Vector2 velocity = (Target.Center - offsetCenter).ToCustomLength(20f);
                        Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, velocity, ProjectileID.BloodNautilusShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);

                        if (num >= 2)
                        {
                            shouldIncreaseTimer = false;
                            ShouldFaceTarget = false;

                            Timer2 = 0;
                            MasterCommandReceiver = BehaviorCommand_ArenaEye.None;
                        }
                    }
                }

                if (shouldIncreaseTimer)
                    Timer2++;
            }
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
