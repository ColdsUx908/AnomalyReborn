using Anomalies.Assets.Effects;
using Anomalies.Bosses.Skeletron.LegendaryNight.Summoning;
using Anomalies.Bosses.Skeletron.LegendaryNight.Arena;
using Anomalies.Visuals.BossBar;
using Anomalies.DataStructures;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHead : AnomalyNPCBehavior<SkeletronLegendHead>
{
    public enum Phase : byte
    {
        Initialize,
        Phase1,
        Phase2,
    }

    public enum BaseBehavior : byte
    {
        Despawn = byte.MaxValue,

        None = 0,

        Normal,
        Rotate,
    }

    public enum Spell : byte
    {
        None = 0,

        Slime,
        Star,
        PlatinumCoin,
    }

    public const float DespawnDistance = 10000f;

    private static readonly ProjectileDamageContainer _SkullDamage = new(60);
    public static int SkullDamage => _SkullDamage.Value;

    private static readonly ProjectileDamageContainer _SlimeMountDamage = new(60);
    public static int SlimeMountDamage => _SlimeMountDamage.Value;

    private static readonly ProjectileDamageContainer _MeteorDamage = new(60);
    public static int MeteorDamage => _MeteorDamage.Value;

    private static readonly ProjectileDamageContainer _FallingStarDamage = new(60);
    public static int FallingStarDamage => _FallingStarDamage.Value;

    private static readonly ProjectileDamageContainer _PlatinumCoinDamage = new(60);
    public static int PlatinumCoinDamage => _PlatinumCoinDamage.Value;

    private static readonly ProjectileDamageContainer _ArenaDamage = new(60);
    public static int ArenaDamage => _ArenaDamage.Value;

    // AnomalyAI32[0].i：竞技场弹幕索引。
    public Projectile ArenaProjectile
    {
        get => Projectile.GetProjectileFromIndex(AnomalyNPC.AnomalyAI32[0].i);
        set
        {
            int index = value?.whoAmI ?? -1;
            if (AnomalyNPC.AnomalyAI32[0].i != index)
            {
                AnomalyNPC.AnomalyAI32[0].i = index;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }

    public bool ArenaProjectileAlive => ArenaProjectile.active && ArenaProjectile.ModProjectile is LegendaryNightArena arena && arena.Master == NPC;
    public LegendaryNightArena ArenaModProjectile => ArenaProjectile.GetModProjectile<LegendaryNightArena>();

    public Phase CurrentPhase
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Phase)union.byte0;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public bool Phase2 => CurrentPhase >= Phase.Phase2;

    /*
    public bool ShouldEnterPhase2 => Ultra && NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && !Phase2;
    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;
    public bool Phase2_2 => CurrentPhase == Phase.Phase2_2;

    public float LifeRatioForPhase2 => Math.Min(NPC.LifeRatio * 2f, 1f);
    public float LostLifeRatioForPhase2 => 1f - LifeRatioForPhase2;
    */

    public BaseBehavior CurrentBaseBehavior
    {
        get
        {
            Union32 union = AI_Union_0;
            return (BaseBehavior)union.byte1;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte1 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public Spell CurrentSpell
    {
        get
        {
            Union32 union = AI_Union_0;
            return (Spell)union.byte2;
        }
        set
        {
            Union32 union = AI_Union_0;
            union.byte2 = (byte)value;
            AI_Union_0 = union;
        }
    }

    // byte0：符卡池剩余位掩码；byte2：首次循环是否已结束。
    public bool FirstSpellCycleCompleted
    {
        get => AI_Union_2.byte2 != 0;
        set
        {
            Union32 union = AI_Union_2;
            union.byte2 = (byte)(value ? 1 : 0);
            AI_Union_2 = union;
        }
    }

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool ShouldProcess
    {
        get
        {
            if (!base.ShouldProcess)
                return false;

            if (AnomalyNPC?.CustomAIType == 1)
                return true;

            //保险机制：在SetDefaults等环节使用，检测当前是否有召唤仪式处于召唤状态
            foreach (Projectile p in Projectile.ActiveProjectiles)
            {
                if (p.ModProjectile is LegendaryNightRitual modP && modP.IsSummoningSkeletron)
                    return true;
            }

            return false;
        }
    }

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaultsFinal()
    {
        ArenaProjectile = Projectile.DummyProjectile;
        NPC.lifeMax = 15000;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
        CurrentAvatar = Avatar.None;
        AvatarTimer = 0;
        AvatarRotation = 0f;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        const float VanillaExpertLifeMultiplier = 2f;
        NPC.lifeMax = (int)MathF.Round(NPC.lifeMax / (VanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
    }

    public override bool PreAI()
    {
        // 默认关闭，由需要竞技场的符卡在本帧显式激活。
        if (ArenaProjectileAlive)
            ArenaModProjectile.IsActivated = false;

        if (CurrentBaseBehavior == BaseBehavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            UpdateAvatar(Avatar.None);
            NPC.active = false;
            NPC.netUpdate = true;
            return false;
        }

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                Timer1++;

                if (Timer1 >= 60)
                {
                    Timer1 = 0;
                    CurrentPhase = Phase.Phase1;
                }
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
        }

        UpdateAvatar(RequestedAvatar);
        if (CurrentAvatar == Avatar.None)
            NPC.Opacity = 1f;
        if (CurrentAvatar != Avatar.None)
        {
            Color lightColor = CurrentAvatar switch
            {
                Avatar.FlyingSlime => LegendaryNightHandler.SlimeColor,
                Avatar.Star => LegendaryNightHandler.StarColor,
                Avatar.PlatinumCoin => LegendaryNightHandler.PlatinumColor,
                _ => LegendaryNightHandler.BoneColor
            };
            Lighting.AddLight(NPC.Center, lightColor.ToVector3());
        }
        return false;
    }

    public override void ApplyCustomMainBossBarShader(BossHealthBar newBar, SpriteBatch spriteBatch, Rectangle destinationRentangle)
    {
        AnomalyEffects.BossBars.Skeletron.Data
            .UseImage1(AnomalyTextures.Noise._Vein)
            .SetCustomParameter("uScreenResolution", Main.ScreenSize.ToVector2() * Math.Max(Main.UIScale, 1f) / 2f)
            .SetCustomParameter("uPosition", destinationRentangle.TopLeft())
            .Apply();
    }
}
