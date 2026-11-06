namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        base.SendAnomalyExtraAI(writer);
        writer.Write(_LightOrbsSpawned);
        writer.Write(AbsorbedLightOrbCount);
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            writer.Write(_PlayerBonuses[i].DamageStacks);
            writer.Write(_PlayerBonuses[i].RegenerationStacks);
            writer.Write(_PlayerBonuses[i].ExpectsPlatinum);
        }
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        base.ReceiveAnomalyExtraAI(reader);
        int orbsSpawned = reader.ReadInt32();
        int absorbedCount = reader.ReadInt32();
        LightOrbBonuses[] bonuses = new LightOrbBonuses[Main.maxPlayers];
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            bonuses[i].DamageStacks = reader.ReadInt32();
            bonuses[i].RegenerationStacks = reader.ReadInt32();
            bonuses[i].ExpectsPlatinum = reader.ReadBoolean();
        }
        bonuses.CopyTo(_PlayerBonuses, 0);
        _LightOrbsSpawned = orbsSpawned;
        AbsorbedLightOrbCount = absorbedCount;
    }
}