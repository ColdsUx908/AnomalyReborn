using Anomalies.Assets.Effects;
using Anomalies.GameContents;
using Terraria.Utilities;

namespace Anomalies.SpecialContents.Developer.EventideReunion;

public sealed class EventideReunion : AnomalyModItem, IDeveloperItem, ILocalizationPrefix
{
    private static readonly UnifiedRandom _TooltipSparkleRandom = new(1);

    public const string LocalizationCategoryForEventideReunion = "SpecialContents.Developer.EventideReunion";
    // 物品名称自动追加 EventideReunion，正好对应公共分组下的 DisplayName / Tooltip。
    public override string LocalizationCategory => "SpecialContents.Developer";
    public string LocalizationPrefix => AnomalySharedData.ModLocalizationPrefix + LocalizationCategoryForEventideReunion;

    public static EnchantedParticleSet EnchantmentEnergyParticles = new(-1, 0.7f,
        () => Main.rand.NextFloat() switch
        {
            < 0.5f => Color.Red,
            _ => AnomalySharedData.RebornColor,
        },
        () => Color.White, 0.1f, 50f);

    public const int OriginalUseTime = 30;

    public override void SetDefaults()
    {
        Item.width = 312;
        Item.height = 312;
        Item.damage = 24;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = OriginalUseTime;
        Item.useAnimation = OriginalUseTime;
        Item.useStyle = ItemUseStyleID.Rapier;
        Item.autoReuse = true;
        Item.channel = true;
        Item.useTurn = true;
        Item.knockBack = 3f;
        Item.shoot = ModContent.ProjectileType<EventideReunionHoldout>();
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = Celestial.CelestialPrice;
        Item.noUseGraphic = true;
        Item.noMelee = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) => itemGroup = ContentSamples.CreativeHelper.ItemGroup.MeleeWeapon;

    public override bool AltFunctionUse(Player ownerPlayer) => true;

