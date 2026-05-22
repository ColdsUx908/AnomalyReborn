// Developed by ColdsUx

using Microsoft.Xna.Framework.Input;

namespace CalamityAnomalies.GameContents.Developer;

public sealed class ColdheartIcicle : CALegendaryItem
{
    #region Static
    public const int SpriteWidth = 24;
    public const string TexturePath = "CalamityMod/Items/Weapons/Typeless/ColdheartIcicle";

    public static bool IsLegendOwner(Player player) => player.name == "ColdsUx";

    public static bool IsDream(Player player) => player.name == "Celessalia";
    #endregion Static

    #region 传奇
    public override void LegendaryUpdate()
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

    public override void LegendaryUpdate(Player player)
    {
        LegendaryUpdate();
        CAPlayer anomalyPlayer = player.Anomaly;
        anomalyPlayer.Coldheart_Phase = Phase;
        anomalyPlayer.Coldheart_SubPhase = SubPhase;
    }
    #endregion 传奇

    public override string Texture => TexturePath;

    public override string LocalizationCategory => "GameContents.Developer";

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
        Item.shoot = ModContent.ProjectileType<ColdheartIcicleHoldout>();
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
        LegendaryUpdate(player);
    }

    public override bool AltFunctionUse(Player player) => true;

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
        Projectile.NewProjectileAction<ColdheartIcicleHoldout>(source, position, velocity, damage, knockback, player.whoAmI, p =>
        {
            ColdheartIcicleHoldout modP = p.GetModProjectile<ColdheartIcicleHoldout>();
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
        CAItemTooltipModifier modifier = new(Item, tooltips);
        if (Main.keyState.PressingShift())
        {

        }
        else
            modifier.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        return base.PreDrawTooltipLine(line, ref yOffset);
    }
}