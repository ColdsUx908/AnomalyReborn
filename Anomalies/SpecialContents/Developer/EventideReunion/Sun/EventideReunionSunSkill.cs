namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSunSkill : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public const int Cooldown = 10 * 60;
    public const float BlastRadius = 8f * 16f;
    private const int PlantDuration = 18;
    private const int SpinDuration = 36;
    private const int ChargeDuration = 30;
    public const int MaxDashDuration = 90;
    public const float DashSpeed = 48f;
    private const float TargetRange = DashSpeed * MaxDashDuration;
    private const int RetractDuration = 4;
    private const int SlashChargeDuration = 12;
    private const int SlashDuration = 10;
    private const int RecoveryDuration = 8;
    private const float SpinWeaponScale = EventideReunionSunGeometry.SweepOuterRadius / 268f;

    private enum SunPhase { Plant, Spin, Charge, Dive, Retract, SlashCharge, Slash, Recovery }
    public Vector2 Target { get; set; }
    private Vector2 DiveDirection { get; set; }
    private Vector2 SlashAim { get; set; }
    private Vector2 RetractOffset { get; set; }
    private Vector2 RetractVector { get; set; }
    private int Timer { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
    private SunPhase Phase { get => (SunPhase)Projectile.ai[1]; set => Projectile.ai[1] = (float)value; }
    private int Facing => Projectile.ai[2] >= 0f ? 1 : -1;
    public bool IsDashing => Phase == SunPhase.Dive;
    public float HorizontalTurnAngle => Phase == SunPhase.Spin ? MathHelper.TwoPi * SpinProgress : 0f;
    private float SpinProgress => _SpinPoseProgress;
    private float SlashProgress => _SlashPoseProgress;
    private Vector2 Grip => Projectile.Owner.MountedCenter + new Vector2(0f, -4f * Projectile.Owner.gravDir);
    private SunPhase? _PreviousPhase;
    private Vector2 _PreviousTip;
    private Vector2 _WeaponCenter;
    private Vector2 _WeaponDirection;
    private float _WeaponScale = 1f;
    private Vector2 _DashStart;
    private float _SpinPoseProgress;
    private float _SlashPoseProgress;
    private Vector2 WeaponTip => _WeaponCenter + (new Vector2(299f, 0f) - new Vector2(144f, 156f))
        .RotatedBy(_WeaponDirection.ToRotation() + MathHelper.PiOver4) * _WeaponScale;

    public override string Texture => EventideReunionAssets.WeaponPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ownerHitCheck = true;
        Projectile.netImportant = true;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => Phase == SunPhase.Spin && _PreviousPhase == SunPhase.Spin && SpinProgress > 0f
        || Phase == SunPhase.Dive && _PreviousPhase == SunPhase.Dive
        || Phase == SunPhase.Slash && _PreviousPhase == SunPhase.Slash && SlashProgress >= 0.05f ? null : false;
    public override bool? CanHitNPC(NPC target) => CanDamage() == false ? false : null;
    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.Write(Target.X);
        writer.Write(Target.Y);
        writer.Write(DiveDirection.X);
        writer.Write(DiveDirection.Y);
        writer.Write(SlashAim.X); writer.Write(SlashAim.Y);
        writer.Write(RetractOffset.X); writer.Write(RetractOffset.Y);
        writer.Write(RetractVector.X); writer.Write(RetractVector.Y);
    }
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        Target = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        DiveDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        SlashAim = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        RetractOffset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        RetractVector = new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }

    public override void AI()
    {
        Player ownerPlayer = Projectile.Owner;
        if (!ownerPlayer.active || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
            || ownerPlayer.HeldItem.type != ModContent.ItemType<EventideReunion>())
        {
            Projectile.Kill();
            return;
        }
        bool startingDive = _PreviousPhase != Phase && Phase == SunPhase.Dive;
        if (!IsDashing || startingDive && Projectile.IsOnOwnerClient)
            Projectile.Center = ownerPlayer.Center;
        _PreviousTip = WeaponTip;
        Projectile.timeLeft = 2;
        ownerPlayer.heldProj = Projectile.whoAmI;
        ownerPlayer.itemTime = ownerPlayer.itemAnimation = 2;
        ownerPlayer.ChangeDir(Facing);
        ownerPlayer.fullRotation = 0f;
        ownerPlayer.noFallDmg = true;
        ownerPlayer.fallStart = (int)(ownerPlayer.position.Y / 16f);
        ownerPlayer.Anomaly.EventideReunion_SunFallProtection = 30;
        Vector2 down = Vector2.UnitY * ownerPlayer.gravDir;

        if (_PreviousPhase != Phase)
        {
            _PreviousPhase = Phase;
            if (Phase == SunPhase.Plant)
            {
                SoundEngine.PlaySound(SoundID.Item20, ownerPlayer.Center);

            }
            else if (Phase == SunPhase.Spin)
                SoundEngine.PlaySound(EventideReunionSunAssets.SunSweep, ownerPlayer.Center);
            else if (Phase == SunPhase.Dive)
            {
                // 横扫每个目标只命中一次；冲刺使用独立的一次命中机会。
                Array.Clear(Projectile.localNPCImmunity);
                SoundEngine.PlaySound(EventideReunionNormalAttackAssets.Thrust, ownerPlayer.Center);
            }
            else if (Phase == SunPhase.Slash)
            {
                Array.Clear(Projectile.localNPCImmunity);
                _PreviousSlashProgress = 0f;
                SoundEngine.PlaySound(EventideReunionSunAssets.SunFollowUpSlash, ownerPlayer.Center);
            }
        }

        if (Projectile.IsOnOwnerClient && Phase is SunPhase.Plant or SunPhase.Spin or SunPhase.Charge)
        {
            // 选择符文后仍可在蓄势期间移动光标，俯冲开始时锁定落点。
            Vector2 offset = Main.MouseWorld - ownerPlayer.Center;
            if (offset.LengthSquared() > TargetRange * TargetRange)
                offset = offset.SafeNormalize(down) * TargetRange;
            Target = ownerPlayer.Center + offset;
            if (Timer % 6 == 0)
                Projectile.netUpdate = true;
        }

        switch (Phase)
        {
            case SunPhase.Plant:
                Plant();
                break;
            case SunPhase.Spin:
                Spin();
                break;
            case SunPhase.Charge:
                Charge();
                break;
            case SunPhase.Dive:
                Dive();
                break;
            case SunPhase.Retract:
                Retract();
                break;
            case SunPhase.SlashCharge:
                HoldSlashCharge();
                break;
            case SunPhase.Slash:
                Slash();
                break;
            case SunPhase.Recovery:
                Recover();
                break;
        }
        if (startingDive)
            _PreviousTip = WeaponTip;

        float armRotation = _WeaponDirection.ToRotation() - MathHelper.PiOver2 - ownerPlayer.fullRotation;
        ownerPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        if (Phase is SunPhase.Spin or SunPhase.Charge or SunPhase.Dive)
            ownerPlayer.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
        Lighting.AddLight(WeaponTip, 1f, 0.5f, 0.06f);
        EmitSparks();
        Timer++;

        void Plant()
        {
            float progress = MathHelper.Clamp(Timer / (float)PlantDuration, 0f, 1f);
            float lift = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((progress - 0.45f) / 0.55f, 0f, 1f));
            float angle = MathHelper.Lerp(-MathHelper.PiOver2 * ownerPlayer.gravDir, 0f, lift);
            _WeaponDirection = new Vector2(MathF.Cos(angle) * Facing, MathF.Sin(angle));
            _WeaponScale = MathHelper.Lerp(1f, SpinWeaponScale, lift);
            Vector2 plantedCenter = ownerPlayer.Center + down * (ownerPlayer.height * 0.5f - 180f);
            _WeaponCenter = Vector2.Lerp(plantedCenter, Grip + _WeaponDirection * (48f * _WeaponScale), lift);
            if (Projectile.IsOnOwnerClient)
            {
                if (Timer >= PlantDuration)
                    Advance(SunPhase.Spin);
            }
        }

        void Spin()
        {
            _SpinPoseProgress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Timer / (float)SpinDuration, 0f, 1f));
            Vector2 vector = EventideReunionSunGeometry.SweepVector(SpinProgress, Facing, ownerPlayer.gravDir) * SpinWeaponScale;
            _WeaponScale = vector.Length();
            _WeaponDirection = vector.SafeNormalize(Vector2.UnitX * Facing);
            _WeaponCenter = Grip + vector * 48f;
            // 水平转身只改变朝向与投影宽度，不绕屏幕法线翻滚。
            ownerPlayer.ChangeDir(vector.X >= 0f ? 1 : -1);
            UpdateSpinParticles();
            if (!Projectile.IsOnOwnerClient)
                return;
            if (Timer >= SpinDuration)
                Advance(SunPhase.Charge);
        }

        void Charge()
        {
            float lift = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Timer / 12f, 0f, 1f));
            float pull = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((Timer - 12f) / 18f, 0f, 1f));
            float angle = -0.85f * ownerPlayer.gravDir * lift;
            _WeaponDirection = new Vector2(MathF.Cos(angle) * Facing, MathF.Sin(angle));
            _WeaponScale = MathHelper.Lerp(SpinWeaponScale, 1f, lift);
            _WeaponCenter = Grip + _WeaponDirection * ((48f - 72f * pull) * _WeaponScale);
            if (!Projectile.IsOnOwnerClient)
                return;
            if (Timer < ChargeDuration)
                return;
            // 取消跃起后直接锁定光标方向，不再强制把目标投到脚下导致地面释放时立即撞地。
            Vector2 aim = Target - ownerPlayer.Center;
            DiveDirection = aim.SafeNormalize(down);
            Projectile.ai[2] = DiveDirection.X >= 0f ? 1f : -1f;
            Advance(SunPhase.Dive);
        }

        void Dive()
        {
            ownerPlayer.fullRotation = 0f;
            Vector2 motion = EventideReunionSunGeometry.DashMotion(DiveDirection, DashSpeed, down);
            _DashStart = Projectile.Center;
            _WeaponDirection = motion / DashSpeed;
            _WeaponScale = 1f;
            // 参考 Fargo 洞察者：以弹幕位置为冲刺锚点，而不是叠加已被玩家物理修改过的速度。
            // 高速位移逐小步检测玩家完整碰撞箱，碰到薄墙立即停止，不沿墙滑行。
            Projectile.Center = EventideReunionSunGeometry.SweepDash(Projectile.Center, motion,
                (position, step) => Collision.TileCollision(position - ownerPlayer.Size * 0.5f,
                    step, ownerPlayer.width, ownerPlayer.height, gravDir: (int)ownerPlayer.gravDir), out bool hitTerrain);
            ownerPlayer.Center = Projectile.Center;
            ownerPlayer.velocity = hitTerrain ? Vector2.Zero : motion;
            _WeaponCenter = ownerPlayer.MountedCenter + _WeaponDirection * 48f;
            if (Projectile.IsOnOwnerClient && Timer % 6 == 0)
                Projectile.netUpdate = true;
            if (!Projectile.IsOnOwnerClient)
                return;
            if (hitTerrain)
                BeginFinisher();
            // 光标只决定方向，不再用光标距离提前截断高速冲刺；地形或时限决定终点。
            else if (Timer >= MaxDashDuration - 1)
                BeginFinisher();
        }

        void Retract()
        {
            float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((Timer + 1f) / RetractDuration, 0f, 1f));
            Vector2 desired = GetSlashVector(0f);
            float angle = RetractVector.ToRotation() + MathHelper.WrapAngle(desired.ToRotation() - RetractVector.ToRotation()) * progress;
            _WeaponDirection = angle.ToRotationVector2();
            _WeaponScale = MathHelper.Lerp(RetractVector.Length(), desired.Length(), progress);
            _WeaponCenter = Grip + Vector2.Lerp(RetractOffset, desired * -22f, progress);
            if (Projectile.IsOnOwnerClient)
            {
                if (Timer >= RetractDuration - 1)
                    Advance(SunPhase.SlashCharge);
            }
        }

        void HoldSlashCharge()
        {
            Vector2 vector = GetSlashVector(0f);
            _WeaponDirection = vector.SafeNormalize(SlashAim);
            _WeaponScale = vector.Length();
            _WeaponCenter = Grip - vector * 22f;
            if (Projectile.IsOnOwnerClient && Timer >= SlashChargeDuration - 1)
                Advance(SunPhase.Slash);
        }

        void Slash()
        {
            _SlashPoseProgress = MathHelper.Clamp(Timer / (float)SlashDuration, 0f, 1f);
            Vector2 vector = GetSlashVector(SlashProgress);
            _WeaponDirection = vector.SafeNormalize(SlashAim);
            _WeaponScale = vector.Length();
            _WeaponCenter = Grip + vector * EventideReunionSunGeometry.SlashArmOffset(SlashProgress);
            UpdateSlashParticles();
            if (Projectile.IsOnOwnerClient)
            {
                if (Timer >= SlashDuration)
                    Advance(SunPhase.Recovery);
            }
        }

        void Recover()
        {
            ownerPlayer.fullRotation = 0f;
            float progress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Timer / (float)RecoveryDuration, 0f, 1f));
            Vector2 from = GetSlashVector(1f);
            float angle = from.ToRotation() + MathHelper.WrapAngle(SlashAim.ToRotation() - from.ToRotation()) * progress;
            _WeaponDirection = angle.ToRotationVector2();
            _WeaponScale = MathHelper.Lerp(from.Length(), 1f, progress);
            _WeaponCenter = Grip + _WeaponDirection * _WeaponScale * 48f;
            if (Projectile.IsOnOwnerClient)
            {
                if (Timer >= RecoveryDuration)
                    Projectile.Kill();
            }
        }

        void EmitSparks()
        {
            if (Main.dedServ || Timer % 2 != 0)
                return;
            Vector2 tip = WeaponTip;
            Vector2 motion = Phase == SunPhase.Plant ? -down * Main.rand.NextFloat(2f, 5f)
                : -_WeaponDirection * Main.rand.NextFloat(2f, 6f);
            ParticleHandler.SpawnParticle(new PointingParticle(tip + Main.rand.NextVector2Circular(10f, 10f), motion,
                false, 18, Main.rand.NextFloat(0.25f, 0.5f), Color.Gold));
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Phase == SunPhase.Spin)
        {
            // Timer 已在 AI 末尾递增；覆盖上帧至本帧的真实三维横扫，不能让整条残光持续造成伤害。
            float previousProgress = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((Timer - 2f) / SpinDuration, 0f, 1f));
            for (int i = 0; i <= 12; i++)
            {
                float sample = MathHelper.Lerp(previousProgress, SpinProgress, i / 12f);
                Vector2 tip = Grip + EventideReunionSunGeometry.SweepVector(sample, Facing, Projectile.Owner.gravDir)
                    * EventideReunionSunGeometry.SweepOuterRadius;
                if (RotatedRectangle.FromLineSegment(Grip, tip, 26f).Collides(targetHitbox))
                    return true;
            }
            return false;
        }
        if (Phase == SunPhase.Dive && RotatedRectangle.FromLineSegment(_DashStart, WeaponTip, 24f).Collides(targetHitbox))
            return true;
        if (Phase == SunPhase.Slash)
        {
            for (int i = 0; i <= 6; i++)
            {
                float sample = MathHelper.Lerp(Math.Max(0f, (Timer - 2f) / SlashDuration), SlashProgress, i / 6f);
                if (RotatedRectangle.FromLineSegment(Grip, GetSlashTip(sample), 26f).Collides(targetHitbox))
                    return true;
            }
        }
        return RotatedRectangle.FromLineSegment(Projectile.Owner.Center, WeaponTip, 24f).Collides(targetHitbox)
            || RotatedRectangle.FromLineSegment(_PreviousTip, WeaponTip, 24f).Collides(targetHitbox);
    }
    public override void ModifyHitNPC(NPC targetNPC, ref NPC.HitModifiers modifiers)
        => modifiers.SourceDamage *= (Phase switch
        {
            SunPhase.Spin => EventideReunionDamageConstants.SunSpinDamageMultiplier,
            SunPhase.Slash => EventideReunionDamageConstants.SunSlashDamageMultiplier,
            _ => EventideReunionDamageConstants.SunChargeDamageMultiplier
        }) * (Main.dayTime ? EventideReunionDamageConstants.SunDayDamageMultiplier : EventideReunionDamageConstants.SunNightDamageMultiplier);
    public override void OnHitNPC(NPC targetNPC, NPC.HitInfo hit, int damageDone)
    {
        targetNPC.AddBuff(BuffID.OnFire, 180);
        if (Phase == SunPhase.Dive)
            BeginFinisher(fromHit: true);
    }

    private void Advance(SunPhase next)
    {
        Phase = next;
        Timer = -1;
        Projectile.netUpdate = true;
    }

    private Vector2 GetSlashVector(float progress) => EventideReunionSunGeometry.SlashVector(progress, SlashAim, Facing, Projectile.Owner.gravDir);
    private Vector2 GetSlashTip(float progress) => Grip + GetSlashVector(progress) * (220f + EventideReunionSunGeometry.SlashArmOffset(progress));

    private void BeginFinisher(bool fromHit = false)
    {
        if (!Projectile.IsOnOwnerClient || Phase != SunPhase.Dive)
            return;
        Player ownerPlayer = Projectile.Owner;
        Vector2 explosionCenter = WeaponTip;
        RetractOffset = _WeaponCenter - Grip;
        RetractVector = _WeaponDirection * _WeaponScale;
        SlashAim = (Main.MouseWorld - Grip).SafeNormalize(DiveDirection);
        Projectile.ai[2] = SlashAim.X >= 0f ? 1f : -1f;
        ownerPlayer.velocity = Vector2.Zero;
        Projectile.Center = ownerPlayer.Center;
        Advance(SunPhase.Retract);
        // 命中回调发生在本帧 AI 之后，直接从第 0 帧开始，避免额外停顿。
        if (fromHit)
            Timer = 0;
        // 在收矛前固定爆心，各客户端通过独立弹幕播放一次爆炸。
        Projectile.NewProjectileAction<EventideReunionSunExplosion>(Projectile.GetSource_FromAI(), explosionCenter, Vector2.Zero,
            Math.Max(1, (int)(Projectile.damage * EventideReunionDamageConstants.SunExplosionDamageMultiplier)), Projectile.knockBack, Projectile.owner);
    }
    public override void OnKill(int timeLeft)
    {
        Player ownerPlayer = Projectile.Owner;
        ownerPlayer.fullRotation = 0f;
        ownerPlayer.fullRotationOrigin = Vector2.Zero;
        ownerPlayer.fallStart = (int)(ownerPlayer.position.Y / 16f);
        ownerPlayer.Anomaly.EventideReunion_SunFallProtection = 30;
    }
}