    public override bool CanUseItem(Player ownerPlayer) => ownerPlayer.altFunctionUse != 2
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSelection>()] == 0
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] == 0
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] == 0
        && ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionHoldout>()] == 0;

    // 仅由持有者客户端在松开右键且选中符文时调用一次。后续技能在这里分派并使用 Action 接口生成同步弹幕。
    public void ReleaseSkill(Player ownerPlayer, EventideReunionAspect aspect, Vector2 aimDirection)
    {
        switch (aspect)
        {
            case EventideReunionAspect.Sun:
                Item weapon = ownerPlayer.HeldItem;
                if (ownerPlayer.whoAmI != Main.myPlayer || ownerPlayer.Anomaly.EventideReunionSunCooldown > 0
                    || weapon.type != ModContent.ItemType<EventideReunion>() || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
                    || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionSunSkill>()] > 0)
                    return;
                Projectile.NewProjectileAction<EventideReunionSunSkill>(ownerPlayer.GetSource_ItemUse(weapon),
                    ownerPlayer.Center, Vector2.Zero, ownerPlayer.GetWeaponDamage(weapon),
                    ownerPlayer.GetWeaponKnockback(weapon, weapon.knockBack), ownerPlayer.whoAmI, p =>
                    {
                        p.ai[2] = aimDirection.X >= 0f ? 1f : -1f;
                        p.GetModProjectile<EventideReunionSunSkill>().Target = ownerPlayer.Center + aimDirection * 320f;
                    });
                ownerPlayer.Anomaly.EventideReunionSunCooldown = EventideReunionSunSkill.Cooldown;
                break;
            case EventideReunionAspect.Samsara:
                break; // 轮回相·业轮回响
            case EventideReunionAspect.Life:
                break; // 生命相·新芽赞歌
            case EventideReunionAspect.Death:
                Item deathWeapon = ownerPlayer.HeldItem;
                if (ownerPlayer.whoAmI != Main.myPlayer || ownerPlayer.Anomaly.EventideReunionDeathCooldown > 0
                    || deathWeapon.type != ModContent.ItemType<EventideReunion>() || ownerPlayer.dead || ownerPlayer.noItems || ownerPlayer.CCed
                    || ownerPlayer.ownedProjectileCounts[ModContent.ProjectileType<EventideReunionDeathSkill>()] > 0)
                    return;
                Projectile.NewProjectileAction<EventideReunionDeathSkill>(ownerPlayer.GetSource_ItemUse(deathWeapon),
                    ownerPlayer.Center, aimDirection.SafeNormalize(Vector2.UnitX * ownerPlayer.direction),
                    ownerPlayer.GetWeaponDamage(deathWeapon), ownerPlayer.GetWeaponKnockback(deathWeapon, deathWeapon.knockBack), ownerPlayer.whoAmI);
                ownerPlayer.Anomaly.EventideReunionDeathCooldown = EventideReunionDeathSkill.Cooldown;
                break;
            case EventideReunionAspect.Sacredness:
                break; // 神圣相·圣裁贯日
            case EventideReunionAspect.Star:
                break; // 恒星相·星核坍缩
        }
    }

    public override bool Shoot(Player ownerPlayer, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int projectileType, int damage, float knockback)
    {
        StatModifier meleeModifier = ownerPlayer.GetTotalDamage(DamageClass.Melee);
        CombinedHooks.ModifyWeaponDamage(ownerPlayer, Item, ref meleeModifier);
        int newDamage = Math.Max(0, (int)(meleeModifier.ApplyTo(Item.damage) + 5E-06f));
        Projectile.NewProjectileAction(source, position, velocity, projectileType, newDamage, knockback, ownerPlayer.whoAmI, p =>
        {
        });
        return false;
    }

    public override bool? CanHitNPC(Player ownerPlayer, NPC target) => false;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float currentScale)
    {
        Texture2D drawTexture = Item.Texture;
        Vector2 particleDrawCenter = position;

        EnchantmentEnergyParticles.Update();
        EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
        TODrawUtils.DrawInInventoryWithCustomSize(spriteBatch, position, frame, drawColor, origin, drawTexture, 0.12f);
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        AnomalyItemTooltipModifier modifier = new(Item, tooltips);
        bool shift = Main.keyState.PressingShift();

        int index = modifier._LastTooltipIndex;

        IDeveloperItem.AddDeveloperItemIdentifier(Mod, tooltips, ++index);
        if (shift)
            IDeveloperItem.AddDeveloperItemOwnerIdentifier(Mod, tooltips, ++index, "VectorChaos");
        ILegendaryItem.AddLegendaryItemIdentifier(Mod, tooltips, ++index);

        modifier.Update();

        if (!shift)
            modifier.AddExpendedDisplayLine();
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (TODrawUtils.TryDrawItemNameWithCustomShader(line, l => AnomalyEffects.Texts.EventideReunion.Data
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uScreenRatio", TODrawUtils.ScreenRatio)
            .SetCustomParameter("uPosition", new Vector2(l.X, l.Y))
            .Apply()))
        {
            DrawNameSparkles(line);
            return false;
        }

        return true;

        static void DrawNameSparkles(DrawableTooltipLine nameLine)
        {
            Texture2D sparkle = AnomalyTextures.Extra.CrystalTextSparkle;
            Vector2 textSize = nameLine.Font.MeasureString(nameLine.Text);
            Vector2 origin = sparkle.Size() * 0.5f;
            Vector2 textPosition = new(nameLine.X, nameLine.Y);
            float time = Main.GlobalTimeWrappedHourly;

            // 固定每颗星芒的位置、相位和转速，避免重绘时跳动或影响世界随机数。
            _TooltipSparkleRandom.SetSeed(1);
            int count = Math.Max(1, (int)(textSize.X / 6f));
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new(_TooltipSparkleRandom.NextFloat(textSize.X), _TooltipSparkleRandom.NextFloat(textSize.Y * 0.6f) + 1f);
                float phase = (time * 4f + _TooltipSparkleRandom.NextFloat(MathHelper.TwoPi)) % MathHelper.TwoPi;
                float rotation = time * _TooltipSparkleRandom.NextFloat(0.8f, 1.5f) + nameLine.Rotation;
                Color gold = Color.Lerp(new Color(255, 145, 35, 0), new Color(255, 210, 75, 0), _TooltipSparkleRandom.NextFloat());
                float brightness = Math.Max(0f, MathF.Sin(phase));
                float progress = phase / MathHelper.TwoPi;
                offset.Y += 2f - phase * 4.5f;
                Vector2 position = textPosition + ((offset - nameLine.Origin) * nameLine.BaseScale).RotatedBy(nameLine.Rotation);
                float rayWidth = progress * 0.15f;
                float rayLength = (MathF.Sin(phase / MathHelper.PiOver2) + 1f) * 0.2f;

                Main.spriteBatch.Draw(sparkle, position, null, gold * brightness, rotation, origin,
                    new Vector2(rayLength, rayWidth) * nameLine.BaseScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(sparkle, position, null, gold * brightness, rotation, origin,
                    new Vector2(rayWidth, rayLength) * nameLine.BaseScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(sparkle, position, null, new Color(255, 240, 190, 0) * brightness, rotation, origin,
                    nameLine.BaseScale * (progress * 0.3f), SpriteEffects.None, 0f);
            }
        }
    }
}
