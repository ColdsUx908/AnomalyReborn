#define CELESS_DEV

using CalamityAnomalies.GameContents;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent.ItemDropRules;
using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.DeveloperContents;

#region 物品
public sealed class ColdheartIcicle : CAModItem
{
    #region Static
    public const int SpriteWidth = 24;
    public const string TexturePath = "CalamityMod/Items/Weapons/Typeless/ColdheartIcicle";

    public static bool IsLegendOwner(Player player) => player.name == "ColdsUx";

    public static bool IsDream(Player player) => player.name == "Celessalia";
    #endregion Static

    #region 传奇
    public int Phase = 1;
    public int SubPhase = 1;

    public void LegendaryUpdate()
    {
        if (DownedBossSystem_Bridge.downedPrimordialWyrm)
        {
            Phase = 7;
            SubPhase = 1;
        }
        else if (DownedBossSystem_Bridge.downedSignus)
        {
            Phase = 6;
            if (NPC.Focus)
                SubPhase = 6;
            else if (DownedBossSystem_Bridge.downedExoMechs || DownedBossSystem_Bridge.downedCalamitas)
                SubPhase = 5;
            else if (DownedBossSystem_Bridge.downedYharon)
                SubPhase = 4;
            else if (DownedBossSystem_Bridge.downedDoG)
                SubPhase = 3;
            else if (DownedBossSystem_Bridge.downedPolterghast)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (NPC.downedAncientCultist)
        {
            Phase = 5;
            if (DownedBossSystem_Bridge.downedProvidence)
                SubPhase = 3;
            else if (NPC.downedMoonlord)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (DownedBossSystem_Bridge.downedLeviathan)
        {
            Phase = 4;
            if (NPC.downedGolemBoss)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (DownedBossSystem_Bridge.downedCryogen)
        {
            Phase = 3;
            if (NPC.downedPlantBoss)
                SubPhase = 3;
            else if (TONPCUtils.DownedMechBossAll)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (NPC.downedDeerclops)
        {
            Phase = 2;
            if (Main.hardMode)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else
        {
            Phase = 1;
            if (NPC.downedBoss3)
                SubPhase = 5;
            else if (NPC.DownedEvilBossT2)
                SubPhase = 4;
            else if (NPC.downedBoss2)
                SubPhase = 3;
            else if (NPC.downedBoss1)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
    }

    public void LegendaryUpdate(Player player)
    {
        LegendaryUpdate();
        CAPlayer anomalyPlayer = player.Anomaly;
        anomalyPlayer.Coldheart_Phase = Phase;
        anomalyPlayer.Coldheart_SubPhase = SubPhase;
    }
    #endregion 传奇

    public override string Texture => TexturePath;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = SpriteWidth;
        Item.height = SpriteWidth;
        Item.damage = 20;
        Item.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Item.useTime = 27;
        Item.useAnimation = 27;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item1;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.shoot = ModContent.ProjectileType<ColdheartIcicleProj>();
        Item.shootSpeed = 1.25f;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.ArmorPenetration = 350258;
        CalamityItem.devItem = true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        LegendaryUpdate();
    }

    public override void UpdateInventory(Player player)
    {
        LegendaryUpdate();
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
#if CELESS_DEV
        if (IsDream(player))
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ColdheartIcicleDream>()] < 1)
            {
                Projectile.NewProjectileAction<ColdheartIcicleDream>(Item.GetSource_FromThis(), player.Center, Vector2.Zero, 0, 0, player.whoAmI, p =>
                {
                    p.Center = player.Center;
                    p.SetVelocityandRotation(Vector2.Zero);
                    ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
                    modP.Target = player;
                    modP.Behavior = player.altFunctionUse == 2 ? ColdheartIcicleDream.BehaviorType.SetOut : ColdheartIcicleDream.BehaviorType.Dream;
                });
            }
            return false;
        }
        else
            return true;
#else
        return true;
#endif
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= (Phase, SubPhase) switch
        {
            (1, 1) => 1f,        //20
            (1, 2) => 1.25f,     //25
            (1, 3) => 2f,        //40
            (1, 4) => 3f,        //60
            (1, 5) => 3.5f,      //70

            (2, 1) => 4f,        //80
            (2, 2) => 8f,        //160

            (3, 1) => 12.5f,     //250
            (3, 2) => 18f,       //360
            (3, 3) => 25f,       //500

            (4, 1) => 35f,       //700
            (4, 2) => 50f,       //1000

            (5, 1) => 65f,       //1300
            (5, 2) => 100f,      //2000
            (5, 3) => 120f,      //2400

            (6, 1) => 150f,      //3000
            (6, 2) => 200f,      //4000
            (6, 3) => 300f,      //6000
            (6, 4) => 400f,      //8000
            (6, 5) => 450f,      //9000
            (6, 6) => 750f,      //15000

            (7, _) => 1500f,     //30000

            _ => 1f
        };
    }

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
    {
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectileAction<ColdheartIcicleProj>(source, position, velocity, damage, knockback, player.whoAmI, p =>
        {
            ColdheartIcicleProj modP = p.GetModProjectile<ColdheartIcicleProj>();
            modP.IsRightClick = player.altFunctionUse == 2;
            modP.Phase = Phase;
            modP.SubPhase = SubPhase;
        });
        if (player.altFunctionUse == 2)
            Projectile.NewProjectileAction<ColdheartIcicleSnowflake>(source, player.Center, velocity.ToCustomLength(3f), damage, 0f, player.whoAmI, p => p.VelocityToRotation());
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        CAItemTooltipModifier tooltipModifier = new(Item, tooltips);
        if (Main.keyState.IsKeyDown(Keys.LeftShift))
        {

        }
        else
            tooltipModifier.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        return base.PreDrawTooltipLine(line, ref yOffset);
    }
}
#endregion 物品

#region 弹幕
public sealed class ColdheartIcicleProj : BaseShortswordProjectile, ICAModProjectile, ILocalizedModType
{
    public override string Texture => ColdheartIcicle.TexturePath;

    public new string LocalizationCategory => "DeveloperContents";

    public bool IsRightClick;
    public int Phase = 1;
    public int SubPhase = 1;

    public override float FadeInDuration => base.FadeInDuration;
    public override float FadeOutDuration => base.FadeOutDuration;
    public override float TotalDuration => base.TotalDuration;

    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 12;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Projectile.timeLeft = 360;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override void SetVisualOffsets()
    {
        const int HalfSpriteWidth = ColdheartIcicle.SpriteWidth / 2;
        DrawOriginOffsetX = 0f;
        DrawOffsetX = Projectile.width / 2 - HalfSpriteWidth;
        DrawOriginOffsetY = Projectile.height / 2 - HalfSpriteWidth;
    }

    public override void AI()
    {
        Behavior();
        if (Projectile.OnOwnerClient)
        {
            if (IsRightClick)
            {
            }
            else
            {
            }
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        float extraDamage = target.statLifeMax2 / 50f;
        modifiers.FinalDamage.Flat += extraDamage;
        CombatText.NewText(target.getRect(), Color.Cyan, extraDamage.ToString(), true, false);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
        if (target.type != NPCID.TargetDummy)
        {
            float extraDamage = target.lifeMax / 50;
            modifiers.FinalDamage.Flat += extraDamage;
        }
    }

    public void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = Color.Cyan);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        new RotatedRectangle(FloatRectangle.FromInnerPoint(Projectile.Center, 9f, 17.5f, 2.25f, 2.25f), Projectile.rotation).Collides(targetHitbox);

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(IsRightClick);
        writer.Write(Phase);
        writer.Write(SubPhase);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        IsRightClick = reader.ReadBoolean();
        Phase = reader.ReadInt32();
        SubPhase = reader.ReadInt32();
    }
}

public sealed class ColdheartIcicleDream : CAModProjectile, IContentLoader
{
    public int TargetIndex = -1;
    public Entity Target
    {
        get => TargetIndex switch
        {
            >= 300 => Main.npc[TargetIndex - 300],
            >= 0 => Main.player[TargetIndex],
            _ => Projectile.Owner
        };
        set => TargetIndex = value switch
        {
            NPC npc => npc.whoAmI + 300,
            Player player => player.whoAmI,
            _ => -1,
        };
    }

