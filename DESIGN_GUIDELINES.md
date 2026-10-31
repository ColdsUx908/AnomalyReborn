# AnomalyReborn 设计规范

本文约定 AnomalyReborn 的代码命名、实体架构、着色器实现及测试文件组织方式。新增或修改相关内容时，遵循对应章节的规则。

## 1. 代码命名与属性实现

### 1.1 命名规则

| 对象 | 命名方式 | 说明 |
| --- | --- | --- |
| 类型、属性、方法、事件 | `PascalCase` | 事件的封装成员（如委托列表）使用 `_PascalCase`。 |
| 参数、局部变量 | `camelCase` | 包括构造函数、本地函数、Lambda 的参数，以及循环变量、模式变量；局部常量除外。 |
| 期望外部直接访问的字段 | `PascalCase` | 静态字段、只读字段同样适用。 |
| 不期望外部直接访问的字段 | `_PascalCase` | 静态字段、只读字段同样适用。 |
| 常量 | `PascalCase` | 不受访问级别影响；私有常量、局部常量均不得添加下划线前缀。 |

### 1.2 属性实现

- 无需自定义访问器逻辑时，使用自动属性。
- 需要自定义访问器逻辑且能够使用 `field` 关键字时，必须使用 `field`，不得另设实例字段作为该属性的后备存储。

### 1.3 命名空间与材质路径

- 命名空间与文件夹结构不一致时，tModLoader 为物品、弹幕、NPC、物块等内容类型推导的默认材质路径可能与资源实际位置不符；每个受影响的类都必须显式重写 `Texture`，指定实际资源路径，不得依赖默认推导。
- 移动文件或调整命名空间时，逐类检查主材质、发光材质及其他附属资源的加载方式，并同步更新 `LoadTexture`、`ModContent.Request` 等显式引用和外部类型引用。共享资源、原版材质或透明材质应继续显式指向其真实路径；无材质加载职责的行为类和辅助类无需添加材质属性。

## 2. 实体架构与行为组织

### 2.1 数据归属

- 继承 `GlobalNPC`、`GlobalProjectile`、`GlobalItem` 等 GlobalType 的数据容器，统一归入 `AnomalyGlobalInstances` 中的对应类型：NPC 数据归入 `AnomalyGlobalNPC`，弹幕数据归入 `AnomalyGlobalProjectile`，其他实体数据按对应类型归属。
- 客户端视觉数据遵循相同的归属规则，不得另建仅用于存储数据的 GlobalType；Behavior 通过实体对应的 GlobalType 访问数据。
- 上述规则适用于 GlobalType 数据容器；`ModNPC`、`ModProjectile`、`ModItem` 等实体自身类型可以持有各自的数据。
- 玩家实例数据统一归入 `AnomalyGlobalInstances` 中的 `AnomalyPlayer`，不得另建 `ModPlayer` 存储数据。

#### 2.1.1 GlobalInstances 数据组织

