using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.Projectiles;

namespace CalamityAnomalies.Core;

public static class CAExtensions
{
    extension(Item item)
    {
        public CAGlobalItem Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => item?.GetGlobalItem<CAGlobalItem>(); }
        public CalamityGlobalItem Calamity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => item?.GetGlobalItem<CalamityGlobalItem>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleItemBehavior itemBehavior, [CallerMemberName] string methodName = null!) => CAEntityChangeHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(CAItemTooltipModifier modifier)
    {
        public void ApplyCATweakColorToDamage() => modifier.Modify(null, "Damage", l => l.OverrideColor = CASharedData.GetGradientColor(0.25f));
    }

    extension(NPC npc)
    {
        public CAGlobalNPC Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => npc?.GetGlobalNPC<CAGlobalNPC>(); }
        public CalamityGlobalNPC Calamity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => npc?.GetGlobalNPC<CalamityGlobalNPC>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleNPCBehavior npcBehavior, [CallerMemberName] string methodName = null!) => CAEntityChangeHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool DesertScourge => npc.ModNPC is DesertScourgeHead or DesertScourgeBody or DesertScourgeTail;

        public bool DesertNuisance => npc.ModNPC is DesertNuisanceHead or DesertNuisanceBody or DesertNuisanceTail;

        public bool DesertNuisanceYoung => npc.ModNPC is DesertNuisanceHeadYoung or DesertNuisanceBodyYoung or DesertNuisanceTailYoung;

        public bool AquaticScourge => npc.ModNPC is AquaticScourgeHead or AquaticScourgeBody or AquaticScourgeTail;

        public bool LeviathanBoss => npc.ModNPC is Leviathan or Anahita;

        public bool Ravager => npc.ModNPC is RavagerBody or RavagerClawLeft or RavagerClawRight or RavagerLegLeft or RavagerLegRight or RavagerHead or RavagerHead2;

        public bool ProfanedGuardianBoss => npc.ModNPC is ProfanedGuardianCommander or ProfanedGuardianDefender or ProfanedGuardianHealer;

        public bool ProfanedGuardianSpawned => npc.ModNPC is ProvSpawnOffense or ProvSpawnDefense or ProvSpawnHealer;

        public bool StormWeaver => npc.ModNPC is StormWeaverHead or StormWeaverBody or StormWeaverTail;

        public bool DoG => npc.ModNPC is DevourerofGodsHead or DevourerofGodsBody or DevourerofGodsTail;

        public bool Thanatos => npc.active && npc.ModNPC is ThanatosHead or ThanatosBody1 or ThanatosBody2 or ThanatosTail;

        public bool ThanatosHead => npc.ModNPC is ThanatosHead;

        public bool ExoTwins => npc.ModNPC is Artemis or Apollo;

        public bool Ares => npc.ModNPC is AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower;

        public bool ExoMechs => npc.Thanatos || npc.ExoTwins || npc.Ares;

        public void ApplyCalamityBossHealthBoost() => npc.lifeMax += (int)(npc.lifeMax * CalamityServerConfig.Instance.BossHealthBoost * 0.01f);
    }

    extension(NPC)
    {
        public static bool DownedEvilBossT2 => DownedBossSystem_Bridge.downedHiveMind || DownedBossSystem_Bridge.downedPerforator;

        public static bool Focus => DownedBossSystem_Bridge.downedExoMechs && DownedBossSystem_Bridge.downedCalamitas;
    }

    extension(Player player)
    {
        public CAPlayer Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => player?.GetModPlayer<CAPlayer>(); }
        public CalamityPlayer Calamity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => player?.GetModPlayer<CalamityPlayer>(); }
    }

    extension(Projectile projectile)
    {
        public CAGlobalProjectile Anomaly { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => projectile?.GetGlobalProjectile<CAGlobalProjectile>(); }
        public CalamityGlobalProjectile Calamity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => projectile?.GetGlobalProjectile<CalamityGlobalProjectile>(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null!) => CAEntityChangeHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}
