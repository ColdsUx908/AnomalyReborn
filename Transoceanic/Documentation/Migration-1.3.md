# Transoceanic 1.3 公共 API 迁移

1.3 收紧框架实现类型的外部访问权限，包含源码和二进制兼容性变更。消费模组须迁移并重新编译。build.txt 从 1.2 升至 1.3，不表示已经发布到工坊或完成真实宿主验证。

## 性能与绘制约定更新

TOBossChallenge.BossList 现在是缓存的实时只读视图，持有该视图会看到后续帧的列表更新和世界退出清空，不是某一帧的快照。

ParticleSet 移除 OrderFunction，统一按提交顺序绘制，不再按 Timer 排序。粒子绘制新增两倍可见视野的屏外剔除，DrawScreenCheckFluff 为每实例额外世界像素边距；PulseRing 自动计算边距。屏外不调用 PreDraw/PostDraw，更新与寿命继续推进，不应在绘制回调中维护游戏状态。

AI32/AI64 快照读取在临时存储不超过 1 KiB 时使用栈空间，大数组仍使用堆数组；保持原有线格式、清零语义和校验失败不修改目标的保证。

共享渲染目标池增加 128 MiB 估算缓存显存预算和绘制入口的过期清理；预算不限制活动租约或驱动额外开销。延迟归还改为复用双缓冲，处理中新登记的租约仍在下一轮归还。

## 行为查询接口

GlobalNPCBehaviorHandler、GlobalProjectileBehaviorHandler、GlobalItemBehaviorHandler、PlayerBehaviorHandler 改为 internal；对应集合字段统一为 internal static readonly 的 _BehaviorSet，不再暴露框架调度集合的修改权限。

引用 Transoceanic.Behaviors，将旧调用：

```csharp
GlobalNPCBehaviorHandler.BehaviorSet.Enumerate<MyNPCBehavior>(nameof(MyNPCBehavior.CustomHook))
```

替换为：

```csharp
BehaviorQueries.EnumerateNPC<MyNPCBehavior>(nameof(MyNPCBehavior.CustomHook))
```

对应入口还有 EnumerateProjectile、EnumerateItem 和 EnumeratePlayer。每种入口均提供泛型筛选和不筛选版本，以及接受实体和不接受实体的版本；methodName 默认使用调用者方法名。需要绑定实体时使用例如 EnumerateNPC<MyNPCBehavior>(npc, methodName)。不传实体的版本沿用原集合的绑定语义，不会建立新的实体关联。

查询保留优先级、方法分组、ShouldProcess 筛选和遍历锁。返回共享 Behavior，不可缓存；同一单例跨实体重入仍被禁止。foreach 提前退出或异常会释放绑定锁。

三个 Single Handler 及 Behavior 集合继续公开。消费方自行拥有的集合仍可 FillSet/Clear；框架集合不再对外提供写入口。

## 内部实现

生命周期与注册实现 TOLoaderHandler、BehaviorLoader、AssetLoader、GoreLoader、TOCommandHelper、TOGeneralChatCommand、DebugModeCommand、AutoHostingSystem、RenderTargetPoolSystem 及当前所有内置 Behavior 改为 internal。通过公开功能 API 使用相应能力，不应手动创建这些对象或触发生命周期。

TOGlobalNPCBehavior、TOGlobalProjectileBehavior、TOGlobalItemBehavior、TOPlayerBehavior 已移除。Behavior 现由 ModType 注册并赋值所属 Mod，这四个空基类不再承担职责；框架内置行为及消费方直接继承 GlobalNPCBehavior、GlobalProjectileBehavior、GlobalItemBehavior、PlayerBehavior，不再实现 Mod 属性。消费方须重新编译。

ModDOTHandler 保留公开，其两个 RegisterDOT 入口继续可用；ModDOT_Player 和 ModDOT_GlobalNPC 为内部实现。

## 集合与运行时辅助入口

