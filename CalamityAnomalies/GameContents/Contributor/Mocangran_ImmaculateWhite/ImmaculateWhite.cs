// Developed by ColdsUx

namespace CalamityAnomalies.GameContents.Contributor.Mocangran_ImmaculateWhite;

public sealed class ImmaculateWhite : CALegendaryItem
{
    public int itemPentrate = 0;

    #region 传奇
    public override void LegendaryUpdate()
    {
        if (NPC.downedMoonlord)//月总
        {
            Phase = 3;

            if (NPC.Focus)//万物的焦点
                SubPhase = 6;
            else if (DownedBossSystem_Bridge.downedYharon)//犽戎
                SubPhase = 5;
            else if (DownedBossSystem_Bridge.downedDoG)//神吞
                SubPhase = 4;
            else if (DownedBossSystem_Bridge.downedPolterghast)//幽花
                SubPhase = 3;
            else if (DownedBossSystem_Bridge.downedProvidence)//亵渎天神
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else if (Main.hardMode)//肉山
        {
            Phase = 2;

            if (NPC.downedAncientCultist)//教徒
                SubPhase = 5;
            else if (NPC.downedGolemBoss)//石巨人
                SubPhase = 4;
            else if (NPC.downedPlantBoss)//世花
                SubPhase = 3;
            else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)//机械三王
                SubPhase = 2;
            else
                SubPhase = 1;
        }
        else
        {
            Phase = 1;
            if (NPC.downedBoss3)//骷髅王
                SubPhase = 3;
            else if (NPC.downedBoss2)//世吞克脑
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

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 60;
        Item.damage = 24;
        Item.DamageType = DamageClass.Magic;
        Item.useAmmo = AmmoID.Arrow;
        Item.mana = 12;
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
    }

    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] <= 0 && player.Anomaly.ImmaculateWhite_Timer == 0;
    }

    //右键可以发射其他弹幕
    public override bool AltFunctionUse(Player player) => NPC.downedEmpressOfLight;
    public override bool ConsumeItem(Player player) => false;

    #region 按住shift+右键可以同比转换攻速和弹幕穿透数。
    public override bool CanRightClick() => (Main.keyState.PressingShift() && Phase >= 3);

    public override void RightClick(Player player)
    {
        if (itemPentrate == 9)
        {
            itemPentrate = 0;
        }
        else
        {
            itemPentrate += 1;
        }
        Item.NetStateChanged();
    }
    #endregion
    public override bool CanConsumeAmmo(Item ammo, Player player) => false; //召唤弓本身不消耗弹药

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        //假如通过某种手段更改了月总的击败状况，自动重置回默认穿透。
        itemPentrate = Phase >= 3 ? itemPentrate : 0;

        StatModifier rangedModifier = player.GetTotalDamage(DamageClass.Magic);
        CombinedHooks.ModifyWeaponDamage(player, Item, ref rangedModifier);
        int newDamage = Math.Max(0, (int)(rangedModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, Item.shoot, newDamage, knockback, player.whoAmI, p =>
        {
            //肉后可以分裂出小弹幕
            if (Phase >= 2)
            {
                p.ai[2] = 1f;
            }
            //光女后可以右键发射另一种弹幕
            if (NPC.downedEmpressOfLight)
            {
                p.ai[2] = 2f;
            }
            if (p.ModProjectile is ImmaculateWhiteBow bow)
            {
                bow.itemPentrate = itemPentrate;
            }
        });
        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        //莫沧然：ModifyWeaponDamage的加成在词缀的加成之后，可以考虑到时候移植到别的地方。唉唉灾厄起的坏头
        damage *= Phase switch
        {
            3 => SubPhase switch//月后
            {
                6 => 300f,    //万物的焦点
                5 => 10f,    //丛林龙后
                4 => 10f,     //神明吞噬者后
                3 => 10f,     //噬魂幽花后
                2 => 12f,     //亵渎天神后
                _ => 10f
            },
            2 => SubPhase switch//肉后
            {
                5 => 10f,     //教徒后
                4 => 7f,     //石巨人后
                3 => 6f,      //世纪之花后
                2 => 4f,      //机械三王后
                _ => 3f
            },
            1 => SubPhase switch//肉前
            {
                3 => 3f,      //骷髅王后
                2 => 1.5f,      //世界吞噬者/克苏鲁之脑后
                _ => 1f
            },
            _ => 1f
        };
        if (player.altFunctionUse != 2)
        {
            damage /= itemPentrate == 0 ? 1 : (itemPentrate + 1);
        }
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
