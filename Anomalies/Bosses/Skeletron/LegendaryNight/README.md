# 传说之夜代码组织

全部源码统一使用 `Anomalies.Bosses.Skeletron.LegendaryNight` 命名空间，不随子目录新增命名空间层级。所有内容属于现有 `ANOMALY_MODE`；专属贴图、音效跟随功能目录移动，代码显式引用实际路径。

## 目录与职责

| 目录 | 职责 |
| --- | --- |
| `Head`、`Hand` | 主体/手部状态机、绘制与手部专属绘制钩子 |
| `Arena` | 场地生命周期、边界、法阵几何、公转、实体化碰撞、八元素圆几何 |
| `Attacks/Bone` | 头骨预警 |
| `Attacks/Slime` | 飞行史莱姆 NPC、坐骑、史莱姆雨及专属蓄力粒子 |
| `Attacks/Stars` | 落星、拍击落星、恶魔之影、陨石 |
| `Attacks/Coins` | 金币、轮盘、审判及其专属 Overlay |
| `Attacks/Starburst` | 星爆公共音乐时序、单次星爆弹幕和音效 |
| `Attacks/Stellar` | 中心恒星、蓝星、光球、激光、结算冲击波及专属粒子 |
| `Attacks/Finale` | 八元素发射编排、元素载体、终幕陨星 |
| `Scene` | 天空、天空星点、云层、实体照明、世界时间、音乐场景 |
| `Players` | 星神祝福 Behavior 和玩家光晕绘制层，数据仍在统一 AnomalyPlayer |
| `Rendering` | 场地遮罩/预警/跨实体绘制次序，以及天空和落星共用的绘制工具 |
| `Particles` | 仪式和主体共用的蓄力星芒 |
| `Combat` | 保留现有免疫语义的本地玩家伤害结算 |
| `Buffs`、`Summoning` | 减益、祝福提示、祭坛、召唤物与仪式 |

不再使用 `Entities`、`Misc` 目录。`LegendSlapStar` 是原 `Mechanic2Star` 的用途命名；本地化已同步更名。

## 攻击和场地边界

机制六从开始蓄力起执行四次步进星爆，每次由中心圆和外侧三环依次起爆，前四次共十六层连续间隔 95 帧。第二至第四次的中心圆分别与第 1、3、5 波激光的伤害起始帧重合，第五次的中心圆与最后一波 12 发激光同时开始造成伤害。只有第一次的中心圆有淡红色预警，从第十二次普通星爆开始淡入。各层在起爆前 6 帧播放普通星爆音效。以竞技场半径为 R，分界半径依次为 R/2、R/√2、√3R/2、R，每层各占场地四分之一。伤害按玩家中心判定，公共边界归内侧。每层金色火焰沿用普通星爆的时序，淡入 10 帧，维持并造成伤害 24 帧，再淡出 18 帧。

普通星爆和钱币审判的金色火焰在伤害生效前淡入 10 帧。普通星爆在整个 24 帧伤害窗口内维持峰值，钱币审判在单帧结算时达到峰值，并沿用 24 帧的火焰维持时间。

Head 保持阶段与音乐时钟的所有权。`Mechanic3/4/5Timeline` 持有各星爆音乐节点，`LegendaryNightStarburstTimeline` 提供共享节拍查询，`LegendaryNightStarburstParameters` 提供相对窗口、音效和颜色，`Arena.Orbits` 提供连续公转及爆点预测，服务器通过 `LegendStarburst.EnsureAttack` 确保当前窗口存在一枚星爆；星爆自己的 AI 负责音效、伤害与淡出。法阵蓄光通过时序查询计算，不依赖星爆实体是否已经出现。

二阶段法阵在拉回目标前判定本机目标是否越界。场外时间每帧加一，回到场内每帧减一。越界时立即结算并以 30 帧为间隔重复，伤害随越界时间从 30 线性增加，在 120 帧达到上限 396。越界结算沿用错色光球的直接伤害入口并绕过受击无敌帧。

