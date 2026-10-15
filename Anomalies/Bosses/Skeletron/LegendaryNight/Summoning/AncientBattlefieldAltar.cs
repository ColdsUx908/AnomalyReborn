using Anomalies.Assets.Effects;
using Anomalies.GameContents;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Summoning;

public sealed class AncientBattlefieldAltarItem : AnomalyModItem
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        AnomalySharedData.DontApplyAnomalyRarityShaderItems[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AncientBattlefieldAltar>());
        Item.rare = ModContent.RarityType<AnomalyUltramundaneRarity>();
        Item.value = AnomalyRarity.AnomalyPrice;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<FallenStarEssence>())
            .AddIngredient(ItemID.Bone, 20)
            .AddIngredient(ItemID.StoneBlock, 50)
            .AddIngredient(ItemID.GoldBar, 10)
            .AddTile(TileID.DemonAltar)
            .Register();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.LegendaryNightGold.Data
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
            .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
            .Apply()))
        {
            return false;
        }

        return true;
    }
}

public sealed class AncientBattlefieldAltar : ModTile
{
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public const int Width = 5;
    public const int Height = 5;
    public const int FrameCount = 7;

    public static readonly SoundStyle LegendaryNightActivate = new(LegendaryNightHandler.LegendaryNightPath + "Summoning/LegendaryNightActivate");

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        // Various data sets to protect this tile from unintentional interactions
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.Origin = new Point16(2, 3);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        AddMapEntry(LegendaryNightHandler.StarColor, ModContent.GetModItem<AncientBattlefieldAltarItem>().DisplayName);

        AnimationFrameHeight = Height * 18;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CreateDust(int i, int j, ref int type)
    {
        type = DustID.YellowStarDust;
        return true;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        LegendaryNightRitual ritual = null;
        bool ritualExists = false;

        foreach (Projectile p  in Projectile.ActiveProjectiles)
        {
            if (p.ModProjectile is LegendaryNightRitual modP)
            {
                ritual = modP;
                ritualExists = true;
                break;
            }
        }

        if (!ritualExists)
        {
            //如果仪式不存在，则回到初始状态
            if (++frameCounter >= 3)
            {
                frameCounter = 0;
                frame = Math.Max(0, frame - 1);
            }

            return;
        }

        frame = Math.Clamp(ritual.Timer1 / 2, 0, FrameCount - 1);
    }

    public override void MouseOver(int i, int j) => DisplayIcon(i, j);
    public override void MouseOverFar(int i, int j) => DisplayIcon(i, j);

    public static void DisplayIcon(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<MemoryofAstraeus>();
        Main.LocalPlayer.noThrow = 2;
        Main.LocalPlayer.cursorItemIconEnabled = true;
    }

    public override bool RightClick(int i, int j)
    {
        if (!Main.LocalPlayer.HasItem(ModContent.ItemType<MemoryofAstraeus>())) //没有召唤物品不允许召唤
            return true;

        if ((!Main.remixWorld && Main.dayTime) || !Ultra) //白天或者非超凡世界不允许召唤
        {
            TOLocalizationUtils.ChatLocalizedText(AnomalySharedData.ModLocalizationPrefix + LocalizationCategory + ".SummonFailed", LegendaryNightHandler.StarColor);
            return true;
        }

        Tile tile = Main.tile[i, j];
        int left = i - tile.TileFrameX / 18;
        int top = j - tile.TileFrameY / 18;
        Vector2 ritualSpawnPosition = new Vector2(left, top).ToWorldCoordinates() + new Vector2(32f, -20f);

        SoundEngine.PlaySound(LegendaryNightActivate, ritualSpawnPosition);
        Projectile.NewProjectileAction<LegendaryNightRitual>(new EntitySource_WorldEvent(), ritualSpawnPosition, Vector2.Zero, 0, 0f, action: p =>
        {
            LegendaryNightRitual modP = p.GetModProjectile<LegendaryNightRitual>();
            modP.AltarTopLeftCoordinate = new Point(left, top);
        });

        return true;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Projectile.KillAll(p => p.ModProjectile is LegendaryNightRitual);
    }
}
