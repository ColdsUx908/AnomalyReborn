using Transoceanic.GlobalInstances;
using Transoceanic.Graphics.Particles;
using Transoceanic.Graphics.Particles.BuiltIn;

namespace Transoceanic.Behaviors.NPCs.BuiltIn;

/// <summary>
/// 维护 NPC 按速度旋转、残影更新与绘制。
/// </summary>
internal sealed class NPCMisc : GlobalNPCBehavior
{
    /// <inheritdoc/>
    /// <remarks>此实现返回 <c>500m</c>。</remarks>
    public override decimal Priority => 500m;

    /// <inheritdoc/>
    /// <remarks>
    /// 返回 true，允许继续执行 NPC AI。
    /// </remarks>
    public override bool PreAI(NPC npc)
    {
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 按开关让 NPC 朝速度旋转，并更新残影、移除超期残影。
    /// </remarks>
    public override void PostAI(NPC npc)
    {
        if (npc.AlwaysRotating)
            npc.VelocityToRotation(npc.RotationOffset);

        TOGlobalNPC ocean = npc.Ocean;

        if (Main.dedServ || ocean.ActiveAfterimages is not { } afterimages)
            return;
        foreach (AfterimageParticle afterimage in afterimages)
            ParticleHandler.UpdateParticle(afterimage);
        afterimages.RemoveAll(a => a.Timer > a.Lifetime);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 绘制启用默认绘制的 NPC 残影，再返回 true 允许绘制 NPC。
    /// </remarks>
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Main.dedServ || npc.Ocean.ActiveAfterimages is not { } afterimages)
            return true;
        foreach (AfterimageParticle afterimage in afterimages)
        {
            if (afterimage.UseDefaultDraw)
                afterimage.Draw(spriteBatch);
        }

        return true;
    }
}