- `AnomalyGlobalInstances.cs` 仅保留稳定的通用代码：实体实例配置、调试数据、AI 数组与变化标记、框架回调入口、AI 数组克隆和通用网络读写。
- 独立于 AI 数组声明的实例字段、自动属性及相关派生属性，按实体类型放入 `Common/GlobalInstances/<类名>/<类名>.ExtraData.cs`，作为对应数据容器的部分类。AI 数组槽位映射属性放入同目录的 `<类名>.AIData.cs`，不混入独立实例数据文件。没有相应数据或逻辑时不创建空文件。
- 独立实例数据按机制或用途采用 `分类_名称` 命名，两部分均使用 `PascalCase`，例如 `BrainOfCthulhu_AfterImages`、`BossBar_IsSmall`、`EventideReunion_DeathCooldown`；不期望外部直接访问的字段保留前导下划线，例如 `_Network_ProjectileReferenceCache`。这是对通用命名规则的专门约定。
- 同一机制使用统一分类前缀；通用数据按实际用途分类，例如 `AnomalyAI`、`BossBar`、`DamageReduction`、`Depth`、`Network`、`Progression`、`Visual`。避免将已有明确归属的数据放入笼统的通用分类。
- 独立实例数据使用外层 `#region 独立实例数据`，内部按机制或用途适量分组；分类按英文前缀的字母顺序排列，组内按成员名称的字母顺序排列，排序忽略私有字段的前导下划线。注释随对应成员移动，保留客户端专用、临时状态及同步边界等说明。
- AI 数组映射属性放入 `#region AI 数组映射数据`，内部按变种 AI 控制、减益、深度、伤害减免等用途分组。整理声明顺序不得改变槽位索引、变化标记或网络读写顺序。
- 重命名时同步更新全部引用，包括克隆、重置、同步和绘制代码；保持初始化值、克隆策略、重置时机及网络协议不变。
- `Clone`、`ResetEffects` 和网络同步的框架入口保留在主文件，通用逻辑仍在入口中执行；仅将额外数据的处理提取为 `CloneExtraData`、`ResetExtraData`、`SyncExtraData` 等独立方法，分别放入对应实体目录的 `<类名>.Clone.cs`、`<类名>.ResetEffects.cs`、`<类名>.NetSync.cs`。通过部分类方法连接入口与实现，不将 AI 数组克隆、通用快照读写等基础逻辑一并搬出。其他行为按 2.2 节组织。

### 2.2 行为与数据分离

- 仅提供行为、不存储数据的 GlobalType，若存在对应的 Behavior 基类且所用回调会被 Behavior 统一接口调用，应重构为对应的 Behavior；不存在对应基类或回调不参与统一调用时，可保留 GlobalType，但以下高频 `Can...` 回调必须遵循集中实现规则。
- 玩家行为统一使用 `AnomalyPlayerBehavior`；不参与 Behavior 统一调用的回调由 `ModPlayer` 实现，且玩家实例数据仍统一保存在 `AnomalyPlayer`。
- 迁移前必须确认回调具有有效的 Behavior 调度路径，不能仅因基类声明了同名虚方法就迁移。

#### 2.2.1 高频 Can 回调集中实现

- 全局 Behavior 抽象类不提供下表中的 `Can...` 接口；这些判定直接由对应的 GlobalType 或 `ModPlayer` 集中实现。
- 命中及碰撞检查本质上需要遍历两类实体的候选配对，即两个实体集合的笛卡尔积。若两类实体的数量分别为 n 和 m，配对检查的复杂度为 O(nm)。
- 每次配对检查都可能调用相应的全局判定钩子；若有 k 个 GlobalType 或 `ModPlayer` 分别声明同一回调，钩子遍历与调用开销会叠加到这些配对上，最坏可达 O(knm)。因此，重复声明会显著放大高密度实体场景的性能开销；每种实体的同名回调应只在一个全局类型中实现，集中合并各机制的判定条件。
- 本项目的相关实现统一放在 `Anomalies/Common/AnomalyGlobalInstances.CanHit.cs`，玩家使用 `AnomalyPlayer`，NPC 使用 `AnomalyGlobalNPC`，弹幕使用 `AnomalyGlobalProjectile`，物品使用 `AnomalyGlobalItem`。新增实现时只应在这些类中添加代码，不得将逻辑散放在其他位置。
- 全局 Behavior 不提供的 `Can...` 接口如下；新增同类限制时同步维护此表。

| 对应全局类型 | 集中实现的 Can 回调 |
| --- | --- |
| `ModPlayer` | `CanHitNPC`、`CanMeleeAttackCollideWithNPC`、`CanHitNPCWithItem`、`CanHitNPCWithProj`、`CanHitPvp`、`CanHitPvpWithProj`、`CanBeHitByNPC`、`CanBeHitByProjectile` |
| `GlobalNPC` | `CanHitPlayer`、`CanHitNPC`、`CanBeHitByNPC`、`CanBeHitByItem`、`CanCollideWithPlayerMeleeAttack`、`CanBeHitByProjectile` |
| `GlobalProjectile` | `CanHitNPC`、`CanHitPvp`、`CanHitPlayer` |
| `GlobalItem` | `CanHitNPC`、`CanMeleeAttackCollideWithNPC`、`CanHitPvp` |

