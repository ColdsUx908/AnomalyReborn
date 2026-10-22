namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 独立读取按下沿，攻击 Holdout 占用 itemTime 时也能进入选择。
public sealed class EventideReunionSelectionPlayer : AnomalyPlayerBehavior
{
    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || Main.dedServ)
            return;
        bool pressed = Main.mouseRight && !AnomalyPlayer.EventideReunionRightWasDown;
        AnomalyPlayer.EventideReunionRightWasDown = Main.mouseRight;
        if (!pressed || Main.gameMenu || Main.playerInventory || Main.blockMouse || Player.mouseInterface
            || Player.dead || Player.noItems || Player.CCed
            || Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0
            || Player.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] > 0
            || Player.HeldItem.type != ModContent.ItemType<EventideReunion>())
            return;

        foreach (Projectile activeProjectile in Main.ActiveProjectiles)
        {
            if (activeProjectile.owner == Player.whoAmI && activeProjectile.ModProjectile is EventideReunionSelection circle)
            {
                circle.Open();
                return;
            }
        }
        Projectile.NewProjectileAction<EventideReunionSelection>(Player.GetSource_ItemUse(Player.HeldItem),
            Player.Center, (Main.MouseWorld - Player.MountedCenter).SafeNormalize(Vector2.UnitX * Player.direction),
            0, 0f, Player.whoAmI);
    }
}

