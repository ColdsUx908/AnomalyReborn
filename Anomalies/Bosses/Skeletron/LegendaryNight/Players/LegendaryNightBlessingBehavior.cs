namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 星神祝福的玩家状态汇总、装备效果与战斗加成。
/// </summary>
/// <remarks>
/// <para>数据槽位：不使用 ai、localAI 或实体 Timer；玩家状态由统一 AnomalyPlayer 保存。</para>
/// <para>LegendaryNight_DamageBonusStacks、RegenerationBonusStacks 每帧汇总；StarGodBlessingActive/Opacity 从主体同步时钟推导；VirtualBoneWings 为本地功能装备缓存，不增加网络载荷。</para>
/// </remarks>
public sealed class LegendaryNightBlessingBehavior : AnomalyPlayerBehavior
{
    public override void UpdateDead()
    {
        AnomalyPlayer.LegendaryNight_DamageBonusStacks = 0;
        AnomalyPlayer.LegendaryNight_RegenerationBonusStacks = 0;
        AnomalyPlayer.LegendaryNight_StarGodBlessingActive = false;
        AnomalyPlayer.LegendaryNight_StarGodBlessingOpacity = 0f;
    }

    public override void PostUpdateEquips()
    {
        if (!Player.active || Player.dead)
            return;

        // 从同步的 Boss 阶段和计时推导效果，支持复活、半途加入和多个 Boss 同时存在。
        foreach (NPC npc in NPC.ActiveNPCs)
        {
            if (!LegendaryNightHandler.IsLegendaryNightHead(npc)
                || !SkeletronLegendHead.GetInstance(npc).StarGodBlessingActive)
                continue;

            AnomalyPlayer.LegendaryNight_StarGodBlessingActive = true;
            AnomalyPlayer.LegendaryNight_StarGodBlessingOpacity = Math.Max(AnomalyPlayer.LegendaryNight_StarGodBlessingOpacity,
                SkeletronLegendHead.GetInstance(npc).StarGodBlessingOpacity);
        }

        if (!AnomalyPlayer.LegendaryNight_StarGodBlessingActive)
            return;

        Player.AddBuff(ModContent.BuffType<AstralBlessing>(), 2);

        // 每帧只汇总一次；高频命中回调直接读取玩家缓存。
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is LegendEnergySphere sphere
                && sphere.Head is SkeletronLegendHead { StellarMechanicsActive: true, StarGodBlessingActive: true } head
                && head.CurrentBehavior != SkeletronLegendHead.Behavior.Despawn)
            {
                LegendEnergySphere.LightOrbBonuses bonuses = sphere.GetBonuses(Player.whoAmI);
                AnomalyPlayer.LegendaryNight_DamageBonusStacks += bonuses.DamageStacks;
                AnomalyPlayer.LegendaryNight_RegenerationBonusStacks += bonuses.RegenerationStacks;
            }
        }

        // 获得飞升之证效果，包含无限飞行与移动加成。
        Player.empressBrooch = true;

        // 功能翅膀优先：已有翅膀时保留其飞行参数。
        if (Player.wingsLogic <= 0)
        {
            AnomalyPlayer.LegendaryNight_VirtualBoneWings ??= new Item(ItemID.BoneWings);
            int visibleWings = Player.wings;
            Player.ApplyEquipFunctional(AnomalyPlayer.LegendaryNight_VirtualBoneWings, hideVisual: true);
            if (visibleWings > 0)
                Player.wings = visibleWings;
        }
        Player.noFallDmg = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!AnomalyPlayer.LegendaryNight_StarGodBlessingActive)
            return;
        // 同一玩家的层数先相加，再统一乘算最终伤害；两层为 1.10，而非 1.05²。
        modifiers.FinalDamage *= SkeletronLegendHead.GetLightOrbDamageBonusMultiplier(AnomalyPlayer.LegendaryNight_DamageBonusStacks);
    }

    public override void UpdateLifeRegen()
    {
        if (AnomalyPlayer.LegendaryNight_StarGodBlessingActive)
            Player.lifeRegen += AnomalyPlayer.LegendaryNight_RegenerationBonusStacks;
    }

    public override void PostUpdateMiscEffects()
    {
        // 在装备及飞行时间加成计算后补满，不修改玩家的实际装备或持久数据。
        if (!AnomalyPlayer.LegendaryNight_StarGodBlessingActive)
            return;

        Player.wingTime = Player.wingTimeMax;

        // 复原太空低重力，保留液体中的原版重力，并免疫星旋失重。
        Player.buffImmune[BuffID.VortexDebuff] = true;
        float worldSizeFactor = Main.maxTilesX / 4200f;
        worldSizeFactor *= worldSizeFactor;
        float spaceGravityMultiplier = (float)((Player.position.Y / 16f - (60f + 10f * worldSizeFactor))
            / (Main.worldSurface / (Main.remixWorld ? 1.0 : 6.0)));
        if (spaceGravityMultiplier < 1f)
        {
            Player.gravity = Player.defaultGravity;
            if (Player.wet)
            {
                if (Player.honeyWet)
                    Player.gravity = 0.1f;
                else if (Player.merman)
                    Player.gravity = 0.3f;
                else if (Player.trident && !Player.lavaWet)
                    Player.gravity = Player.controlUp ? 0.1f : 0.25f;
                else
                    Player.gravity = 0.2f;
            }
        }
        DrawBlessingEffects();
    }

    private void DrawBlessingEffects()
    {
        if (Main.dedServ)
            return;

        // 淡金色呼吸光与上浮星尘持续提示祝福，避免遮挡玩家和战斗弹幕。
        float glow = TOGameTimeFunctions.GetTimeSin(0.3f, 3f) + 0.7f;
        Lighting.AddLight(Player.Center, LegendaryNightHandler.StarColor.ToVector3() * (glow * AnomalyPlayer.LegendaryNight_StarGodBlessingOpacity));
        if (Main.rand.NextBool(3))
        {
            Vector2 position = Player.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-8f, 20f));
            Vector2 velocity = Player.velocity * 0.15f + new Vector2(0f, -0.8f);
            Dust.NewDustPerfectAction(position, DustID.YellowStarDust,
                action: dust =>
                {
                    dust.velocity = velocity;
                    dust.alpha = 100;
                    dust.color = LegendaryNightHandler.StarColor;
                    dust.scale *= 0.65f;
                    dust.noGravity = true;
                    dust.noLight = true;
                });
        }
    }
}