机制二的半场拍击伤害在 Head 中结算。`LegendaryNightElementBarrage` 由机制七调度，负责八元素载体补建、发射、声音与出现粒子；Arena 只提供圆的位置、半径与旋转。

机制五在 1:28:15 开始稀疏落星和雨伞史莱姆。三波坐骑按一阶段常态符卡的对应间距从场地上方直接下落，同时启动玩家头顶的轮盘。轮盘在每轮第 90 帧锁币，锁定币种继续显示在玩家头顶直至结算，复制币和蓝圈出现在法阵中心，第 190、380、570 帧各结算一次审判。连续星爆期间，左右手各自处理蓄势、挥击和回收，最后一拍回收接入举球动作。

中心黄色光球及膨胀后的恒星共用 `SphereRadius` 圆形接触判定，黄色阶段伤害为300，开始变红后为450，命中施加15秒群星威压；完全消散后停止碰撞。错色拦截的180伤害与蓝星撞击按吸收数量计算的伤害均为直接结算值，使用 `PlayerHurtDamageIgnoresDifficultyScaling` 避免 `Player.Hurt` 按弹幕来源再次乘难度倍率；防御、减伤和原有免疫规则继续由受击流程处理。其他使用 `ProjectileDamageContainer` 的攻击继续沿用原有难度换算。

恒星系统的三个主要弹幕职责：

- `LegendEnergySphere`：中心星、光球生成/拦截/吸收、玩家奖励和循环音效。
- `LegendBlueStar`：蓝星公转、透视投影、物质转移和吸积流缓存；通过 owner + identity 引用中心星，服务器生成撞击冲击波。
- `LegendCompanionShockwave`：独立持有爆发半径、伤害、单次命中标记和爆裂粒子。

`Attacks/Stellar/LegendStellarShockwave.cs` 定义扩张与绘制基类；蓝星结算实现位于同目录 `LegendCompanionShockwave.cs`，终幕实现位于 `Attacks/Finale/LegendFinalShockwave.cs`。两者保留各自伤害规则。普通符卡的 `LegendShockwave` 通过 `BaseShockwaveProjectile` 使用通用的 `Shockwave` 着色器绘制波前、余波与火花。关闭着色器时回退到纹理光环。

Overlay 在原层次调用中心星的完整绘制入口，由中心星组合蓝星后层、星体、光球、核心、蓝星前层。终幕冲击波使用自身的普通弹幕绘制入口，由终幕陨星在落点生成并独立计时。主体被成功击杀后，陨星同步记录成功标记，仍在终幕生成无伤害的淡金色光环：外圈与失败版本使用相同的最大半径与扩张曲线，并减弱爆炸音量和震屏；失败版本保留原秒杀判定。成功版本使用共享着色器的 CurtainCall Pass 绘制，包含浓金色光环和十六枚符文星芒，中心圆角五角星展开后接续两轮烟花；关闭着色器时使用光环回退，并继续绘制烟花。

## 着色器命名与职责

着色器按实际视觉职责命名，恒星系统统一使用 LegendStellar 前缀。文件名、AnomalyEffects 字段和编译产物保持一致；实体类型名不随着色器重命名。资源继续由统一容器加载和清理。

