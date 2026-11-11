using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Parameters;
using static Anomalies.Bosses.Skeletron.LegendaryNight.SkeletronLegendHead.Mechanic6Timeline;

namespace Anomalies.Bosses.Skeletron.LegendaryNight;

public sealed partial class LegendEnergySphere
{
    /// <summary>小球抵达球心时仅由服务器/单人端触发一次；不含拦截或退场取消。</summary>
    public event Action<LegendLightOrb> LightOrbAbsorbed;

    internal struct LightOrbBonuses
    {
        public int DamageStacks;
        public int RegenerationStacks;
        public bool ExpectsPlatinum;
        // 仅服务器进行拦截判定，客户端只同步奖励和下次颜色。
        public bool HasAcceptedOrb;

        public bool ApplyCollision(int color)
        {
            if (HasAcceptedOrb && (color == 1) != ExpectsPlatinum)
            {
                DamageStacks--;
                RegenerationStacks--;
                return false;
            }
            if (color == 1)
                RegenerationStacks++;
            else
                DamageStacks++;
            HasAcceptedOrb = true;
            ExpectsPlatinum = color == 0;
            return true;
        }
    }

    internal void RecordLightOrbAbsorption()
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        AbsorbedLightOrbCount++;
        Projectile.netUpdate = true;
    }

    internal void OnLightOrbAbsorbed(LegendLightOrb orb)
    {
        if (!TONetworkEnvironment.NotClient)
            return;
        const int StarCount = 50;
        for (int i = 0; i < StarCount; i++)
        {
            Projectile.NewProjectileAction<FallingStar>(SourceAI, Projectile.Center,
                Main.rand.NextPolarVector2(4f, 6f), SkeletronLegendHead.FallingStarDamage, 0f,
                action: p =>
                {
                    p.ai[2] = Projectile.ai[0];
                    p.scale = 0.2f;
                    p.Resize(16, 16);
                    p.timeLeft = 600;
                    FallingStar star = p.GetModProjectile<FallingStar>();
                    star.TrailType = 1; // 既有的蓝白光尾。
                    star.AccelerationStartTime = 300;
                    p.netUpdate = true;
                });
        }
        LightOrbAbsorbed?.Invoke(orb);
    }

    internal bool InterceptLightOrb(int playerIndex, int color)
    {
        if (!TONetworkEnvironment.NotClient)
            return false;
        bool correct = _PlayerBonuses[playerIndex].ApplyCollision(color);
        //if (!correct)
        //    SpawnLasers(PunishmentLaserCount, punishment: true);
        Projectile.netUpdate = true;
        return correct;
    }

    internal void SpawnLasers(int count, bool punishment = false)
    {
        if (!TONetworkEnvironment.NotClient || Head is not SkeletronLegendHead head || head.Phase2Arena is not LegendaryNightPhase2Arena arena)
            return;
        int time = head.Phase2MusicElapsedTime;
        // 全轮共用偏移，保留束间夹角约束，同时消除固定扇区边界上的方向空缺。
        float angleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        for (int sector = 0; sector < count; sector++)
        {
            float angle = GetLaserAngle(sector, count, Main.rand.NextFloat()) + angleOffset;
            float turnAngle = GetLaserTurnAngle(Main.rand.NextFloat());
            Projectile.NewProjectileAction<LegendEnergyLaser>(SourceAI, arena.Projectile.Center,
                Vector2.Zero, SkeletronLegendHead.EnergyLaserDamage, 0f, action: p =>
                {
                    p.ai[0] = Projectile.ai[0];
                    p.ai[1] = angle;
                    p.ai[2] = time;
                    LegendEnergyLaser laser = p.GetModProjectile<LegendEnergyLaser>();
                    laser.TurnAngle = turnAngle;
                    laser.Punishment = punishment;
                    p.netUpdate = true;
                });
        }
    }

    internal LightOrbBonuses GetBonuses(int playerIndex) => _PlayerBonuses[playerIndex];
}