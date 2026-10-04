using Transoceanic.DataStructures.Particles;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Projectile projectile)
    {
        /// <summary>
        /// 获取弹幕的全局数据 <see cref="TOGlobalProjectile"/>。
        /// </summary>
        public TOGlobalProjectile Ocean => projectile?.GetGlobalProjectile<TOGlobalProjectile>();

        /// <summary>
        /// 获取弹幕的所有者玩家。
        /// </summary>
        /// <returns>所有者的 <see cref="Player"/> 实例，若索引无效则返回 <see langword="null"/>。</returns>
        public Player Owner
        {
            get
            {
                int owner = projectile.owner;
                if (owner >= 0 && projectile.owner < Main.maxPlayers)
                    return Main.player[projectile.owner];
                return null;
            }
        }

        /// <summary>
        /// 获取弹幕所关联的 <see cref="ModProjectile"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModProjectile"/> 类型。</typeparam>
        /// <returns>转换后的实例，若不存在则返回 <see langword="null"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModProjectile<T>() where T : ModProjectile => projectile?.ModProjectile as T;

        /// <summary>
        /// 获取弹幕所关联的 <see cref="ModProjectile"/> 实例，并转换为指定类型；若不存在则抛出异常。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModProjectile"/> 类型。</typeparam>
        /// <returns>转换后的实例。</returns>
        /// <exception cref="ArgumentException">当弹幕没有指定类型的 <see cref="ModProjectile"/> 时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModProjectileThrow<T>() where T : ModProjectile => projectile.GetModProjectile<T>() ?? throw new ArgumentException($"Projectile {projectile.Name} ({projectile.type}) does not have a ModProjectile of type {typeof(T).FullName}.", nameof(projectile));

        /// <summary>
        /// 尝试获取弹幕所关联的 <see cref="ModProjectile"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModProjectile"/> 类型。</typeparam>
        /// <param name="result">输出转换后的实例，成功时为有效值，否则为 <see langword="null"/>。</param>
        /// <returns>如果成功获取则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModProjectile<T>([NotNullWhen(true)] out T result) where T : ModProjectile => (result = projectile.GetModProjectile<T>()) is not null;

        /// <summary>
        /// 获取弹幕的纹理贴图。
        /// </summary>
        public Texture2D Texture => TOAssetUtils.GetProjectileTexture(projectile.type);

        /// <summary>
        /// 获取或设置弹幕的图形透明度。0 表示完全透明，255 表示完全不透明。
        /// </summary>
        public byte GraphicAlpha
        {
            get => (byte)(255 - projectile.alpha);
            set => projectile.alpha = Math.Clamp(255 - value, 0, 255);
        }

        /// <summary>
        /// 判断弹幕的所有者是否为本地客户端。
        /// </summary>
        public bool IsOnOwnerClient => projectile.owner == Main.myPlayer;

        /// <summary>
        /// 判断弹幕是否正在进行最后一次更新（<see cref="Projectile.numUpdates"/> == -1）。
        /// </summary>
        public bool IsFinalUpdate => projectile.numUpdates == -1;

        /// <summary>
        /// 设置弹幕的速度，同时自动更新其旋转角度。
        /// </summary>
        /// <param name="velocity">新的速度向量。</param>
        /// <param name="rotationOffset">旋转偏移值（弧度）。例如，对于贴图向上的弹幕，可设为 <see cref="MathHelper.PiOver2"/>。</param>
        /// <remarks>为性能考虑，不要在不改变方向的情况下重复调用此方法。</remarks>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            projectile.velocity = velocity;
            projectile.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 根据当前速度向量更新弹幕的旋转角度。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值（弧度）。</param>
        public void VelocityToRotation(float rotationOffset = 0f) => projectile.rotation = projectile.velocity.ToRotation(rotationOffset);

        /// <summary>
        /// 改变弹幕的缩放比例，并固定指定点的世界坐标不变。
        /// </summary>
        /// <param name="width">原始宽度（缩放前）。</param>
        /// <param name="height">原始高度（缩放前）。</param>
        /// <param name="newScale">新的缩放比例。</param>
        /// <param name="fixedPoint">固定点（世界坐标），缩放时此点位置不变。</param>
        public void BetterChangeScale(int width, int height, float newScale, Vector2 fixedPoint)
        {
            if (projectile.scale == newScale)
                return;

            projectile.position = Vector2.Homothetic(projectile.position, fixedPoint, newScale / projectile.scale);
            projectile.width = (int)(width * newScale);
            projectile.height = (int)(height * newScale);
            projectile.scale = newScale;
        }

        /// <summary>
        /// 执行一个飞行宠物的 AI 行为，使弹幕跟随玩家移动。
        /// </summary>
        /// <param name="reverseDitection">是否反转方向（速度向右时设置方向为 -1，而不是 1）。</param>
        /// <param name="tiltFloat">倾斜浮动值。</param>
        /// <param name="lightPet">是否为光源宠物。</param>
        public void FloatingPetAI(bool reverseDitection, float tiltFloat, bool lightPet = false)
        {
            Player player = Main.player[projectile.owner];

            //anti sticking movement as a failsafe
            float SAImovement = 0.05f;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile otherProj = Main.projectile[k];
                // Short circuits to make the loop as fast as possible
                if (!otherProj.active || otherProj.owner != projectile.owner || !Main.projPet[otherProj.type] || k == projectile.whoAmI)
                    continue;

                // If the other projectile is indeed another pet owned by the same player and they're too close, nudge them away.
                bool isPet = Main.projPet[otherProj.type];
                float taxicabDist = Math.Abs(projectile.position.X - otherProj.position.X) + Math.Abs(projectile.position.Y - otherProj.position.Y);
                if (isPet && taxicabDist < projectile.width)
                {
                    if (projectile.position.X < otherProj.position.X)
                        projectile.velocity.X -= SAImovement;
                    else
                        projectile.velocity.X += SAImovement;

                    if (projectile.position.Y < otherProj.position.Y)
                        projectile.velocity.Y -= SAImovement;
                    else
                        projectile.velocity.Y += SAImovement;
                }
            }

            float passiveMvtFloat = 0.5f;
            projectile.tileCollide = false;
            float range = 100f;
            Vector2 projPos = projectile.Center;
            float xDist = player.Center.X - projPos.X;
            float yDist = player.Center.Y - projPos.Y;
            yDist += Main.rand.NextFloat(-10, 20);
            xDist += Main.rand.NextFloat(-10, 20);
            //Light pets lead the player, normal pets trail the player
            xDist += 60f * (lightPet ? (float)player.direction : -(float)player.direction);
            yDist -= 60f;
            Vector2 playerVector = new Vector2(xDist, yDist);
            float playerDist = playerVector.Length();
            float returnSpeed = 18f;

            //If player is close enough, resume normal
            if (playerDist < range && player.velocity.Y == 0f &&
                projectile.Bottom.Y <= player.Bottom.Y &&
                !Collision.SolidCollision(projectile.position, projectile.width, projectile.height))
            {
                if (projectile.velocity.Y < -6f)
                {
                    projectile.velocity.Y = -6f;
                }
            }

            //Teleport to player if too far
            if (playerDist > 2000f)
            {
                projectile.Center = player.Center;
                projectile.netUpdate = true;
            }

            if (playerDist < 50f)
            {
                if (Math.Abs(projectile.velocity.X) > 2f || Math.Abs(projectile.velocity.Y) > 2f)
                {
                    projectile.velocity *= 0.99f;
                }
                passiveMvtFloat = 0.01f;
            }
            else
            {
                if (playerDist < 100f)
                {
                    passiveMvtFloat = 0.1f;
                }
                if (playerDist > 300f)
                {
                    passiveMvtFloat = 1f;
                }
                playerDist = returnSpeed / playerDist;
                playerVector.X *= playerDist;
                playerVector.Y *= playerDist;
            }
            if (projectile.velocity.X < playerVector.X)
            {
                projectile.velocity.X += passiveMvtFloat;
                if (passiveMvtFloat > 0.05f && projectile.velocity.X < 0f)
                {
                    projectile.velocity.X += passiveMvtFloat;
                }
            }
            if (projectile.velocity.X > playerVector.X)
            {
                projectile.velocity.X -= passiveMvtFloat;
                if (passiveMvtFloat > 0.05f && projectile.velocity.X > 0f)
                {
                    projectile.velocity.X -= passiveMvtFloat;
                }
            }
            if (projectile.velocity.Y < playerVector.Y)
            {
                projectile.velocity.Y += passiveMvtFloat;
                if (passiveMvtFloat > 0.05f && projectile.velocity.Y < 0f)
                {
                    projectile.velocity.Y += passiveMvtFloat * 2f;
                }
            }
            if (projectile.velocity.Y > playerVector.Y)
            {
                projectile.velocity.Y -= passiveMvtFloat;
                if (passiveMvtFloat > 0.05f && projectile.velocity.Y > 0f)
                {
                    projectile.velocity.Y -= passiveMvtFloat * 2f;
                }
            }
            if (projectile.velocity.X >= 0.25f)
            {
                projectile.direction = reverseDitection ? 1 : -1;
            }
            else if (projectile.velocity.X < -0.25f)
            {
                projectile.direction = reverseDitection ? -1 : 1;
            }
            //Tilting and change directions
            projectile.spriteDirection = projectile.direction;
            projectile.rotation = projectile.velocity.X * tiltFloat;
        }

        #region GlobalProjectile
        /// <summary>
        /// 获取或设置弹幕是否始终根据速度旋转。
        /// </summary>
        public bool AlwaysRotating
        {
            get => projectile.Ocean.OceanAI32[0].bits[0];
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[0].bits[0] != value)
                {
                    ocean.OceanAI32[0].bits[0] = value;
                    ocean.AIChanged32[0] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置弹幕旋转时的偏移角度。
        /// </summary>
        public float RotationOffset
        {
            get => projectile.Ocean.OceanAI32[1].f;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[1].f != value)
                {
                    ocean.OceanAI32[1].f = value;
                    ocean.AIChanged32[1] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 1（整数值）。
        /// </summary>
        public int Timer1
        {
            get => projectile.Ocean.OceanAI32[^6].i;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[^6].i != value)
                {
                    ocean.OceanAI32[^6].i = value;
                    ocean.AIChanged32[^6] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 2（整数值）。
        /// </summary>
        public int Timer2
        {
            get => projectile.Ocean.OceanAI32[^5].i;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[^5].i != value)
                {
                    ocean.OceanAI32[^5].i = value;
                    ocean.AIChanged32[^5] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 3（整数值）。
        /// </summary>
        public int Timer3
        {
            get => projectile.Ocean.OceanAI32[^4].i;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[^4].i != value)
                {
                    ocean.OceanAI32[^4].i = value;
                    ocean.AIChanged32[^4] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 4（浮点值）。
        /// </summary>
        public float Timer4
        {
            get => projectile.Ocean.OceanAI32[^3].f;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[^3].f != value)
                {
                    ocean.OceanAI32[^3].f = value;
                    ocean.AIChanged32[^3] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 5（浮点值）。
        /// </summary>
        public float Timer5
        {
            get => projectile.Ocean.OceanAI32[^2].f;
            set
            {
                TOGlobalProjectile ocean = projectile.Ocean;
                if (ocean.OceanAI32[^2].f != value)
                {
                    ocean.OceanAI32[^2].f = value;
                    ocean.AIChanged32[^2] = true;
                }
            }
        }

        /// <summary>
        /// 生成一个残影特效并添加到弹幕的全局数据中。
        /// </summary>
        /// <param name="lifetime">残影存活时间（帧数）。</param>
        /// <param name="color">残影颜色。</param>
        /// <param name="frame">可选的源矩形区域。</param>
        /// <param name="drawOffset">绘制偏移量（可选）。</param>
        /// <param name="useDefaultDraw">是否使用默认绘制方法（可选），默认为 <see langword="true"/>。</param>
        public void SpawnAfterimage(int lifetime, Color color, Rectangle? frame = null, Vector2? drawOffset = null, bool useDefaultDraw = true) =>
            projectile.Ocean.Afterimages.Add(new AfterimageParticle(projectile.Texture, frame, projectile.Center, lifetime, projectile.rotation, projectile.scale, color, projectile.Opacity, drawOffset) { UseDefaultDraw = useDefaultDraw });

        /// <summary>
        /// 添加一个自定义的残影粒子到弹幕的全局数据中。
        /// </summary>
        /// <param name="afterimage">残影粒子实例。</param>
        public void SpawnAfterimage(AfterimageParticle afterimage) => projectile.Ocean.Afterimages.Add(afterimage);
        #endregion GlobalProjectile
    }

    extension(Projectile)
    {
        /// <summary>
        /// 获取一个虚拟的“哑元”弹幕（位于索引 <see cref="Main.maxProjectiles"/> 处）。
        /// </summary>
        public static Projectile DummyProjectile => Main.projectile[Main.maxProjectiles];

        /// <summary>
        /// 根据传入的索引尝试获取 <see cref="Main.projectile"/> 数组中对应的弹幕实例。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>
        /// 弹幕实例。
        /// <br/>若索引越界或等于 <see cref="Main.maxProjectiles"/>（对应弹幕为 Dummy），返回 DummyProjectile。
        /// <br/>永不返回 <see langword="null"/>。
        /// </returns>
        public static Projectile GetProjectileFromIndex(int index) => index >= 0 && index < Main.maxProjectiles ? Main.projectile[index] : Projectile.DummyProjectile;

        /// <summary>
        /// 尝试根据传入的索引获取 <see cref="Main.projectile"/> 数组中对应的弹幕实例。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <param name="projectile">输出参数，返回对应的弹幕实例。</param>
        /// <returns>如果索引有效且对应弹幕存在，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool TryGetProjectileFromIndex(int index, [NotNullWhen(true)] out Projectile projectile)
        {
            if (index >= 0 && index < Main.maxProjectiles)
            {
                projectile = Main.projectile[index];
                return true;
            }
            else
            {
                projectile = Projectile.DummyProjectile;
                return false;
            }
        }

        /// <summary>
        /// 获取一个迭代器，用于遍历所有激活状态的弹幕。
        /// </summary>
        public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewProjectileIterator(IteratorMatches.Projectile_IsActive);

        /// <summary>
        /// 根据类型 ID 创建一个新的弹幕实例（不加入世界）。
        /// </summary>
        /// <param name="type">弹幕类型 ID。</param>
        /// <returns>新创建的弹幕实例。</returns>
        public static Projectile CreateProjectile(int type)
        {
            Projectile projectile = new();
            projectile.SetDefaults(type);
            return projectile;
        }

        /// <summary>
        /// 根据 ModProjectile 类型创建一个新的弹幕实例（不加入世界）。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <returns>新创建的弹幕实例。</returns>
        public static Projectile CreateProjectile<T>() where T : ModProjectile => CreateProjectile(ModContent.ProjectileType<T>());

        /// <summary>
        /// 根据类型 ID 创建一个新的弹幕实例，并执行一个初始化 <see cref="Action{Projectile}"/>。
        /// </summary>
        /// <param name="type">弹幕类型 ID。</param>
        /// <param name="action">对创建后的弹幕执行的行为。</param>
        /// <returns>新创建的弹幕实例。</returns>
        public static Projectile CreateProjectile(int type, Action<Projectile> action)
        {
            Projectile projectile = CreateProjectile(type);
            action?.Invoke(projectile);
            return projectile;
        }

        /// <summary>
        /// 根据 ModProjectile 类型创建一个新的弹幕实例，并执行一个初始化 <see cref="Action{Projectile}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <param name="action">对创建后的弹幕执行的行为。</param>
        /// <returns>新创建的弹幕实例。</returns>
        public static Projectile CreateProjectile<T>(Action<Projectile> action) where T : ModProjectile
        {
            Projectile projectile = CreateProjectile<T>();
            action?.Invoke(projectile);
            return projectile;
        }

        /// <summary>
        /// 生成一个新的弹幕到世界中，并在生成后执行一个 <see cref="Action{Projectile}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="type">弹幕类型 ID。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引，默认为 -1。</param>
        /// <param name="action">生成成功后对弹幕执行的行为。</param>
        public static void NewProjectileAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
            if (index < Main.maxProjectiles)
            {
                Projectile projectile = Main.projectile[index];
                projectile.velocity = velocity;
                action?.Invoke(projectile);
            }
        }

        /// <summary>
        /// 生成一个新的 ModProjectile 弹幕到世界中，并在生成后执行一个 <see cref="Action{Projectile}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引，默认为 -1。</param>
        /// <param name="action">生成成功后对弹幕执行的行为。</param>
        public static void NewProjectileAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileAction(source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成指定数量的弹幕，所有弹幕的速度关于中心方向对称分布，每个弹幕之间相差固定角度。
        /// </summary>
        /// <param name="amount">弹幕总数。</param>
        /// <param name="radian">每次递增的旋转角度（顺时针，弧度）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="centerVelocity">
        /// 中心速度向量，所有弹幕的速度大小与此向量相同。
        /// 弹幕方向以此向量为中心对称分布：
        /// 若数量为奇数，中间弹幕将沿此方向发射；两侧弹幕依次旋转 ±<paramref name="radian"/> 的整数倍。
        /// 若数量为偶数，弹幕将对称分布在中心方向两侧。
        /// </param>
        /// <param name="type">弹幕类型 ID。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">每个弹幕生成后执行的行为。</param>
        public static void NewProjectilesArc(int amount, float radian,
            IEntitySource source, Vector2 position, Vector2 centerVelocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            Vector2 velocity = centerVelocity.RotatedBy(-radian * (amount - 1) / 2f);
            for (int i = 0; i < amount; i++)
                NewProjectileAction(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的 ModProjectile 弹幕，所有弹幕的速度关于中心方向对称分布，每个弹幕之间相差固定角度。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <param name="amount">弹幕总数。</param>
        /// <param name="radian">每次递增的旋转角度（顺时针，弧度）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="centerVelocity">
        /// 中心速度向量，所有弹幕的速度大小与此向量相同。
        /// <br/>弹幕方向以此向量为中心对称分布：
        /// 若数量为奇数，中间弹幕将沿此方向发射；两侧弹幕依次旋转 ±<paramref name="radian"/> 的整数倍；
        /// 若数量为偶数，弹幕将对称分布在中心方向两侧。
        /// </param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">每个弹幕生成后执行的行为。</param>
        public static void NewProjectilesArc<T>(int amount, float radian,
            IEntitySource source, Vector2 position, Vector2 centerVelocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
            where T : ModProjectile =>
            Projectile.NewProjectilesArc(amount, radian, source, position, centerVelocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);
    }
}
