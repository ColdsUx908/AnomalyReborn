namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionSamsaraSkill : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionAssets.WeaponPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
    internal const int ChargeDuration = 24;
    internal const int SpinDuration = 36;
    internal const int TurnDuration = 6;
    internal const int PullbackDuration = 5;
    internal const int ThrustDuration = 12;
    internal const int RecoveryDuration = 30;
    internal const float WeaponTipDistance = 219.91f;
    internal const float ThrustExtension = EventideReunionSamsaraVisualGeometry.ThrustExtension;
    private enum SamsaraPhase { Charge, Spin, Turn, Pullback, Thrust, Recovery }
    private SamsaraPhase Phase => (SamsaraPhase)(int)Projectile.ai[1];
    internal bool IsCharging => Phase == SamsaraPhase.Charge;
    internal float BodyTurnAngle => Phase == SamsaraPhase.Spin
        ? MathHelper.TwoPi * MathHelper.SmoothStep(0f, 1f, Progress)
        : Phase == SamsaraPhase.Turn ? MathHelper.Pi * Progress : 0f;
    private int Timer { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
    private int Facing => Projectile.ai[2] < 0f ? -1 : 1;
    private Vector2 Aim => Projectile.velocity.SafeNormalize(Vector2.UnitX * Facing);
    private bool _StrikeSpawned;
    private SamsaraPhase _LastSweepPhase = (SamsaraPhase)(-1);
    private Vector2 _PullbackFromAim = Vector2.UnitX;
    private SamsaraPhase _LastSoundPhase = (SamsaraPhase)(-1);
    private Vector2 Grip => Projectile.Owner.MountedCenter;
    private float Progress => Timer / (float)(Phase switch
    {
        SamsaraPhase.Charge => ChargeDuration,
        SamsaraPhase.Spin => SpinDuration,
        SamsaraPhase.Turn => TurnDuration,
        SamsaraPhase.Pullback => PullbackDuration,
        SamsaraPhase.Thrust => ThrustDuration,
        _ => RecoveryDuration
    });
    private float PlanarWeaponAngle => Phase switch
    {
        SamsaraPhase.Charge => Facing > 0 ? MathHelper.Pi + 0.15f : -0.15f,
        SamsaraPhase.Spin => (Facing > 0 ? MathHelper.Pi + 0.15f : -0.15f) + MathHelper.TwoPi * Facing * MathHelper.SmoothStep(0f, 1f, Progress),
        SamsaraPhase.Turn => Aim.ToRotation(),
        SamsaraPhase.Pullback => _PullbackFromAim.ToRotation()
            + MathHelper.WrapAngle(Aim.ToRotation() - _PullbackFromAim.ToRotation()) * MathHelper.SmoothStep(0f, 1f, Progress),
        SamsaraPhase.Thrust => Aim.ToRotation(),
        _ => Aim.ToRotation() + MathHelper.WrapAngle(-MathHelper.PiOver2 * Projectile.Owner.gravDir - Aim.ToRotation())
            * MathHelper.SmoothStep(0f, 1f, Math.Min(1f, Progress * 2f))
    };
    private Vector2 WeaponVector => Phase == SamsaraPhase.Spin
        ? EventideReunionSamsaraVisualGeometry.SpinVector(Progress, Facing)
        : Phase == SamsaraPhase.Turn
        ? EventideReunionSamsaraVisualGeometry.TurnVector(Progress,
            Facing > 0 ? MathHelper.Pi + 0.15f : -0.15f, Aim, Projectile.Owner.gravDir)
        : PlanarWeaponAngle.ToRotationVector2();
    private float WeaponAngle => WeaponVector.ToRotation();
    private float WeaponScale => WeaponVector.Length();
    private float Reach => Phase switch
    {
        SamsaraPhase.Pullback => -EventideReunionSamsaraVisualGeometry.PullbackDistance * MathHelper.SmoothStep(0f, 1f, Progress),
        SamsaraPhase.Thrust => EventideReunionSamsaraVisualGeometry.ThrustReach(Progress),
        _ => 0f
    };
    private Vector2 WeaponCenter => Grip + WeaponVector * Reach;
    private Vector2 WeaponTip => WeaponCenter + WeaponVector * WeaponTipDistance;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.netImportant = true;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.Write(_PullbackFromAim.X);
        writer.Write(_PullbackFromAim.Y);
    }
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
        => _PullbackFromAim = new Vector2(reader.ReadSingle(), reader.ReadSingle());

    public override void AI()
    {
        Player player = Projectile.Owner;
        if (!player.active || player.dead || player.noItems || player.CCed
            || player.HeldItem.type != ModContent.ItemType<EventideReunion>())
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = player.Center;
        Projectile.timeLeft = 2;
        if (Projectile.ai[2] == 0f)
            Projectile.ai[2] = player.direction;
        player.heldProj = Projectile.whoAmI;
        player.itemTime = player.itemAnimation = 2;
        player.ChangeDir(Phase == SamsaraPhase.Spin && MathF.Cos(BodyTurnAngle) < 0f ? -Facing
            : Phase >= SamsaraPhase.Turn ? Aim.X >= 0f ? 1 : -1 : Facing);
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, WeaponAngle - MathHelper.PiOver2);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, WeaponAngle - MathHelper.PiOver2 - 0.3f * player.direction);
        if (!Main.dedServ)
        {
            if (_LastSoundPhase != Phase)
            {
                _LastSoundPhase = Phase;
                SoundEngine.PlaySound(Phase switch
                {
                    SamsaraPhase.Thrust => EventideReunionNormalAttackAssets.Thrust,
                    SamsaraPhase.Spin => SoundID.Item8 with { Volume = 0.4f, Pitch = -0.5f },
                    _ => SoundID.Item35 with { Volume = 0.35f, Pitch = -0.7f }
                }, Grip);
            }
            UpdateCausalTrail();
            EmitParticles();
            Lighting.AddLight(WeaponTip, new Vector3(0.15f, 0.35f, 0.5f));
        }
        if (Phase is SamsaraPhase.Spin or SamsaraPhase.Turn && _LastSweepPhase != Phase
            && Main.netMode != NetmodeID.MultiplayerClient)
        {
            _LastSweepPhase = Phase;
            Projectile.NewProjectileAction<EventideReunionSamsaraSweep>(Projectile.GetSource_FromThis(), Grip, Aim,
                (int)(Projectile.damage * EventideReunionDamageConstants.SamsaraSweepDamageMultiplier), Projectile.knockBack, Main.myPlayer, p =>
                {
                    p.ai[0] = player.whoAmI;
                    p.ai[1] = Phase == SamsaraPhase.Turn ? 1f : 0f;
                    p.ai[2] = Facing;
                    p.timeLeft = Phase == SamsaraPhase.Turn ? TurnDuration : SpinDuration;
                    p.CritChance = Projectile.CritChance;
                });
        }
        if (Phase == SamsaraPhase.Thrust && !_StrikeSpawned && Main.netMode != NetmodeID.MultiplayerClient)
        {
            _StrikeSpawned = true;
            Projectile.NewProjectileAction<EventideReunionSamsaraStrike>(Projectile.GetSource_FromThis(), Grip, Aim,
                (int)(Projectile.damage * EventideReunionDamageConstants.SamsaraThrustDamageMultiplier), Projectile.knockBack, Main.myPlayer, p =>
                {
                    p.ai[0] = player.whoAmI;
                    p.CritChance = Projectile.CritChance;
                });
        }
        int duration = Phase switch
        {
            SamsaraPhase.Charge => ChargeDuration,
            SamsaraPhase.Spin => SpinDuration,
            SamsaraPhase.Turn => TurnDuration,
            SamsaraPhase.Pullback => PullbackDuration,
            SamsaraPhase.Thrust => ThrustDuration,
            _ => RecoveryDuration
        };
        // 转身首帧由持有者读取光标并同步；其他端在旋身结束处等待这一状态。
        if (Phase is SamsaraPhase.Spin or SamsaraPhase.Turn && !Projectile.IsOnOwnerClient)
        {
            Timer = Math.Min(Timer + 1, duration - 1);
            return;
        }
        Timer++;
        if (!Main.dedServ)
            UpdateCausalTrail();
        if (Timer < duration)
            return;
        if (Phase == SamsaraPhase.Recovery)
        {
            Projectile.Kill();
            return;
        }
        if (Phase == SamsaraPhase.Turn)
            _PullbackFromAim = Aim;
        if (Phase is SamsaraPhase.Spin or SamsaraPhase.Turn)
            Projectile.velocity = (Main.MouseWorld - Grip).SafeNormalize(Aim);
        Projectile.ai[1]++;
        Timer = 0;
        if (!Main.dedServ)
            UpdateCausalTrail();
        if (Projectile.IsOnOwnerClient)
            Projectile.netUpdate = true;
    }
}