- `GlobalProjectileBehavior` 同样不提供 `Colliding` 接口；全局碰撞判定也应在对应数据容器的同一专用部分类文件中集中实现。单实体 Behavior 可提供实体专属的判定接口。
- 只声明有实际判定逻辑的回调，不为覆盖列表而添加返回默认值的空重写。合并逻辑时保留各条件的生效范围、短路行为和可空布尔返回语义，尤其不能将表示“不作决定”的 `null` 改为强制允许的 `true`。
- 此集中规则针对全局回调；`ModNPC`、`ModProjectile`、`ModItem` 自身的实体专属回调继续由实体类型实现，不因同名而迁入全局入口。

### 2.3 克隆与同步

- 实体克隆时，必须独立处理可变容器，避免不同实体共享状态。
- 玩家克隆时，可变容器必须独立复制或重置。
- 仅用于绘制的数据不得加入网络快照。

### 2.4 实体生成与初始化

- 生成 NPC、弹幕、Dust、Gore 等实体或视觉对象时，统一使用现有的 Action 后缀接口，复用接口提供的生成检查、初始化及适用的同步逻辑。常用接口包括：
  - `NPC.NewNPCAction`
  - `Projectile.NewProjectileAction`
  - `Dust.NewDustAction`
  - `Dust.NewDustPerfectAction`
  - `Gore.NewGoreAction`
- AI 参数、目标、速度、寿命等生成后的初始化，以及附加属性设置，统一放在 Action 回调内，在实体同步前完成。
- 替换生成接口时，核对坐标语义（中心或左上角）、默认速度、缩放等差异，保持原有表现。
- 具有专用网络协议或召唤语义的框架入口（如 `NPC.SpawnOnPlayer`）予以保留。

### 2.5 AI 行为组织

- 行为函数优先使用本地函数，放在直接调用它们的 AI 方法内；跨阶段或被其他回调共同使用的逻辑保留为类方法。
- 较大的阶段 AI 可拆分为独立文件中的私有阶段方法，由主 AI 入口分派；阶段内的具体行为和辅助函数放在对应的阶段方法内。

### 2.6 Boss 状态机与代码组织

- Boss 及其弹幕的战斗逻辑必须由本项目自身实现，不得依赖灾厄等可选模组的 AI 或绘制覆盖。移植行为时一并迁移运动、计时、颜色与绘制逻辑，通过明确的 AI 参数协议区分模式，并使用本项目的难度设置选择参数。

