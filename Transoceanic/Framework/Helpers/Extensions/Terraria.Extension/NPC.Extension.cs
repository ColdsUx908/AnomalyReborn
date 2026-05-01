// Developed by ColdsUx

using Transoceanic.DataStructures.Particles;
using Transoceanic.Hooks.Framework.Helpers;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(NPC npc)
    {
        /// <summary>
        /// 获取 NPC 的全局数据 <see cref="TOGlobalNPC"/>。
        /// </summary>
        public TOGlobalNPC Ocean => npc?.GetGlobalNPC<TOGlobalNPC>();

        /// <summary>
        /// 获取 NPC 所关联的 <see cref="ModNPC"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModNPC"/> 类型。</typeparam>
        /// <returns>转换后的实例，若不存在则返回 <c>null</c>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModNPC<T>() where T : ModNPC => npc?.ModNPC as T;

        /// <summary>
        /// 获取 NPC 所关联的 <see cref="ModNPC"/> 实例，并转换为指定类型；若不存在则抛出异常。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModNPC"/> 类型。</typeparam>
        /// <returns>转换后的实例。</returns>
        /// <exception cref="ArgumentException">当 NPC 没有指定类型的 <see cref="ModNPC"/> 时抛出。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModNPCThrow<T>() where T : ModNPC => npc.GetModNPC<T>() ?? throw new ArgumentException($"NPC {npc.FullName} ({npc.type}) does not have a ModNPC of type {typeof(T).FullName}.", nameof(npc));

        /// <summary>
        /// 尝试获取 NPC 所关联的 <see cref="ModNPC"/> 实例，并转换为指定类型。
        /// </summary>
        /// <typeparam name="T">目标 <see cref="ModNPC"/> 类型。</typeparam>
        /// <param name="result">输出转换后的实例，成功时为有效值，否则为 <c>null</c>。</param>
        /// <returns>如果成功获取则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModNPC<T>([NotNullWhen(true)] out T result) where T : ModNPC => (result = npc.GetModNPC<T>()) is not null;

        /// <summary>
        /// 获取 NPC 的纹理贴图。
        /// </summary>
        public Texture2D Texture => TOAssetUtils.GetNPCTexture(npc.type);

        /// <summary>
        /// 获取 NPC 当前锁定的玩家目标（如果存在）。
        /// </summary>
        public Player PlayerTarget => npc.HasPlayerTarget ? Main.player[npc.target] : null;

        /// <summary>
        /// 获取 NPC 当前锁定的 NPC 目标（如果存在）。
        /// </summary>
        public NPC NPCTarget => npc.HasNPCTarget ? Main.npc[npc.target - 300] : null;

        /// <summary>
        /// 获取 NPC 当前生命值比例（当前生命值 / 最大生命值）。
        /// </summary>
        public float LifeRatio => npc.lifeMax <= 0 ? 0f : (float)npc.life / npc.lifeMax;

        /// <summary>
        /// 获取 NPC 已损失生命值比例（1 - 生命值比例）。
        /// </summary>
        public float LostLifeRatio => 1f - npc.LifeRatio;

        /// <summary>
        /// 获取或设置 NPC 的图形透明度。0 表示完全透明，255 表示完全不透明。
        /// </summary>
        public byte GraphicAlpha
        {
            get => (byte)(255 - npc.alpha);
            set => npc.alpha = Math.Clamp(255 - value, 0, 255);
        }

        /// <summary>
        /// 判断 NPC 是否为友好单位（友方、城镇 NPC 或生命值极低）。
        /// </summary>
        public bool IsFriendly => npc.active && (npc.friendly || npc.townNPC || npc.lifeMax <= 5);

        /// <summary>
        /// 判断 NPC 是否为敌对单位（非友方且生命值 >= 5）。
        /// </summary>
        public bool IsEnemy => npc.active && !npc.friendly && npc.lifeMax >= 5;

        /// <summary>
        /// 检查 NPC 是否为 Boss 或等效 Boss 单位。
        /// </summary>
        /// <returns>如果是 Boss 则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool IsBossEnemy => npc.active && (npc.boss || npc.EoW || npc.type == NPCID.WallofFleshEye) || On_TOExtensions.Impl_get_IsBossEnemy(npc);

        /// <summary>
        /// 检查 NPC 是否为世界吞噬者或其身体/尾部。
        /// </summary>
        public bool EoW => npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

        /// <summary>
        /// 检查 NPC 是否为毁灭者或其身体/尾部。
        /// </summary>
        public bool Destroyer => npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;

        /// <summary>
        /// 检查 NPC 是否为双子魔眼（激光眼或魔焰眼）。
        /// </summary>
        public bool Twins => npc.type is NPCID.Retinazer or NPCID.Spazmatism;

        /// <summary>
        /// 检查 NPC 是否为机械骷髅王的四臂之一。
        /// </summary>
        public bool SkeletronPrimeHand => npc.type is >= 128 and <= 131;

        /// <summary>
        /// 检查 NPC 是否为石巨人之拳。
        /// </summary>
        public bool GolemFist => npc.type is NPCID.GolemFistLeft or NPCID.GolemFistRight;

        /// <summary>
        /// 检查 NPC 是否为拜月教邪教徒的幻影龙（各段）。
        /// </summary>
        public bool CultistDragon => npc.type is >= 454 and <= 459;

        /// <summary>
        /// 检查 NPC 是否为霜月事件中的敌人。
        /// </summary>
        public bool FrostMoonEnemy => npc.type is >= 338 and <= 352;

        /// <summary>
        /// 获取目标相对于 NPC 的水平方向（-1 左，1 右）。
        /// </summary>
        public int TargetDirection => Math.Sign((npc.HasNPCTarget ? Main.projectile[npc.target - 300] : (Entity)Main.player[npc.target]).Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        /// <summary>
        /// 判断 NPC 是否面向其目标。
        /// </summary>
        public bool IsFacingTarget => npc.direction == npc.TargetDirection;

        /// <summary>
        /// 使 NPC 面向指定实体。
        /// </summary>
        /// <param name="target">要面向的目标实体。</param>
        public void FaceTarget(Entity target)
        {
            npc.direction = Math.Sign(target.Center.X - npc.Center.X) switch
            {
                -1 => -1,
                _ => 1
            };
            npc.directionY = Math.Sign(target.Center.Y - npc.Center.Y) switch
            {
                -1 => -1,
                _ => 1
            };
        }

        /// <summary>
        /// 设置 NPC 的速度，同时自动更新其旋转角度。
        /// </summary>
        /// <param name="velocity">新的速度向量。</param>
        /// <param name="rotationOffset">旋转偏移值（弧度）。例如，对于贴图向上的弹幕，可设为 <see cref="MathHelper.PiOver2"/>。</param>
        /// <remarks>为性能考虑，不要在不改变方向的情况下重复调用此方法。</remarks>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            npc.velocity = velocity;
            npc.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 根据当前速度向量更新 NPC 的旋转角度。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值（弧度）。</param>
        public void VelocityToRotation(float rotationOffset = 0f) => npc.rotation = npc.velocity.ToRotation(rotationOffset);

        /// <summary>
        /// 如果当前目标无效，则重新寻找最近的有效目标。
        /// </summary>
        /// <param name="faceTarget">是否在找到目标后立即面向目标。</param>
        /// <param name="distanceThreshold">距离阈值，超出此范围则重新寻找。</param>
        /// <returns>最终目标是否有效。</returns>
        public bool TargetClosestIfInvalid(bool faceTarget = true, float distanceThreshold = 4000f)
        {
            if (!npc.HasValidTarget)
                npc.TargetClosest(faceTarget);

            Player target = npc.PlayerTarget;

            if (distanceThreshold >= 0f && !npc.WithinRange(target.Center, distanceThreshold - target.aggro))
                npc.TargetClosest(faceTarget);

            return npc.HasValidTarget && npc.WithinRange(target.Center, distanceThreshold - target.aggro);
        }

        /// <summary>
        /// 根据自定义条件重新选择目标。
        /// </summary>
        /// <param name="faceTarget">是否在找到目标后立即面向目标。</param>
        /// <param name="validMatch">判断玩家是否为合法目标的条件委托。</param>
        /// <returns>是否成功找到合法目标。</returns>
        public bool TargetIfInvalid(bool faceTarget, Func<Player, bool> validMatch)
        {
            validMatch ??= p => false;
            if (npc.HasValidTarget && validMatch(npc.PlayerTarget))
                return true;

            foreach (Player player in Player.AlivePlayers)
            {
                if (validMatch(player))
                {
                    npc.target = player.whoAmI;
                    if (faceTarget)
                        npc.FaceTarget(player);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 改变 NPC 的缩放比例，并固定底部位置不变。
        /// </summary>
        /// <param name="width">原始宽度（缩放前）。</param>
        /// <param name="height">原始高度（缩放前）。</param>
        /// <param name="newScale">新的缩放比例。</param>
        public void ChangeScaleFixBottom(int width, int height, float newScale)
        {
            if (npc.scale == newScale)
                return;

            Vector2 oldBottom = npc.Bottom;
            npc.width = (int)(width * newScale);
            npc.height = (int)(height * newScale);
            npc.scale = newScale;
            npc.Bottom = oldBottom;
        }

        /// <summary>
        /// 应用持续性伤害效果（dot）。
        /// </summary>
        /// <param name="dot">每秒减少的生命回复量（负值）。</param>
        /// <param name="damageValue">基础伤害值。</param>
        /// <param name="damage">引用传递的伤害值，会被至少设置为 <paramref name="damageValue"/>。</param>
        public void ApplyDOT(int dot, int damageValue, ref int damage)
        {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;
            npc.lifeRegen -= dot;
            if (damage < damageValue)
                damage = damageValue;
        }

        /// <summary>
        /// 为 NPC 添加一个 ModBuff。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModBuff"/> 的增益类型。</typeparam>
        /// <param name="time">持续时间（帧数）。</param>
        /// <param name="quiet">是否安静添加（不发出声音或特效）。</param>
        public void AddBuff<T>(int time, bool quiet = false) where T : ModBuff => npc.AddBuff(ModContent.BuffType<T>(), time, quiet);

        #region GlobalNPC
        /// <inheritdoc cref="TOGlobalNPC.Identifier"/>
        public long Identifier => npc.Ocean.Identifier;

        /// <inheritdoc cref="TOGlobalNPC.SpawnTime"/>
        public int SpawnTime
        {
            get => npc.Ocean.SpawnTime;
            internal set => npc.Ocean.SpawnTime = value;
        }

        /// <summary>
        /// 获取或设置 NPC 是否始终根据速度旋转。
        /// </summary>
        public bool AlwaysRotating
        {
            get => npc.Ocean.OceanAI32[0].bits[0];
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[0].bits[0] != value)
                {
                    ocean.OceanAI32[0].bits[0] = value;
                    ocean.AIChanged32[0] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 NPC 的存活时间（用于 AI 逻辑）。
        /// </summary>
        public int ActiveTime
        {
            get => npc.Ocean.OceanAI32[1].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[1].i != value)
                {
                    ocean.OceanAI32[1].i = value;
                    ocean.AIChanged32[1] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 NPC 旋转时的偏移角度。
        /// </summary>
        public float RotationOffset
        {
            get => npc.Ocean.OceanAI32[2].f;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[2].f != value)
                {
                    ocean.OceanAI32[2].f = value;
                    ocean.AIChanged32[2] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 NPC 的“主人” NPC（用于仆从类 NPC）。
        /// </summary>
        public NPC Master
        {
            get
            {
                int masterIndex = npc.Ocean.OceanAI32[3].i;
                return masterIndex >= 0 && masterIndex < Main.maxNPCs ? Main.npc[masterIndex] : null;
            }
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                int temp = value?.whoAmI ?? Main.maxNPCs;
                if (ocean.OceanAI32[3].i != temp)
                {
                    ocean.OceanAI32[3].i = temp;
                    ocean.AIChanged32[3] = true;
                }
            }
        }

        /// <summary>
        /// 尝试获取主人 NPC，并检查其类型是否匹配。
        /// </summary>
        /// <param name="masterType">期望的主人 NPC 类型 ID。</param>
        /// <param name="master">输出主人 NPC 实例，成功时非空。</param>
        /// <returns>如果存在且类型匹配则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool TryGetMaster(int masterType, out NPC master)
        {
            NPC temp = npc.Master;
            if (temp is not null && temp.active && temp.type == masterType)
            {
                master = temp;
                return true;
            }
            master = null;
            return false;
        }

        /// <summary>
        /// 尝试获取主人 NPC，并检查其 ModNPC 类型是否匹配。
        /// </summary>
        /// <typeparam name="T">期望的 <see cref="ModNPC"/> 类型。</typeparam>
        /// <param name="master">输出主人 NPC 实例。</param>
        /// <returns>如果存在且类型匹配则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool TryGetMaster<T>(out NPC master) where T : ModNPC => npc.TryGetMaster(ModContent.NPCType<T>(), out master);

        /// <summary>
        /// 尝试获取主人 NPC 及其 ModNPC 实例。
        /// </summary>
        /// <typeparam name="T">期望的 <see cref="ModNPC"/> 类型。</typeparam>
        /// <param name="master">输出主人 NPC 实例。</param>
        /// <param name="modNPC">输出主人 NPC 的 ModNPC 实例。</param>
        /// <returns>如果存在且类型匹配则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public bool TryGetMaster<T>(out NPC master, out T modNPC) where T : ModNPC
        {
            if (npc.TryGetMaster(ModContent.NPCType<T>(), out master))
            {
                modNPC = master.GetModNPC<T>();
                return true;
            }
            modNPC = null;
            return false;
        }

        /// <summary>
        /// 获取或设置 AI 计时器 1（整数值）。
        /// </summary>
        public int Timer1
        {
            get => npc.Ocean.OceanAI32[^9].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^9].i != value)
                {
                    ocean.OceanAI32[^9].i = value;
                    ocean.AIChanged32[^9] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 2（整数值）。
        /// </summary>
        public int Timer2
        {
            get => npc.Ocean.OceanAI32[^8].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^8].i != value)
                {
                    ocean.OceanAI32[^8].i = value;
                    ocean.AIChanged32[^8] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 3（整数值）。
        /// </summary>
        public int Timer3
        {
            get => npc.Ocean.OceanAI32[^7].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^7].i != value)
                {
                    ocean.OceanAI32[^7].i = value;
                    ocean.AIChanged32[^7] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 4（整数值）。
        /// </summary>
        public int Timer4
        {
            get => npc.Ocean.OceanAI32[^6].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^6].i != value)
                {
                    ocean.OceanAI32[^6].i = value;
                    ocean.AIChanged32[^6] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 5（整数值）。
        /// </summary>
        public int Timer5
        {
            get => npc.Ocean.OceanAI32[^5].i;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^5].i != value)
                {
                    ocean.OceanAI32[^5].i = value;
                    ocean.AIChanged32[^5] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 6（浮点值）。
        /// </summary>
        public float Timer6
        {
            get => npc.Ocean.OceanAI32[^4].f;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^4].f != value)
                {
                    ocean.OceanAI32[^4].f = value;
                    ocean.AIChanged32[^4] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 7（浮点值）。
        /// </summary>
        public float Timer7
        {
            get => npc.Ocean.OceanAI32[^3].f;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^3].f != value)
                {
                    ocean.OceanAI32[^3].f = value;
                    ocean.AIChanged32[^3] = true;
                }
            }
        }

        /// <summary>
        /// 获取或设置 AI 计时器 8（浮点值）。
        /// </summary>
        public float Timer8
        {
            get => npc.Ocean.OceanAI32[^2].f;
            set
            {
                TOGlobalNPC ocean = npc.Ocean;
                if (ocean.OceanAI32[^2].f != value)
                {
                    ocean.OceanAI32[^2].f = value;
                    ocean.AIChanged32[^2] = true;
                }
            }
        }

        /// <summary>
        /// 生成一个残影特效并添加到 NPC 的全局数据中。
        /// </summary>
        /// <param name="lifetime">残影存活时间（帧数）。</param>
        /// <param name="color">残影颜色。</param>
        /// <param name="drawOffset">绘制偏移量（可选）。</param>
        /// <param name="affectedByLight">残影是否受光照影响（可选），默认为 <see langword="true"/>，即受光照影响。</param>
        public void SpawnAfterimage(int lifetime, Color color, Vector2? drawOffset = null, bool affectedByLight = true) =>
            npc.Ocean.Afterimages.Add(new AfterimageParticle(npc.Texture, npc.frame, npc.Center, lifetime, npc.rotation, npc.scale, color, npc.Opacity, drawOffset, affectedByLight));

        /// <summary>
        /// 添加一个自定义的残影粒子到 NPC 的全局数据中。
        /// </summary>
        /// <param name="afterimage">残影粒子实例。</param>
        public void SpawnAfterimage(AfterimageParticle afterimage) => npc.Ocean.Afterimages.Add(afterimage);
        #endregion GlobalNPC
    }

    extension(NPC)
    {
        /// <summary>
        /// 获取一个虚拟的“哑元” NPC（位于索引 <see cref="Main.maxNPCs"/> 处，通常用于临时操作）。
        /// </summary>
        public static NPC DummyNPC => Main.npc[Main.maxNPCs];

        /// <summary>
        /// 检查是否存在指定 ModNPC 类型的活跃 NPC。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <returns>如果存在则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool AnyNPCs<T>() where T : ModNPC => NPC.AnyNPCs(ModContent.NPCType<T>());

        /// <summary>
        /// 检查是否存在指定类型的活跃 NPC，并输出找到的第一个实例。
        /// </summary>
        /// <param name="type">NPC 类型 ID。</param>
        /// <param name="npc">输出找到的 NPC 实例，若不存在则为 <c>null</c>。</param>
        /// <returns>如果存在则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool AnyNPCs(int type, [NotNullWhen(true)] out NPC npc)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.type == type)
                    return true;
            }
            npc = null;
            return false;
        }

        /// <summary>
        /// 检查是否存在指定 ModNPC 类型的活跃 NPC，并输出找到的第一个实例。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="npc">输出找到的 NPC 实例，若不存在则为 <c>null</c>。</param>
        /// <returns>如果存在则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T)
                    return true;
            }
            npc = null;
            return false;
        }

        /// <summary>
        /// 检查是否存在指定 ModNPC 类型的活跃 NPC，并输出找到的第一个实例及其 ModNPC。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="npc">输出找到的 NPC 实例。</param>
        /// <param name="modNPC">输出对应的 ModNPC 实例。</param>
        /// <returns>如果存在则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc, [NotNullWhen(true)] out T modNPC) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T t)
                {
                    modNPC = t;
                    return true;
                }
            }
            npc = null;
            modNPC = null;
            return false;
        }

        /// <summary>
        /// 获取一个迭代器，用于遍历所有激活状态的 NPC。
        /// </summary>
        public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsActive);

        /// <summary>
        /// 获取一个迭代器，用于遍历所有敌对 NPC。
        /// </summary>
        public static TOIterator<NPC> Enemies => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsEnemy);

        /// <summary>
        /// 获取一个迭代器，用于遍历所有 Boss 类 NPC。
        /// </summary>
        public static TOIterator<NPC> Bosses => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsBossEnemy);

        /// <summary>
        /// 根据类型 ID 创建一个新的 NPC 实例（不加入世界）。
        /// </summary>
        /// <param name="type">NPC 类型 ID。</param>
        /// <returns>新创建的 NPC 实例。</returns>
        public static NPC CreateNPC(int type)
        {
            NPC npc = new();
            npc.SetDefaults(type);
            return npc;
        }

        /// <summary>
        /// 根据 ModNPC 类型创建一个新的 NPC 实例（不加入世界）。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <returns>新创建的 NPC 实例。</returns>
        public static NPC CreateNPC<T>() where T : ModNPC => CreateNPC(ModContent.NPCType<T>());

        /// <summary>
        /// 根据类型 ID 创建一个新的 NPC 实例，并执行一个 <see cref="Action{NPC}"/>。
        /// </summary>
        /// <param name="type">NPC 类型 ID。</param>
        /// <param name="action">对创建后的 NPC 执行的行为。</param>
        /// <returns>新创建的 NPC 实例。</returns>
        public static NPC CreateNPC(int type, Action<NPC> action)
        {
            NPC npc = CreateNPC(type);
            action?.Invoke(npc);
            return npc;
        }

        /// <summary>
        /// 根据 ModNPC 类型创建一个新的 NPC 实例，并执行一个 <see cref="Action{NPC}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="action">对创建后的 NPC 执行的行为。</param>
        /// <returns>新创建的 NPC 实例。</returns>
        public static NPC CreateNPC<T>(Action<NPC> action) where T : ModNPC
        {
            NPC npc = CreateNPC<T>();
            action?.Invoke(npc);
            return npc;
        }

        /// <summary>
        /// 在指定玩家位置生成一个 ModNPC。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="plr">玩家索引。</param>
        public static void SpawnOnPlayer<T>(int plr) where T : ModNPC => NPC.SpawnOnPlayer(plr, ModContent.NPCType<T>());

        /// <summary>
        /// 生成一个新的 NPC 到世界中，并在生成后执行一个 <see cref="Action{NPC}"/>。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">NPC 类型 ID。</param>
        /// <param name="start">起始索引偏移。</param>
        /// <param name="action">生成成功后对 NPC 执行的行为。</param>
        public static void NewNPCAction(IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
        {
            int index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
            if (index < Main.maxNPCs)
            {
                action?.Invoke(Main.npc[index]);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
            }
        }

        /// <summary>
        /// 生成一个新的 ModNPC 到世界中，并在生成后执行一个 <see cref="Action{NPC}"/>。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="start">起始索引偏移。</param>
        /// <param name="action">生成成功后对 NPC 执行的行为。</param>
        public static void NewNPCAction<T>(IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
            NewNPCAction(source, position, ModContent.NPCType<T>(), start, action);

        /// <summary>
        /// 生成一个新的 NPC 到世界中，并在生成后执行一个 <see cref="Action{NPC}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <param name="index">输出 NPC 在 <see cref="Main.npc"/> 中的索引。</param>
        /// <param name="npc">输出 NPC 实例，生成失败时为 <c>null</c>。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">NPC 类型 ID。</param>
        /// <param name="start">起始索引偏移。</param>
        /// <param name="action">生成成功后对 NPC 执行的行为。</param>
        /// <returns>如果生成成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static bool NewNPCActionCheck(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
        {
            index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
            if (index < Main.maxNPCs)
            {
                npc = Main.npc[index];
                action?.Invoke(npc);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
                return true;
            }
            else
            {
                npc = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的 ModNPC 到世界中，并在生成后执行一个 <see cref="Action{NPC}"/>，同时返回生成结果和索引。
        /// </summary>
        /// <typeparam name="T">继承自 <see cref="ModNPC"/> 的类型。</typeparam>
        /// <param name="index">输出 NPC 索引。</param>
        /// <param name="npc">输出 NPC 实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="start">起始索引偏移。</param>
        /// <param name="action">生成成功后对 NPC 执行的行为。</param>
        /// <returns>如果生成成功则返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
        public static void NewNPCActionCheck<T>(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
            NewNPCActionCheck(out index, out npc, source, position, ModContent.NPCType<T>(), start, action);
    }
}