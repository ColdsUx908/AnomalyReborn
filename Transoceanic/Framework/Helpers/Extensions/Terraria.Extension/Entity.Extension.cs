namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 追踪算法类型。
/// </summary>
public enum HomingAlgorithm
{
    /// <summary>平滑插值（使用 <see cref="Vector2.SmoothStep"/>）。</summary>
    SmoothStep,
    /// <summary>线性插值（使用 <see cref="Vector2.Lerp"/>）。</summary>
    Linear,
    /// <summary>相比 <see cref="SmoothStep"/> 更加平滑的插值（使用 <see cref="TOExtensions.SmootherStep(Vector2, Vector2, float, bool)"/>）。</summary>
    SmootherStep,
}

public static partial class TOExtensions
{
    extension(Entity entity)
    {
        /// <summary>
        /// 尝试获取实体的实际类型标识。
        /// </summary>
        /// <returns>
        /// 实体的类型值：
        /// <br/>- 对于 <see cref="NPC"/>：若 <see cref="NPC.netID"/> 小于 0 则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。
        /// <br/>- 对于 <see cref="Projectile"/>：返回 <see cref="Projectile.type"/>。
        /// <br/>- 对于 <see cref="Item"/>：返回 <see cref="Item.type"/>。
        /// <br/>- 对于 <see cref="Player"/>：返回 -1。
        /// <br/>- 其他类型：返回 -1。
        /// </returns>
        /// <exception cref="ArgumentException">当实体类型不被支持时抛出（理论上不会发生）。</exception>
        public int EntityType => entity switch
        {
            NPC npc => npc.netID < 0 ? npc.netID : npc.type,
            Projectile projectile => projectile.type,
            Item item => item.type,
            Player => -1,
            _ => -1
        };

        /// <summary>
        /// 获取一个指向目的地的速度向量，长度为 <paramref name="length"/>。
        /// </summary>
        /// <param name="destination">目标地点坐标。</param>
        /// <param name="length">所需速度向量的长度。</param>
        /// <returns>从实体中心指向目的地的单位向量乘以 <paramref name="length"/>。</returns>
        public Vector2 GetVelocityTowards(Vector2 destination, float length) => (destination - entity.Center).ToCustomLength(length);

        /// <summary>
        /// 获取一个指向目标实体的速度向量，长度为 <paramref name="length"/>。
        /// </summary>
        /// <param name="target">目标实体。</param>
        /// <param name="length">所需速度向量的长度。</param>
        /// <returns>从实体中心指向目标实体中心的单位向量乘以 <paramref name="length"/>。</returns>
        public Vector2 GetVelocityTowards(Entity target, float length) => (target.Center - entity.Center).ToCustomLength(length);

        /// <summary>
        /// 使实体追踪指定地点。
        /// </summary>
        /// <param name="destination">追踪的目标地点坐标。</param>
        /// <param name="algorithm">采用的追踪算法类型。</param>
        /// <param name="homingRatio">追踪强度，取值范围 [0,1]。0 表示不追踪，1 表示直接指向目标。该值作为插值方法的 amount 参数。</param>
        /// <param name="maxHomingDistance">最大追踪距离。若实体与目标距离超过此值，追踪失败。为 <c>null</c> 时不限制。</param>
        /// <param name="sightAngle">视野角度（弧度）。以当前速度方向为中心，左右各 <c>sightAngle/2</c> 的扇形区域。默认为 <see cref="MathHelper.TwoPi"/>（全方向）。</param>
        /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在 <paramref name="homingRatio"/> 不为 1 时有效。</param>
        /// <param name="velocityOverride">覆盖的速度大小。若提供，则使用此值作为速度大小进行计算，而非实体当前速度大小。</param>
        /// <returns>追踪是否成功。若距离超限或超出视野则返回 <see langword="false"/>。</returns>
        public bool Homing(Vector2 destination, HomingAlgorithm algorithm = HomingAlgorithm.SmoothStep, float homingRatio = 1f, float? maxHomingDistance = null, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true, float? velocityOverride = null)
        {
            homingRatio = MathHelper.Clamp(homingRatio, 0f, 1f);
            Vector2 distanceVector = destination - entity.Center;
            float distance = distanceVector.Length();

            if ((maxHomingDistance is not null && distance > maxHomingDistance) || (sightAngle != MathHelper.TwoPi && Vector2.IncludedAngle(entity.velocity, distanceVector) > sightAngle / 2f))
                return false;

            float velocityLength = velocityOverride ?? entity.velocity.Length();
            Vector2 distanceVector2 = distanceVector.ToCustomLength(velocityLength);
            if (homingRatio == 1f)
                entity.velocity = distance < velocityLength ? distanceVector : distanceVector2;
            else
            {
                Vector2 newVelocity = algorithm switch
                {
                    HomingAlgorithm.SmoothStep => Vector2.SmoothStep(entity.velocity, distanceVector2, homingRatio),
                    HomingAlgorithm.Linear => Vector2.Lerp(entity.velocity, distanceVector2, homingRatio),
                    HomingAlgorithm.SmootherStep => Vector2.SmootherStep(entity.velocity, distanceVector2, homingRatio),
                    _ => Vector2.SmoothStep(entity.velocity, distanceVector2, homingRatio)
                };
                entity.velocity = newVelocity;
                if (keepVelocity)
                    entity.velocity.Modulus = velocityLength;
            }

            return true;
        }

        /// <summary>
        /// 使实体追踪指定目标实体。
        /// </summary>
        /// <typeparam name="T">目标实体类型，必须继承自 <see cref="Entity"/>。</typeparam>
        /// <param name="target">追踪的目标实体。</param>
        /// <param name="algorithm">采用的追踪算法类型。</param>
        /// <param name="homingRatio">追踪强度，取值范围 [0,1]。</param>
        /// <param name="maxHomingDistance">最大追踪距离。为 <c>null</c> 时不限制。</param>
        /// <param name="sightAngle">视野角度（弧度），默认为全方向。</param>
        /// <param name="keepVelocity">是否保持速度大小。</param>
        /// <param name="velocityOverride">覆盖的速度大小。</param>
        /// <returns>追踪是否成功。若目标实体无效（<c>null</c> 或未激活）或追踪失败则返回 <see langword="false"/>。</returns>
        public bool Homing<T>(T target, HomingAlgorithm algorithm = HomingAlgorithm.SmoothStep, float homingRatio = 1f, float? maxHomingDistance = null, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true, float? velocityOverride = null) where T : Entity =>
            target is not null && target.active && entity.Homing(target.Center, algorithm, homingRatio, maxHomingDistance, sightAngle, keepVelocity, velocityOverride);
    }
}