- SimpleEntityBehaviorSet 和 SingleEntityBehaviorSet 的 _Data 字典改为 private；通过公开填充和查询方法使用集合。
- 行为处理器及枚举器类型继续公开，显式构造函数改为 internal。通过 Enumerate 和 GetEnumerator 获取；foreach 所需成员保持公开。
- TODetourHandler.DetourSet 改为 internal，Hook 安装继续使用公开 Modify 等入口。
- EvaluateDetourName、EvaluateTypedDetourName、EvaluateManipulatorName、ResolveSourceMethod 改为 internal。公开 Detour/IL 安装入口仍支持对应的名称解析。
- PublicizerHandler 保留公开，因为第三方 Publicizer 生成代码需要调用它。Detour 模板、配置特性及手动 Hook/IL 接口继续公开。

TOBossChallenge.BossList 改为 IReadOnlyList<NPC>，底层列表通过只读包装提供访问。NPC 对象本身仍可变，列表按原生命周期更新和清空。

TOReflectionUtils.TerrariaTypes 和 TerrariaTypesByFullName 改为 IReadOnlyDictionary，使用只读字典包装。TerrariaTypes 的 Type[] 值保留原结构，不承诺深度不可变，调用方不得修改缓存数组。

ParticleSet.Particles 改为只读活动视图。将原来的 Particles.Add 改为 TrySpawnParticle(particle)，AddRange 改为逐项提交。提交会调用 PreSpawn/PostSpawn，容量不足或否决返回 false；不保证整组原子提交。自动 SpawnParticle() 仍由子类实现，返回新对象给集合提交。新增 Clear 清空活动与待加入列表，不能在更新或绘制回调中调用。

NPC 与弹幕的 Afterimages 均按需创建；主动访问列表或 SpawnAfterimage 会启用。DisableAfterimages 清空并释放列表引用；克隆恢复未启用。框架更新/绘制不会隐式分配，专用服务器上的生成接口跳过资源访问。

GetPvPPlayerTarget 新增末尾可选 int team 参数；-1 使用 owner.team，0 无队伍，1 至 5 排除对应队伍。旧源码调用无需补参，但已编译程序集须重新编译。GetNPCTarget_Advanced 的免疫过滤已按实际 tML 模式和单次穿透规则修正。

NPCTargetingParameters 的相等和哈希现在包含排除玩家集合，按内容比较、忽略插入顺序，null 与空集合等价。作为哈希键期间不能修改参数和集合。

## 验证与剩余限制

从仓库根运行 `pwsh -File Tests/Transoceanic/Verify.ps1 -IncludeConsumers`。Behavior 测试覆盖 internal Handler 的生成与执行，并读取实际框架程序集检查类型边界与独立消费程序集的访问限制；单独运行该测试前须先构建 Transoceanic。

本版本还包含实体 AI 容量调整：Ocean ExtraAI 仅保留 AI32 稀疏完整快照，NPC 12 数据槽、弹幕 7 数据槽；删除 OceanAI64 段并重排计时器槽位。这是网络协议变化，各端必须一起升级为相同构建。通用 Union64 和 AI64 编解码工具仍保留给消费方使用。真实 tML 自动加载、卸载、多人连接和绘制仍需宿主验证；仓库外第三方需要按上述规则迁移。

## ModType 内容迁移（P0–P3）

本轮继续作为未发布的 1.3 变更，消费方必须重新编译；没有新增或改写网络载荷。

### Behavior

- EntityBehavior<TEntity> 现在间接继承非泛型 ModType；自身 Entity、LockEntity、Priority、ShouldProcess、ApplyingType 及 Handler 分发保持原职责。
- 删除所有 `override ... Mod => ...`，直接使用继承的 Mod。需要强类型访问时另外声明命名明确的属性，例如 `ConsumerMod => (ExampleConsumer)Mod`，不要隐藏 Mod。
- 将 `public override void Load(Mod mod)` 改为 `public override void Load()`，从 Mod 属性取得所属模组。旧入口已移除：保留同签名公开方法可能让 C# 重新映射 ILoadable，绕过 ModType 的标准加载过程。
- 不再显式实现 ILoadable.Load。Register/SetupContent 已由 RegisteredContent 封闭；类型级初始化使用 SetStaticDefaults，跨模组后置初始化仍使用 PostSetupContent。
- IContentLoader、ITOLoader 及当前优先级和逆序清理桥接保留。UnModUnload 与 ModType.Unload 是不同阶段，不要为相同资源重复释放。
- 未实际注册的 ILoadable 不再由反射补建：Autoload(false)、IsLoadingEnabled=false、客户端限制和关闭自动加载均受尊重。手动内容必须在所属 Mod.Load 中 AddContent。
- 手工 new 的临时 Behavior 没有 Mod 和内容初始化，只适合独立逻辑；不要调用 ILoadable.Load 来“初始化”临时实例，也不要浅克隆注册单例。