    public bool TargetValid => Target switch
    {
        Player player => player.Alive,
        NPC npc => npc.active,
        _ => false
    };


#if CELESS_DEV
    public const int LifeTime = 100;
    public const int LifeTime2 = 120;

    public enum BehaviorType
    {
        Normal,
        /// <summary>
        /// 幻梦。
        /// </summary>
        Dream,
        /// <summary>
        /// 启程。
        /// </summary>
        SetOut
    }

    public BehaviorType Behavior
    {
        get => (BehaviorType)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    public float RealRotation
    {
        get => Projectile.rotation - MathHelper.PiOver2;
        set => Projectile.rotation = value + MathHelper.PiOver2;
    }

    #region 幻梦
    public float SnowflakeScale;
    public float SnowflakeRotation = MathHelper.PiOver2;
    public bool ShouldStopRotating;

    public Vector2 SnowflakeCenter
    {
        get
        {
            float ratio = Math.Clamp(Timer1 / 240f, 0f, 1f);
            return Projectile.Center + new PolarVector2(450f * ratio * (2f - ratio), RealRotation - MathHelper.PiOver2);
        }
    }
    #endregion 幻梦

    #region 启程
    public int SetOutPhase = 1;
    #endregion 启程
#else
    public const int LifeTime = 50;
#endif

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.netImportant = true;
        Projectile.scale = 1f;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LifeTime;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public static class EllipseData
    {
        /// <summary>
        /// 椭圆半长轴缩放常数。
        /// </summary>
        public const float AMultiplier = 1.1075061f;
        /// <summary>
        /// 椭圆半短轴缩放常数。
        /// </summary>
        public const float BMultiplier = 0.7658784f;
        /// <summary>
        /// 参数方程最大角度偏移量。
        /// </summary>
        public const float MaxAngleOffset = 1.3127555f;
    }

    public override void AI()
    {
        Timer1++;
#if CELESS_DEV
        switch (Behavior)
        {
            case BehaviorType.Normal:
                NormalAI();
                break;
            case BehaviorType.Dream:
                Projectile.Center = Projectile.Owner.Center;
                Projectile.timeLeft = LifeTime;
                float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 > 2398 ? (2598 - Timer1) / 200f : Timer1 / 240f);
                if (Main.rand.NextProbability((float)(Timer1 > 2398 ? (2598 - Timer1) / 200f : Math.Clamp(Timer1 / 240f, 0f, 1f)) * 0.8f))
                    ParticleHandler.SpawnParticle(new OrbParticle(SnowflakeCenter, Main.rand.NextVector2Circular(6f, 4f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.4f, 0.7f), Color.White, Main.rand.NextFloat(0.9f, 1.4f), 0.8f) { Important = true });
                SnowflakeScale = 1.4f * interpolation;
                SnowflakeRotation += 0.03f * interpolation * interpolation;
                if (!ShouldStopRotating)
                {
                    Projectile.rotation += TOMathUtils.PiOver3 / LifeTime2 * interpolation;
                    if (RealRotation > MathHelper.TwoPi - MathHelper.PiOver4)
                        Projectile.rotation -= TOMathUtils.PiOver3 / 130f * Utils.Remap(Projectile.rotation, MathHelper.Pi * 2.25f, MathHelper.Pi * 2.5f, 0f, 1f);
                    if (RealRotation > MathHelper.TwoPi)
                    {
                        RealRotation = 0f;
                        ShouldStopRotating = true;
                    }
                }
                if (Timer1 > 2598)
                    Projectile.Kill();
                break;
            case BehaviorType.SetOut:
                const int DepartX = 45768;
                const int DepartY = 7720;
                const int DestinationX = 46816;
                const int DestinationY = 7312;
                Vector2 depart = new(DepartX, DepartY);
                Vector2 destination = new(DestinationX, DestinationY);
                Projectile.timeLeft = LifeTime;
                float interpolation3 = 1f;
                float interpolation4 = 1f;
                switch (SetOutPhase)
                {
                    case 1:
                        if (Timer1 == 1)
                        {
                            Projectile.Center = destination;
                            Projectile.velocity = new PolarVector2(3f, MathHelper.PiOver2 + TOMathUtils.PiOver6);
                        }
                        float distance = Projectile.Distance(depart);
                        Projectile.Homing(depart, HomingAlgorithm.SmoothStep, distance < 50f ? 1f : 0.075f, velocityOverride: Projectile.velocity.Length() * Utils.Remap(distance, 0f, 1000f, 0.9943f, 1f));
                        if (Projectile.Center == depart && Projectile.Owner.Center.X > DepartX - 88)
                        {
                            Timer1 = 0;
                            SetOutPhase = 2;
                        }
                        break;
                    case 2:
                        if (Timer1 > 100)
                        {
                            Timer1 = 0;
                            SetOutPhase = 3;
                        }
                        break;
                    case 3:
                        if (Timer1 == 1)
                        {
                            Projectile.Center = depart;
                            Projectile.velocity = new PolarVector2(0.02f, -MathHelper.PiOver2);
                        }
                        float distance2 = Projectile.Distance(destination);
                        Projectile.Homing(destination, HomingAlgorithm.SmoothStep, distance2 < 50f ? 1f : distance2 < 200f ? 0.1f : 0.045f, velocityOverride: distance2 < 500f ? Projectile.velocity.Length() * Utils.Remap(distance2, 0f, 500f, 0.9947f, 1f) : Math.Min(Projectile.velocity.Length() + 0.01f, 1.5f));
                        if (Projectile.Center == destination && Projectile.Owner.Center.X > DestinationX + 256)
                        {
                            Timer1 = 0;
                            SetOutPhase = 4;
                        }
                        break;
                    case 4:
                        if (Timer1 > 200)
                            interpolation3 = TOMathUtils.Interpolation.QuadraticEaseOut((700 - Timer1) / 500f);
                        if (Timer1 > 100)
                            interpolation4 = TOMathUtils.Interpolation.QuadraticEaseOut((700 - Timer1) / 600f);
                        if (Timer1 == 500)
                        {
                            for (int i = 0; i < 2000; i++)
                                ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Owner.Center + new Vector2(Main.rand.NextFloat(-1600f, 1600f), -1300f - Main.rand.NextFloat(3700f)), Main.rand.NextVector2Circular(8f, 6f), Main.rand.Next(370, 660), Main.rand.NextFloat(0.4f, 0.7f), Color.White, Main.rand.NextFloat(0.25f, 0.7f), 0.8f) { Important = true });
                        }
                        if (Timer1 > 700)
                            Projectile.Kill();
                        break;
                }
                Projectile.BetterChangeScale(16, 16, 0.7f * interpolation3, Projectile.Center);
                Projectile.rotation += MathHelper.Lerp(0.0025f, 0.03f, interpolation4);
                break;
        }
#else
        NormalAI();
#endif
        void NormalAI()
        {
            bool targetValid = TargetValid;
            if (targetValid)
                Projectile.Center = Target.Center;
            Projectile.timeLeft = LifeTime;
            if (Timer2 > 0 || !targetValid || Timer1 > 6000)
                Timer2 = Math.Max(Timer2 + 1, 100 - Timer1 / 3);
            if (Timer2 >= 100)
                Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
#if CELESS_DEV
        switch (Behavior)
        {
            case BehaviorType.Normal:
                NormalPreDraw();
                return false;
            case BehaviorType.Dream:
                Main.Rasterizer.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
                Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                if (Timer1 < 2200)
                {
                    Texture2D texture = ParticleHandler.GetTexture<OrbParticle>();
                    float radius = 150f; //圆心半径
                    for (int j = 0; j < 6; j++)
                    {
                        int localTimer = Timer1 > 2000 ? (2200 - Timer1) / 2 : Math.Min(Timer1 - LifeTime2 * j - 235, LifeTime);
                        for (int i = 0; i <= localTimer * 10; i++)
                        {
                            float a = radius * EllipseData.AMultiplier; //椭圆半长轴
                            float b = radius * EllipseData.BMultiplier; //椭圆半短轴
                            float amount = i * 1.83f / (LifeTime * 10);
                            if (amount > 1f)
                            {
                                float multiplier = MathHelper.Lerp(0.4f, 1f, 2f - amount);
                                a *= multiplier;
                                b *= multiplier;
                            }
                            float angle = TOMathUtils.PiOver6 + TOMathUtils.PiOver3 * (j - 2);
                            Vector2 circleCenter = Projectile.Center + new PolarVector2(radius, angle);
                            (float sin, float cos) = MathF.SinCos(MathHelper.Lerp(-EllipseData.MaxAngleOffset, EllipseData.MaxAngleOffset, amount));
                            Vector2 position = circleCenter + new Vector2(a * cos, b * sin).RotatedBy(angle);
                            Main.spriteBatch.DrawFromCenter(texture, position - Main.screenPosition, null, Color.White, 0f, 0.5f * Utils.Remap(amount, 0f, 1.83f, 0.4f, 1.2f));
                        }
                    }
                }
                ColdheartIcicleSnowflake.DrawSnowflake(SnowflakeCenter, SnowflakeScale, SnowflakeRotation);
                return false;
            case BehaviorType.SetOut:
                ColdheartIcicleSnowflake.DrawSnowflake(Projectile.Center, Projectile.scale, RealRotation);
                return false;
            default:
                return false;
        }
#else
        NormalPreDraw();
        return false;
#endif
        void NormalPreDraw()
        {
            Main.Rasterizer.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Texture2D texture = ParticleHandler.GetTexture<OrbParticle>();
            float radius = 40f; //圆心半径
            for (int j = 0; j < 6; j++)
            {
                int localTimer = TOMathUtils.Min(100 - Timer2, Timer1 - j * 10, LifeTime);
                for (int i = 0; i <= localTimer * 10; i++)
                {
                    float a = radius * EllipseData.AMultiplier; //椭圆半长轴
                    float b = radius * EllipseData.BMultiplier; //椭圆半短轴
                    float amount = i * 1.83f / (LifeTime * 10);
                    if (amount > 1f)
                    {
                        float multiplier = MathHelper.Lerp(0.4f, 1f, 2f - amount);
                        a *= multiplier;
                        b *= multiplier;
                    }
                    float angle = TOMathUtils.PiOver6 + TOMathUtils.PiOver3 * (j - 2);
                    Vector2 circleCenter = Projectile.Center + new PolarVector2(radius, angle);
                    (float sin, float cos) = MathF.SinCos(MathHelper.Lerp(-EllipseData.MaxAngleOffset, EllipseData.MaxAngleOffset, amount));
                    Vector2 position = circleCenter + new Vector2(a * cos, b * sin).RotatedBy(angle);
                    Main.spriteBatch.DrawFromCenter(texture, position - Main.screenPosition, null, Color.White with { A = 0 }, 0f, 0.3f * Utils.Remap(amount, 0f, 1.83f, 0.4f, 1.2f));
                }
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? CanHitNPC(NPC target) => false;

    public override void SendExtraAI(BinaryWriter writer)
    {
#if CELESS_DEV
        writer.Write(SnowflakeScale);
        writer.Write(SnowflakeRotation);
        writer.Write(ShouldStopRotating);
        writer.Write(SetOutPhase);
#endif
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
#if CELESS_DEV
        SnowflakeScale = reader.ReadSingle();
        SnowflakeRotation = reader.ReadSingle();
        ShouldStopRotating = reader.ReadBoolean();
        SetOutPhase = reader.ReadInt32();
#endif
    }
}

public sealed class ColdheartIcicleSnowflake : CAModProjectile, ICAModProjectile
{
    public const int LeftTime = 360;

    public override string Texture => TOTextures.InvisibleTexturePath;

    public override string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Projectile.width = 80;
        Projectile.height = 80;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.timeLeft = LeftTime * 2;
        Projectile.extraUpdates = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.White.ToVector3());

        Timer1++;

        float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(Timer1 <= 20 ? Timer1 / 20f : Timer1 >= (LeftTime - 25) * 2 ? (LeftTime - Timer1) / 50f : 1f);
        Projectile.velocity.Modulus = Utils.Remap(Timer1, 0, LeftTime, 10f, 15f) * interpolation;
        Projectile.BetterChangeScale(80, 80, interpolation, Projectile.Center);
        Projectile.rotation += 0.05f * interpolation;
        if (Main.rand.NextBool(3))
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(4f, 4f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        if (Projectile.timeLeft <= 40 && Main.rand.NextBool(3))
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        if (Projectile.timeLeft <= 15 && Main.rand.NextBool(3))
        {
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(6f, 6f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
            ParticleHandler.SpawnParticle(new OrbParticle(Projectile.Center, Projectile.velocity + Main.rand.NextVector2Circular(7f, 7f), Main.rand.Next(40, 75), Main.rand.NextFloat(0.35f, 0.6f), Color.White, Main.rand.NextFloat(0.8f, 1.3f), 0.8f) { Important = true });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawSnowflake(Projectile.Center, Projectile.scale, Projectile.rotation);
        return false;
    }

    public static void DrawSnowflake(Vector2 center, float scale, float rotation)
    {
        Texture2D scaleTexture = CATextures.Scale2;
        Vector2 position = center - Main.screenPosition;
        Vector2 origin = new(scaleTexture.Width / 2f, scaleTexture.Height);
        Main.spriteBatch.ChangeBlendState(BlendState.Additive);
        for (int i = 0; i < 6; i++)
        {
            float angle = rotation + MathHelper.PiOver2 + TOMathUtils.PiOver3 * i;
            Main.spriteBatch.Draw(scaleTexture, position, null, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
        }
        Main.spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        new Circle(Projectile.Center, 64f * Projectile.scale).Collides(targetHitbox);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
    }

    public override void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = TOSharedData.CelestialColor);
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
                return;
        }
        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.rotation = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == target)
                return;
        }
        SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
        Projectile.NewProjectileAction<ColdheartIcicleDream>(Projectile.GetSource_OnHit(target), target.Center, Vector2.Zero, 0, 0, Projectile.owner, p =>
        {
            p.Center = target.Center;
            p.ai[1] = Projectile.rotation;
            ColdheartIcicleDream modP = p.GetModProjectile<ColdheartIcicleDream>();
            modP.Target = target;
        });
    }
}

