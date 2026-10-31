# AnomalyReborn 代理工作指南

本文件适用于整个仓库，帮助代码代理定位实现、遵守设计约束并完成验证。

## 开始工作前

- 先阅读根目录 `DESIGN_GUIDELINES.md`；它是完整设计规范，本文件是执行摘要，不能用摘要放宽原文要求。
- 修改具体模块前，阅读其目录内的设计文档和 README。例如落日遗逢遵循 `Anomalies/SpecialContents/Developer/EventideReunion/DESIGN_GUIDELINES.md`，Boss 可参考各自 README。
- 修改 Behavior 或 Handler 生成逻辑前，阅读 `docs/behavior-generation.md`；修改同步或着色器时，参考 `docs/network-sync-review.md`、`docs/mod-entity-sync-audit.md`、`docs/shader-aspect-ratio-audit.md`。审计文档记录历史状态，其中的路径、测试及结论须以当前源码核实。
- 先检查工作区差异，保留已有修改。围绕任务进行改动，不附带全仓库格式化或无关重构。

## 仓库与依赖

| 路径 | 职责 |
| --- | --- |
| `Anomalies/` | 模组内容、变种行为、Boss、公共实体数据和视觉系统 |
| `Transoceanic/` | 通用框架与 API；可复用工具不能反向依赖 Anomalies |
| `TransoceanicCodeAssist/` | Roslyn 分析器与源生成器 |
| `AnomaliesMusic/` | 音乐资源模组 |
| `HumanMiracle/` | 独立模组项目 |
| `Tests/` | 全部测试源码与验证脚本 |
| `docs/` | 实现说明与专项审计记录 |

- 主模组与框架使用 .NET 8、C# `preview`；代码辅助项目面向 `netstandard2.0`，不要统一改写目标框架。
- 模组项目导入仓库外的 `../../tModLoader.targets`，Anomalies 还引用相邻 CalamityMod 的 DLL。完整解决方案包含相邻仓库源码项目，公共解决方案为 `AnomalyRebornPublic.slnx`。
- 不要把本机绝对路径写入项目配置，或为修复环境缺失而随意修改相邻仓库。构建受依赖阻塞时，报告具体缺失项。

## 命名、属性与资源

- 类型、属性、方法、事件和对外直接访问的字段使用 `PascalCase`；不期望外部直接访问的字段、事件封装成员使用 `_PascalCase`。
- 参数、局部变量使用 `camelCase`；所有常量（包括私有与局部常量）使用 `PascalCase`，不加下划线。
- 无自定义逻辑时使用自动属性；自定义访问器能够使用 `field` 时必须使用，不另建后备实例字段。
- 遵循 `.editorconfig`：C# 四空格缩进、CRLF、文件范围命名空间、显式类型优先，花括号换行。
- 命名空间与目录不一致的内容类型必须显式重写 `Texture`。移动文件或重命名时同步核查主材质、发光材质、其他资源、显式请求路径及类型引用；无材质加载职责的辅助类无需添加 `Texture`。

## 实体数据与行为

- GlobalType 实例数据归入 `AnomalyGlobalInstances` 的对应容器：`AnomalyGlobalNPC`、`AnomalyGlobalProjectile` 等；玩家实例数据统一放在 `AnomalyPlayer`。客户端视觉数据也遵守此规则，不另建仅存储数据的 GlobalType 或 ModPlayer。
- `ModNPC`、`ModProjectile`、`ModItem` 等实体自身可以持有各自数据。
- `AnomalyGlobalInstances.cs` 只保留稳定的通用逻辑与框架入口。独立实例数据放入 `Common/GlobalInstances/<类名>/<类名>.ExtraData.cs`，AI 槽位映射放入同目录的 `.AIData.cs`，无内容不创建空文件。
- 独立实例数据按 `分类_名称` 命名，私有字段保留前导下划线。同一机制使用一致前缀；使用 `独立实例数据` region，分类和组内成员按字母排序，排序忽略前导下划线。AI 映射使用 `AI 数组映射数据` region，按用途分组。
- 额外数据的克隆、重置和同步实现分别放入容器目录的 `.Clone.cs`、`.ResetEffects.cs`、`.NetSync.cs`，通过部分类方法连接主文件入口；通用 AI 数组克隆、快照读写仍留在主文件。
- 克隆可变容器时独立复制或重置，防止实体间共享状态。仅绘制数据不得进入网络快照。
- 纯行为优先使用对应 Behavior，玩家行为使用 `AnomalyPlayerBehavior`。迁移前确认回调存在实际调度路径，不能只检查基类有同名虚方法；未参与统一调度的回调可保留在 GlobalType 或 ModPlayer 中。

