namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class SkeletronLegendHand
{
    internal void DrawCircleHand(SpriteBatch spriteBatch)
    {
        if (!DetachedFromArms(NPC) || NPC.hide)
            return;
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(Master);
        int index = NPC.ai[0] < 0f ? 0 : 1;
        float opacity = SkeletronLegendHead.Mechanic7Parameters.ElementOpacity(head.Phase2MusicElapsedTime, index);
        if (opacity <= 0f)
            return;
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        float scale = SkeletronLegendHead.Mechanic7Parameters.CircleRadius * 1.9f / NPC.frame.Size().Length();
        spriteBatch.Draw(texture, NPC.Center + NPC.netOffset - Main.screenPosition, NPC.frame,
            Color.White * opacity, NPC.rotation, NPC.frame.Size() * 0.5f, scale,
            NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }
    internal static bool DetachedFromArms(NPC npc) => npc.Anomaly.CustomAIType == 1
        && npc.TryGetMaster(NPCID.SkeletronHead, out NPC master)
        && LegendaryNightHandler.IsLegendaryNightHead(master)
        && SkeletronLegendHead.GetInstance(master) is SkeletronLegendHead { Mechanic7Active: true } head
        && head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic7Timeline.CircleStartMusicTime;

    /// <summary>匹配原版 type 36 的贴图底部对齐；调用方负责绘制作用域中的网络偏移。</summary>
    internal static Vector2 GetAttachedHandDrawCenter(NPC hand) =>
        new(hand.Center.X, hand.Bottom.Y - hand.frame.Height * hand.scale * 0.5f + 4f + hand.gfxOffY);

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.hide) return false;
        if (DetachedFromArms(NPC))
            return false;
        // 原版骨节与血尘生成先执行；血尘由独立列表在本体的 Z 投影作用域内绘制。
        AnomalyNPC.Skeletron_HandBloodDust?.UpdateAndDraw(spriteBatch, -screenPos, NPC.Opacity);
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || !LegendaryNightHandler.IsLegendaryNightHead(master))
            return true;
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        if (!head.Mechanic2InBackground)
            return true;
        float size = GetMechanic2VisualScale(head);
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        // DrawNPCs 已将 netOffset 临时加入 NPC.position；骨节和手掌必须使用同一坐标。
        spriteBatch.Draw(texture, GetAttachedHandDrawCenter(NPC) - screenPos, NPC.frame, Color.White * NPC.Opacity,
            NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale * size,
            NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        return false;
    }

    private float GetMechanic2VisualScale(SkeletronLegendHead head)
    {
        int strike = SkeletronLegendHead.Mechanic2Parameters.GetStrikeIndex(head.Mechanic2Age);
        bool selected = head.Mechanic2HandPattern != 0 && -Math.Sign(NPC.ai[0]) == head.Mechanic2StrikeSide(strike);
        int relativeAge = head.Mechanic2Age - SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime(strike);
        float growth = SkeletronLegendHead.Mechanic2Parameters.HandRaiseProgress(relativeAge);
        float recovery = 1f - MathHelper.SmoothStep(0f, 1f,
            MathHelper.Clamp((head.Mechanic2Age - SkeletronLegendHead.Mechanic2Parameters.ReturnStartAge) / (float)SkeletronLegendHead.Mechanic2Parameters.ReturnDuration, 0f, 1f));
        return selected ? 1f + 4f * growth * recovery * SkeletronLegendHead.Mechanic2Parameters.HandAttackProgress(head.Mechanic2Age) : 1f;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        if (NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master)
            && LegendaryNightHandler.IsLegendaryNightHead(master) && SkeletronLegendHead.GetInstance(master) is SkeletronLegendHead { Mechanic2InBackground: true } head)
        {
            // 原版 hover box 不知道拍击手的额外放大；先匹配手的视觉尺寸，再由通用系统投影 Z。
            // HoverOverNPCs 已临时加入 netOffset，这里不再重复加入。
            boundingBox = Utils.CenteredRectangle(GetAttachedHandDrawCenter(NPC), NPC.frame.Size() * NPC.scale * GetMechanic2VisualScale(head));
        }
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master)
            || !LegendaryNightHandler.IsLegendaryNightHead(master) || SkeletronLegendHead.GetInstance(master) is not SkeletronLegendHead { Mechanic2InBackground: true } head)
            return null;
        float halfHeight = NPC.frame.Height * NPC.scale * GetMechanic2VisualScale(head) * 0.5f;
        position = GetAttachedHandDrawCenter(NPC) + new Vector2(0f, hbPosition == 1 ? halfHeight + 10f : -halfHeight - 24f);
        return true;
    }

    private float AttackFlashIntensity
    {
        get
        {
            if (!NPC.TryGetMaster(NPCID.SkeletronHead, out NPC master) || !LegendaryNightHandler.IsLegendaryNightHead(master))
                return 0f;
            SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
            if (head.StellarMechanicsActive || head.Mechanic5Active
                && head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic6Timeline.HandPrepareStartMusicTime)
                return 0f;
            if (head.Mechanic5Active)
            {
                int attack = SkeletronLegendHead.Mechanic5Parameters.GetHandAttackIndex(head.Phase2MusicElapsedTime);
                if (attack < 0)
                    return 0f;
                int remaining = SkeletronLegendHead.Mechanic5Parameters.GetHandStrikeTime(attack) - head.Phase2MusicElapsedTime;
                return remaining > 0 ? MathF.Pow(MathF.Sin(remaining * MathHelper.Pi
                    / SkeletronLegendHead.Mechanic5Parameters.HandWindupDuration), 2f) : 0f;
            }
            if (head.Mechanic2Active)
            {
                int strike = SkeletronLegendHead.Mechanic2Parameters.GetStrikeIndex(head.Mechanic2Age);
                int remaining = SkeletronLegendHead.Mechanic2Parameters.GetStrikeTime(strike) - head.Mechanic2Age;
                return head.Mechanic2HandPattern != 0 && -Math.Sign(NPC.ai[0]) == head.Mechanic2StrikeSide(strike)
                    && remaining > 0 && remaining < 45 ? MathF.Sin(remaining * MathHelper.Pi / 45f) : 0f;
            }
            int timer = head.Timer1 % AttackCycleDuration;
            if (!head.Phase2 || head.Mechanic1Rotating || timer < WindupStart || timer >= StrikeTime)
                return 0f;
            return MathF.Pow(MathF.Sin((timer - WindupStart) / (float)(StrikeTime - WindupStart) * MathHelper.Pi), 2f);
        }
    }

    public override Color? GetAlpha(Color drawColor)
    {
        float flash = AttackFlashIntensity;
        return flash > 0f ? Color.Lerp(drawColor, Color.White, flash) : null;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.hide) return;
        float flash = AttackFlashIntensity;
        if (flash <= 0f)
            return;

        // 全亮环境下仍可见的叠加预警，不再依赖实体取色变亮。
        Vector2 position = GetAttachedHandDrawCenter(NPC) - screenPos;
        Texture2D bloom = BloomParticle.BloomCircleLarge;
        Color gold = (LegendaryNightHandler.StarColor with { A = 0 }) * (flash * NPC.Opacity);
        Color white = (Color.White with { A = 0 }) * (flash * NPC.Opacity);
        float size = NPC.scale * MathHelper.Lerp(90f, 170f, flash);
        spriteBatch.DrawFromCenter(bloom, position, null, gold * 0.65f, 0f, size / bloom.Width);

        Texture2D rays = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        Vector2 rayScale = new(14f * NPC.scale / rays.Width, size / rays.Height);
        spriteBatch.Draw(rays, position, null, white * 0.9f, 0f, rays.Size() * 0.5f, rayScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(rays, position, null, white * 0.9f, MathHelper.PiOver2, rays.Size() * 0.5f, rayScale, SpriteEffects.None, 0f);
    }
}