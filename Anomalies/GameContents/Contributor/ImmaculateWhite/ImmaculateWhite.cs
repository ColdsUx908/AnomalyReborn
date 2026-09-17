namespace Anomalies.GameContents.Contributor.ImmaculateWhite;

public sealed class ImmaculateWhite : AnomalyModItem, ILegendaryItem, ILocalizationPrefix
{
    public const int OriginalUseTime = 100;

    #region 传奇
    public int Phase = 1;
    public int SubPhase = 1;

    public static void GetPhase(out int phase, out int subPhase)
    {
        if (NPC.downedMoonlord) //月总
        {
            phase = 3;
            /*
            if (NPC.Focus) //万物的焦点
                subPhase = 6;
            else if (DownedBossSystem_Bridge.downedYharon) //犽戎
                subPhase = 5;
            else if (DownedBossSystem_Bridge.downedDoG) //神吞
                subPhase = 4;
            else if (DownedBossSystem_Bridge.downedPolterghast) //幽花
                subPhase = 3;
            else if (DownedBossSystem_Bridge.downedProvidence) //亵渎天神
                subPhase = 2;
            else*/
            subPhase = 1;
        }
        else if (Main.hardMode) //肉山
        {
            phase = 2;

            if (NPC.downedAncientCultist) //教徒
                subPhase = 5;
            else if (NPC.downedGolemBoss) //石巨人
                subPhase = 4;
            else if (NPC.downedPlantBoss) //世花
                subPhase = 3;
            else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) //机械三王
                subPhase = 2;
            else
                subPhase = 1;
        }
        else
        {
            phase = 1;
            if (NPC.downedBoss3) //骷髅王
                subPhase = 3;
            else if (NPC.downedBoss2) //世吞克脑
                subPhase = 2;
            else
                subPhase = 1;
        }
    }

    public static float GetDamageMultiplier(int phase, int subPhase)
    {
        float multiplier = phase switch
        {
            3 => subPhase switch //月后
            {
                6 => 300f,    //万物的焦点
                5 => 50f,     //丛林龙后
                4 => 27.5f,   //神明吞噬者后
                3 => 20f,     //噬魂幽花后
                2 => 17.5f,   //亵渎天神后
                _ => 13.5f
            },
            2 => subPhase switch //肉后
            {
                5 => 9f,      //教徒后
                4 => 7f,      //石巨人后
                3 => 6f,      //世纪之花后
                2 => 4f,      //机械三王后
                _ => 3f
            },
            1 => subPhase switch //肉前
            {
                3 => 3f,      //骷髅王后
                2 => 1.5f,    //世界吞噬者/克苏鲁之脑后
                _ => 1f
            },
            _ => 1f
        };
        return multiplier;
    }

    public void LegendaryUpdate() => GetPhase(out Phase, out SubPhase);

    public void LegendaryUpdate(Player player) => LegendaryUpdate();
    #endregion 传奇

    public override string LocalizationCategory => "GameContents.Contributor";

    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".ImmaculateWhite";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 60;
        Item.damage = 24;
        Item.DamageType = DamageClass.Ranged;
        Item.useAmmo = AmmoID.Arrow;
        Item.useTime = OriginalUseTime;
        Item.useAnimation = OriginalUseTime;
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

    public override bool CanConsumeAmmo(Item ammo, Player player) => false; //召唤弓本身不消耗弹药

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        StatModifier rangedModifier = player.GetTotalDamage(DamageClass.Ranged);
        CombinedHooks.ModifyWeaponDamage(player, Item, ref rangedModifier);
        int newDamage = Math.Max(0, (int)(rangedModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, Item.shoot, newDamage, knockback, player.whoAmI, p =>
        {
            ImmaculateWhiteBow modP = p.GetModProjectile<ImmaculateWhiteBow>();

            modP.ShootSpeedMultiplier = (float)Item.useTime / OriginalUseTime;

            if (Phase >= 2) //肉后可以分裂出小弹幕
                modP.CanShootSplitBolt = true;

            if (NPC.downedEmpressOfLight) //光女后可以右键发射另一种弹幕
                modP.CanUseRightClickFunction = true;
        });
        return false;
    }

    public override void UpdateInventory(Player player)
    {
        LegendaryUpdate(player);
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        //莫沧然：ModifyWeaponDamage的加成在词缀的加成之后，可以考虑到时候移植到别的地方。唉唉灾厄起的坏头
        float multiplier = GetDamageMultiplier(Phase, SubPhase);
        damage *= multiplier;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();
        if (!modifier.TryGet(null, "Tooltip0", out int index, out _))
            return;

        if (Phase >= 3)
            tooltips.Insert(++index, new TooltipLine(Mod, "Tooltip1", this.GetTextValue("Tooltip1")));
        if (NPC.downedEmpressOfLight)
            tooltips.Insert(++index, new TooltipLine(Mod, "Tooltip2", this.GetTextValue("Tooltip2")));

        IContributorItem.AddContributorItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IContributorItem.AddContributorItemOwnerIdentifier(Mod, tooltips, ++index, "莫沧然");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();

        if (!shift)
            modifier.AddExpendedDisplayLine();
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