### 全局命中与碰撞入口

- 设计规范 2.2.1 列出的高频全局 `Can...` 回调，以及全局弹幕 `Colliding`，统一由 `Anomalies/Common/AnomalyGlobalInstances.CanHit.cs` 内对应容器实现，不在全局 Behavior 或其他全局类型中重复声明。
- 新增判定先核对设计规范的完整回调表，只实现有实际逻辑的回调；保持适用范围、短路顺序及可空布尔语义，不能将“不作决定”的 `null` 改成 `true`。
- 单实体 Behavior 与实体自身的专属判定按现有调用路径实现，不因同名迁入全局入口。

### 生成与 AI 组织

- NPC、弹幕、Dust、Gore 等使用现有 `NewNPCAction`、`NewProjectileAction`、`NewDustAction`、`NewDustPerfectAction`、`NewGoreAction` 接口；初始化放在 Action 回调中，在同步前完成。
- 替换接口时核查中心/左上角、默认速度、缩放等语义；保留 `NPC.SpawnOnPlayer` 等具有专用网络或召唤语义的入口。
- 行为函数优先放在直接调用它的 AI 方法中，使用本地函数；跨阶段或跨回调共享逻辑保留为类方法。大型阶段可拆成私有阶段方法。

## Boss 状态机与模块边界

- Boss 及其弹幕的 AI、运动、计时、颜色和绘制由本项目实现，不依赖灾厄等可选模组的覆盖；参数通过本项目难度设置选择。
- 状态机按 `Phase + Behavior + LocalPhase + Timer` 分责。阶段与行为用嵌套 `Phase : byte`、`Behavior : byte` 枚举，局部步骤通常用整数并注释含义，不用裸 AI 数值混合多种职责。
- 使用 `CurrentPhase`、`CurrentBehavior`、`CurrentLocalPhase` 和计时属性。参考蜂王、克眼，将阶段和行为放入 `AI_Union_0` 的 `Byte0`、`Byte1`；取出 `Union32` 后只改目标字节再写回。
- 状态变化由服务器请求同步；LocalPhase 封装同步槽位。计时器可使用 `ref float`，不要因逐帧递增无条件同步。额外状态沿用 `AnomalyAI32` 和变化标记。
- 保留主人索引、左右侧等已有槽位职责，记录槽位用途；部件通过主人 Behavior 的属性访问状态，不能依赖其裸槽位或魔法数。
- 阶段阈值、进入条件与攻击参数集中命名。入口先处理同步、目标、初始化与通用属性，再明确分派阶段和行为。
- 切换行为时明确处理 LocalPhase、计时器和临时状态；独立计时器按语义保留。保持原攻击时序、阈值边界、难度差异和每帧分支推进语义。
- 按整体规模决定拆分：较大 Boss 可用 `.AI.cs`、`.Phase1AI.cs`、`.State.cs`、`.Drawing.cs`；较小 Boss 或部件保持单文件，不为拆分而拆分。
- 每个 `Anomalies/Bosses/<BossName>/` 都是可独立移除的模块。移除任一直属 Boss 文件夹后仍须编译、加载，其他 Boss 的行为与资源加载保持不变。
- Boss 模块不得引用其他 Boss 模块的类型、静态成员或资源；公共层和其他内容不得直接引用 Boss 类型。专属掉落、血条、网络、测试物品等集成代码留在本模块，通过可选部分类方法或明确注册接口接入，不能靠反射按名查找掩盖依赖。
- 涉及 Boss 类型的统一容器字段及其处理，作为文件位置特例放在 Boss 模块内的容器部分类中；不因此另建数据容器。公共入口仅保留可选连接方法。
- 模块移除时公共网络入口仍消费固定消息载荷，保持消息 ID、读取顺序和注册时机。
- 无 Anomalies 依赖的通用几何、运动、寻敌等放入 Transoceanic；依赖变种系统的通用逻辑放入 Anomalies 公共层；专属参数、槽位与兼容逻辑留在 Boss 模块。

## 联机、碰撞与生命周期

