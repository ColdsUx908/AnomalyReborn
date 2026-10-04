using Anomalies.Assets.Effects;
using Anomalies.Visuals.BossBar;

namespace Anomalies.Bosses.Skeletron.SkeletronLegend;

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

        NormalMovement,
        RotatingMovement,
    }

    public enum Spell : byte
    {
        None = 0,
    }

    public const float DespawnDistance = 10000f;

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

    /*
    public bool ShouldEnterPhase2 => Ultra && NPC.LifeRatio < Phase2LifeRatio;
    public bool InvalidPhase1 => ShouldEnterPhase2 && !Phase2;
    public bool Phase1_2 => CurrentPhase == Phase.Phase1_2;
    public bool Phase2 => CurrentPhase is Phase.Phase2 or Phase.Phase2_2;
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

    public int CurrentLocalPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool LegendFlag
    {
        get => AnomalyNPC.SpecialFlags[0];
        set => AnomalyNPC.SpecialFlags[0] = value;
    }

    public override int ApplyingType => NPCID.SkeletronHead;

    public override bool AllowCalamityLogic(CalamityLogicType_NPCBehavior type) => type switch
    {
        CalamityLogicType_NPCBehavior.VanillaOverrideAI => false,
        _ => true,
    };

    public override void SetDefaultsFinal()
    {
        NPC.lifeMax = 15000;
        BridgeUtils.ApplyCalamityHealthBoost(NPC);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        const float VanillaExpertLifeMultiplier = 2f;
        NPC.lifeMax = (int)MathF.Round(NPC.lifeMax / (VanillaExpertLifeMultiplier * 1.5f) / bossAdjustment / 10f) * 10;
    }

    public override bool PreAI()
    {
        if (CurrentBaseBehavior == BaseBehavior.Despawn || !NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.active = false;
            NPC.netUpdate = true;
            return false;
        }

        LegendFlag = true;

        switch (CurrentPhase)
        {
            case Phase.Initialize:
                CurrentPhase = Phase.Phase1;
                break;
            case Phase.Phase1:
                Phase1AI();
                break;
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
