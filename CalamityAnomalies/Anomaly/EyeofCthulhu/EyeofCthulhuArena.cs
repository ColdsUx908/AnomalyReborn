// Designed by ColdsUx

using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed partial class EyeofCthulhuArena : CAModProjectile, IContentLoader
{
    #region 数据
    public static int EyeSpawnGateValue => 15;
    public static float NormalRotationSpeed => 0.005f;

    public static readonly UnaryFunctionWithDomain EyeSpinEffectIntensityFunction = UnaryFunctionWithDomain.Piecewise(
        (new MathInterval(float.NegativeInfinity, EyeofCthulhu_Handler.EyeSpinTime - 30f, false, false), x => TOMathUtils.Interpolation.QuadraticEaseInOut(x / 10f)),
        (new MathInterval(EyeofCthulhu_Handler.EyeSpinTime - 30f, float.PositiveInfinity, true, false), x => TOMathUtils.Interpolation.QuadraticEaseInOut((EyeofCthulhu_Handler.EyeSpinTime - x) / 30f))
        );

    public bool IsActivated;
    public BehaviorCommand_Arena MasterCommandReceiver;
    public bool MasterPhase3_2;

    public float RealArenaRadius = EyeofCthulhu_Handler.MaxArenaRadius;
    public float ArenaRadius = EyeofCthulhu_Handler.MaxArenaRadius;
    public float RealRotationSpeed;
    public float RotationSpeed;

    public readonly ArenaEye[] Eyes = new ArenaEye[32];
    public readonly List<ArenaStatModifier> ArenaStatModifiers = [];

    public NPC Master
    {
        get
        {
            int temp = (int)Projectile.ai[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[0] = value.whoAmI;
    }
    public EyeofCthulhu_Anomaly MasterBehavior => new() { _entity = Master };

    public Ring ArenaRing => new(Projectile.Center, RealArenaRadius - 20f, RealArenaRadius + 20f);

    public float ArenaRotation
    {
        get => Projectile.rotation;
        set => Projectile.rotation = value;
    }
    #endregion 数据

    #region 交互方法
    public float GetEyeRotation(int index)
    {
        index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
        return ArenaRotation + TOMathUtils.PiOver16 * index;
    }

    public PolarVector2 GetEyeCenterDirection(int index)
    {
        index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
        return new(GetEyeRotation(index));
    }

    public Vector2 GetEyeCenter(int index)
    {
        index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
        return Projectile.Center + GetEyeCenterDirection(index) * RealArenaRadius;
    }

    public void ChangeArenaRadius(float deltaArenaRadius, int duration, bool linear = false)
    {
        if (deltaArenaRadius != 0f)
            ArenaStatModifiers.Add(new ArenaStatModifier(ArenaStatModifier.ModifierType.ArenaRadius, duration, deltaArenaRadius, linear));
    }

    public void ChangeArenaRadiusTo(float targetArenaRadius, int duration)
    {
        float realArenaRadius = RealArenaRadius;
        foreach (ArenaStatModifier modifier in ArenaStatModifiers)
        {
            if (modifier.Type == ArenaStatModifier.ModifierType.ArenaRadius)
                realArenaRadius += modifier.Delta;
        }
        ChangeArenaRadius(targetArenaRadius - realArenaRadius, duration);
    }

    public void ChangeRotationSpeed(float deltaRotationSpeed, int duration, bool linear = false)
    {
        if (deltaRotationSpeed != 0f)
            ArenaStatModifiers.Add(new ArenaStatModifier(ArenaStatModifier.ModifierType.RotationSpeed, duration, deltaRotationSpeed, linear));
    }

    public void ChangeRotationSpeedTo(float targetRotationSpeed, int duration)
    {
        float realRotationSpeed = RealRotationSpeed;
        foreach (ArenaStatModifier modifier in ArenaStatModifiers)
        {
            if (modifier.Type == ArenaStatModifier.ModifierType.RotationSpeed)
                realRotationSpeed += modifier.Delta;
        }
        ChangeRotationSpeed(targetRotationSpeed - realRotationSpeed, duration);
    }

    public void ExecuteActionToArenaEye(int index, Action<ArenaEye> action)
    {
        index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
        ArenaEye eye = Eyes[index];
        if (eye is not null)
            action?.Invoke(eye);
    }

    public void AddHighlightTo(int index, int lifetime)
    {
        index = (int)TOMathUtils.NormalizeWithPeriod(index, 32);
        SoundEngine.PlaySound(SoundID.Item8, GetEyeCenter(index));
        ExecuteActionToArenaEye(index, e => e.Highlights.Add(new EyeHighlight(lifetime, 20, 10f)));
    }
    #endregion 交互方法

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Master = NPC.DummyNPC;
    }

    public override void AI()
    {
        if (Master is null || !Master.active || Master.type != NPCID.EyeofCthulhu)
            Projectile.Kill();

        EyeofCthulhu_Anomaly masterBehavior = MasterBehavior;
        Projectile.Center = masterBehavior.Phase3ArenaCenter;
        Projectile.timeLeft = 100;

        if (IsActivated)
        {
            switch (Timer1)
            {
                case 0:
                    ChangeRotationSpeedTo(0.25f, 10);
                    break;
                case EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 - 10:
                    ChangeRotationSpeedTo(NormalRotationSpeed, 10);
                    break;
            }

            float realArenaRadius = ArenaRadius;
            float realRotationSpeed = RotationSpeed;

            foreach (ArenaStatModifier modifier in ArenaStatModifiers)
            {
                if (modifier is null)
                    continue;

                modifier.Timer++;
                float delta = modifier.GetDeltaValue();

                switch (modifier.Type)
                {
                    case ArenaStatModifier.ModifierType.ArenaRadius:
                        realArenaRadius += delta;
                        if (modifier.Timer >= modifier.LifeTime)
                            ArenaRadius += delta;
                        break;
                    case ArenaStatModifier.ModifierType.RotationSpeed:
                        realRotationSpeed += delta;
                        if (modifier.Timer >= modifier.LifeTime)
                            RotationSpeed += delta;
                        break;
                }
            }
            ArenaStatModifiers.RemoveAll(modifier => modifier is null || modifier.Timer >= modifier.LifeTime);

            RealArenaRadius = realArenaRadius;
            RealRotationSpeed = realRotationSpeed;

            ArenaRotation += RealRotationSpeed;

            UpdateEyes();

            Timer1++;
        }

        //执行命令
        switch (MasterCommandReceiver)
        {
            case BehaviorCommand_Arena.Charge:
                Charge();
                break;
            case BehaviorCommand_Arena.EyeSpin or BehaviorCommand_Arena.EyeSpinLast:
                EyeSpin();
                break;
        }

        void UpdateEyes()
        {
            if (Timer1 % EyeSpawnGateValue == 0) //生成眼睛
            {
                int num = Timer1 / EyeSpawnGateValue;

                //num = 0: 第0、1层
                //num = 1: 第2层
                //num = 2: 第3层
                //num = 3: 第4层

                if (num <= 3)
                {
                    if (num == 0)
                        SpawnEyes(0);
                    SpawnEyes(num + 1);
                }
                else
                {
                    for (int i = 0; i < Eyes.Length; i++)
                        Eyes[i] ??= new ArenaEye(this, i);
                }
            }

            foreach (ArenaEye eye in Eyes)
                eye?.AI();

            void SpawnEyes(int num)
            {
                int eyesToSpawn = (int)Math.Pow(2, Math.Max(num, 1));
                int firstEyeIndex = num == 0 ? 0 : 16 / (int)Math.Pow(2, num);
                int numIncreasePerEye = 32 / eyesToSpawn;

                for (int j = 0; j < eyesToSpawn; j++)
                {
                    int index = (firstEyeIndex + numIncreasePerEye * j) % 32;
                    ArenaEye item = new(this, index);
                    if (num == 0)
                        item.Timer = 15;
                    Eyes[index] = item;
                }
            }
        }

        void Charge()
        {
            switch (masterBehavior.AttackCounter)
            {
                case <= 2: //普通冲刺
                    NormalCharge();
                    break;
                case 3: //第一次快速冲刺
                    FirstRapidCharge();
                    break;
                default:
                    break;
            }

            if (masterBehavior.CurrentBehavior != EyeofCthulhu_Anomaly.Behavior.Phase3_Charge)
            {
                ChangeArenaRadiusTo(EyeofCthulhu_Handler.MaxArenaRadius, 15);
                MasterCommandReceiver = BehaviorCommand_Arena.None;
            }

            void NormalCharge()
            {
                bool firstCharge = masterBehavior.AttackCounter == 0;

                int teleportDuration = firstCharge ? EyeofCthulhu_Handler.NormalTeleportDuration + 30 : EyeofCthulhu_Handler.NormalTeleportDuration;

                switch (masterBehavior.CurrentAttackPhase)
                {
                    case 1 when masterBehavior.Timer1 == 20:
                        float targetRotationSpeed = MasterPhase3_2 ? NormalRotationSpeed : 0f;
                        ChangeRotationSpeedTo(targetRotationSpeed, 15);
                        break;
                }

                if (masterBehavior.Timer1 - 1 == teleportDuration - EyeofCthulhu_Handler.NormalTeleportDuration)
                {
                    for (int i = -1; i <= 1; i++)
                        AddHighlightTo((int)TOMathUtils.NormalizeWithPeriod(masterBehavior.UsedEyeIndex1 + i, 32), EyeofCthulhu_Handler.NormalTeleportDuration + 10);
                }
            }

            void FirstRapidCharge()
            {
                switch (masterBehavior.CurrentAttackPhase)
                {
                    case 1: //调整竞技场半径，一次性生成4个高光
                        switch (masterBehavior.Timer1)
                        {
                            case 1: //调整竞技场半径
                                //ChangeArenaRadiusTo(EyeofCthulhu_Handler.MaxArenaRadius2, 75);
                                break;
                            case 5: //第一个高光
                                AddHighlightTo(masterBehavior.UsedEyeIndex1, 135);
                                break;
                            case 35: //第二个高光
                                AddHighlightTo(masterBehavior.UsedEyeIndex2, 135);
                                break;
                            case 65: //第三个高光
                                AddHighlightTo(masterBehavior.UsedEyeIndex3, 135);
                                break;
                            case 95: //第四个高光
                                AddHighlightTo(masterBehavior.UsedEyeIndex4, 135);
                                break;
                        }
                        break;
                }
            }
        }

        void EyeSpin()
        {
            bool buff = MasterCommandReceiver == BehaviorCommand_Arena.EyeSpinLast;

            int index1 = masterBehavior.UsedEyeIndex1;

            int attackPhase = masterBehavior.CurrentAttackPhase;
            int timer1 = masterBehavior.Timer1;

            switch (attackPhase)
            {
                case 1 when timer1 == 1:
                    for (int i = 0; i < 32; i++)
                    {
                        bool shouldIncreaseHighlightTime = i % (32 / (buff ? 4 : 2)) == 0;
                        int actualIndex = (int)TOMathUtils.NormalizeWithPeriod(index1 + i, 32);
                        int hightliteTime = shouldIncreaseHighlightTime ? EyeofCthulhu_Handler.EyeSpinTime + 15 : EyeofCthulhu_Handler.EyeSpinTime;
                        AddHighlightTo(actualIndex, hightliteTime);

                        if (!buff)
                        {
                            ExecuteActionToArenaEye(actualIndex, e =>
                            {
                                e.ShouldUseCustomRotation = true;
                                int iClone = i;
                                e.CustomFindRotationFunction = e1 =>
                                {
                                    Vector2 originalVector = GetEyeCenterDirection(index1) * (RealArenaRadius - 15f);
                                    float rotationOffset = TOMathUtils.PiOver16 * iClone;
                                    Vector2 destination = Projectile.Center + EyeofCthulhu_Handler.EyeShapeHelper.GetVector(originalVector, rotationOffset);
                                    Vector2 center = e1.Center;
                                    return (destination - center).ToRotation(MathHelper.Pi);
                                };
                            });
                        }
                    }
                    break;
                case 1 when Timer1 == 11:
                    SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
                    break;
                case 1 when timer1 == 12:
                    (float targetRotationSpeed, int duration) = buff ? masterBehavior.AttackCounter switch
                    {
                        0 => (0.01f, 20),
                        1 => (0.016f, 20),
                        2 => (NormalRotationSpeed, 20),
                        _ => (0f, 1)
                    } : (NormalRotationSpeed, 20);
                    ChangeRotationSpeedTo(targetRotationSpeed, duration);
                    break;
                case 1 when timer1 == EyeofCthulhu_Handler.EyeSpinTime - 1:
                    //生成弹幕

                    Vector2 originalVector = GetEyeCenterDirection(index1) * (RealArenaRadius - 15f);
                    Vector2 originalVector2 = originalVector.RotatedBy(MathHelper.PiOver2);
                    int projectileAmountPerEye = 5;
                    int maxOffset = (projectileAmountPerEye - 1) / 2;
                    int projectileAmount = Eyes.Length * projectileAmountPerEye;
                    float singleRadian = MathHelper.TwoPi / projectileAmount;
                    float? verticalHeightMultiplierOverride = buff ? EyeofCthulhu_Handler.EyeShapeHelper.VerticalHeightMultiplier2 : null;

                    for (int i = 0; i < 32; i++)
                    {
                        int actualIndex = (int)TOMathUtils.NormalizeWithPeriod(index1 + i, 32);
                        ArenaEye eye = Eyes[actualIndex];
                        if (eye is null)
                            break;

                        Vector2 offsetCenter = eye.OffsetCenter;

                        SoundEngine.PlaySound(SoundID.Item17, offsetCenter);

                        int particleAmount = 10;
                        for (int j = 0; j < particleAmount; j++)
                            EyeofCthulhu_Handler.SpawnOrbParticle(offsetCenter, Main.rand.NextFloat(3f, 4f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.5f, 0.8f));

                        PolarVector2 originalDirection = GetEyeCenterDirection(actualIndex).RotatedBy(MathHelper.Pi);

                        for (int j = -maxOffset; j <= maxOffset; j++)
                        {
                            int projectileIndex = (int)TOMathUtils.NormalizeWithPeriod(i * projectileAmountPerEye + j, projectileAmount);
                            float rotationOffset = singleRadian * projectileIndex;
                            Vector2 destination = Projectile.Center + EyeofCthulhu_Handler.EyeShapeHelper.GetVector(originalVector, rotationOffset, verticalHeightMultiplierOverride);

                            Vector2 velocity = (destination - offsetCenter) / BloodOrbProjectile.StillTime;

                            Projectile.NewProjectileAction<BloodOrbProjectile>(SourceAI, offsetCenter, velocity, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p =>
                            {
                                p.VelocityToRotation();
                                p.timeLeft = 120;
                                BloodOrbProjectile modP = p.GetModProjectile<BloodOrbProjectile>();
                                modP.Master = Master;
                                modP.ArenaProjectile = Projectile;
                                modP.Destination = destination;

                                if (p.whoAmI < Projectile.whoAmI)
                                    p.Update(p.whoAmI);
                            });
                        }

                        if (buff)
                        {
                            for (int k = -maxOffset; k <= maxOffset; k++)
                            {
                                int projectileIndex2 = (int)TOMathUtils.NormalizeWithPeriod((i + 24) * projectileAmountPerEye + k, projectileAmount);
                                float rotationOffset2 = singleRadian * projectileIndex2;
                                Vector2 destination2 = Projectile.Center + EyeofCthulhu_Handler.EyeShapeHelper.GetVector(originalVector2, rotationOffset2, verticalHeightMultiplierOverride);

                                Vector2 velocity2 = (destination2 - offsetCenter) / BloodOrbProjectile.StillTime;

                                Projectile.NewProjectileAction<BloodOrbProjectile>(SourceAI, offsetCenter, velocity2, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p =>
                                {
                                    p.VelocityToRotation();
                                    p.timeLeft = 120;
                                    BloodOrbProjectile modP = p.GetModProjectile<BloodOrbProjectile>();
                                    modP.Master = Master;
                                    modP.ArenaProjectile = Projectile;
                                    modP.Destination = destination2;

                                    if (p.whoAmI < Projectile.whoAmI)
                                        p.Update(p.whoAmI);
                                });
                            }

                            ExecuteActionToArenaEye(index1 + i, e =>
                            {
                                e.ShouldUseCustomRotation = false;
                                e.CustomFindRotationFunction = null;
                                e.ShouldFaceTarget = false;
                            });
                        }
                    }

                    break;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!IsActivated)
            return false;

        if (Master is null || !Master.active || Master.type != NPCID.EyeofCthulhu)
            return false;

        EyeofCthulhu_Anomaly masterBehavior = MasterBehavior;
        SpriteBatch spriteBatch = Main.spriteBatch;

        #region 绘制旋转攻击预警粒子
        if (masterBehavior.CurrentBehavior == EyeofCthulhu_Anomaly.Behavior.Phase3_EyeSpin && masterBehavior.CurrentAttackPhase == 1)
        {
            //逐渐睁开的眼睛

            bool buff = MasterCommandReceiver == BehaviorCommand_Arena.EyeSpinLast;

            ParticleHandler.EnterDrawRegion_Additive(spriteBatch);

            int index1 = masterBehavior.UsedEyeIndex1;

            int iterationAmount = 800;
            int iterationAmount2 = 400;
            float singleRadian = MathHelper.TwoPi / iterationAmount;
            float singleRadian2 = MathHelper.TwoPi / iterationAmount2;

            int timer1 = masterBehavior.Timer1;
            float scale = 1.25f;
            float intensity = EyeSpinEffectIntensityFunction.Process(timer1);
            Texture2D particleTexture = ParticleHandler.GetTexture<OrbParticle>();

            float rotation = GetEyeRotation(index1);
            float originalVectorLength = RealArenaRadius - 25f;
            Vector2 originalVector = new(originalVectorLength, 0f);
            Vector2 innerVector = EyeofCthulhu_Handler.EyeShapeHelper.GetInnerVector(originalVector);
            float innerVectorLength = innerVector.Length();
            float distanceMultiplier = TOMathUtils.Interpolation.ExponentialEaseInOut((timer1 - 20f) / 30f, 1.5f);
            float archHeightMultiplier = (buff ? EyeofCthulhu_Handler.EyeShapeHelper.CalculateArchHeightMultiplier(EyeofCthulhu_Handler.EyeShapeHelper.VerticalHeightMultiplier2) : EyeofCthulhu_Handler.EyeShapeHelper.ArchHeightMultiplier) * distanceMultiplier;

            for (int i = 0; i < iterationAmount; i++)
            {
                float rotationOffset = i * singleRadian;
                Vector2 offset = EyeofCthulhu_Handler.EyeShapeHelper.GetVectorDirect(originalVector, rotationOffset, archHeightMultiplier);

                float realScale = scale * EyeofCthulhu_Handler.EyeShapeHelper.GetOuterParticleScaleMultiplier(rotationOffset) * intensity;
                spriteBatch.DrawFromCenter(particleTexture, Projectile.Center + offset.RotatedBy(rotation) - Main.screenPosition, null, Color.Red * Math.Min(intensity * 1.5f, 1f), 0f, realScale);
                if (buff)
                    spriteBatch.DrawFromCenter(particleTexture, Projectile.Center + offset.RotatedBy(rotation + MathHelper.PiOver2) - Main.screenPosition, null, Color.Red * Math.Min(intensity * 1.5f, 1f), 0f, realScale);
            }

            //瞳孔部分

            float verticalHeightMultiplier = EyeofCthulhu_Handler.EyeShapeHelper.CalculateVerticalHeightMultiplier(archHeightMultiplier);
            float radiusMultiplier = EyeofCthulhu_Handler.EyeShapeHelper.CalculateRadiusMultiplier(verticalHeightMultiplier);
            float verticalHeight = originalVectorLength * verticalHeightMultiplier;
            float radius = originalVectorLength * radiusMultiplier;

            for (int i = 0; i < iterationAmount2; i++)
            {
                float rotationOffset = i * singleRadian2;
                Vector2 innerOffset = new PolarVector2(innerVectorLength, rotationOffset);

                bool shouldDrawInnerParticle =
                    Vector2.Distance(innerOffset, new Vector2(0f, verticalHeight)) < radius
                    && Vector2.Distance(innerOffset, new Vector2(0f, -verticalHeight)) < radius
                    && (!buff ||
                        (Vector2.Distance(innerOffset, new Vector2(verticalHeight, 0f)) < radius
                        && Vector2.Distance(innerOffset, new Vector2(-verticalHeight, 0f)) < radius));

                if (shouldDrawInnerParticle)
                    spriteBatch.DrawFromCenter(particleTexture, Projectile.Center + innerOffset.RotatedBy(rotation) - Main.screenPosition, null, Color.Red * Math.Min(intensity * 1.5f, 1f), 0f, scale * EyeofCthulhu_Handler.EyeShapeHelper.InnerParticleScaleMultiplier * EyeofCthulhu_Handler.EyeShapeHelper.GetOuterParticleScaleMultiplier(rotationOffset) * intensity);
            }

            ParticleHandler.ExitParticleDrawRegion(spriteBatch);
        }
        #endregion 绘制旋转攻击预警粒子

        #region 绘制血珠弹幕
        Texture2D orbTexture = EyeofCthulhu_Handler.BloodOrbTexture;
        Texture2D orbBorderTexture = EyeofCthulhu_Handler.BloodOrbBorderTexture;
        Texture2D orbBorderBigTexture = EyeofCthulhu_Handler.BloodOrbBigBorderTexture;

        List<Projectile> bloodOrbs = TOIteratorFactory.NewActiveProjectileIterator(p => p.ModProjectile is BloodOrbProjectile orb && orb.ArenaProjectile == Projectile).ToList();

        if (bloodOrbs.Count > 0) //依次进行三次绘制：大边框、主体、高亮材质
        {
            spriteBatch.ChangeBlendState(BlendState.Additive);

            foreach (Projectile p in bloodOrbs)
            {
                float intensity = TOMathUtils.Interpolation.QuadraticEaseOut((p.Timer1 - 10) / 10f);
                spriteBatch.DrawFromCenter(orbBorderBigTexture, p.Center - Main.screenPosition, null, Color.DarkRed * intensity, p.rotation, p.scale);
            }

            spriteBatch.ChangeBlendState(BlendState.AlphaBlend);

            foreach (Projectile p in bloodOrbs)
                spriteBatch.DrawFromCenter(orbTexture, p.Center - Main.screenPosition, null, Color.White, p.rotation, p.scale);

            spriteBatch.ChangeBlendState(BlendState.Additive);

            foreach (Projectile p in bloodOrbs)
            {
                float intensity = p.Timer2 > BloodOrbProjectile.StillTime ? TOMathUtils.Interpolation.QuadraticEaseOut((BloodOrbProjectile.StillTime + 10 - p.Timer2) / 10f) : TOMathUtils.Interpolation.QuadraticEaseOut((p.Timer1 - 12) / 10f);
                spriteBatch.DrawFromCenter(orbBorderTexture, p.Center - Main.screenPosition, null, Color.Red * intensity, p.rotation, p.scale);
            }

            spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
        }
        #endregion 绘制血珠弹幕

        #region 绘制眼睛
        Texture2D texture = TOAssetUtils.GetNPCTexture(ModContent.NPCType<BloodlettingServant>());

        //计算帧
        Projectile.frameCounter++;
        int frameNum;
        switch (Projectile.frameCounter)
        {
            case < 8:
                frameNum = 0;
                break;
            case < 16:
                frameNum = 1;
                break;
            default:
                Projectile.frameCounter = 0;
                frameNum = 0;
                break;
        }
        Rectangle frame = texture.Frame(1, 4, 0, frameNum);

        Color color = Color.Red * 0.75f;

        foreach (ArenaEye eye in Eyes)
            eye?.Draw(spriteBatch, texture, frame, color);
        #endregion 绘制眼睛

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Timer1 > EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 && ArenaRing.Collides(targetHitbox);

    public override void SendExtraAI(BinaryWriter writer)
    {
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
    }
}

public sealed class EyeofCthulhuArena_Player : CAPlayerBehavior
{
    public override void PostUpdate()
    {
        //黑视效果
        bool success = false;

        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is EyeofCthulhuArena arena && arena.IsActivated)
            {
                success = true;
                float scaleMultiplier = 0.008f;
                EnhancedDarknessSystem_Bridge.AddLightSource(projectile.Center, BloomParticle.BloomCircleLarge, scale: arena.RealArenaRadius * scaleMultiplier);
                EnhancedDarknessSystem_Bridge.AddLightSource(scale: 2f, opacity: MathHelper.Clamp(Main.LocalPlayer.Distance(projectile.Center) / 640f, 0, 1));
            }
        }

        if (success)
        {
            EnhancedDarknessSystem_Bridge.ChangeDarknessIntensity(Main.LocalPlayer, f => f + 0.065f);
        }
    }

    public override void ModifyScreenPosition()
    {
        foreach (Projectile arenaProjectile in Projectile.ActiveProjectiles)
        {
            if (arenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.IsActivated && Main.ScreenSize.X > arena.RealArenaRadius * 2.2f && Main.ScreenSize.Y > arena.RealArenaRadius * 2.2f)
            {
                OceanPlayer.ScreenFocusCenter = arenaProjectile.Center;
                OceanPlayer.ScreenFocusInterpolant += 0.12f;
                OceanPlayer.ScreenFocusHoldInPlaceTime = 5;
                break;
            }
        }
    }
}