- Boss AI 统一围绕 `Phase + Behavior + LocalPhase + Timer` 组织：`Phase` 表示战斗阶段（含初始化、转阶段等），`Behavior` 表示当前攻击或退场行为，`CurrentLocalPhase` 表示行为内部步骤，`Timer`（或已有的 `Timer1`、`Timer2`）表示计时。不得继续用裸 AI 数值同时表达多种职责。
- 阶段与行为分别声明为嵌套的 `Phase : byte`、`Behavior : byte` 枚举，使用有意义的成员名；阶段按战斗进程排列，跨阶段共用行为可复用成员，退场可使用 `Despawn = byte.MaxValue`。LocalPhase 通常使用整数，并在属性或行为函数处注明各值含义；它不是另一个战斗阶段枚举。
- 按整体逻辑规模决定是否拆分 AI 文件。规模较大的 Boss 使用 `partial`，将 `AI`、`PreAI` 等 AI 入口及其本地行为函数放到 `<BossName>.AI.cs`；主文件 `<BossName>.cs` 按“枚举、常量与参数、状态属性、其他回调及共享辅助方法”组织。整体逻辑较小的 Boss 或部件（如常规骷髅王手部）应保持单文件，将 AI 放在状态属性之后、绘制及共享辅助方法之前，不为拆分而创建 `.AI.cs`。是否拆分不以主 Boss 或部件身份决定，也不以单个 AI 方法的行数作为唯一依据。较大的阶段 AI 可继续拆到 `<BossName>.Phase1AI.cs`、`<BossName>.Phase2AI.cs` 等阶段文件，由 `.AI.cs` 中的入口分派；状态及绘制也可按规模分别拆入 `.State.cs`、`.Drawing.cs`。
- 通用入口为 `CurrentPhase`、`CurrentBehavior`、`CurrentLocalPhase` 和计时属性。参考蜂王、克苏鲁之眼，将阶段和行为打包进 `AI_Union_0` 的 `Byte0`、`Byte1`；写入时先取出 `Union32`，只修改对应字节再写回，避免覆盖其他状态。状态改变时由服务器设置 `NPC.netUpdate`。
- `CurrentLocalPhase` 通过属性封装同步槽位，改变时请求同步；计时器可使用 `ref float` 属性映射 AI 槽位，不能为逐帧增长无条件请求同步。额外状态使用现有 `AnomalyAI32` 存储，按同步协议设置相应变化标记。仅绘制数据遵循 2.3 节，不放入网络状态。
- 若原版槽位承担部件主人索引、左右侧等不可复用的职责，应保留这些槽位，并在额外同步槽位打包阶段与行为。记录每个槽位的用途；部件通过主人 Behavior 的属性读取状态，禁止直接依赖主人的裸 AI 槽位或魔法数。
- 使用 `Phase2`、`Phase3`、`ShouldEnterPhase2` 等派生属性集中表达阶段范围与进入条件。血量阈值和攻击参数集中命名，阶段判定不能散落在各行为中。重构时保留原有阈值边界、阶段重叠关系及难度差异。
- AI 入口先处理同步、目标有效性、初始化及通用属性，再处理阶段变化和行为分派；使用 `switch (CurrentPhase)` 或 `switch (CurrentBehavior)` 清楚展示调度关系。可并行发生的传送、弹幕等逻辑使用明确命名的本地函数，在原有执行位置调用。
- 具体行为函数放在直接调用它的 AI 方法内，LocalPhase 的推进在行为函数内部组织。大型阶段按 2.5 节拆成私有阶段方法，阶段方法再定义本地行为函数；跨回调、跨实体复用的辅助方法保留为类方法。
- 切换行为时明确设置新行为所需的 LocalPhase、Timer 及临时状态；不能默认所有计时器都应清零，独立的传送、预警和生命周期计时按各自语义保留。保持每帧执行一个行为分支，除非原设计明确要求当帧继续执行下一步。
- 服务器负责随机决策、实体生成和权威状态切换，客户端按同步状态预测表现。结构重构必须检查部件联动、退场、狂暴、首次初始化及中途加入同步，并保持原有攻击时序、伤害和生成坐标语义。

#### 2.6.1 Boss 模块解耦

