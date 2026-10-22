namespace Anomalies.SpecialContents.Developer.EventideReunion;

// 独立治疗逻辑，保留幽魂套的灵魂粒子表现。
public sealed class EventideReunionDeathSoul : AnomalyModProjectile
{
    public override string LocalizationCategory => EventideReunion.LocalizationCategoryForEventideReunion;
    public override string Texture => TOAssetUtils.FormatVanillaProjectileTexturePath(ProjectileID.SpiritHeal);
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
        Projectile.timeLeft = 600;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        int owner = (int)Projectile.ai[0];
        if (owner < 0 || owner >= Main.maxPlayers || !Main.player[owner].active || Main.player[owner].dead)
        {
            Projectile.Kill();
            return;
        }
        Player target = Main.player[owner];
        Projectile.velocity = EventideReunionDeathSoulMotion.Velocity(target.Center - Projectile.Center, target.velocity);
        if (!Main.dedServ)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDustPerfectAction(Projectile.Center - Projectile.velocity * (i / 5f), 175, d =>
                {
                    d.noGravity = true;
                    d.velocity = Vector2.Zero;
                    d.scale = 1.3f;
                    d.alpha = 100;
                });
        }
        // 只有灵魂所属客户端修改生命；使用原版幽魂治疗包同步，避免重复治疗。
        float collisionPoint = 0f;
        bool reached = target.Hitbox.Intersects(Projectile.Hitbox)
            || Collision.CheckAABBvLineCollision(target.position, target.Size, Projectile.Center,
                Projectile.Center + Projectile.velocity, 12f, ref collisionPoint);
        if (!Projectile.IsOnOwnerClient || !reached)
            return;
        int healing = EventideReunionDeathSoulMotion.Healing(target.statLife, target.statLifeMax2, target.moonLeech);
        if (healing > 0)
        {
            target.statLife += healing;
            target.HealEffect(healing, broadcast: false);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SpiritHeal, number: owner, number2: healing);
        }
        Projectile.Kill();
    }
}