| 分类 | 名称 | 职责 |
| --- | --- | --- |
| Overlays | LegendImpactFlame | 钱币审判、二阶段拍击与星爆共用的金色火焰。橙色边缘沿危险区并集绘制，亮度逐像素取覆盖该位置的攻击中的最大值 |
| Overlays | Shockwave | 通用扩张冲击波 |
| Overlays | LegendStellarExplosion | Pass0 为基础爆炸，CurtainCall 为成功谢幕，SupernovaRemnant 为伴星撞击与终幕失败的星云，GoldenImpact 为转阶段落星 |
| Overlays | LegendStellarSphere | 星体基础球面、星野和 Core 星核层 |
| Overlays | LegendStellarPhotosphere | 恒星光球层、日冕与消散 |
| Primitives | LegendStellarAccretion | 星体间的物质吸积流 |
| Primitives | LegendStellarLaser | 恒星激光主体 |
| Primitives | LegendStellarLaserWarning | 恒星激光预警 |
| Primitives | LegendStellarLightOrb | 恒星系统的小型光球 |
| Overlays | LegendaryNightArenaMist | 场地外侧雾幕 |
| Overlays | LegendaryNightArenaEdge | 场地金色边缘 |
| Texts | LegendaryNightGold | 名称与提示的金色文字 |
| BossBars | Skeletron | 骷髅王血条，包含传说之夜表现 |

一阶段转二阶段的落星由 `LegendTransitionShockwave` 独立绘制。它展开双层金色外圈，向外抛出星屑，并留下带金色星芒的短时星云。

终幕两种冲击波的内外圈独立消散。成功时外圈符文星芒逐渐消散，内圈五角星炸成两轮带尾迹的金色火花，受重力下坠并逐颗熄灭。失败时外壳缓慢变稀，内侧抛射物则扩散成更持久的冷色残云。

两种终幕冲击波的外圈先沿原扩张曲线推进，第 36 帧起连续减速并继续越过初始半径，直至消散。内圈和失败判定沿用各自的时间与半径。

落星飞行星屑使用本模块的 `StarTrailParticle` 及蓝、黄、橙、红四种颜色粒子，贴图位于 `Attacks/Stars/`。蓝、黄贴图分别保留原版 Gore 17、16，橙、红版本保留星形轮廓与高光调色。暖色拖尾使用黄橙星芒；蓝色拖尾保留原版黄蓝比例及十周年世界变体。生成频率、初始漂移、缩放衰减和发光沿用原版，生命周期与清理由 ParticleHandler 管理，不再调用原版 Gore。

## 实体数据槽位查阅

每个实体主声明统一使用 XML `<summary>` 描述用途，`<remarks>` 内的 `<para>` 依次列出原版 `ai`、`localAI`、框架 Timer、额外同步载荷和客户端状态。未使用的槽位明确说明；继承的有效槽位也列在具体实体中。

弹幕 Timer1/Timer2 分别对应 `OceanAI32[2].I`、`[3].I`；NPC Timer1/Timer2/Timer5 分别对应 `OceanAI32[4].I`、`[5].I`、`[8].I`。这些不属于原版 `ai` 数组。主体/手部的槽位复用说明位于各自主文件的类型 XML 注释；对应语义属性仍放在 State 或主文件中。

所有跨端弹幕引用使用 owner + identity。粒子缓存、一次性本地音效/命中标记和绘制顶点不加入网络快照。世界时间系统独立于客户端天空运行，专用服务器仍能控制时间。

## 验证资料

自动化测试、审计、覆盖记录和历轮资料由仓库外独立验证资料包提供。本文保留当前产品结构与实现约定。

## 文件职责

