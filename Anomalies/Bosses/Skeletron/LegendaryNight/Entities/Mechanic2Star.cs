using Anomalies.Bosses.Skeletron.LegendaryNight.Arena;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Entities;

/// <summary>半场拍击生成的帧伤落星；ai[0] 拍击序号，ai[1] 左右侧，ai[2] 主体索引。</summary>
public sealed class Mechanic2Star : AnomalyModProjectile
{
    public override string Texture => LegendaryNightHandler.LegendaryNightEntityPath + "FallingStar_Hard";
    public override string LocalizationCategory => LegendaryNightHandler.LocalizationCategory;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = SkeletronLegendHead.Mechanic2Data.StarTrailLength;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 28;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = SkeletronLegendHead.Mechanic2Data.RainDuration + 2;
        Projectile.scale = 0.22f;
    }

    public override bool? CanDamage() => false;

    public override void AI()
    {
        NPC master = NPC.GetNPCFromIndex((int)Projectile.ai[2] - 1);
        if (!master.active || master.type != NPCID.SkeletronHead || master.Anomaly.CustomAIType != 1)
        {
            Projectile.Kill();
            return;
        }
        SkeletronLegendHead head = SkeletronLegendHead.GetInstance(master);
        LegendaryNightPhase2Arena arena = head.Phase2Arena;
        if (!head.Mechanic2Active || arena is null)
        {
            Projectile.Kill();
            return;
        }
        int age = head.Mechanic2Age - SkeletronLegendHead.Mechanic2Data.GetStrikeTime((int)Projectile.ai[0]);
        if (age < 0)
            return;
        float x = Projectile.Center.X - arena.Projectile.Center.X;
        float bottom = MathF.Sqrt(Math.Max(0f, arena.Radius * arena.Radius - x * x));
        if (age >= SkeletronLegendHead.Mechanic2Data.RainDuration || Projectile.Center.Y > arena.Projectile.Center.Y + bottom + 32f)
        {
            Projectile.Kill();
            return;
        }
        Projectile.rotation += 0.14f;
        if (!Main.dedServ)
        {
            Player player = Main.LocalPlayer;
            // 高速落星使用本帧扫掠体积，避免越过玩家；半场边界仍以竞技场圆心为准。
            RotatedRectangle sweep = new(Projectile.Center + Projectile.velocity * 0.5f,
                Projectile.velocity.Length(), 24f, Projectile.velocity.ToRotation());
            bool hit = sweep.Collides(player.Hitbox);
            if (hit && Math.Sign(player.Center.X - arena.Projectile.Center.X) == (int)Projectile.ai[1]
                && player.Distance(arena.Projectile.Center) <= arena.Radius)
                LegendaryNightDamage.HurtLocalPlayer(Projectile, Projectile.damage, false);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        FallingStar.DrawTrailType2(Projectile.Center, Projectile.scale, Projectile.velocity, Projectile.oldPos, Projectile.Size * 0.5f);
        Texture2D texture = FallingStar.HardTexture;
        Vector2 origin = new(texture.Width / 2f, texture.Height / (1f + MathF.Cos(TOMathUtils.PiOver5)));
        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White,
            Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
