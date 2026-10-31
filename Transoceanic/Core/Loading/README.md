# 加载器生命周期

无内容与排序依赖的初始化通过 tML 原生 `Load` 执行：ModType/ModSystem 重写无参 Load，普通服务实现 `ILoadable.Load(Mod)`。这些入口在所属 Mod.Load 之前的自动加载阶段运行。`ITOLoader` 仅保留给集中排序任务；`IContentLoader` 在 PostSetupContent 中处理仍需内容就绪的任务。每次模组加载期间，框架按类型保存实例，对 ILoadable 类型仅使用所属模组通过 tModLoader 实际注册的内容实例，未注册类型不会被反射复活，手动注册的闭合泛型及其他程序集提供的内容按实际注册实例收集；普通非 ILoadable 加载器继续按类型构造并缓存。Behavior 集合与生命周期调度共用该实例，不再重新构造。

普通加载器必须具有可正常执行的无参构造函数。构造失败会保留异常，不回退到未初始化对象。粒子的 ModType 定义由 ParticleAutoloader 注册，运行粒子的带参构造不参与加载器实例化。

`LoadPriority` 从接口映射指向的实现方法读取，支持显式、隐式及继承实现。数值越大越先调用；相同时按程序集名和类型全名排序。配方回调采用相同规则。世界退出和模组卸载按实际初始化顺序反向清理；已进入但失败的回调也会进入清理列表。单个清理失败不阻止列表中其余实例清理。

Behavior 仍是按类型共享的对象，实体数据须保存在实体或对应 Global/Player 数据容器中。`PlayerBehavior` 不提供 `CopyClientState`、`SyncPlayer`、`SendClientChanges`，生成器也不生成它们的分发。需要玩家同步时，在实际存储数据的 `ModPlayer` 中实现 tModLoader 回调；`SyncEnabled` 不会启用这些已禁用的 Behavior API。

行为集合的 foreach 枚举器在筛选和执行当前行为时锁定绑定引用；Single Handler 的生成分发也会创建相同作用域。使用 break、return 或抛出异常都会由 using/foreach 的 finally 释放。相同实体可以嵌套调用，回调运行中改绑到其他实体会抛出 `InvalidOperationException`，不会静默改变外层上下文。锁定只保护引用，不阻止修改实体数据，也不是跨线程锁。

直接使用 `TryGetBehavior` 查询返回的单例时，调用方应在查询后立即使用 `using EntityBehavior<NPC>.EntityLock entityLock = behavior.LockEntity();` 包围实际调用；Item/Projectile/Player 使用各自的实体类型。不要缓存返回的行为，或复制其锁定作用域。

`TOMain.Load` 捕获初始化异常，并在 finally 完成状态复位后抛出保留原始异常的 `AggregateException`，中止后续加载。已经进入初始化的加载器保留原有卸载清理资格。

单例 Behavior 明确禁止跨实体重入：不要在 A 的回调中通过同一个单例查询并调用 B。需要这种处理时，仅在逻辑不依赖 Mod 与内容初始化的条件下显式 new 具体 Behavior，绑定 B 后在 using LockEntity() 作用域中调用。手工实例的 Mod 为 null，不得手工调用 ILoadable.Load 或浅克隆已注册单例。

## ModType 注册和自定义清理

Behavior、TOCommand、TypeDetour<T> 和 ParticleDefinition 使用 RegisteredContent<TContent>。Register 与 SetupContent 封闭，后者调用 SetStaticDefaults；Mod.Load 仍早于跨模组 PostSetupContent，不能互换。Behavior 只重写无参 Load()，原 Load(Mod) 已移除，避免接口重映射绕过 ModType 的所属模组赋值和注册。

名称默认为 CLR 完整类型名（含命名空间），ParticleDefinition 使用其 ParticleType 的完整名称。名称查询族分别为 IEntityBehavior、TOCommand、TypeDetour<T>、ParticleDefinition；例如 ModContent.Find<IEntityBehavior>("ExampleConsumer/ExampleConsumer.BlueSlimeBehavior")。GetInstance 与 GetContent 仍由 tML 管理，数字粒子 ID 只在当前客户端加载周期有效。

需要提前清理自有资源或静态引用时，在分配前调用 TOCleanup.Register(cleanup)。回调在加载器的世界与内容退出回调之后逆序执行；所有失败聚合，失败项不会阻止其他回调。TOMain.Unload 另作兜底，重复清理为空操作。清理回调不可递归运行或追加新回调；每轮须重新登记，不自动重试失败项。回调仅在加载/卸载主线程使用。

自定义 ModTypeLookup 表的两个字典会主动清空；Behavior 解除实体引用，粒子定义释放模板资产引用。tML 管理的 ContentInstance、On/IL 钩子不由此系统重复卸载。普通加载器接口、优先级、配方和世界回调继续保留。

## 无顺序依赖的原生加载迁移

TOGlobalNPC、TOGameTime、TOBossChallenge 使用原生 Load；TOWorldState 通过 Load 登记卸载清理。On_TOExtensions 在框架自动加载阶段初始化自有事件，消费方随后订阅；PostSetupContent 不再清空这些订阅，原来的 LoadPriority(5) 已移除。

消费方在原生内容 Load 中使用入口参数或继承的 Mod 登记同步、请求自身资产和输出加载诊断，不依赖主 Mod.Load 才赋值的静态单例。

独立钩子可以在 Load 注册；NPCID.Sets 等由 tML 扩容的静态数组应在 SetStaticDefaults 中修改，避免设置被扩容覆盖。一般的 Priority 是 Behavior 执行优先级，不等同于 LoadPriority。是否迁移须同时检查即时读取的内容、静态单例和其他加载器依赖，不能仅看优先级为零。

迁移后的自有引用在分配或订阅前登记 TOCleanup；普通服务仍须实现 tML 要求的 ILoadable.Unload，空实现仅用于满足原生契约，不承担重复释放职责。普通自定义事件的订阅由 TOCleanup 解除；tML 管理的 On/IL 钩子不手动退订。

Behavior/命令/DOT 汇总、跨模组资源扫描、粒子缓存、依赖粒子纹理的效果以及有序兼容扩展继续使用现有后置接口。ITOLoader、IContentLoader 及其调度入口本轮保留，不作整套接口删除。
