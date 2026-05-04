// Developed by ColdsUx

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
        /// <returns>所有者的 <see cref="Player"/> 实例，若索引无效则返回 <c>null</c>。</returns>
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
        /// <returns>转换后的实例，若不存在则返回 <c>null</c>。</returns>
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
        /// <param name="result">输出转换后的实例，成功时为有效值，否则为 <c>null</c>。</param>
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
        /// <param name="drawOffset">可选的绘制偏移量。</param>
        public void SpawnAfterimage(int lifetime, Color color, Rectangle? frame = null, Vector2? drawOffset = null) =>
            projectile.Ocean.Afterimages.Add(new AfterimageParticle(projectile.Texture, frame, projectile.Center, lifetime, projectile.rotation, projectile.scale, color, projectile.Opacity, drawOffset));

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
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
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
        /// 生成一个新的弹幕到世界中，并在生成后执行一个 <see cref="Action{Projectile}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出弹幕在 <see cref="Main.projectile"/> 中的索引。</param>
        /// <param name="projectile">输出弹幕实例，生成失败时为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="type">弹幕类型 ID。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">生成成功后对弹幕执行的行为。</param>
        /// <returns>如果生成成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool NewProjectileActionCheck(out int index, [NotNullWhen(true)] out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
            if (index < Main.maxProjectiles)
            {
                projectile = Main.projectile[index];
                projectile.velocity = velocity;
                action?.Invoke(projectile);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                return true;
            }
            else
            {
                projectile = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的 ModProjectile 弹幕到世界中，并在生成后执行一个 <see cref="Action{Projectile}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <param name="index">输出弹幕索引。</param>
        /// <param name="projectile">输出弹幕实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">初始速度。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">生成成功后对弹幕执行的行为。</param>
        /// <returns>如果生成成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool NewProjectileActionCheck<T>(out int index, [NotNullWhen(true)] out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileActionCheck(out index, out projectile, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成指定数量的弹幕，每个弹幕的速度方向按固定角度递增旋转。
        /// </summary>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">每次递增的旋转角度（顺时针，弧度）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">基础速度向量（第一个弹幕的方向）。</param>
        /// <param name="type">弹幕类型 ID。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">每个弹幕生成后执行的行为。</param>
        public static void RotatedProj(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            for (int i = 0; i < number; i++)
                NewProjectileAction(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的 ModProjectile 弹幕，每个弹幕的速度方向按固定角度递增旋转。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModProjectile"/> 的类型。</typeparam>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">每次递增的旋转角度（顺时针，弧度）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">基础速度向量。</param>
        /// <param name="damage">伤害值。</param>
        /// <param name="knockback">击退力。</param>
        /// <param name="owner">所有者玩家索引。</param>
        /// <param name="action">每个弹幕生成后执行的行为。</param>
        public static void RotatedProj<T>(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
            where T : ModProjectile =>
            Projectile.RotatedProj(number, radian, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);
    }
}