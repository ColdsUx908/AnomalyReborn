namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public abstract class LegendCircleElement : AnomalyModProjectile
{
    protected abstract int ItemTexture { get; }
    protected virtual bool IsCrown => false;
    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.netImportant = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.Opacity = 0f;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        if (!LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true)
            || !head.Mechanic7Active || head.Phase2MusicElapsedTime >= SkeletronLegendHead.Mechanic7Data.T9MusicTime
            || head.Phase2Arena is not { } arena)
        {
            if (TONetworkEnvironment.NotClient)
                Projectile.Kill();
            return;
        }
        int index = (int)Projectile.ai[1];
        if (index is < 2 or >= 8)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 180;
        Projectile.Center = arena.GetMechanic7Circle(index).Center;
        float inward = (arena.Projectile.Center - Projectile.Center).ToRotation();
        if (IsCrown)
            Projectile.rotation = inward - MathHelper.PiOver2;
        else
        {
            // 朝左时镜像枪身并补偿半圈，使枪口仍朝场心，枪身上侧保持朝上。
            Projectile.spriteDirection = MathF.Cos(inward) < 0f ? -1 : 1;
            Projectile.rotation = MathHelper.WrapAngle(inward
                + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f));
        }
        Projectile.Opacity = SkeletronLegendHead.Mechanic7Data.ElementOpacity(head.Phase2MusicElapsedTime, index);
    }
    public override bool PreDraw(ref Color lightColor) => false;

    // 由竞技场覆盖层在所有法阵之后统一提交。
    internal void DrawElement(SpriteBatch spriteBatch)
    {
        if (!LegendaryNightHandler.IsLegendaryNightHead((int)Projectile.ai[0], out SkeletronLegendHead head, oneBased: true)
            || !head.Mechanic7Active || (int)Projectile.ai[1] is < 2 or >= 8)
            return;
        // 绘制直接读取音乐时钟，避免生成后尚未执行AI或网络透明度尚未更新时闪现。
        float opacity = SkeletronLegendHead.Mechanic7Data.ElementOpacity(head.Phase2MusicElapsedTime, (int)Projectile.ai[1]);
        if (opacity <= 0f)
            return;
        Texture2D texture;
        if (IsCrown)
            texture = TextureAssets.Extra[ExtrasID.KingSlimeCrown].Value; // KingSlimeCrown
        else
        {
            Main.instance.LoadItem(ItemTexture);
            texture = TextureAssets.Item[ItemTexture].Value;
        }
        // 对角线也完全位于圆内，同时令武器长边接近法阵直径。
        float scale = SkeletronLegendHead.Mechanic7Data.CircleRadius * 1.9f / texture.Size().Length();
        spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * opacity,
            Projectile.rotation, texture.Size() * 0.5f, scale,
            !IsCrown && Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }
}

public sealed class LegendCircleCrown : LegendCircleElement
{
    protected override int ItemTexture => 0;
    protected override bool IsCrown => true;
}

public sealed class LegendCircleStarCannon : LegendCircleElement
{
    protected override int ItemTexture => ItemID.StarCannon;
}

public sealed class LegendCircleCoinGun : LegendCircleElement
{
    protected override int ItemTexture => ItemID.CoinGun;
}