- `LegendaryNightHandler.cs`：共享路径、颜色、主体身份检查、主体 Behavior 查询和祝福粒子。
- `Head/SkeletronLegendHead.cs`：Behavior 身份、默认属性、受击与死亡规则。
- `Head/SkeletronLegendHead.State.cs`：枚举、同步槽位映射、竞技场引用、阶段和表现查询；所有伤害容器、直接结算伤害、伤害公式和增减伤倍率集中在“伤害数据”region。
- `Head/SkeletronLegendHead.AI.cs`：目标选择、退场、阶段分派及跨阶段攻击辅助。
- `Head/SkeletronLegendHead.Phase1AI.cs`：常态/自转（Spin）与符卡组合，符卡池与行为切换。
- `Head/SkeletronLegendHead.PhaseChangeAI.cs`：转场开始、旧攻击清理、回血、召手、祝福和进入二阶段。
- `Head/SkeletronLegendHead.Avatar.cs`：化身选择、切换、计时、朝向和切换粒子。
- `Head/SkeletronLegendHead.Drawing.cs`：主体、化身、拖尾、光晕、图标和血条着色。
- `Head/Phase2/SkeletronLegendHead.Phase2AI.cs`：二阶段时钟、阶段推进、七套机制分派及竞技场恢复。
- `Head/Phase2/SkeletronLegendHead.Mechanic1.cs` 至 `Mechanic7.cs`：每个文件包含一套机制的参数、纯计算及主体 AI；不合并机制。各 `MechanicNTimeline` 和 `MechanicNParameters` 保持为 Head 的嵌套类型，分别集中音乐绝对帧与相对时长、运动/攻击参数和纯计算。
- `Hand/SkeletronLegendHand.cs`：身份、实体规则、动作槽位映射、共享几何与主体发出的手部命令。
- `Hand/SkeletronLegendHand.AI.cs`：主人验证、公共更新、手部运动优先级及各运动的本地函数。
- `Hand/SkeletronLegendHand.Drawing.cs`：手掌、预警、脱离骨节绘制、悬停框和血条位置。

## 状态与时间

`Phase` 是战斗阶段，`Behavior` 是一阶段常态/自转（Spin）等行为，`Spell` 是独立符卡选择，`CurrentLocalPhase` 是当前行为内部步骤。阶段和行为分别保留在原版 AI 联合体的 Byte0、Byte1，符卡保留在 Byte2。

| 时间 | 用途 | 重置边界 |
| --- | --- | --- |
| `Timer1` | 当前行为或局部步骤的帧数 | 按各行为原有切换规则重置 |
| `Timer2` | 转场回血插值的初始生命值 | 转场开始时保存 |
| `Timer5` / `Phase2MusicElapsedTime` | 从转场开始累计的音乐绝对帧 | 开始转场时归零，机制切换不重置 |
| `Phase2ElapsedTime` | 二阶段生存时间，供动态减伤使用 | 进入二阶段时归零，最多累计 10800 帧 |
| `Mechanic2Age` | 相对机制 2 起点的帧数 | 由音乐时钟和同步起点计算 |

不要按枚举声明位置重新编号：机制 5/6/7 的网络值分别是 8/9/10，历史 `Phase2_Finale` 值 7 继续保留。阶段分类由显式查询统一解释，不依赖枚举大小顺序；未知网络值不激活二阶段音乐或场地。

## 同步数据

| 数据 | 当前同步位置 |
| --- | --- |
| 竞技场引用 | `AnomalyAI64[1]` 的 owner + identity |
| 一阶段审判币种位图、机制五首轮审判币种 | `AnomalyAI32[0]` 的低四位和高位 |
| 化身、化身计时、化身旋转 | `AnomalyAI32[1]`、`[2]`、`[3]` |
| 生存时间、正在召手、拍击左右位图、机制 2 音乐起点 | `AnomalyAI32[4]` 至 `[7]` |
| 停靠起点/终点/初速度 | `AnomalyAI64[2]`、`[3]`、`[4]`，仅停靠期间有效 |
| 机制 2 拍击起点/目标 | `AnomalyAI64[2]`、`[3]` |
| 机制 3 跳跃起点/落点 | `AnomalyAI64[2]`、`[3]` |
| 机制 4 回中起点/冲刺速度 | `AnomalyAI64[2]`、`[3]` |
| 停靠状态/拍击锁定序号/史莱姆雨目标 | `AnomalyAI32[8]`，按机制解释 |

上述复用通过语义属性访问，不能增加 Behavior 实例存储。Head 的业务容量为 9 个 AI32 槽和 5 个 AI64 槽；容器另行附加尾部同步标记槽。二维向量完整占用一个 AI64 槽，对应 `AIChanged64` 标记。