- Boss 随机决策、生成和权威状态切换由服务器负责；客户端按同步状态预测表现。其他实体生成沿用现有服务器或所属客户端的权威边界，避免重复生成。
- 修改同步字段时成对检查写入/读取、类型、顺序、变化标记、初始值、克隆及重置；结构重构不得顺带改变协议。检查专用服务器、中途加入、部件联动、初始化、退场与狂暴。
- 禁用 `Collision.CheckAABBvLineCollision` 的带宽度重载，包括间接包装调用。带宽度区域使用 `RotatedRectangle.FromLineSegment(start, end, width).Collides(targetHitbox)`；`width` 是完整宽度，端点为短边中点。
- 保留端点、宽度、缩放和扫掠采样语义，检查零位移、包围与斜向接触。首次接触距离及排序另行求解；圆形、AABB 和地形碰撞继续使用适合的接口。
- Anomalies 中禁止手动卸载 `On_*`、`IL_*` 等钩子，交由 tModLoader 管理；移除仅为卸载存在的空回调及调用链。自有纹理释放、缓存、静态引用和世界状态清理仍须保留。

## 着色器与绘制

- Anomalies 的像素着色器统一使用 `Main`、`ps_3_0`、`Technique1`。单 Pass 必须命名 `Pass0`，编译入口为 `PixelShader = compile ps_3_0 Main();`，C# 使用相同 Pass 名。
- 多 Pass 保留默认 `Pass0`，其他 Pass 使用有意义且唯一的 PascalCase 名称，可通过 `Main(常量参数)` 选择效果；不添加无用途 Pass 规避命名要求。调用前显式设置参数和正确 Pass。
- 一般资源引用集中在 `AnomalyEffects` 对应嵌套类，字段按字母排列并与资源文件同名，复用统一加载与卸载；绘制处使用 `Asset.Value`，不额外静态缓存或逐帧重复请求。特殊加载须说明原因和生命周期。
- 兼容专用服务器、关闭着色器及资源未加载的情况，跳过效果或使用已有回退。共享 Effect 每次绘制都设置所需参数，防止状态串用。
- 屏幕归一化效果以 16:9 为视觉基准，必要时对 X 乘 `(width / height) / (16.0 / 9.0)`；世界坐标、像素坐标或已有修正不得重复处理。区分空间坐标与采样 UV。
- 修改后验证 16:9 的表现兼容，以及 4:3、16:10、超宽屏、竖屏下的比例和动画速度。
- 当前 Anomalies 项目禁用了统一着色器预编译入口；修改 `.fx` 时检查现有编译器与调用方式，不能将 C# 构建通过视为着色器编译成功。

## 验证与交付

以下命令均从仓库根目录运行，按修改范围选择。新增测试统一放在 `Tests/`，不在内容目录散放测试源码。

```powershell
# 主模组 C# 构建；BuildMod=false 避免模组打包
dotnet build Anomalies/Anomalies.csproj -p:BuildMod=false -m:1

# 框架命名空间、构建及 AutoHosting / Primitives 测试，同时检查消费项目
pwsh -File Tests/Transoceanic/Verify.ps1 -IncludeConsumers

# Behavior / Handler 生成及 CanHit 路由
dotnet run --project Tests/Transoceanic/Behaviors/BehaviorGenerator.Tests.csproj

# 世界吞噬者链拓扑
dotnet run --project Tests/EaterofWorlds.Topology/EaterofWorlds.Topology.csproj

# Boss 模块依赖与逐模块排除编译（需要已还原依赖）
python Tests/BossIsolation/verify_dependencies.py
pwsh -File Tests/BossIsolation/Verify.ps1
```

- 修改框架须验证消费方；生成器修改须运行 Behavior 测试，不能假设框架验证脚本包含它。
- 调整 Boss 模块目录或资源归属后，必须完整构建并运行 Boss 隔离验证，同时检查跨模块资源引用。
- 已还原依赖时可用 `--no-restore`；缺少还原产物时先解决依赖，不把环境失败当作代码验证通过。
- 编译和静态测试不能替代游戏内战斗、多人同步、动态资源加载与视觉验证。只报告实际执行的检查及结果，明确尚未验证的部分。
- 纯文档修改检查内容、引用路径及差异即可；代码修改运行与风险相称的验证。交付说明修改内容、验证结果及剩余限制。
