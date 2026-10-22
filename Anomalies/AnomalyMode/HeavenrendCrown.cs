using Anomalies.GameContents;
using Terraria.GameContent.Creative;

namespace Anomalies.AnomalyMode;

public sealed class HeavenrendCrown : AnomalyModItem
{
    public const int Lifetime = 300;

    public override string LocalizationCategory => "AnomalyMode";

    public static EnchantedParticleSet EnchantmentEnergyParticles = new(-1, 0.5f,
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

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.EventItem;

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
        Vector2 particleDrawCenter = position;

        EnchantmentEnergyParticles.Update();
        EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
        //TODrawUtils.DrawInInventoryWithCustomSize(spriteBatch, position, frame, drawColor, origin, texture, 0.5f);
        return true;
    }
}

public sealed class HeavenrendCrownHoldout : AnomalyModProjectile
{
    private const int VisibleDuration = 58;
    private const int HiddenDuration = 90;

    // ai[0]：0 尚未确定操作，1 开启模式，-1 关闭模式；由服务端确定并同步。
    public bool IsActivatingAnomaly => Projectile.ai[0] == 1f;

    public override LocalizedText DisplayName => ModContent.GetModItem<HeavenrendCrown>()?.DisplayName;
    public override string Texture => ModContent.GetModItem<HeavenrendCrown>()?.Texture;

    public override void AI()
    {
        Timer1++;
        if (Timer1 < VisibleDuration)
            Lighting.AddLight(Projectile.Center, AnomalySharedData.AnomalyUltramundaneTitleColor.ToVector3());

        if (Projectile.IsOnOwnerClient && Timer1 < VisibleDuration)
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
                SoundEngine.PlaySound(AnomalyHandler.ActivationSound);
                if (!TOSharedData.NotClient)
                    break;
                Projectile.ai[0] = AnomalySharedData.Anomaly ? -1f : 1f;
                Projectile.netUpdate = true;
                if (IsActivatingAnomaly)
                {
                    if (Main.GameMode != GameModeID.Creative)
                    {
                        Main.GameMode = GameModeID.Master;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.WorldData);
                    }
                    AnomalySharedData.Anomaly = true;
                }
                else
                {
                    AnomalySharedData.Anomaly = false;
                }
                break;
            case VisibleDuration + HiddenDuration:
                Projectile.Kill();
                break;
        }

        if (!Main.dedServ && Projectile.active && IsActivatingAnomaly)
            AnomalyStarVisual.Intensity += 1f / 30f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Timer1 >= VisibleDuration)
            return false;
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, null, Color.White);
        return false;
    }
}