手的原版 `ai[0]` 和 `ai[1]` 保留左右侧和主人索引。`ai[2]` 的常态动作与脚本动作分别由 `NormalAction`、`ScriptedAction` 解释；机制 5 的 `ai[3]` 是 1-based 已锁定攻击序号。机制 5 的起点/终点分别使用 `AnomalyAI64[0/1]`，机制 6 准备阶段的相对起点复用机制 5 起点槽；机制 2 入场复用这两个槽保存相对起点和初速度，由服务器锁定，并用动作值 6 区分常态动作，保证客户端等待及中途加入可恢复。入场用固定 Hermite 曲线衔接普通待机惯性，非拍击手保持下垂朝向，随预警移至本体侧下方，留出横向骨节空间并从玩家侧旁掠过，继续沿用本体深度和绘制锚点，不另存状态。

## 必须保留的顺序

主入口先用上帧竞技场激活状态选择目标，再清除激活标志，由本帧符卡重新开启。二阶段由 `AdvanceMechanics` 的 switch 表和循环统一推进；保留同帧连续跨越多个机制、随后增加 `Timer1`、最后只执行最终选中机制的顺序。前两次切换保留客户端音乐预测，其余由服务器推进，不能用单次互斥分支替换循环或移动计时位置。

召唤早期尚未赋值主人和自定义 AI 标志，因此保留仪式和 `IsSummoningHands` 的身份兜底。手部锁定数据未到达时，客户端保留等待分支。机制 2 绘制尺寸、悬停框及血条位置必须一起维护。手掌自定义绘制沿用原版 type 36 的底部对齐中心，常规 DrawNPCs/悬停作用域已临时加入 netOffset，不再重复添加。常态待机远距离复用普通手部逐轴追随，近距离替换为相对头部的阻尼收敛，避免固定加速度在目标两侧形成持续振荡；普通手部不启用此可选收敛分支。

测试与改动记录保存在 Git 工作区之外；不使用 Git 忽略规则排除这些资料。

## 机制二的同步与读取边界

机制二保持在 `Head/Phase2/SkeletronLegendHead.Mechanic2.cs` 单文件内，参数和纯查询集中在 `Mechanic2Parameters`，实际 AI 按侧位图、伤害、回程、场地等待、主体轨迹和攻击生成组织。本地函数直接服务于该 AI，不另建机制对象或实体状态容器。

三次拍击的随机左右位图及每次起点/目标由服务器锁定。客户端缺少位图、当前拍击锁定或最后一次回程锁定时停速等待，不自行使用本机目标位置补写同步槽。完整快照到达后按音乐年龄恢复轨迹。主体暂缺有效目标时，客户端等待服务器状态，不自行决定退场；一阶段血量触发转场也由服务器决定。

`GetStrikeIndex` 查询手部动作所属的拍击，`GetBodyStrikeIndex` 查询主体后撤至回收的有效窗口；两者结束边界不同，不可互相替换。拍击前以手部动作为提示，半场金色火焰在命中前淡入五帧，在命中帧达到峰值，随后淡出。半场伤害只在确切拍击帧执行，并用统一 GlobalNPC 中的本地位图防止音乐回拨重复结算；该位图不进入快照，初始化及克隆重置。错过伤害帧不补算历史伤害。

拍击落星先于主体、阶段或场地引用到达客户端时等待，依赖恢复后继续执行；服务器负责确认退场。所有参数、三次拍击音乐帧、弹幕生成顺序及同步载荷布局保留。

公共黑暗遮罩的内容初始化在服务器跳过客户端粒子纹理读取，卸载时清理借用引用；通用时钟在首次宿主更新前返回零，支持内容注册期间的稀有度颜色查询。


## 共用运动与发射

普通骷髅王与传说之夜直接调用父目录的 `SkeletronKinematics`（悬停、旋转、手部待机和蓄势）及 `SkeletronCombat`（头骨与加速扇形发射）。运动实现不再由 `SkeletronHand` 或 `SkeletronHandler` 转发。
