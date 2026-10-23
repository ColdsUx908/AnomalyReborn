namespace Anomalies.Common;

public static class SlimeGelMotion
{
    public static bool Gel_OnTileCollide(Projectile projectile, Vector2 oldVelocity, int dustID, Action<Dust> dustAction)
    {
        for (int i = 0; i < 10; i++)
            Dust.NewDustPerfectAction(projectile.Center, dustID, dustAction);

        ref float bounceCount = ref projectile.ai[0]; // 当前已碰撞次数
        bounceCount++;

        if (bounceCount < 2f)
        {
            if (projectile.velocity.X != oldVelocity.X)
                projectile.velocity.X = -oldVelocity.X * 0.4f; // 反向并衰减至 40%

            if (projectile.velocity.Y != oldVelocity.Y)
            {
                projectile.velocity.Y = -oldVelocity.Y * 0.8f; // 反向并衰减至 80%
                projectile.velocity.X = oldVelocity.X * 0.8f;
            }

            return false; // 弹射物不销毁，继续存在
        }
        else
        {
            for (int j = 0; j < 15; j++)
            {
                Dust.NewDustPerfectAction(projectile.Center, dustID, d =>
                {
                    d.velocity = Main.rand.NextPolarVector2(1f, 15f); //速度范围更大
                    d.noGravity = true;
                });
            }

            return true; // 销毁弹射物
        }
    }

    public static void Gel_AI(Projectile projectile)
    {
        projectile.velocity.X *= 0.996f;
        projectile.velocity.Y += 0.15f;
        if (projectile.velocity.Y > 15f)
            projectile.velocity.Y = 15f;

        projectile.rotation += 0.05f;
    }

}
