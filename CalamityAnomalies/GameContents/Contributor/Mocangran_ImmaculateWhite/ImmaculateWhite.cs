// Developed by ColdsUx

using CalamityAnomalies.GameContents.Developer;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Ranged;

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateWhite : CALegendaryItem
{
    #region 传奇
    public override void LegendaryUpdate()
    {
        if (NPC.downedEmpressOfLight)
        {
            Phase = 2;

            if (NPC.Focus)
                SubPhase = 6;
            else if (DownedBossSystem_Bridge.downedYharon)
                SubPhase = 5;
            else if (DownedBossSystem_Bridge.downedDoG)
                SubPhase = 4;
            else if (DownedBossSystem_Bridge.downedProvidence)
                SubPhase = 3;
            else if (NPC.downedMoonlord)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else
        {
            Phase = 1;

            if (NPC.downedGolemBoss)
                SubPhase = 5;
            else if (NPC.downedPlantBoss)
                SubPhase = 4;
            else if (Main.hardMode)
                SubPhase = 3;
            else if (NPC.downedBoss2)
                SubPhase = 2;
            else
                SubPhase = 1;
        }
    }

    public override void LegendaryUpdate(Player player)
    {
        LegendaryUpdate();
    }
    #endregion 传奇

    public override string LocalizationCategory => "GameContents.Contributor";

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 60;
        Item.damage = 10;
        Item.DamageType = DamageClass.Ranged;
        Item.useAmmo = AmmoID.Arrow;
        Item.useTime = 100;
        Item.useAnimation = 100;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.autoReuse = true;
        Item.channel = true;
        Item.UseSound = SoundID.Item1;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.shoot = ModContent.ProjectileType<ImmaculateWhiteBow>();
        Item.shootSpeed = 20f;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
        CalamityItem.devItem = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0; //召唤弓本身不消耗弹药

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        StatModifier rangedModifier = player.GetTotalDamage(DamageClass.Ranged);
        CombinedHooks.ModifyWeaponDamage(player, Item, ref rangedModifier);
        int newDamage = Math.Max(0, (int)(rangedModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, Item.shoot, newDamage, knockback, player.whoAmI, p =>
        {
            if (Phase >= 2)
                p.ai[2] = 1f;
        });
        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        damage *= Phase switch
        {
            2 => SubPhase switch
            {
                6 => 300f,    //万物的焦点
                5 => 120f,    //丛林龙后
                4 => 80f,     //神明吞噬者后
                3 => 40f,     //亵渎天神后
                2 => 20f,     //月亮领主后
                _ => 10f      //光之女皇后
            },
            1 => SubPhase switch
            {
                5 => 20f,     //石巨人后
                4 => 12f,     //世纪之花后
                3 => 6f,      //血肉墙后
                2 => 2f,      //世界吞噬者/克苏鲁之脑后
                _ => 1f
            },
            _ => 1f
        };
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.WoodenBow)
            .AddIngredient(ItemID.AngelStatue)
            .AddIngredient(ItemID.AngelHalo)
            .AddCondition(Condition.NearShimmer)
            .Register();
    }
}
