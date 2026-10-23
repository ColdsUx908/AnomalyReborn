namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed partial class EventideReunionDeathSkill : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public const int Cooldown = 600;
    private const int ChargeDuration = 30;
    private const int DashDuration = 12;
    internal const int SlashDuration = 14;
    private const int RecoveryDuration = EventideReunionDeathRecall.CompletionAge - SlashDuration + 8;
    private const int ActionRecoveryDuration = 4;
    private const float DashSpeed = EventideReunionDeathGeometry.DashSpeed;
    private const float WeaponTipDistance = 219.91f;
    private bool _WaveReleased;
    private bool _DashStrikeSpawned;
    private int _HitTimer = -1;
    private Vector2 _SafePosition;
    private bool _HasSafePosition;
    private Vector2 _TargetCenter;
    private Rectangle _TargetBounds;
    private int _TargetType;
    private Vector2 _SlashAim;
    private float _SwingSign = 1f;
    private enum DeathPhase { Charge, Dash, Slash, Recovery }
    private DeathPhase Phase => (DeathPhase)(int)Projectile.ai[1];
    private int Timer { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
    // ai[2] = 首个接触目标的索引 + 1；0 表示尚未接触。
    private int TargetIndex { get => (int)Projectile.ai[2] - 1; set => Projectile.ai[2] = value + 1; }
    public bool IsDashing => Phase == DeathPhase.Dash;
    internal bool IsSlashing => Phase == DeathPhase.Slash;
    internal bool BlocksActions => Phase != DeathPhase.Recovery || Timer < ActionRecoveryDuration;

    internal static bool IsBlockingActions(Player player)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] == 0)
            return false;
        foreach (Projectile active in Main.ActiveProjectiles)
            if (active.owner == player.whoAmI && active.ModProjectile is EventideReunionDeathSkill { BlocksActions: true })
                return true;
        return false;
    }
    internal float EdgeDarknessOpacity => IsDashing
        ? 0.55f * MathHelper.SmoothStep(0f, 1f, Math.Min(1f, Timer / 6f))
        : IsSlashing ? 0.55f * MathHelper.SmoothStep(1f, 0f, Math.Min(1f, Timer / (float)SlashDuration)) : 0f;
    internal Vector2 DashDirection => Aim;
    private Vector2 Aim => Projectile.velocity.SafeNormalize(Vector2.UnitX);
    private Vector2 SlashAim => _SlashAim.SafeNormalize(-Aim);
    private Vector2 WeaponVector => Phase == DeathPhase.Slash
        ? EventideReunionDeathGeometry.SlashVector(Timer / (float)SlashDuration, SlashAim, _SwingSign) / WeaponTipDistance
        : (Phase == DeathPhase.Charge ? Aim.ToRotation() + MathHelper.Pi * 0.85f
            : Phase == DeathPhase.Recovery ? SlashAim.ToRotation() + 1.85f * _SwingSign : Aim.ToRotation()).ToRotationVector2();
    private float WeaponAngle => WeaponVector.ToRotation();
    public override string Texture => EventideReunionAssets.WeaponPath;
    public override LocalizedText DisplayName => ModContent.GetModItem<EventideReunion>().DisplayName;
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
        writer.Write(_TargetCenter.X); writer.Write(_TargetCenter.Y);
        writer.Write(_TargetType);
        writer.Write(_SlashAim.X); writer.Write(_SlashAim.Y);
        writer.Write(_SwingSign);
    }
    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        _TargetCenter = new(reader.ReadSingle(), reader.ReadSingle());
        _TargetType = reader.ReadInt32();
        _SlashAim = new(reader.ReadSingle(), reader.ReadSingle());
        _SwingSign = reader.ReadSingle();
    }
    public override void AI()
    {
        Player player = Projectile.Owner;
        if (!player.active || player.dead || BlocksActions
            && (player.noItems || player.CCed || player.HeldItem.type != ModContent.ItemType<EventideReunion>()))
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = player.Center;
        Projectile.timeLeft = 2;
        if (Phase == DeathPhase.Charge && Projectile.IsOnOwnerClient)
        {
            Projectile.velocity = (Main.MouseWorld - player.Center).SafeNormalize(Aim);
            if (Timer % 6 == 0)
                Projectile.netUpdate = true;
        }
        if (BlocksActions)
        {
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;
            Vector2 facing = Phase >= DeathPhase.Slash ? SlashAim : Aim;
            player.ChangeDir(facing.X >= 0f ? 1 : -1);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, WeaponAngle - MathHelper.PiOver2);
        }
        else if (player.heldProj == Projectile.whoAmI)
            player.heldProj = -1;
        switch (Phase)
        {
            case DeathPhase.Charge:
                if (Projectile.IsOnOwnerClient)
                    player.velocity.X *= 0.8f;
                break;
            case DeathPhase.Dash:
                Protect();
                if (!_DashStrikeSpawned && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    _DashStrikeSpawned = true;
                    SpawnStrike(false);
                }
                if (Projectile.IsOnOwnerClient)
                    Dash();
                if (IsDashing)
                {
                    EmitParticles();
                    Timer++;
                }
                return;
            case DeathPhase.Slash:
                if (!_WaveReleased)
                {
                    _WaveReleased = true;
                    SoundEngine.PlaySound(SoundID.Item71, player.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SpawnStrike(true);
                        Projectile.NewProjectileAction<EventideReunionDeathWave>(Projectile.GetSource_FromThis(),
                            player.Center, SlashAim * 18f, (int)(Projectile.damage * EventideReunionDamageConstants.DeathWaveDamageMultiplier), Projectile.knockBack,
                            Main.myPlayer, p => { p.ai[0] = player.whoAmI; p.ai[1] = _SwingSign; });
                    }
                }
                break;
        }
        EmitParticles();
        // 持有者决定蓄力和穿身何时结束，其他端等待同步，不能按固定帧数提前回斩。
        if (Phase == DeathPhase.Charge && !Projectile.IsOnOwnerClient)
        {
            Timer = Math.Min(Timer + 1, ChargeDuration - 1);
            return;
        }
        int duration = Phase switch { DeathPhase.Charge => ChargeDuration, DeathPhase.Slash => SlashDuration, _ => RecoveryDuration };
        if (++Timer < duration)
            return;
        if (Phase == DeathPhase.Recovery)
            Projectile.Kill();
        else
            Advance();

        void Protect()
        {
            // 不缩短玩家已有的更长无敌时间。
            player.immune = true;
            player.immuneTime = Math.Max(player.immuneTime, 10);
            for (int i = 0; i < player.hurtCooldowns.Length; i++)
                player.hurtCooldowns[i] = Math.Max(player.hurtCooldowns[i], 10);
            player.Anomaly.EventideReunion_DeathProtection = 10;
        }

        void SpawnStrike(bool slash)
        {
            Projectile.NewProjectileAction<EventideReunionDeathStrike>(Projectile.GetSource_FromThis(), player.Center,
                Vector2.Zero, (int)(Projectile.damage * (slash ? EventideReunionDamageConstants.DeathSlashDamageMultiplier : EventideReunionDamageConstants.DeathDashDamageMultiplier)),
                Projectile.knockBack, Main.myPlayer, p =>
                {
                    p.ai[0] = slash ? 1f : 0f;
                    p.ai[1] = player.whoAmI;
                    p.CritChance = Projectile.CritChance;
                    p.GetModProjectile<EventideReunionDeathStrike>().Source.Set(Projectile);
                });
        }

        void Dash()
        {
            if (Timer == 0)
            {
                _SafePosition = player.position;
                _HasSafePosition = !OverlapsEnemy(player.Hitbox);
            }
            Vector2 start = player.position;
            Vector2 motion = Aim * DashSpeed;
            bool hitTerrain = false;
            player.position = EventideReunionDeathGeometry.SweepDash(start, motion,
                (position, step) =>
                {
                    Vector2 actual = Collision.TileCollision(position, step, player.width, player.height, false, false, (int)player.gravDir);
                    hitTerrain |= Vector2.DistanceSquared(actual, step) > 0.001f;
                    return actual;
                },
                InspectPosition);
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(player.position.Y / 16f);
            if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[TargetIndex];
                if (target.active && target.type == _TargetType)
                {
                    _TargetCenter = target.Center;
                    _TargetBounds = target.Hitbox;
                }
            }
            bool overlapping = OverlapsEnemy(player.Hitbox);
            bool needsClearance = TargetIndex >= 0 && !EventideReunionDeathGeometry.HasPassedTarget(player.Hitbox, _TargetBounds, Aim);
            bool finish = EventideReunionDeathGeometry.ShouldFinishDash(Timer + 1, _HitTimer, overlapping || needsClearance, DashDuration);
            bool hitTimeout = _HitTimer >= 0 && Timer - _HitTimer >= EventideReunionDeathGeometry.HitDashDuration;
            if (hitTerrain || hitTimeout)
            {
                // 目标与地形夹住玩家时退回最后安全位置，不在敌人体内结束。
                if (overlapping && _HasSafePosition)
                    player.position = _SafePosition;
                finish = hitTerrain || !OverlapsEnemy(player.Hitbox);
            }
            if (finish)
            {
                if (TargetIndex >= 0 && TargetIndex < Main.maxNPCs)
                {
                    NPC target = Main.npc[TargetIndex];
                    if (target.active && target.type == _TargetType)
                        _TargetCenter = target.Center;
                }
                // 瞄准目标最近的碰撞箱部位，巨大或狭长目标也不会因中心过远而回斩落空。
                Vector2 slashTarget = EventideReunionDeathGeometry.SlashTarget(_TargetBounds, player.Center);
                _SlashAim = TargetIndex >= 0 ? (slashTarget - player.Center).SafeNormalize(-Aim) : -Aim;
                _SwingSign = (_SlashAim.X >= 0f ? 1f : -1f) * player.gravDir;
                Protect();
                Advance();
            }
            if (Timer % 3 == 0 || finish)
                NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
        }

        void InspectPosition(Vector2 previous, Vector2 position)
        {
            Rectangle box = new((int)position.X, (int)position.Y, player.width, player.height);
            bool overlapping = false;
            NPC firstTarget = null;
            float nearestContact = float.MaxValue;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!IsTarget(target))
                    continue;
                bool bodyContact = box.Intersects(target.Hitbox);
                overlapping |= bodyContact;
                Vector2 grip = position + player.Size * 0.5f;
                bool spearContact = TargetIndex < 0 && RotatedRectangle.FromLineSegment(
                    grip, grip + Aim * WeaponTipDistance, 28f).Collides(target.Hitbox);
                if (TargetIndex < 0 && (bodyContact || spearContact))
                {
                    float contactDistance = bodyContact ? 0f : SpearContactDistance(grip, target.Hitbox);
                    if (contactDistance < nearestContact)
                    {
                        firstTarget = target;
                        nearestContact = contactDistance;
                    }
                }
            }
            if (firstTarget is not null)
            {
                TargetIndex = firstTarget.whoAmI;
                _TargetType = firstTarget.type;
                _HitTimer = Timer;
                _TargetCenter = firstTarget.Center;
                _TargetBounds = firstTarget.Hitbox;
                Projectile.netUpdate = true;
            }
            if (!overlapping && !Collision.SolidCollision(position, player.width, player.height))
            {
                _SafePosition = position;
                _HasSafePosition = true;
            }
        }
    }

    // 对已确认命中的目标二分最短相交矛身，保留按首次接触距离选敌的顺序。
    private float SpearContactDistance(Vector2 grip, Rectangle target)
    {
        float start = 0f;
        float end = WeaponTipDistance;
        while (end - start > 0.01f)
        {
            float middle = (start + end) * 0.5f;
            if (RotatedRectangle.FromLineSegment(grip, grip + Aim * middle, 28f).Collides(target))
                end = middle;
            else
                start = middle;
        }
        return end;
    }
    private static bool IsTarget(NPC npc) => npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0 && npc.lifeMax > 5;
    internal bool SlashCollision(Rectangle target)
    {
        float head = MathHelper.Clamp(Timer / (float)SlashDuration, 0f, 1f);
        float tail = Math.Max(0f, (Timer - 2f) / SlashDuration);
        for (int i = 0; i <= 16; i++)
        {
            Vector2 vector = EventideReunionDeathGeometry.SlashVector(MathHelper.Lerp(tail, head, i / 16f), SlashAim, _SwingSign);
            if (RotatedRectangle.FromLineSegment(Projectile.Owner.MountedCenter,
                Projectile.Owner.MountedCenter + vector, 24f).Collides(target))
                return true;
        }
        return false;
    }
    private static bool OverlapsEnemy(Rectangle box)
    {
        foreach (NPC target in Main.ActiveNPCs)
            if (IsTarget(target) && box.Intersects(target.Hitbox))
                return true;
        return false;
    }
    private void Advance()
    {
        Timer = 0;
        Projectile.ai[1]++;
        Projectile.netUpdate = true;
    }
}
