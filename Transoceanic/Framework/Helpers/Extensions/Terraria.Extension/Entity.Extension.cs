namespace Transoceanic.Framework.Helpers;

/// <summary>
/// 追踪算法类型。
/// </summary>
public enum HomingAlgorithm
{
    /// <summary>平滑插值。</summary>
    SmoothStep,
    /// <summary>线性插值。</summary>
    Linear,
    /// <summary>相比 <see cref="SmoothStep"/> 更加平滑的插值。</summary>
    SmootherStep,
}

public static partial class TOExtensions
{
    extension(Entity entity)
    {
        /// <summary>
        /// 尝试获取实体的 <c>type</c>。
        /// </summary>
        /// <returns>
        /// 获取的 <c>type</c> 值。
        /// <br/>对于 <see cref="NPC"/>，如果其 <see cref="NPC.netID"/> 小于0，则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
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
        public Vector2 GetVelocityTowards(Vector2 destination, float length) => (destination - entity.Center).ToCustomLength(length);

        /// <summary>
        /// 获取一个指向目标的速度向量，长度为 <paramref name="length"/>。
        /// </summary>
        public Vector2 GetVelocityTowards(Entity target, float length) => (target.Center - entity.Center).ToCustomLength(length);

        /// <summary>
        /// 使实体追踪指定地点。
        /// </summary>
        /// <param name="destination">追踪地点。</param>
        /// <param name="algorithm">采用的追踪算法类型。</param>
        /// <param name="homingRatio">追踪强度。应输入 [0, 1] 范围内的值（为0时不追踪，为1时强制追踪）。
        /// <br/>具体而言，该值指的是方法内部调用插值方法时传入的 <see langword="float"/> <c>amount</c> 参数的值。</param>
        /// <param name="maxHomingDistance">可选的最大追踪距离。如果提供，当实体与目的地的距离超过此值时，追踪失败。
        /// <br/>默认值为 <see langword="null"/>，即无追踪距离限制。</param>
        /// <param name="sightAngle">视野角度。
        /// <br/>表示以当前速度方向为中心，左右各 <c>sightAngle / 2</c> 范围的扇形视野区域。
        /// <br/>当目标方向与当前速度方向的夹角超过 <c>sightAngle / 2</c> 时，追踪失败。
        /// <br/>默认值为<see cref="MathHelper.TwoPi"/>，表示全方向视野，不进行视野限制。</param>
        /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在追踪强度不为1时有效。</param>
        /// <param name="velocityOverride">可选的速度大小覆盖值。如果提供，将使用此值作为速度大小进行计算，而不是使用实体的当前速度大小。
        /// <br/>默认值为 <see langword="null"/>，即不覆盖速度。</param>
        /// <remarks>须由具体实现决定地点确定机制。</remarks>
        /// <returns>追踪是否成功。<br/>若目的地不符合追踪条件，将返回 <see langword="false"/>。</returns>
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
        /// 使实体追踪指定目标。
        /// </summary>
        /// <param name="target">追踪目标。</param>        
        /// <param name="algorithm">采用的追踪算法类型。</param>
        /// <param name="homingRatio">追踪强度。应输入 [0, 1] 范围内的值（为0时不追踪，为1时强制追踪）。
        /// <br/>具体而言，该值指的是方法内部调用插值方法时传入的 <see langword="float"/> <c>amount</c> 参数的值。</param>
        /// <param name="maxHomingDistance">可选的最大追踪距离。如果提供，当实体与目的地的距离超过此值时，追踪失败。
        /// <br/>默认值为 <see langword="null"/>，即无追踪距离限制。</param>
        /// <param name="sightAngle">视野角度。
        /// <br/>表示以当前速度方向为中心，左右各 <c>sightAngle / 2</c> 范围的扇形视野区域。
        /// <br/>当目标方向与当前速度方向的夹角超过 <c>sightAngle / 2</c> 时，追踪失败。
        /// <br/>默认值为 <see cref="MathHelper.TwoPi"/>，表示全方向视野，不进行视野限制。</param>
        /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在追踪强度不为1时有效。</param>
        /// <param name="velocityOverride">可选的速度大小覆盖值。如果提供，将使用此值作为速度大小进行计算，而不是使用实体的当前速度大小。
        /// <br/>默认值为 <see langword="null"/>，即不覆盖速度。</param>
        /// <remarks>须由具体实现决定目标确定机制。</remarks>
        /// <returns>追踪是否成功。<br/>若 <paramref name="target"/> 处于非法状态或不符合追踪条件，将返回 <see langword="false"/>。</returns>
        public bool Homing<T>(T target, HomingAlgorithm algorithm = HomingAlgorithm.SmoothStep, float homingRatio = 1f, float? maxHomingDistance = null, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true, float? velocityOverride = null) where T : Entity =>
            target is not null && target.active && entity.Homing(target.Center, algorithm, homingRatio, maxHomingDistance, sightAngle, keepVelocity, velocityOverride);
    }
}