public sealed class ColdheartIcicleSnowFlake_GlobalNPC : CAGlobalNPCBehavior
{
    public override void OnKill(NPC npc)
    {
        foreach (Projectile p in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is ColdheartIcicleDream dream && dream.Target == npc)
                dream.Timer2++;
        }
    }
}

public sealed class ColdheartIcicleIceRain : CAModProjectile
{
    public override string Texture => CASharedData.CATexturePath + "Touhou/Ice1";

    public override string LocalizationCategory => "DeveloperContents";
}

#endregion 弹幕

#region 获取
public sealed class ColdheartIcicle_Loot : CAGlobalNPCBehavior
{
    public override bool ShouldProcess => false;

    public override void ModifyGlobalLoot(GlobalLoot globalLoot)
    {
        globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.value > 0f && i.player.ZoneSnow && !Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 400000));
        globalLoot.Add(ItemDropRule.ByCustomCondition(i => !i.IsInSimulation && i.npc.FrostMoonEnemy && Main.snowMoon, null, null, ModContent.ItemType<ColdheartIcicle>(), 20000));
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.ModNPC is DevourerofGodsHead)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ColdheartIcicle>(), 500));
    }
}

public sealed class ColdheartIcicle_Starter : CAPlayerBehavior
{
    public override bool ShouldProcess => false;

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        if (!mediumCoreDeath && ColdheartIcicle.IsLegendOwner(Player))
            yield return Item.CreateItem<ColdheartIcicle>();
    }
}
#endregion 获取