### 名称与清理

Behavior、TOCommand、TypeDetour<T>、ParticleDefinition 均使用 RegisteredContent。默认 Name 是包含命名空间的 CLR 完整类型名，同一模组下不同命名空间的同名类可以共存。改名会影响名称查询。Behavior、命令和 Detour 可显式重写 Name 或使用 LegacyName 提供旧别名；自动粒子定义不转发运行粒子类上的 LegacyName，更名时同步查询，或使用手动定义明确名称与别名。

原生名称查询使用各自注册族：`ModContent.Find<IEntityBehavior>(fullName)`、`ModContent.Find<TOCommand>(fullName)`、`ModContent.Find<TypeDetour<TTarget>>(fullName)`、`ModContent.Find<ParticleDefinition>(fullName)`。不是每个派生基类都有独立查询表；按派生类型枚举使用 Mod.GetContent<T>()。

分配自有资源前调用 `TOCleanup.Register(() => ...);` 登记本轮清理。清理在加载器退出回调后逆序执行，单个失败不会阻止其他回调，异常最后聚合。TOMain.Unload 有兜底入口。不要在清理期间追加回调，也不要让它重复释放由 Unload/OnModUnload 负责的同一资源。

自定义查询族不依赖 tML 自动清空：框架主动清理 ModTypeLookup 的全名和分层字典；注册部分失败也保留清理资格。重复加载将重新登记。该系统只处理自有资源与静态引用，不重复卸载 tML 的 On/IL 钩子。

### 命令与 Detour

TOCommand 现为 ModType 内容，仍是由 //chat 分发的子命令；没有改成独立 ModCommand。现有 Command、Type、Action、Help 保留；需无参构造或手动 AddContent。命令关键词、help 保留字与原覆盖策略不变，内容 Name 与用户输入的 Command 是两套标识。

TypeDetour<T> 及全部派生模板现在是 ModType；在 Load 中不提前安装钩子。TODetourHandler 继续在 PostSetupContent 按 LoadPriority 应用提供者，并保留 ShouldApplyDetour 与静态 Attribute 扫描。已注册提供者复用实际内容实例，禁用类型不重建。普通 ITODetourProvider 接口实现继续通过旧加载器构造和缓存，不要求改变其继承体系。

### 粒子

Particle 仍是普通运行对象，带参构造函数和生成调用保持可用；新增 ParticleDefinition : ModType 保存所属 Mod、名称、本地 ID 和类型配置模板。Particle 通过 IAutoload<ParticleAutoloader> 在其所属模组自动注册对应定义，不需要无参运行构造函数。ParticleHandler 只收集实际注册的定义，不再全局扫描并创建未注册粒子模板。

`particle.Definition` 和 `ParticleHandler.GetDefinition<T>()` 可以取得定义；`GetTemplateInstance<T>()` 保留兼容用途。模板不能提交到全局、原子批次或局部 ParticleSet。纹理配置仍通过原 AutoLoadTexture/TexturePath 重写读取：兼容模板刻意不执行运行构造函数，配置 getter 不得依赖构造初始化字段；这不是运行粒子实例。

Autoload(false) 等 tML 筛选会禁用自动定义。需要条件化手动定义时，在粒子类标记 Autoload(false)，在所属 Mod.Load 中 AddContent(new MyParticleDefinition(...))，由自定义定义重写 IsLoadingEnabled；不要同时自动和手动注册同一粒子。专用服务器不注册粒子定义，也不请求其纹理。缓存未就绪或定义禁用时不能构造运行粒子，会收到明确异常。

