namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

public sealed class DemonShadow : AnomalyModProjectile
{
    private const int ShotInterval = 12;
    private const float ShotSpeed = 14f;

    private Player _drawPlayer;

    // ai[0]：目标玩家索引（1-based，0 表示未指定目标）。ai[1]：射击计时器。
    // ai[2]：所属骷髅王索引（1-based，0 表示独立测试实例）。
    public int TargetIndex
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 20;
        Projectile.height = 42;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
    }

    public override void AI()
    {
        if (Projectile.ai[2] > 0f)
        {
            NPC master = NPC.GetNPCFromIndex((int)Projectile.ai[2] - 1);
            if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1
                || !SkeletronLegendHead.GetInstance(master).StarAvatarActive)
            {
                // 由服务端确认所属符卡结束，避免生成包早于 NPC 状态包到达时客户端误删。
                if (TOSharedData.NotClient)
                    Projectile.Kill();
                return;
            }

            int targetIndex = master.target + 1;
            if (TargetIndex != targetIndex)
            {
                TargetIndex = targetIndex;
                Projectile.netUpdate = true;
            }
            Projectile.timeLeft = 2;
            Vector2 awayFromTarget = master.GetVelocityTowards(Main.player[master.target], -180f);
            if (awayFromTarget == Vector2.Zero)
                awayFromTarget = Vector2.UnitX * 180f;
            Vector2 destination = master.Center + awayFromTarget - Vector2.UnitY * 160f;
            Vector2 velocity = (destination - Projectile.Center) * 0.08f;
            if (velocity.Length() > 24f)
                velocity = Projectile.GetVelocityTowards(destination, 24f);
            Projectile.velocity = velocity;
        }

        bool hasTarget = Player.TryGetPlayerFromIndex(TargetIndex - 1, out Player target) && target.active && !target.dead;
        Vector2 aimDirection = hasTarget
            ? Projectile.GetVelocityTowards(target)
            : Vector2.UnitX * Projectile.direction;
        if (aimDirection == Vector2.Zero)
            aimDirection = Vector2.UnitX * Projectile.direction;

        if (aimDirection.X != 0f)
            Projectile.direction = aimDirection.X > 0f ? 1 : -1;

        if (hasTarget)
        {
            Projectile.ai[1]++;
            if (Projectile.ai[1] >= ShotInterval)
            {
                Projectile.ai[1] = 0f;
                Vector2 muzzlePosition = Projectile.Center + aimDirection * (40f * Projectile.scale);
                if (TOSharedData.NotClient)
                {
                    Projectile.NewProjectileAction<FallingStar>(SourceAI, muzzlePosition,
                        aimDirection * ShotSpeed, SkeletronLegendHead.FallingStarDamage, Projectile.knockBack, Projectile.owner, action: p =>
                        {
                            p.scale = 0.2f;
                            p.GetModProjectile<FallingStar>().TrailType = 1;
                            p.GetModProjectile<FallingStar>().AccelerationStartTime = 420;
                            p.netUpdate = true;
                        });
                }

                if (!Main.dedServ)
                    SoundEngine.PlaySound(SoundID.Item9, muzzlePosition);
            }
        }
        else
            Projectile.ai[1] = 0f;

        if (Main.dedServ)
            return;

        if (_drawPlayer is null)
        {
            // new Player() 保留原版默认性别、发型、肤色及衣物颜色，不复制召唤者外观。
            _drawPlayer = new Player();
            _drawPlayer.armor[0].SetDefaults(ItemID.ShadowHelmet);
            _drawPlayer.armor[1].SetDefaults(ItemID.ShadowScalemail);
            _drawPlayer.armor[2].SetDefaults(ItemID.ShadowGreaves);
            _drawPlayer.armor[3].SetDefaults(ItemID.DemonWings);
            _drawPlayer.inventory[0].SetDefaults(ItemID.StarCannon);
            _drawPlayer.lastVisualizedSelectedItem = _drawPlayer.inventory[0];
        }

        // 参照原版 PlayerSegment / UICharacter，只更新绘制需要的状态。
        using Main.CurrentPlayerOverride currentPlayer = new(_drawPlayer);
        _drawPlayer.ResetEffects();
        _drawPlayer.ResetVisibleAccessories();
        _drawPlayer.UpdateMiscCounter();
        _drawPlayer.UpdateDyes();
        _drawPlayer.Center = Projectile.Center;
        _drawPlayer.velocity = Projectile.velocity;
        // 只给绘制用 Player 一个非零竖直速度，使悬停时也保持空中姿态。
        if (_drawPlayer.velocity.Y == 0f)
            _drawPlayer.velocity.Y = -0.01f;
        _drawPlayer.direction = Projectile.direction;
        _drawPlayer.controlJump = true;
        _drawPlayer.wingsLogic = _drawPlayer.armor[3].wingSlot;
        _drawPlayer.wingTime = 1f;
        _drawPlayer.itemAnimationMax = ShotInterval;
        _drawPlayer.itemAnimation = ShotInterval - (int)Projectile.ai[1];
        _drawPlayer.itemRotation = (aimDirection * Projectile.direction).ToRotation();
        // 原版 Shoot 使用姿势：武器锚点位于人物中心左移 direction * 2、上移半个物品高度。
        // 不调用完整 ItemCheck，避免虚拟玩家消耗弹药或额外生成原版星星。
        _drawPlayer.itemLocation = _drawPlayer.MountedCenter + new Vector2(-2f * Projectile.direction, -_drawPlayer.HeldItem.height * 0.5f);
        _drawPlayer.PlayerFrame();
        _drawPlayer.WingFrame(wingFlap: true);
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 70; i++)
        {
            Color color = Color.Lerp(new Color(55, 25, 85), new Color(145, 90, 200), Main.rand.NextFloat());
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 24f),
                Main.rand.NextPolarVector2(2f, 9f), Main.rand.Next(25, 45), Main.rand.NextFloat(0.3f, 0.8f), color, lifeEndRatio: 0.2f, glowCenter: false));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (_drawPlayer is null)
            return false;

        using Main.CurrentPlayerOverride currentPlayer = new(_drawPlayer);
        Vector2 drawOffset = Projectile.Center - _drawPlayer.Center;
        _drawPlayer.Center = Projectile.Center;
        _drawPlayer.itemLocation += drawOffset;
        _drawPlayer.opacityForAnimation = Projectile.Opacity;

        // DrawPlayer 接收世界坐标，内部会减去 Main.screenPosition。
        // 原版以脚底为缩放中心；补偿位置，使缩放后的人物中心仍与弹幕中心重合。
        Vector2 position = _drawPlayer.position + Vector2.UnitY * (_drawPlayer.height * (Projectile.scale - 1f) * 0.5f);
        Main.PlayerRenderer.DrawPlayer(Main.Camera, _drawPlayer, position, Projectile.rotation,
            _drawPlayer.Size * 0.5f, scale: Projectile.scale);
        return false;
    }
}
