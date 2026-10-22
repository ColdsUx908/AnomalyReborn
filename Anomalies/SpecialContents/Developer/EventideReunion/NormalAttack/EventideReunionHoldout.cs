namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionHoldout : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>()?.DisplayName;
    public override string Texture => AssetPath + "EventideReunion_NoStreamer";
    public const string AssetPath = "Anomalies/SpecialContents/Developer/EventideReunion/";

    [LoadTexture(AssetPath + "EventideReunion_NoStreamer")]
    private static Asset<Texture2D> _WeaponTexture;

    public static Color GetRandomColor() => Color.Lerp(Color.Red, Color.Orange, Main.rand.NextFloat());
    public Vector2 OrbCenter => Projectile.Center + new PolarVector2(60.81f * DrawScale, Projectile.rotation);
    public int Phase
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    public bool FaceRight
    {
        get => Projectile.ai[2] != 0f;
        set => Projectile.ai[2] = value ? 1f : 0f;
    }

    private float _AttackSpeed = 1f;
    private Vector2 _Grip;
    private Vector2 _BladeVector;
    private Vector2 _ReturnFrom;
    private float _ReturnReach = 60f;
    private float _PreviousProgress;
    private const float SweepDuration = 30f;
    private const float ThrustDuration = 12f;
    private const float ThrustTransitionDuration = 2f;
    private const float ReturnDuration = 4f;
    private const float DownwardDuration = 32f;
    private const float DownwardStart = 14f / DownwardDuration;
    private const float DownwardEnd = 22f / DownwardDuration;
    private float ArcStart => Phase == 4 ? DownwardStart : SweepStart;
    private float ArcEnd => Phase == 4 ? DownwardEnd : SweepEnd;
    private const float LowerEnd = 12f / SweepDuration;
    private const float SweepStart = 17f / SweepDuration;
    private const float SweepEnd = 25f / SweepDuration;
    private static readonly Vector2 _WeaponOrigin = new(144f, 156f);
    private static readonly Vector2 _WeaponTipPixel = new(299f, 0f);
    private float DrawScale => Projectile.scale * _BladeVector.Length();
    // 6、7 分别衔接第二刺和上挑，复用回矛的平滑转向。
    private float Progress => Projectile.ai[0] / (Phase switch { 3 => SweepDuration, 4 => DownwardDuration, 6 => ThrustTransitionDuration, 5 or 7 => ReturnDuration, _ => ThrustDuration });
    private bool IsAttacking => Phase is 1 or 2 ? Progress is >= 0.08f and <= 0.65f
        : Phase is 3 or 4 && Progress >= ArcStart && _PreviousProgress < ArcEnd;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 312;
        Projectile.scale = 1f;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        // 每次攻击开始时清空免疫，每一击对同一目标至多命中一次。
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 9999;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => IsAttacking ? null : false;

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.Write(_AttackSpeed);
        writer.Write(_ReturnFrom.X);
        writer.Write(_ReturnFrom.Y);
        writer.Write(_ReturnReach);
    }
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        _AttackSpeed = reader.ReadSingle();
        _ReturnFrom = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        _ReturnReach = reader.ReadSingle();
    }

    public override void AI()
    {
        Player ownerPlayer = Projectile.Owner;
        if (ownerPlayer.CanNotUseHoldOut(needsToHold: false) || ownerPlayer.HeldItem.type != ModContent.ItemType<EventideReunion>()
            || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSelection>()] > 0
            || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0
            || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] > 0)
        {
            Projectile.Kill();
            return;
        }

        if (Phase == 0)
        {
            if (!Projectile.IsOnOwnerClient)
                return;
            AimNextCombo();
            Phase = 1;
            Projectile.netUpdate = true;
        }

        ownerPlayer.heldProj = Projectile.whoAmI;
        ownerPlayer.itemTime = ownerPlayer.itemAnimation = 2;
        Projectile.timeLeft = 2;
        ownerPlayer.ChangeDir(FaceRight ? 1 : -1);
        _Grip = ownerPlayer.RotatedRelativePoint(ownerPlayer.MountedCenter, true) + new Vector2(0f, 10f * ownerPlayer.gravDir);

        if (Projectile.ai[0] == 0f)
        {
            Array.Clear(Projectile.localNPCImmunity);
            if (Phase is 1 or 2)
                SoundEngine.PlaySound(EventideReunionSounds.Thrust, _Grip);
        }
        float previousTime = Projectile.ai[0];
        _PreviousProgress = Progress;
        Projectile.ai[0] += _AttackSpeed;
        float currentProgress = MathHelper.Clamp(Progress, 0f, 1f);
        if (Phase < 3)
        {
            _BladeVector = Projectile.velocity;
            Projectile.rotation = _BladeVector.ToRotation();
            Projectile.Center = GetThrustCenter(currentProgress);
            UpdateThrustParticles();
        }
        else if (Phase == 3)
        {
            if (previousTime < SweepStart * SweepDuration && Projectile.ai[0] >= SweepStart * SweepDuration)
                SoundEngine.PlaySound(EventideReunionSounds.SweepUpward, _Grip);
            _BladeVector = GetSweepVector(currentProgress);
            Projectile.rotation = _BladeVector.ToRotation();
            Projectile.Center = _Grip + _BladeVector * 60f * Projectile.scale;
            UpdateSweepSparks();
        }
        else if (Phase == 4)
        {
            if (previousTime < DownwardStart * DownwardDuration && Projectile.ai[0] >= DownwardStart * DownwardDuration)
                SoundEngine.PlaySound(EventideReunionSounds.DownwardSlash, _Grip);
            _BladeVector = GetDownwardVector(currentProgress);
            Projectile.rotation = _BladeVector.ToRotation();
            Projectile.Center = GetDownwardCenter(currentProgress);
            UpdateDownwardCharge();
            UpdateSweepSparks();
        }
        else
        {
            // 两刺之间用 2 帧，其余回矛用 4 帧；插值角度和长度，转向另一侧时也不会把矛缩成零。
            float blend = MathHelper.SmoothStep(0f, 1f, currentProgress);
            float rotation = _ReturnFrom.ToRotation() + MathHelper.WrapAngle(Projectile.velocity.ToRotation() - _ReturnFrom.ToRotation()) * blend;
            _BladeVector = rotation.ToRotationVector2() * MathHelper.Lerp(_ReturnFrom.Length(), 1f, blend);
            Projectile.rotation = rotation;
            Projectile.Center = _Grip + _BladeVector * MathHelper.Lerp(_ReturnReach, 60f, blend) * Projectile.scale;
        }

        float armRotation = (Projectile.Center - _BladeVector * 80f * Projectile.scale - _Grip).ToRotation() - MathHelper.PiOver2;
        ownerPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        Lighting.AddLight(OrbCenter, 0.85f, 0.4f, 0.15f);

        if (Progress >= 1f)
        {
            if (Phase is 1 or 2)
            {
                if (!Projectile.IsOnOwnerClient)
                    return;
                // 戳刺完全收回后锁定下一击方向，保留当前姿态作为转向起点。
                _ReturnFrom = _BladeVector;
                _ReturnReach = 60f;
                AimNextCombo(updateAttackSpeed: false);
                Phase += 5;
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
                return;
            }
            if (Phase is 6 or 7)
            {
                Phase -= 4;
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
                return;
            }
            if (Phase == 4)
            {
                if (!Projectile.IsOnOwnerClient)
                    return;
                _ReturnFrom = _BladeVector;
                _ReturnReach = 125f;
                AimNextCombo();
                Phase = 5;
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
                return;
            }
            if (Phase == 5)
            {
                if (!Projectile.IsOnOwnerClient)
                    return;
                if (!ownerPlayer.channel)
                {
                    Projectile.Kill();
                    return;
                }
                // 同一 Holdout 直接进入下一轮，避免重新生成弹幕时出现空帧和姿态跳变。
                Phase = 1;
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
                return;
            }
            Phase++;
            Projectile.ai[0] = 0f;
            Projectile.netUpdate = true;
        }

        void AimNextCombo(bool updateAttackSpeed = true)
        {
            Vector2 aim = Main.MouseWorld - ownerPlayer.RotatedRelativePoint(ownerPlayer.MountedCenter, true);
            if (ownerPlayer.gravDir == -1f)
                aim.Y = Main.screenHeight - Main.mouseY + Main.screenPosition.Y - ownerPlayer.MountedCenter.Y;
            Projectile.velocity = aim.SafeNormalize(Vector2.UnitX * ownerPlayer.direction);
            FaceRight = Projectile.velocity.X >= 0f;
            if (updateAttackSpeed)
                _AttackSpeed = MathHelper.Clamp(ownerPlayer.GetTotalAttackSpeed(DamageClass.Melee), 0.1f, 4f);
        }
    }

    // 仿照 MEAC Helper.Vector2Elipse2：圆周先绕 X 倾斜，再绕 Z 对准瞄准方向，最后透视投影。
    // 角度由下向上推进，左右朝向和倒重力都改变挥动手性。
    private Vector2 GetSweepVector(float currentProgress)
    {
        // 12 帧下放、5 帧压住蓄力、8 帧爆发、5 帧收势，停在高位接第四击（随近战攻速缩放）。
        // 从 angle=0、tilt=0 开始，严格衔接第二刺完全收回时的方向、长度和持握点。
        float lower = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(currentProgress / LowerEnd, 0f, 1f));
        float angle = 1.25f * lower;
        if (currentProgress >= SweepStart)
        {
            float swing = MathHelper.Clamp((currentProgress - SweepStart) / (SweepEnd - SweepStart), 0f, 1f);
            // 停顿后快速释放，主体行程集中在前半段，末端减速保留重量感。
            float burst = 1f - MathF.Pow(1f - swing, 3f);
            angle = MathHelper.Lerp(1.25f, -1.8f, burst);
        }
        if (currentProgress > SweepEnd)
        {
            float settle = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((currentProgress - SweepEnd) / (1f - SweepEnd), 0f, 1f));
            angle = MathHelper.Lerp(-1.8f, -1.65f, settle);
        }
        float handedness = (FaceRight ? 1f : -1f) * Projectile.Owner.gravDir;
        Vector3 point = new(MathF.Cos(angle), MathF.Sin(angle) * handedness, 0f);
        point = Vector3.Transform(point, Matrix.CreateRotationX(0.72f * lower));
        point = Vector3.Transform(point, Matrix.CreateRotationZ(Projectile.velocity.ToRotation()));
        return new Vector2(point.X, point.Y) * (900f / (900f - point.Z * 250f));
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 tip = GetWeaponTip(Projectile.Center, _BladeVector);
        Vector2 tail = Projectile.Center + _BladeVector * (60.81f - 253f) * Projectile.scale;
        if (RotatedRectangle.FromLineSegment(tail, tip, 18f * DrawScale).Collides(targetHitbox))
            return true;
        if (Phase is 3 or 4)
        {
            // 检测本帧扫过的矛刃，攻速较高时也不会跳过窄目标。
            for (int i = 0; i <= 20; i++)
            {
                float sample = MathHelper.Lerp(Math.Max(_PreviousProgress, ArcStart), Math.Min(Progress, ArcEnd), i / 20f);
                Vector2 vector = GetArcVector(sample);
                Vector2 outer = GetArcTip(sample);
                Vector2 inner = Vector2.Lerp(_Grip, outer, 0.23f);
                if (RotatedRectangle.FromLineSegment(inner, outer, (Phase == 4 ? 32f : 22f) * Projectile.scale * vector.Length()).Collides(targetHitbox))
                    return true;
            }
        }
        return new Circle(OrbCenter, 40f * DrawScale).Collides(targetHitbox);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Phase == 0)
            return false;
        if (Phase is 3 or 4)
            DrawSweep();
        else if (Phase is 1 or 2)
            DrawThrust();
        DrawWeapon(Projectile.Center, _BladeVector, Color.White);
        return false;
    }

    private void DrawWeapon(Vector2 center, Vector2 vector, Color drawTint)
    {
        Main.spriteBatch.Draw(_WeaponTexture.Value, center - Main.screenPosition, null, drawTint,
            vector.ToRotation() + MathHelper.PiOver4, _WeaponOrigin, Projectile.scale * vector.Length(), SpriteEffects.None, 0f);
    }

    // 与 DrawWeapon 完全相同的原点、旋转及伪立体缩放，不再近似为固定半径。
    private Vector2 GetWeaponTip(Vector2 center, Vector2 vector) => center
        + (_WeaponTipPixel - _WeaponOrigin).RotatedBy(vector.ToRotation() + MathHelper.PiOver4) * Projectile.scale * vector.Length();

    private Vector2 GetSweepTip(float currentProgress)
    {
        Vector2 vector = GetSweepVector(currentProgress);
        return GetWeaponTip(_Grip + vector * 60f * Projectile.scale, vector);
    }

    private Vector2 GetThrustCenter(float currentProgress)
    {
        currentProgress = MathHelper.Clamp(currentProgress, 0f, 1f);
        float extension = currentProgress < 0.36f ? MathHelper.SmoothStep(0f, 1f, currentProgress / 0.36f)
            : MathHelper.SmoothStep(1f, 0f, (currentProgress - 0.36f) / 0.64f);
        return _Grip + Projectile.velocity * (60f + (Phase == 1 ? 130f : 170f) * extension) * Projectile.scale;
    }

    private Vector2 GetThrustTip(float currentProgress) => GetWeaponTip(GetThrustCenter(currentProgress), Projectile.velocity);
}