粒子 ID 每轮从零建立，仅供本地视觉使用，不能持久化或作为网络协议 ID；纹理路径与帧更新、生成容量、重要粒子和队列时序保持原语义。

## 无顺序依赖加载任务迁移至 tML（后三批）

本轮继续作为未发布的 1.3 迁移，不增加网络字段或协议变更。

1. 原有 ModType/ModSystem 改用 `public override void Load()`；普通独立服务改用 tML 的 `ILoadable.Load(Mod mod)`。只有普通服务显式实现此接口，Behavior 仍不得重新接管 ILoadable.Load。
2. 原生内容 Load 在所属 Mod.Load 之前执行。需要所属模组时使用继承的 Mod 或入口参数，不能读取依赖 Mod.Load 赋值的静态主实例。早期同步登记、资产请求和加载诊断均应使用显式所属 Mod。
3. 原 On_TOExtensions 优先级 5 的后置清空已迁至框架原生 Load，消费方应在原生加载阶段订阅，防止消费方提前订阅后被后置清空。
4. 独立钩子可以在 Load 中安装。原版 ID 常量集合可以提前建立，但修改 tML 管理的 NPCID.Sets 等静态数组要等待 SetStaticDefaults 阶段。
5. 自有列表、事件、委托、模组与资产引用在分配前登记 TOCleanup。普通自定义事件允许解除自身订阅；On/IL 钩子仍由 tML 管理。不要再由旧 IContentLoader/ITOLoader 卸载入口重复清理同一对象。
6. 移除没有实际逻辑的冗余接口声明或空回调时，注意原生接口必须实现的入口；Behavior 基类仍保留后置契约，删除派生类冗余接口不改变此继承关系。

框架迁移范围：TOGlobalNPC、TOGameTime、TOBossChallenge、TOWorldState、On_TOExtensions。

需要完整注册集合、跨模组资源或内容 ID 的 BehaviorLoader、TOCommandHelper、ModDOTHandler、AssetLoader/GoreLoader、ParticleHandler 等仍保留原后置阶段；消费方依赖粒子缓存或有序兼容结果的初始化也应保留正确时机。自定义接口和集中调度本轮没有整体移除。

## 物品 Behavior 网络方法

GlobalItemBehavior、SingleItemBehavior 及其派生类型和 Handler 不再提供 NetSend、NetReceive。生成器不再生成这两个物品钩子，Handler 中的转发和禁用代码已移除。原有重写应迁移到数据所属的 ModItem 或 GlobalItem，成对保持读写类型与顺序；物品 Behavior 网络方法不受支持，启用 EnableSync 也不会接入。消费方须重新编译。

## 发布前生成器、数学边界与宿主检查

Publicizer 修复同名类型、泛型实参和数组导致的辅助成员冲突；冲突辅助名改为完整签名编码，重新编译消费方，不直接调用这些反射字段或委托。TypeDetour 模板现在生成重载入口，非重载入口保留原名。不支持的泛型方法、引用返回或不可访问签名等报告 TOGEN002；无效包装类型报告 TOGEN001。框架整类宿主包装显式抑制预期的 TOGEN002，不影响消费方。

旋转矩形碰撞的 AABB 预筛修复相切和退化形状漏检。Circle/Annulus 构造拒绝无效半径，NormalizeWithPeriod 拒绝非有限或非正周期。MathInterval 修复倒置闭区间没有标为空的问题；支持无穷端点并将其规范化为开放，Contains 不包含无穷值本身。完整边界见 [兼容性契约](Compatibility-1.3.md)。

NPC 缩放依赖在原生加载时提前校验；宿主签名或 IL 不兼容将带明确诊断中止加载，不等到战斗中第一次调用才失败。本轮不改变网络字节格式；SyncEnabled setter 在 1.3 保留兼容，正式启用仍使用 EnableSync。
