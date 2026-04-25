// Designed by ColdsUx

using Transoceanic.DataStructures.Particles;

namespace Transoceanic.Common.SingleBehaviors;

public sealed class NPCMisc : TOGlobalNPCBehavior
{
    public override decimal Priority => 500m;

    public override bool PreAI(NPC npc)
    {
        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (npc.AlwaysRotating)
            npc.VelocityToRotation(npc.RotationOffset);

        TOGlobalNPC ocean = npc.Ocean;

        foreach (AfterimageParticle afterimage in ocean.Afterimages)
            ParticleHandler.UpdateParticle(afterimage);
        ocean.Afterimages.RemoveAll(a => a.Timer >= a.Lifetime);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (AfterimageParticle afterimage in npc.Ocean.Afterimages)
            afterimage.Draw(spriteBatch);

        return true;
    }
}
