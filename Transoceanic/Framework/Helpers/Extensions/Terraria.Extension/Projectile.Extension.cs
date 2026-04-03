using Transoceanic.DataStructures;
using Transoceanic.DataStructures.Particles;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(Projectile projectile)
    {
        public TOGlobalProjectile Ocean => projectile?.GetGlobalProjectile<TOGlobalProjectile>();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModProjectile<T>() where T : ModProjectile => projectile?.ModProjectile as T;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModProjectileThrow<T>() where T : ModProjectile => projectile.GetModProjectile<T>() ?? throw new ArgumentException($"Projectile {projectile.Name} ({projectile.type}) does not have a ModProjectile of type {typeof(T).FullName}.", nameof(projectile));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModProjectile<T>([NotNullWhen(true)] out T result) where T : ModProjectile => (result = projectile.GetModProjectile<T>()) is not null;

        public Texture2D Texture => TOAssetUtils.GetProjectileTexture(projectile.type);

        public byte GraphicAlpha
        {
            get => (byte)(255 - projectile.alpha);
            set => projectile.alpha = Math.Clamp(255 - value, 0, 255);
        }

        public bool OnOwnerClient => projectile.owner == Main.myPlayer;

        public bool IsFinalUpdate => projectile.numUpdates == -1;

        /// <summary>
        /// 将弹幕速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity">速度。</param>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        /// </param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            projectile.velocity = velocity;
            projectile.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 将 <see cref="Entity.velocity"/> 转换为 <see cref="Projectile.rotation"/>，并应用于弹幕。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        public void VelocityToRotation(float rotationOffset = 0f) => projectile.rotation = projectile.velocity.ToRotation(rotationOffset);

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

        public void SpawnAfterimage(int lifetime, Color color, Rectangle? frame = null, Vector2? drawOffset = null) =>
            projectile.Ocean.Afterimages.Add(new AfterimageParticle(projectile.Texture, frame, projectile.Center, lifetime, projectile.rotation, projectile.scale, color, projectile.Opacity, drawOffset));

        public void SpawnAfterimage(AfterimageParticle afterimage) => projectile.Ocean.Afterimages.Add(afterimage);
        #endregion GlobalProjectile
    }

    extension(Projectile)
    {
        public static Projectile DummyProjectile => Main.projectile[Main.maxProjectiles];

        public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewProjectileIterator(IteratorMatches.Projectile_IsActive);

        public static Projectile CreateProjectile(int type)
        {
            Projectile projectile = new();
            projectile.SetDefaults(type);
            return projectile;
        }

        public static Projectile CreateProjectile<T>() where T : ModProjectile => CreateProjectile(ModContent.ProjectileType<T>());

        public static Projectile CreateProjectile(int type, Action<Projectile> action)
        {
            Projectile projectile = CreateProjectile(type);
            action?.Invoke(projectile);
            return projectile;
        }

        public static Projectile CreateProjectile<T>(Action<Projectile> action) where T : ModProjectile
        {
            Projectile projectile = CreateProjectile<T>();
            action?.Invoke(projectile);
            return projectile;
        }

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
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
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void NewProjectileAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileAction(source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Projectile索引。</param>
        /// <param name="projectile">输出的Projectile实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>生成Projectile是否成功。</returns>
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
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="index">输出的Projectile索引。</param>
        /// <param name="projectile">输出的Projectile实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>生成Projectile是否成功。</returns>
        public static bool NewProjectileActionCheck<T>(out int index, [NotNullWhen(true)] out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileActionCheck(out index, out projectile, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void RotatedProj(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            for (int i = 0; i < number; i++)
                NewProjectileAction(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void RotatedProj<T>(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
            where T : ModProjectile =>
            Projectile.RotatedProj(number, radian, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);
    }
}