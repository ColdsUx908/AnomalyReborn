using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed class QueenBeeStinger_Anomaly : AnomalyProjectileBehavior<QueenBeeStinger_Anomaly>
{
    public override int ApplyingType => ProjectileID.QueenBeeStinger;

    public override void SetDefaults()
    {
        Projectile.ignoreWater = true;
    }
}