- `Anomalies/Bosses/<BossName>/` 是独立模块。删除任意一个直属 Boss 子文件夹后，项目必须仍能编译、加载，其他 Boss 的 AI、伤害、同步、绘制和资源加载行为保持不变；不得要求同时修改其他文件。
- Boss 模块不得引用其他 Boss 模块的类型、静态成员或资源路径。通过原版 NPC/弹幕 ID 交互时，不得依赖另一模块的自定义实现必然存在。宝石等相似机制按所属 Boss 拆分专属 Handler、状态与资源；独立使用的音效或贴图应放入公共资源目录或各自拥有完整副本。
- 可复用且不依赖 Anomalies 的寻敌、运动、几何、插值及实体工具放入 Transoceanic 对应 API；依赖变种模式、Anomaly 数据或视觉系统的通用逻辑放入 Anomalies 对应公共层。Boss 的攻击参数、AI 槽位布局和专属兼容逻辑保留在本模块，不能仅重命名后整体移入公共层。
- 公共层和其他内容不得直接引用 Boss 类型。Boss 专属的掉落、血条扩展、网络处理、测试物品等集成实现放入所属 Boss 文件夹，通过可选部分类方法或明确的注册接口接入公共入口；缺失模块时入口自然不执行，不得用反射按名称查找类型掩盖硬依赖。
- Boss 专属 GlobalType 数据仍由统一数据容器持有。作为 2.1.1 文件位置规则的特例，涉及 Boss 类型的字段及其克隆等处理放入 Boss 文件夹中的容器部分类文件，公共容器只声明可选连接方法。删除模块后不得遗留公共字段对 Boss 类型的引用。此例外不允许另建存储数据的 GlobalType，也不改变同步槽位或网络协议。
- 模块移除后，公共网络入口仍应消费已定义消息的固定载荷，保持消息 ID 与读取顺序不变；Boss 专属处理通过可选入口接入。注册逻辑保持原调用时机，避免内容加载先后顺序改变其他 Boss 的行为。
- 调整目录和资源归属时遵循 1.3，逐一核对默认 Texture 路径、特效、粒子、音效和本地化。修改后运行完整构建，并用 `Tests/BossIsolation/Verify.ps1` 逐模块排除源码进行编译验证；同时检查跨模块资源引用。编译检查不能代替游戏内战斗、联机与资源加载验证。

### 2.7 碰撞检测

- 永远不要使用 `Collision` 类的带宽度线段碰撞检测（`Collision.CheckAABBvLineCollision` 的带宽度重载），也不得通过包装方法间接调用；该检测存在严重漏检问题。
- 矛身、刀刃、射线及位移扫掠等带宽度区域统一使用 Transoceanic 的 `RotatedRectangle.FromLineSegment(start, end, width).Collides(targetHitbox)`。`width` 为完整宽度，端点对应矩形两条短边的中点；保持原有端点、宽度、缩放和扫掠采样，检查零位移、包围和斜向接触。
- 需要首次接触距离或目标排序时，基于旋转矩形另行求解，不得依赖上述线段检测接口。圆形、实体 AABB 和地形碰撞继续使用适用的几何或地形接口。

### 2.8 钩子生命周期

- Anomalies 项目中禁止手动卸载钩子；钩子的卸载由 tModLoader 统一管理。注册 `On_*`、`IL_*` 等钩子后，不得在 `Unload`、`OnModUnload` 或辅助方法中重复退订、移除、撤销或释放钩子，也不得为此保留卸载调用链。
- 删除手动卸载逻辑后，同时删除仅承担该职责的空回调和辅助方法。钩子注册及其执行逻辑保持原有时机与行为。
- 此规定仅针对钩子卸载；运行时创建的纹理等自有资源仍需释放，静态引用、缓存与世界状态仍按原有生命周期清理，不得将这些必要清理一并删除。

## 3. 着色器规范

### 3.1 入口与编译配置

`Anomalies` 项目中的着色器统一使用 Shader Model 3.0（`ps_3_0`），`technique` 名称统一为 `Technique1`。根据是否包含多个 Pass，分别遵循以下入口与命名规则。

**没有多 Pass 的着色器（仅有一个 Pass）必须严格遵循以下命名要求，不得因多 Pass 特例而放宽：**

| 配置项 | 统一值 |
| --- | --- |
| 主函数名称 | `Main` |
| 像素着色器模型 | Shader Model 3.0（`ps_3_0`） |
| `technique` 名称 | `Technique1` |
| `pass` 名称 | `Pass0` |

- 以上名称的大小写必须保持一致；辅助函数可按用途命名。
- 单 Pass 着色器的编译入口必须写为 `PixelShader = compile ps_3_0 Main();`，C# 中注册、加载或应用时必须使用 `Pass0`。

**多 Pass 着色器：**

