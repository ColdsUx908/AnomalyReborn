namespace Anomalies.Bosses.Skeletron.LegendaryNight;

/// <summary>
/// 在覆盖层原位置收集攻击实体，保持实体槽位顺序和全部 SpriteBatch 边界。
/// </summary>
internal static class LegendaryNightAttackDrawing
{
    private static readonly List<LegendEnergySphere> _Spheres = [];
    private static readonly List<LegendBlueStar> _BlueStars = [];
    private static readonly List<LegendLightOrb> _LightOrbs = [];
    private static readonly List<LegendCompanionShockwave> _Shockwaves = [];
    private static readonly List<LegendCircleElement> _Elements = [];

    internal static void Draw(SpriteBatch spriteBatch, SkeletronLegendHead head)
    {
        if (head is null)
            return;
        try
        {
            Collect(head.NPC);
            // 恒星完整系统全部绘制完，再按冲击波实体顺序分别绘制爆炸和碎屑。
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (LegendEnergySphere sphere in _Spheres)
                sphere.DrawSystem(spriteBatch, _BlueStars, _LightOrbs);
            foreach (LegendCompanionShockwave wave in _Shockwaves)
            {
                wave.DrawExplosion(spriteBatch);
                wave.DrawDebris(spriteBatch);
            }
            spriteBatch.End();

            if (!head.Mechanic7Active)
                return;
            // 手部先于所有元素载体；保留独立于恒星系统的原有批次。
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (NPC npc in NPC.ActiveNPCs)
            {
                if (SkeletronLegendHand.IsOwnedBy(npc, head.NPC))
                    SkeletronLegendHand.GetInstance(npc).DrawCircleHand(spriteBatch);
            }
            foreach (LegendCircleElement element in _Elements)
                element.DrawElement(spriteBatch);
            spriteBatch.End();
        }
        finally
        {
            // 只缓存本次绘制的引用，不跨帧持有已死亡或被复用的实体。
            _Spheres.Clear();
            _BlueStars.Clear();
            _LightOrbs.Clear();
            _Shockwaves.Clear();
            _Elements.Clear();
        }
    }

    private static void Collect(NPC head)
    {
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            switch (projectile.ModProjectile)
            {
                case LegendEnergySphere sphere when sphere.Head?.NPC == head:
                    _Spheres.Add(sphere);
                    break;
                case LegendBlueStar blue:
                    _BlueStars.Add(blue);
                    break;
                case LegendLightOrb orb:
                    _LightOrbs.Add(orb);
                    break;
                case LegendCompanionShockwave wave when wave.IsOwnedBy(head):
                    _Shockwaves.Add(wave);
                    break;
                case LegendCircleElement element when element.IsOwnedBy(head):
                    _Elements.Add(element);
                    break;
            }
        }
    }
}