public sealed class EventideReunionSelection : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => EventideReunionRunes.SunPath;
    private const float OuterRadius = 432f;
    private const float InnerRadius = 288f;
    private const float RuneRadius = (OuterRadius - InnerRadius) * 0.5f;
    private const float RuneDistance = (OuterRadius + InnerRadius) * 0.5f;
    private const float AnimationFrames = 20f;
    private const float HoverScaleBoost = 0.3f;
    private readonly float[] _RuneHover = new float[6];
    private bool _ReadyBurstPlayed;
    private int _AimSyncTimer;

    private float Progress => Projectile.ai[0] / AnimationFrames;
    private float CircleScale => MathHelper.SmoothStep(0f, 1f, Progress);
    private bool Closing => Projectile.ai[1] != 0f;
    private int SelectedRune
    {
        get => (int)Projectile.ai[2] - 1;
        set
        {
            if (SelectedRune == value)
                return;
            Projectile.ai[2] = value + 1;
            Projectile.netUpdate = true;
        }
    }
    private Vector2 Grip => Projectile.Owner.RotatedRelativePoint(Projectile.Owner.MountedCenter, true)
        + new Vector2(0f, 10f * Projectile.Owner.gravDir);
    private Vector2 AimDirection => Projectile.velocity.SafeNormalize(Vector2.UnitX * Projectile.Owner.direction);
    private Vector2 WeaponCenter => Grip + AimDirection * 60f;
    // 与 Holdout 的纹理原点、旋转及真实矛尖像素保持一致。
    private Vector2 WeaponTip => WeaponCenter
        + (new Vector2(299f, 0f) - new Vector2(144f, 156f)).RotatedBy(AimDirection.ToRotation() + MathHelper.PiOver4);

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

    public void Close()
    {
        if (Projectile.ai[1] != 0f)
            return;
        Projectile.ai[1] = 1f;
        Projectile.netUpdate = true;
    }

    public void Open()
    {
        if (!Closing)
            return;
        Projectile.ai[1] = 0f;
        SelectedRune = -1;
        _ReadyBurstPlayed = false;
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        Player ownerPlayer = Projectile.Owner;
        Projectile.Center = ownerPlayer.Center;
        bool canHold = ownerPlayer.active && !ownerPlayer.dead
            && ownerPlayer.HeldItem.type == ModContent.ItemType<EventideReunion>() && !ownerPlayer.noItems && !ownerPlayer.CCed;
        if (!canHold)
        {
            SelectedRune = -1;
            Close();
        }

        if (Projectile.IsOnOwnerClient && canHold)
        {
            Vector2 newAim = (Main.MouseWorld - Grip).SafeNormalize(AimDirection);
            Projectile.velocity = newAim;
            if (++_AimSyncTimer >= 6)
            {
                _AimSyncTimer = 0;
                Projectile.netUpdate = true;
            }
            if (!Closing)
            {
                UpdateSelection();
                if (!Main.mouseRight || Main.gameMenu || Main.playerInventory || Main.blockMouse)
                {
                    if (!Main.gameMenu && !Main.playerInventory && !Main.blockMouse && !ownerPlayer.mouseInterface
                        && SelectedRune >= 0 && Progress >= 1f)
                    {
                        ModContent.GetInstance<EventideReunion>().ReleaseSkill(ownerPlayer,
                            (EventideReunionAspect)SelectedRune, AimDirection);
                    }
                    Close();
                }
            }
        }

        Projectile.ai[0] = MathHelper.Clamp(Projectile.ai[0] + (Closing ? -1f : 1f), 0f, AnimationFrames);
        if (Closing && Projectile.ai[0] <= 0f)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;
        if (canHold && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0
            && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] == 0)
        {
            ownerPlayer.heldProj = Projectile.whoAmI;
            ownerPlayer.itemTime = ownerPlayer.itemAnimation = 2;
            ownerPlayer.ChangeDir(AimDirection.X >= 0f ? 1 : -1);
            float armRotation = (WeaponCenter - AimDirection * 80f - Grip).ToRotation() - MathHelper.PiOver2;
            ownerPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }
        if (!Closing && Progress >= 1f && !_ReadyBurstPlayed)
        {
            _ReadyBurstPlayed = true;
            EmitReadyBurst();
        }
        if (Projectile.localAI[0] > 0f)
        {
            Lighting.AddLight(WeaponTip, new Vector3(1f, 0.6f, 0.08f) * (Projectile.localAI[0] / 18f));
            Projectile.localAI[0]--;
        }
        for (int i = 0; i < _RuneHover.Length; i++)
            _RuneHover[i] = MathHelper.Clamp(_RuneHover[i] + (!Closing && SelectedRune == i ? 1f / 6f : -1f / 10f), 0f, 1f);

        void UpdateSelection()
        {
            int hovered = -1;
            if (Progress >= 1f && !ownerPlayer.mouseInterface && !Main.playerInventory && !Main.blockMouse)
            {
                for (int i = 0; i < _RuneHover.Length; i++)
                {
                    Texture2D rune = EventideReunionRunes.GetTexture((EventideReunionAspect)i);
                    float scale = GetRuneScale(rune, i);
                    Vector2 halfSize = rune.Size() * scale * 0.5f;
                    Vector2 offset = Main.MouseWorld - GetRuneCenter(i);
                    if (Math.Abs(offset.X) <= halfSize.X && Math.Abs(offset.Y) <= halfSize.Y)
                    {
                        hovered = i;
                        break;
                    }
                }
            }
            // 与 ModeIndicatorUI 一致：展开完成后，进入新图标时只播放一次。
            if (hovered >= 0 && hovered != SelectedRune)
                SoundEngine.PlaySound(SoundID.MenuTick);
            SelectedRune = hovered;
        }

        void EmitReadyBurst()
        {
            if (Main.dedServ)
                return;
            Projectile.localAI[0] = 18f;
            Vector2 tip = WeaponTip;
            ParticleHandler.SpawnParticle(new GenericSparkle(tip, Vector2.Zero, Color.White,
                Color.Gold, 1.4f, 24, rotationSpeed: 0.015f, bloomScale: 1.8f));
            ParticleHandler.SpawnParticle(new BloomParticle(tip, Vector2.Zero, new Color(255, 180, 35),
                0.1f, 0.65f, 18, lifeEndRatio: 0.1f));
            for (int i = 0; i < 24; i++)
            {
                Vector2 motion = (MathHelper.TwoPi * i / 24f).ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                ParticleHandler.SpawnParticle(new PointingParticle(tip, motion, false, Main.rand.Next(14, 25),
                    Main.rand.NextFloat(0.4f, 0.8f), i % 3 == 0 ? Color.White : Color.Gold));
            }
        }
    }

    private Vector2 GetRuneCenter(int index) => Projectile.Owner.Center
        + (-TOMathUtils.PiOver6 + index * TOMathUtils.PiOver3).ToRotationVector2() * RuneDistance * CircleScale;

    private float GetRuneScale(Texture2D rune, int index) => RuneRadius * 1.4f * CircleScale
        * (1f + HoverScaleBoost * _RuneHover[index]) / Math.Max(rune.Width, rune.Height);

    public override bool PreDraw(ref Color lightColor)
    {
        float currentProgress = Projectile.ai[0] / AnimationFrames;
        float currentScale = MathHelper.SmoothStep(0f, 1f, currentProgress);
        if (currentScale <= 0f)
            return false;

        Vector2 center = Projectile.Owner.Center;
        Color goldOrange = new(255, 155, 35);
        SpriteBatch batch = Main.spriteBatch;
        batch.End();
        AnomalyDrawUtils.DrawGlowingCircle(batch, center, OuterRadius * currentScale, goldOrange, currentProgress, 3f, 12f);
        AnomalyDrawUtils.DrawGlowingCircle(batch, center, InnerRadius * currentScale, goldOrange, currentProgress, 3f, 12f);
        Span<(Vector2 Start, Vector2 End)> edges = stackalloc (Vector2, Vector2)[6];
        for (int i = 0; i < 6; i++)
        {
            // 2、4、6、8、10、12 点钟；两个交叠的正三角形组成正六芒星。
            float angle = -TOMathUtils.PiOver6 + i * TOMathUtils.PiOver3;
            Vector2 direction = angle.ToRotationVector2();
            Vector2 runeCenter = center + direction * RuneDistance * currentScale;
            AnomalyDrawUtils.DrawGlowingCircle(batch, runeCenter, RuneRadius * currentScale, goldOrange, currentProgress, 3f, 12f);
            edges[i] = (center + direction * InnerRadius * currentScale,
                center + (angle + MathHelper.TwoPi / 3f).ToRotationVector2() * InnerRadius * currentScale);
        }
        AnomalyDrawUtils.DrawGlowingLines(batch, edges, goldOrange, currentProgress, 3f, 12f);
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        for (int i = 0; i < 6; i++)
        {
            Texture2D rune = EventideReunionRunes.GetTexture((EventideReunionAspect)i);
            Vector2 position = GetRuneCenter(i);
            float runeScale = GetRuneScale(rune, i);
            int cooldown = i == 0 ? Projectile.Owner.Anomaly.EventideReunionSunCooldown
                : i == 3 ? Projectile.Owner.Anomaly.EventideReunionDeathCooldown : 0;
            Color tint = cooldown > 0 ? new Color(140, 110, 85) : Color.White;
            batch.Draw(rune, position - Main.screenPosition, null, tint * currentProgress,
                0f, rune.Size() * 0.5f, runeScale, SpriteEffects.None, 0f);
            if (cooldown > 0)
                Utils.DrawBorderString(batch, Math.Ceiling(cooldown / 60f).ToString(),
                    position - Main.screenPosition + new Vector2(0f, RuneRadius * 0.65f * currentScale),
                    Color.Gold * currentProgress, 0.6f, 0.5f, 0.5f);
        }
        Player ownerPlayer = Projectile.Owner;
        if (ownerPlayer.active && !ownerPlayer.dead && ownerPlayer.HeldItem.type == ModContent.ItemType<EventideReunion>()
            && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0
            && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] == 0)
        {
            Texture2D weapon = ModContent.Request<Texture2D>(EventideReunionHoldout.AssetPath + "EventideReunion_NoStreamer").Value;
            batch.Draw(weapon, WeaponCenter - Main.screenPosition, null, Color.White,
                AimDirection.ToRotation() + MathHelper.PiOver4, new Vector2(144f, 156f), 1f, SpriteEffects.None, 0f);
        }
        return false;
    }
}