- 保留默认效果的 `Pass0`，其他 Pass 使用体现用途或主题的 PascalCase 名称，名称在同一 `Technique1` 中不得重复。
- 允许多个 Pass 共用 `Main`，通过编译期 `uniform` 参数选择效果；编译入口可以写为 `PixelShader = compile ps_3_0 Main(常量参数);`。例如 `EventideReunion.fx` 的默认名称效果使用 `Pass0` / `Main(0.0)`，六相效果使用 `Sun`、`Samsara`、`Life`、`Death`、`Sacredness`、`Star`，第二行渐变使用 `FinalSun`，开发者名字的混沌效果使用 `Chaos`。
- C# 中注册、加载或应用时，必须引用实际需要的 Pass 名称，并与文件中的拼写和大小写完全一致；默认效果仍使用 `Pass0`。通过 `Effect.CurrentTechnique.Passes[passName].Apply()` 选择效果时，应先设置该 Pass 所需的参数。
- 这些 Pass 可作为独立效果按需选择，不要求依次叠加执行；不得仅为规避单 Pass 命名要求而添加无用途的 Pass。

无论单 Pass 还是多 Pass，像素着色器入口名称均保持为 `Main`，不得使用 `ps_2_0`、`ps_4_0` 或其他模型版本。

### 3.2 长宽比适配

- 视觉效果不得因屏幕长宽比变化而拉伸或压缩；图案比例、圆形轮廓、纹理尺度及空间动画速度应保持一致。
- 依赖屏幕归一化坐标的效果必须进行长宽比修正，以 16:9 为视觉基准，保持该比例下原有的图案、尺度和动画表现不变。
- 对分别按屏幕宽、高归一化的坐标，可将 X 分量乘以 `(width / height) / (16.0 / 9.0)`，转换到以 16:9 为基准、按高度统一尺度的空间。
- 已使用世界坐标、局部像素坐标或已有长宽比修正的效果，不得重复修正。
- 计算距离、半径、方向与折射偏移时，必须区分修正后的空间坐标和纹理采样 UV，确保二者转换正确。
- 修改后，检查 16:9 下的视觉兼容性，并验证 4:3、16:10、超宽屏和竖屏下均无比例拉伸。

### 3.3 着色器引用与生命周期

- 一般着色器引用统一存储在 `AnomalyEffects` 及其嵌套类中，按资源目录归入 `Primitives`、`Overlays`、`Texts`、`BossBars` 或 `ScreenFilters`；嵌套类内字段按字母顺序排列，名称与资源文件名保持一致。
- 可沿用现有加载方式的着色器，复用 `AnomalyEffects` 的统一加载、注册和卸载流程；绘制类从对应容器的 `Asset.Value` 获取 `Effect`，不得另行保存静态 `Asset<Effect>` 或在绘制时重复请求同一资源。确需特殊加载方式时，应说明原因及生命周期管理方式。
- 调用方必须兼容专用服务器、关闭着色器配置或资源尚未加载时容器为空的情况；无可用着色器时跳过对应效果，或沿用已有的不依赖着色器的绘制路径。
- 同一着色器在不同效果间复用时，每次绘制显式设置所需参数和 Pass，避免共享状态影响后续绘制。

## 4. 测试文件组织

- 测试文件统一放在仓库根目录下的 `Tests/` 目录中。
- `Tests/` 测试文件和 `docs/` 改动记录不作为 Git 项目成员，通过项目维护者提供的独立验证资料包获取；不得为新测试或改动记录添加忽略例外。已有被跟踪文件不能仅靠 .gitignore 取消跟踪，调整索引时保留本地文件。
- 验证资料包应与源码构建对应，包含测试源码、项目、验证脚本、必要审计资料及改动记录；未取得资料包时明确报告验证不可用，不将缺失文件认定为产品源码缺陷。
- 面向框架使用者的 README、接入指南和迁移文档放在 Transoceanic 项目内，随源码维护；生成的 Transoceanic.xml 为构建产物，不纳入 Git。
