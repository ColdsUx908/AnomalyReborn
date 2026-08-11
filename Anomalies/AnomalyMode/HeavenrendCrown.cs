using Anomalies.DataStructures;
using Anomalies.GameContents;
using Terraria.GameContent.Creative;

namespace Anomalies.AnomalyMode;

public sealed class HeavenrendCrown : AnomalyModItem, ILocalizationPrefix
{
    public const int Lifetime = 300;

    public override string LocalizationCategory => "AnomalyMode";
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + "AnomalyMode";

    public static EnergyParticleSet EnchantmentEnergyParticles = new(-1, 2,
        () => Main.rand.NextFloat() switch
        {
            < 0.3f => AnomalySharedData.MainColor,
            < 0.6f => AnomalySharedData.AnomalyUltramundaneColor,
            _ => AnomalySharedData.RebornColor,
        },
        () => Color.White, 0.1f, 50f);

    public override void SetDefaults()
    {
        Item.width = 82;
        Item.height = 74;
        Item.useAnimation = 60;
        Item.useTime = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.value = 10000;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.shoot = ModContent.ProjectileType<HeavenrendCrownHoldout>();
        Item.noUseGraphic = true;
    }

    public override bool CanUseItem(Player player)
    {
        bool result = Main.GameMode != GameModeID.Creative
            ? !TOSharedData.BossActive
            : CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>().GetIsUnlocked();

        return result;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = Item.Texture;
        Rectangle itemFrame = Main.itemAnimations[Type] is null ? texture.Frame() : Main.itemAnimations[Type].GetFrame(texture);
        Vector2 particleDrawCenter = position;

        EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
        TODrawUtils.DrawInInventoryWithCustomSize(spriteBatch, position, frame, drawColor, origin, texture, 0.5f);
        return false;
    }
}

public sealed class HeavenrendCrownSystem : ModSystem
{
    public override void UpdateUI(GameTime gameTime)
    {
        HeavenrendCrown.EnchantmentEnergyParticles.Update();
    }
}

public sealed class HeavenrendCrownHoldout : AnomalyModProjectile
{
    public override LocalizedText DisplayName => ModContent.GetModItem<HeavenrendCrown>()?.DisplayName;
    public override string Texture => ModContent.GetModItem<HeavenrendCrown>()?.Texture;

    public override void AI()
    {
        Timer1++;
        Lighting.AddLight(Projectile.Center, AnomalySharedData.UltraIdentifierColor.ToVector3());

        if (Projectile.IsOnOwnerClient)
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.spriteDirection = Owner.direction;
                Projectile.localAI[0] = 1f;
            }
            Owner.itemRotation = 0f;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.ChangeDir(Projectile.spriteDirection);

            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true) + new Vector2(Projectile.spriteDirection * 32f, -35f);
        }

        switch (Timer1)
        {
            case 1:
                if (!AnomalySharedData.Anomaly)
                {
                    SoundEngine.PlaySound(AnomalyModeHandler.ActivationSound);
                    if (Main.GameMode != GameModeID.Creative)
                        Main.GameMode = GameModeID.Master;
                    AnomalySharedData.Anomaly = true;
                }
                else
                {
                    SoundEngine.PlaySound(AnomalyModeHandler.ActivationSound);
                    AnomalySharedData.Anomaly = false;
                }
                break;
            case 58:
                Projectile.Kill();
                break;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Color.White);
        return false;
    }
}