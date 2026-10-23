# EventideReunion 文件组织指导

本文件约定落日遗逢的文件职责、技能扩展方式和资源路径。通用代码规范同时遵循仓库根目录的 `DESIGN_GUIDELINES.md`。

## 目录与职责

根目录保留武器入口、技能选择和跨技能共享模块；普通攻击代码与音效放入 `NormalAttack/`，每个技能的专属实现与资源放入以技能名命名的子目录。

```text
EventideReunion/
├── DESIGN_GUIDELINES.md
├── EventideReunion.cs                 # 物品定义、使用入口与技能释放分派
├── EventideReunionAspect.cs           # 技能枚举
├── EventideReunionDamageConstants.cs  # 全部技能、印记与共鸣的伤害倍率及使用位置
├── EventideReunionSelection.cs        # 选择输入、技能轮盘及其绘制
├── EventideReunionBladeTrail.cs       # 普通攻击与技能共用的刀光渲染
├── EventideReunionAfterimageSystem.cs # 独立世界绘制阶段统一绘制轮回/死亡人物残影
├── EventideReunionMarkDrawing.cs      # 双标记共用的绘制顺序、布局和 shader 调用
├── EventideReunion.png
├── EventideReunion_NoStreamer.png
├── Resonance/                       # 轮回共鸣专属模块
│   ├── EventideReunionResonance.cs       # 玩家行为、Buff 与同步
│   ├── EventideReunionResonanceRules.cs  # 印记、伤害、冷却与计时规则
│   ├── EventideReunionResonanceDrawing.cs # 已用相位的金色星光与呼吸外发光
│   └── SamsaraResonance.png             # 轮回共鸣 Buff 专用图标
├── SpearTrail.png                    # 共享刀光材质
├── NormalAttack/
│   ├── EventideReunionHoldout.cs           # 普通攻击主逻辑；显式引用根目录武器材质
│   ├── EventideReunionHoldout.Downward.cs  # 普通攻击下劈
│   ├── EventideReunionHoldout.Drawing.cs   # 普通攻击绘制
│   ├── EventideReunionHoldout.Particles.cs # 普通攻击粒子
│   ├── EventideReunionNormalAttackAssets.cs            # 普通攻击音效定义（Sun 起手也复用 Thrust）
│   ├── Thrust.ogg
│   ├── SweepUpward.ogg
│   └── DownwardSlash.ogg
├── Runes/                            # 选择轮盘使用的全部技能符文
│   ├── EventideReunionRunesAssets.cs        # 六个符文材质加载及技能枚举映射
│   ├── Sun.png
│   ├── Samsara.png
│   ├── Life.png
│   ├── Death.png
│   ├── Sacredness.png
│   └── Star.png
├── Samsara/
│   ├── EventideReunionSamsaraSkill.cs         # 起手、旋身、锁定、刺击及收招同步
│   ├── EventideReunionSamsaraSkill.Drawing.cs # 六道无武器残影、因果线与粒子
│   ├── EventideReunionSamsaraSkill.BladeTrail.cs # 共用弧面刀光及青紫切线粒子
│   ├── EventideReunionSamsaraSkill.Thrust.cs    # 包覆实体矛刃并向外延伸的连续发光矛形
│   ├── EventideReunionSamsaraVisualGeometry.cs # 残影无重叠布局及只追加的矛尖轨迹
│   ├── EventideReunionSamsaraLayers.cs        # 人物转身投影；齿轮由独立世界阶段绘制
│   ├── EventideReunionSamsaraLayers.cs        # 身后齿轮层和以左半身为轴的旋身投影
│   ├── EventideReunionSamsaraStrike.cs        # 服务器刺击、命中集合与目标筛选
│   ├── EventideReunionSamsaraMark.cs          # 服务器标记、各来源伤害观测和真实伤害结算
│   ├── EventideReunionSamsaraRecord.cs        # 独立可测的计账、计时、取整及排序规则
│   ├── EventideReunionSamsaraEcho.cs          # 联机结算钟声、扩散齿轮、星芒与伤害数字
│   └── EventideReunionSamsaraPlayer.cs        # 施法闭眼、操作限制和独立冷却
├── Death/
│   ├── EventideReunionDeathSkill.cs         # 蓄力、瞬步、回身横斩与同步状态
│   ├── EventideReunionDeathSkill.Drawing.cs # 武器绘制、黑雾与粒子入口
│   ├── EventideReunionDeathAfterimageGlow.cs # 死亡残影的紫色外发光与独立绘制上下文
│   ├── EventideReunionDeathSkill.Afterimages.cs # 残影姿态、立矛回收动画与回收星光
│   ├── EventideReunionDeathRecall.cs        # 残影顺序与回收插值
│   ├── EventideReunionDeathVignette.cs      # 仅屏幕边缘变暗的独立遮罩
│   ├── EventideReunionDeathGeometry.cs      # 小步穿身扫描、退场判定与伪 3D 挥动投影
│   ├── EventideReunionDeathDrawing.cs       # 横斩与镰刀波共用的紫黑弧面渲染
│   ├── EventideReunionDeathStrike.cs        # 突进矛与回身斩的服务器伤害代理
│   ├── EventideReunionDeathPlayer.cs        # 技能控制与独立冷却
│   ├── EventideReunionDeathWave.cs          # 服务器权威镰刀波与印记登记
│   ├── EventideReunionDeathWave.Particles.cs # 沿飞行轨迹补点的紫色星屑与细长光点
│   ├── EventideReunionDeathMark.cs          # 印记、非 Boss 斩杀与击杀灵魂生成
│   ├── EventideReunionDeathMarkFade.cs      # NPC 死亡后播放完标记缩小退场的本地粒子
│   ├── EventideReunionDeathSoul.cs          # 独立的幽魂治疗弹幕与治疗同步
│   ├── EventideReunionDeathSoulMotion.cs    # 高速追踪与治疗量计算
│   └── Starfield.png                      # 本地星空贴图，来源 MEAC/Images/Cosmic.png
└── Sun/
    ├── EventideReunionSunSkill.cs           # Sun 技能状态、AI、命中与同步
    ├── EventideReunionSunSkill.Particles.cs # Sun 技能粒子
    ├── EventideReunionSunDrawing.cs         # Sun 技能绘制与专属刀光几何采样
    ├── EventideReunionSunGeometry.cs        # Sun 技能几何及冲刺碰撞计算
    ├── EventideReunionSunEffects.cs         # Sun 爆炸弹幕与闪光覆盖层
    ├── EventideReunionSunPlayer.cs          # Sun 玩家控制、冷却与坠落保护行为
    ├── EventideReunionSunTurnLayer.cs       # Sun 横向转身绘制图层
    ├── EventideReunionSunAssets.cs          # Sun 专属音效定义
    ├── SunSweep.ogg
    ├── SunDashHit.ogg
    └── SunFollowUpSlash.ogg
```

`Resonance/SamsaraResonance.png` 位于共鸣专属目录，仅供轮回共鸣 Buff 使用，不用于轮回相标记或结算特效；选择符文 `Runes/Samsara.png` 保留在公共符文目录。

## 通用模块与技能专属模块

- 判断文件归属时，以职责和使用范围为准。只服务于 Sun 的 AI、几何、玩家行为、图层、粒子、特效和音效放入 `Sun/`；其他技能采用同样规则。
- 普通攻击 Holdout 的主逻辑、下劈、绘制、粒子及音效定义与文件统一放在 `NormalAttack/`。Sun 起手复用其中的 `Thrust` 音效，引用继续通过 `EventideReunionNormalAttackAssets.Thrust` 访问。选择轮盘、技能枚举、武器贴图、共享刀光渲染与材质保留在根目录。通用文件调用某个技能类型，并不意味着该文件属于该技能。
- `Runes/` 为技能选择界面提供统一资源，包含 Sun 在内的所有符文均保留在此，不能因为符文名称与某个技能相同就将其迁入技能目录。
- 技能可以复用根目录的公共模块；跨技能复用逻辑应放在公共模块，避免一个技能依赖另一个技能的内部实现。
- 玩家实例数据继续放在 `AnomalyGlobalInstances.AnomalyPlayer` 中，技能目录中的 `AnomalyPlayerBehavior` 仅实现行为，不另建玩家数据容器。

## 命名空间与资源加载

所有 C# 文件统一使用以下命名空间，包括 `NormalAttack/`、`Resonance/`、`Sun/`、`Death/` 及后续技能子目录中的文件：

```csharp
namespace Anomalies.SpecialContents.Developer.EventideReunion;
```

子目录仅用于文件组织，不追加 `.NormalAttack`、`.Sun` 等命名空间后缀。同名技能辅助类应带上技能名称，例如 `EventideReunionSunGeometry`、`EventideReunionSunAssets`，避免统一命名空间内的类型冲突。

命名空间与文件夹结构不一致可能使 tModLoader 推导出错误的默认材质路径。每个受影响的物品、弹幕、NPC、物块等内容类必须显式重写 `Texture`，指向资源的实际位置；不能因自行绘制或使用 `PreDraw` 就忽略默认材质加载。无材质加载职责的辅助类、玩家行为和绘制图层无需添加材质属性。

材质与音效按资源实际目录统一归入对应的 `xxAssets` 类。主目录由 `EventideReunionAssets` 管理共享材质；子目录由各自的 Assets 类管理本目录材质和音效，例如 `EventideReunionDeathAssets`、`EventideReunionResonanceAssets`。固定纹理使用 `LoadTexture` 标记静态资产字段，绘制代码通过静态引用访问，不在绘制回调中调用 `ModContent.Request`。

当前资源路径约定：

| 用途 | 路径或入口 |
| --- | --- |
| 武器及共享贴图 | `EventideReunionAssets.AssetPath`，即 `Anomalies/SpecialContents/Developer/EventideReunion/`；Holdout 文件移入 `NormalAttack/` 不改变此共享路径 |
| 选择符文 | `EventideReunionRunesAssets.AssetPath` 加 `<技能名>`；加载与枚举映射统一由 `Runes/EventideReunionRunesAssets.cs` 维护 |
| 普通攻击音效 | `EventideReunionNormalAttackAssets.AssetPath`，指向 `Anomalies/SpecialContents/Developer/EventideReunion/NormalAttack/` |
| Sun 专属音效 | `EventideReunionSunAssets.AssetPath`，指向 `Anomalies/SpecialContents/Developer/EventideReunion/Sun/` |

资源标识使用 `/` 分隔，省略扩展名。移动资源或修改命名空间时，同步核对 `Texture`、`LoadTexture`、`ModContent.Request`、`SoundStyle` 以及发光和其他附属材质的引用；路径必须对应真实文件，不能凭目录惯例添加不存在的 `Sounds/` 等层级。

Anomalies 不强依赖灾厄等可选模组。技能所需的外部贴图必须复制到本模组对应技能目录，并显式指向 `Anomalies/` 下的本地资源，不能通过无条件外部资源路径要求玩家启用可选模组。

## 本地化归属

- EventideReunion 管辖的全部本地化内容统一位于 `Mods.Anomalies.SpecialContents.Developer.EventideReunion`，在中英文 `*_Mods.Anomalies.SpecialContents.hjson` 中沿用已有的 `Developer.EventideReunion` 分组格式补充，不分散到默认 `Buffs`、`Projectiles` 或其他根键。
- 武器的 `LocalizationCategory` 为 `SpecialContents.Developer`：tModLoader 会自动追加物品名 `EventideReunion`，对应分组中已有的 `DisplayName` 和 `Tooltip`。`LocalizationPrefix` 直接指向统一分组，避免重复追加物品名。
- 普通攻击、选择轮盘、各技能弹幕及专属 Buff 的 `LocalizationCategory` 统一返回 `EventideReunion.LocalizationCategoryForEventideReunion`（`SpecialContents.Developer.EventideReunion`）。死亡印记为纯字段机制，不注册 Buff，也不保留 Buff 图标及本地化；无本地化职责的辅助类、玩家行为和绘制层无需添加该属性。
- 移动文本或新增技能时同步核对所有语言、`LocalizationCategory`、`LocalizationPrefix` 和显式键引用，并移除旧位置的重复文本。
- 具有独立效果身份的弹幕使用自身的默认 `DisplayName` 本地化入口，不重写为武器名称：当前包括 `EventideReunionSunExplosion`（日冕耀斑）、`EventideReunionDeathWave`（寂灭镰波）、`EventideReunionDeathSoul`（收割之魂）。文本按现有 `<类型名>.DisplayName` 格式补充到统一分组。矛身、技能动作载体、选择时持矛及不可见伤害代理继续共用武器显示名，不为这些实现实体增设独立名称。

## 碰撞检测

- 永远不要使用 Collision 类的带宽度线段碰撞检测，会严重漏检；遵循仓库设计规范 2.7 节，统一使用 Transoceanic 的旋转矩形碰撞。
- 普攻、Sun、轮回刺击、死亡突进选敌与伤害代理、回身斩、镰刀波和治疗灵魂的带宽度扫掠使用 RotatedRectangle.FromLineSegment，并保留宽度、缩放及运动采样。死亡突进通过二分最短相交矛身求首次接触距离，保持最近接触目标优先。

## 后续技能扩展

1. 新建与技能枚举名称一致的目录，例如 `Samsara/`、`Life/`、`Death/`、`Sacredness/`、`Star/`，集中放置该技能专属代码和资源。
2. 使用 `EventideReunion<技能名>...` 命名类型和代码文件，继续采用根命名空间。按逻辑规模拆分 AI、绘制、粒子、几何等职责，无需为尚不存在的职责创建空文件。
3. 在物品入口与选择模块中接入技能，专属战斗细节仍放在技能目录；通用选择、普通攻击和共享渲染不随技能迁移。
4. 为受目录与命名空间差异影响的内容类显式指定材质，并核对专属音效和共享资源的路径。
5. 检查外部引用和测试工程的源文件链接。Sun 几何测试位于仓库的 `Tests/Eventide.Sun/`，移动其被引用源文件时同步更新项目路径。

调整文件组织后同步维护本文，确保目录树和职责说明与实际代码一致。

## Death 技能

- 飞行波寿命 72 帧、每帧速度保留 0.99，初速 18 时中心飞行约 918 像素；前 54 帧平滑扩大到 1.6 倍，之后保持尺寸并在末尾淡出。弧面绘制、星屑位置和碰撞使用同一缩放，刃口扫掠包含本帧移动及扩张。`tileCollide = false`、`ownerHitCheck = false` 保证穿墙及隔墙伤害，`penetrate = -1` 保证无限穿透，`ignoreWater = true` 无视液体；每个敌人仍只命中一次，防止大范围波逐帧重复伤害。

- 标记生成时用 12 帧平滑放大，消失时用 12 帧缩小退场；进度保存在 `AnomalyGlobalNPC.EventideReunionDeathMarkProgress`，不参与网络同步。字段计时归零后继续绘制到退场结束，刷新字段计时不重置入场；NPC 死亡时由纯本地 `EventideReunionDeathMarkFade` 粒子接续缩小，避免瞬间截断。

- 死亡印记通过 `ModifyIncomingHit` 将目标受到的最终命中伤害乘以 1.15，对普通敌人和 Boss 均生效；仅斩杀判定排除 Boss。

- 继续使用当前右键轮盘的松键释放入口，冷却为 600 帧，印记持续 360 帧。文档未定的伤害集中为 `EventideReunionDeathSkill` 的 `DashDamageMultiplier`、`SlashDamageMultiplier`、`WaveDamageMultiplier`（当前分别为武器伤害的 3、5、5 倍）。三者独立使用每目标一次的局部命中免疫，回身斩与镰刀波均施加印记，突进矛仅造成伤害。
- 技能按蓄力、瞬步、回身横斩和收招组织；蓄力期间跟随持有者光标，瞬步开始时锁定并同步最终方向，行为与太阳技能一致。瞬步使用玩家尺寸进行地形碰撞，不穿墙。专属表现复用已有武器、烟雾、暗影焰，暗屏使用 Death 专属边缘遮罩。
- 瞬步以不超过 4 像素的小步检查地形和敌人，记录首个穿过的有效敌人。未命中时持续 12 帧，以每帧 48 像素移动共 576 像素。矛或玩家碰撞箱首次命中目标后，从该时刻重新计时，给予 240 帧（4 秒）的穿身预算；目标后方至少留出 96 像素的碰撞箱间距，再结束突进。遇到地形阻挡或持续过长时，若仍重叠则退回最近的无重叠安全位置；没有安全落点时保持保护，不能在敌人体内强制结束。
- 突进期间通过玩家行为提供全伤害免疫，结束后再保留 10 帧，状态存入 `AnomalyPlayer.EventideReunionDeathProtection`。已有更长的无敌时间不被缩短。
- 死亡残影的人物与矛使用不受环境光衰减的紫色扩散轮廓，人物本体保留最低紫色亮度，外发光随回收缩放与透明度同步减弱。绘制上下文只在单次死亡残影绘制期间启用并在 finally 中恢复，不影响真实玩家或其他技能残影；通过 PlayerBehavior 的 TransformDrawData 回调插入轮廓层，避免再次调用玩家渲染器造成重入。残影保存生成时的矛角度、缩放、人物方向、手臂姿态和身体帧，不读取真实矛之后的横斩方向。横斩开始后先保持原姿态 18 帧（0.3 秒），再按生成顺序间隔 10 帧回收：先用 8 帧平滑立矛，再用 20 帧加速移向玩家并缩小；每次回收在玩家处产生紫黑星光，最后一枚触发较大的爆发。回斩结束后仅保留 4 帧动作硬直，随后解除持矛、转向、手臂及攻击输入占用；普通攻击与选择轮盘统一通过 `IsBlockingActions` 判断，不能因残影载体弹幕仍存在而阻止操作。残影在后台继续回收，载体寿命由回收总时序推导（当前收招阶段保留 60 帧），确保三道残影全部回收并留出 8 帧余量；这段纯视觉寿命不再延长动作硬直。
- 结束突进时朝首个穿过目标碰撞箱的最近部位锁定横斩方向，保证巨大或狭长目标也能进入斩击范围；目标失效时使用其最后记录的碰撞箱，未接触敌人时向突进后方回斩。方向及挥动符号随技能弹幕同步。
- 横斩和同步释放的镰刀波共用 `EventideReunionDeathGeometry.SlashVector` 的近圆弧与轻微透视投影（半径 320、横向比例 0.92），武器姿态、弧面推进和伤害碰撞使用一致方向。回斩本体复用 `EventideReunionBladeTrail` 与 `EventideSweep` 的现有刀光材质、流纹和短拖尾，仅切换紫色调并沿挥动切线生成紫白星芒；共享着色器每次调用必须重设配色，避免影响普通攻击和 Sun。飞行镰刀波保持独立星空弧面，不使用原版死神镰刀贴图，使用本项目 `Assets/Effects/Primitives/EventideDeathSweep.fx/.xnb` 绘制明艳紫色亮刃和内部星空。参考 MEAC 永夜刃夜间 `Effects/Screen/Cosmic.fx` 的遮罩星空采样，将 `Images/Cosmic.png` 复制为本地 `Death/Starfield.png`；世界像素采样避免随屏幕比例或圆弧 UV 压扁星空，不需要启用 MEAC。
- 刀光两端、内侧和外缘使用平滑透明度过渡，星空随同一遮罩消失。飞行波沿前后两帧之间的世界轨迹补发紫色细长光点与白紫星屑，粒子脱离波体后向后缓慢漂移并自行消散；拖尾历史仅存于客户端，寿命末期同步减少发射并平滑淡出。
- 镰刀波及突进、横斩伤害代理由单人端或服务器生成并拥有；伤害代理通过 `ProjectileNetReference` 记录技能的 owner + identity，不能跨端使用本地弹幕槽位。印记归属存入 `AnomalyGlobalNPC.EventideReunionDeathOwner`。服务器负责施加印记、斩杀及每次击杀唯一的灵魂生成，避免客户端重复结算。
- 回斩和波在 `ModifyHitNPC` 中登记本次命中的临时归属 `EventideReunionDeathHitOwner`，并在 `OnHitNPC` 中清除。`OnKill` 先于命中后回调执行，直接击杀因此不依赖后续命中回调；分节目标同步登记共享生命根实体，但只有根实体死亡时生成一次治疗球。临时命中归属在结算后清除，持续治疗资格仅在字段印记有效时存在。
- 标记 NPC 使用 `Assets/Effects/Primitives/EventideDeathMark.fx/.xnb` 在身体处以纯程序形状绘制深紫主体、冷紫刃缘及扩大的冷白亮芯的凹弧四角星与断裂封印环。星体及断环始终具有水平、竖直对称轴，不能用旋转角相位破坏对称；随时间平滑呼吸缩放并发光，亮芯增强但保留深紫外围。标记本身不在轴线上生成星芒；被标记 NPC 每 4 帧从自身中心放射 3 颗紫白星芒，方向覆盖整周并随机转动，生成后独立向外减速、旋转和渐隐，复用 Transoceanic 的 `GenericSparkle`。仅在字段计时有效时发射粒子，绘制保留平滑退场，大小按 NPC 尺寸限制，不引入额外网络视觉状态。
- 印记目标生命严格低于 25% 才执行斩杀，排除 Boss 及其多节部件。带印记目标被击杀后，治疗灵魂归属于施加印记的玩家。灵魂保留原版幽魂粒子，但用独立追踪 AI，以玩家速度前馈加距离自适应速度追踪；路径碰撞检查防止高速越过玩家。所属客户端实际增加最多 5 点生命（不超过上限，遵循月噬限制），通过原版 `SpiritHeal` 消息同步治疗，并立即销毁灵魂以防重复结算。

突进边缘暗屏使用 `Assets/Effects/Overlays/EventideDeathVignette.fx/.xnb` 的解析遮罩，屏幕中心透明，四边按屏幕高度统一尺度平滑变暗，进出阶段平滑淡入淡出；不再写入通用全屏 `DarknessIntensity`。

`Tests/Eventide.Death/` 验证薄目标扫描、宽目标延长穿身、迟命中、空挥距离、地形阻挡、安全退回、96 像素间距、目标后方判定、残影回收顺序与加速缩小、扩大近圆弧与投影旋转一致性，以及静止和高速玩家、远距离传送后的灵魂追踪、治疗上限与月噬限制。


## Samsara 技能

- 最新行为依据 `Design/落日遗逢EventideReunion.docx` 的轮回相八条规则及动作参考图。保留现有右键轮盘松键释放入口；基础冷却 900 帧，不提供击杀冷却返还。原有共鸣系统的设计不在此技能内重写或移除；共鸣由 Resonance/ 目录的 EventideReunionResonance / EventideReunionResonanceRules 模块实现。
- 动作分为起手 24、旋身 36、快速转身 6、后撤 5、刺击 12、收招 30 帧。旋身结束、转身首帧由持有者读取光标并锁定方向，其他端等待方向同步。刺击只伸出武器，不改玩家位置或速度；收招先收矛立矛，因果线回到矛尖后解锁操作。刺击伤害暂设为武器伤害的 4 倍，集中于 `ThrustDamageMultiplier`；时长与距离同样可调。
- 六道残影只复用玩家当前身体、手臂和装备姿态，不绘制手持物品或武器，不生成伤害。人物绘制期间修改的 heldProj、itemAnimation、itemTime 在 finally 中恢复。青紫钟表齿轮和六束交织因果线用像素坐标程序绘制，无屏幕比例拉伸；复用公共符文、武器材质、普通刺击音效与 Transoceanic 的 GenericSparkle。低鸣使用原版 Item35 降调，不依赖可选模组。
- 只有单人端/服务器创建伤害代理，命中集合在服务器保存 NPC 实例引用。刺击结束后把 `realLife` 部件映射至共享生命本体，排除死亡/消失/已有标记目标；优先 Boss，再按 lifeMax 降序，同级按实体索引稳定排序。独立生命部件不合并。每次仅标记一个目标；本次刺击全部结束后才初始化生命快照和五秒计时。
- 标记是 NPC 自身的独立状态，不使用可能被 Buff 免疫或净化的减益作为权威计时器。所有实例数据保存在 AnomalyGlobalNPC，行为不自行存储实例数据。服务器的 StrikeNPC 与 UpdateNPC_BuffApplyDOTs 钩子在原逻辑前后观察共享生命本体，帧末补采直接扣血；同一生命变化只能记录一次，治疗只更新基线、不抵消累计伤害。不会逐个部件叠加 damageDone。累计使用 long，结算以整数运算计算 ceil(累计 × 35 / 100)，实际 NPC 伤害上限为 int.MaxValue。
- 标记五秒计时由 ModSystem 驱动，不依赖 NPC 是否执行 AI，也不依赖施放者存活、在线或维持动作；目标死亡/消失不结算。服务端先清理标记，再直接修改生命并执行 checkDead，跳过防御、减伤、伤害浮动、dontTakeDamage 以及玩家/弹幕命中附加效果；正常保留死亡、掉落和 Boss 转阶段钩子。在线施放者获得 lastInteraction 和 playerInteraction 归属；离线仍按时结算。
- 同步只传标记剩余帧数供客户端绘制，不同步累计账本或客户端视觉容器；中途加入通过 NPC 快照得到标记。结束时同步生命与清除状态，独立无伤害 Echo 弹幕同步结算钟声、光环和数字。结算数字使用额外 AI 的 int，避免 float AI 槽位丢失大整数精度。
- `Tests/Eventide.Samsara/` 验证标记前伤害排除、重复标记不刷新、全来源累计、治疗后伤害、重复观察不重复计数、300 帧边界、再次标记、无伤害不结算、小额向上取整、long 累计与最终溢出保护、Boss/生命上限优先级及稳定排序。

## 轮回共鸣

- 已用相位符文持续发出较大的金色星光，平均每 3 帧生成一次，持续 30–42 帧；共鸣视觉集中于 Resonance/EventideReunionResonanceDrawing.cs。符文外发光以 2.4 秒为周期同步呼吸，包含柔光底层和轮廓外扩，颜色依次为太阳橙金、轮回青紫、生命翠绿、死亡紫、神圣白金、恒星蓝；随轮盘展开和收起淡入淡出。

- 右键共鸣 Buff 可主动取消：清空全部相位印记及剩余时间，立即移除当前近战伤害加成，并同步清空状态；已经开始的技能冷却不重置。

- 状态位掩码与剩余时间归属 AnomalyPlayer，覆盖全部六相；目前太阳、轮回、死亡三相在技能弹幕成功生成的 Action 回调中登记，未实现的技能不授予印记。
- 新相位授予一枚印记并刷新 1800 帧。重复相位先移除自身，再等概率移除一个剩余印记，不刷新时间；没有印记时立即结束。到期或死亡清空，不随存档保存。
- 每层近战伤害独立乘以 1.08；技能冷却按本次印记变化后的层数减少每层 5%，重复施放额外乘以 1.2，最终向上取整。冷却在释放时固定，不追溯修改已运行的冷却。本次技能伤害使用更新后的层数。
- 共鸣 Buff 显示剩余时间和层数，轮盘已使用相位持续产生金色星光。持有者计算释放结果并发送快照，服务器验证发送者归属及掩码/时间范围后转发；中途加入通过 AnomalyPlayer.SyncPlayer 同步，持续时间各端自行递减。
- Tests/Eventide.Resonance 穷举重复相位的全部印记组合和随机候选，并验证六层上限、乘算伤害、冷却组合与 1800 帧到期边界。终极技动作与其余三相技能另行实现。

### 轮回 / 死亡标记协同绘制

- `EventideReunionMarkDrawing` 统一负责两种 NPC 标记的 PostDraw，在一次 SpriteBatch 中断内先绘制轮回齿轮、再绘制死亡四角星，避免行为加载顺序导致不同叠色。两个技能各自的标记行为仅管理机制，不重复绘制。
- 轮回标记使用 `Assets/Effects/Primitives/EventideSamsaraMark.fx/.xnb`：十八枚平顶圆角轮齿、蓝紫/冷蓝刃缘、深靛齿体、反向交织因果线和从十二点钟顺时针消退的五秒计时内圈。圆心透明，不使用任何 Buff 图标；`SamsaraResonance.png` 仅供轮回共鸣 Buff 使用，结算特效也不引用此图标。
- 两枚标记单独出现时保持各自尺寸；共存强度由平滑入退场计算，外齿轮最多扩大 15%，死亡星体最多缩小 20%，以形成外齿轮、内四角星的同心布局。两 shader 通过 `CompanionPresence` 收敛交叠光晕，死亡星体保持水平/竖直对称。死亡后的退场粒子接续共存时的实际尺寸和光晕参数。
- 绘制参数每次调用显式重设，包括无伴随标记时的零值；GraphicsDevice 状态与 SpriteBatch 在 finally 中恢复。使用世界像素正方形网格和相同相机矩阵，长宽比变化不会压扁齿轮。shader 使用统一的 `Main / ps_3_0 / Technique1 / Pass0`，已生成对应 XNB。
- `Tests/Eventide.MarkShaders/` 保存隔离的 shader 编译配置与实际 XNA 渲染预览工具；单独编译这两个效果，不批量重写其他 shader。预览比较不同时间、明暗背景下的轮回单标记、死亡单标记和共存效果。


### 人物残影与绘制重入

- 轮回和死亡的人物残影统一由 `EventideReunionAfterimageSystem.PostDrawTiles` 绘制。此阶段位于地形批次结束后、持有弹幕/主角色绘制之前，自己开始并在 finally 结束 SpriteBatch。技能原有的姿态暂存和 finally 恢复逻辑继续保留。
- 禁止从持有弹幕的 PreDraw 或人物绘制图层中直接/间接调用 Main.PlayerRenderer.DrawPlayer。持有弹幕在 PlayerDrawLayers.DrawPlayer_RenderAllLayers 中绘制，而 LegacyPlayerRenderer 每次调用会清空自身 DrawData 缓存，同时使用全局 Main.spriteBuffer；嵌套调用会破坏外层尚未消费的绘制缓存，造成索引越界。仅改为另一个 LegacyPlayerRenderer 实例仍无法隔离全局 spriteBuffer，因此必须拆开绘制阶段。
- `Tests/Eventide.Rendering/` 检查实际生产程序集的调用图：两技能的 PreDraw 均不能到达玩家渲染器，独立世界阶段必须保留两种人物残影；在内存中重新插入旧调用边时，检查必须能识别该回归。


### 像素线段的材质采样

- `EventideReunionSamsaraDrawing.Line` 的缩放单位是世界像素长度/宽度，必须显式设置源矩形 `(0, 0, 1, 1)`，不能用 null 采样整张 MagicPixel。否则贴图的原始宽高会再次乘上线段尺寸，使齿轮和因果线变为满屏放射光线。直接绘制与人物 DrawData 缓存共用此规则。
- `Tests/Eventide.MarkShaders/LineRegression.cs` 使用生产 Line 方法及实际 XNA 贴图，验证不同贴图尺寸、线段方向和两种绘制路径下，输出长度/宽度始终为指定像素尺寸。


### 轮回施法齿轮、紧凑残影与累积因果线

- 施法身后齿轮的完整半径为 156 像素（原为 78），提高显示强度并增加低亮宽光晕，采用更饱和的冷蓝与紫色；NPC 的标记齿轮保持独立尺寸。
- 六道人物残影采用紧凑椭圆布局，中心距离由玩家实际身体宽高和残影缩放计算。正上/正下残影与玩家的身体边缘保持 9 像素间隙；入场/退场缩放变化时重新计算间隙，不靠重叠身体表现归位，而是在近身位置缩小淡出。
- 因果线使用每个技能弹幕独立的客户端 EventideReunionSamsaraTrail 保存世界坐标历史。在 AI 更新采样而非 Draw 中累积，按矛角度、手持中心和伸出距离插值补点，旋身、转向和刺击期间只追加、不滑动删除旧尾迹。六束线沿同一完整路径交织，不再根据当前角度每帧重新生成一段固定短弧。进入 Recovery 才冻结轨迹，按累计弧长从最旧端逐段消去，仅插值裁剪当前端点；尚未回收到的轨迹位置、形状及亮度保持不变，不再整体向矛尖收缩；历史无需网络同步。
- `Tests/Eventide.SamsaraVisuals/` 验证不同身高/缩放下的残影边缘间距及互不重叠、完整圆周轨迹长度、旧采样保留、静止不重复采样、刺击追加、收招冻结、按弧长连续裁剪及未触及轨迹保持不变。


### 轮回齿轮定位与快速回身投影

- 施法齿轮与玩家绘制中心重合，仅在图层顺序上位于人物身后，不再向左/右或上方偏移；齿轮不绘制中央符文贴图。
- 因果线束的基础间距从 2.6 调整为 4 像素，保留固定轨迹与沿轨迹回收。残影横向额外距离从 24 调到 30 像素，上下身体边缘间隙从 3 调到 9 像素，仍按缩放计算，避免重叠。
- 旋身一周后的 Turn 使用快速 6 帧，使用与太阳相横扫相同的圆周倾斜/深度透视方法：以锁定瞄准方向为投影轴，回身时同时改变矛的屏幕方向和投影长度，首尾衔接旋身末姿态及刺击方向。武器材质缩放、矛尖位置与因果线采样共同使用投影向量，不再做固定长度的二维角度插值。纯几何规则置于 SamsaraVisualGeometry，不依赖 Sun 内部类型。


### 轮回转矛及回身刀光

- 转矛与快速 3D 横抽复用公共 `EventideReunionBladeTrail.DrawArc`，其 96 点弧面采样、0.23 内缘比例、径向 UV、渐隐和 `SpearTrail / EventideSweep` 材质与太阳相完全共用；太阳相原有私有 DrawBlade 已迁入公共模块，保持原有采样和参数。
- 轮回只通过 `SamsaraPalette` 将同一 shader 改为青色至淡紫色；不改变遮罩、流纹、刀刃、前锋或亮芯的计算。公共入口每次同时重设 `DeathPalette` 和 `SamsaraPalette`，其他技能继续使用原有配色。对应 EventideSweep.xnb 已重新编译。
- 刀光与矛共享 SpinVector / TurnVector 几何，回身时同步使用投影方向和长度。旋身末尾刀光在回身期间淡出，横抽余光在刺击前 6 帧淡出，因果线仍按原规则独立保存及回收。沿挥动切线补发青色至淡紫色 PointingParticle 与冷白 GenericSparkle，替换此阶段原有的定点星芒。
- `Tests/Eventide.SamsaraBladeShaders/` 的 GPU 配色回归使用实际 SpearTrail 贴图及编译后的 XNB，逐像素验证默认、轮回、死亡三种配色的刀光 alpha 遮罩一致，以及连续切换后默认配色恢复。


### 横抽因果线与矛尖一致性

- TurnVector 在一次横抽中固定投影平面倾角，不再用 sin(阶段进度) 改变倾角；由起手方向反投影得到初始圆周角及半径，保证起手/结束矛尖与相邻动作一致。挥动方向接续旋身方向，避免线束折返和法线翻转。
- 横抽时长为 6 帧。生产轨迹采样改用 AppendCurve，直接调用武器/刀光共用的 SpinVector、TurnVector 和刺击伸出函数，每阶段按 512 个子帧间隔补点，计时推进后也同步记录当前矛尖；禁止将 3D 投影后的屏幕角度和长度重新作二维极坐标插值。玩家平移单独插值，不改变武器轨迹几何。收招冻结与逐段裁剪规则保持不变。
- 几何测试覆盖双向挥动、正反重力下每一横抽帧的精确矛尖终点、全部子帧点与 3D 函数一致，以及冻结后轨迹不被追加修改。


### 因果线顶点带及连接点

- 因果线使用 `EventideReunionSamsaraThreadDrawing` 和 `Assets/Effects/Primitives/EventideSamsaraThread.fx/.xnb` 绘制连续顶点带，约 2.5 像素亮芯和轻微柔光；六线从实际矛尖汇合，沿后方约 220 像素逐渐展开，完整线束的基准间距为 8 像素。
- 参数细分采样后，末段小于 2 像素时合并到活动端点，保留精确矛尖而不积累近重合顶点。线束偏移法线使用前后各 6 像素弧长邻域，顶点带边缘法线使用前后各 3 像素邻域，不再直接依赖帧边界的两个近邻顶点。冻结后不再合并；回收只裁剪端点，尚未涉及的轨迹与边缘保持原样。
- shader 使用导数抗锯齿、圆形端帽及沿端点衰减的外光，避免裸露四边形截面形成方块。StartDistance / EndDistance 每次显式设置为可见轨迹弧长边界，不用全局渐隐改变剩余轨迹。
- TurnDuration 为 6 帧；以独立 512 子采样解析真实矛尖函数，无需额外推进 AI、伤害或标记计时。测试覆盖六帧终点精度、微短段合并、圆轨迹跨帧切线连续性和 shader 实际 GPU 亮芯/外光/透明边缘。


### 轮回齿轮锐利边缘与无穷链内圈

- 施法齿轮线条使用低强度窄外光、清晰主线和更细的浅蓝亮芯三层绘制；减少外光强度与宽度，圆周采样提升至 192 段。NPC 标记 shader 同步改为导数抗锯齿的窄亮轮齿边缘、弱外光，降低暗色齿体填充，保证明暗场景下边界可辨。
- 保留倒计时圆弧和轮齿结构边界。移除施法齿轮的时针/分针，标记不再使用独立的装饰内圆与刻度；装饰改为两条反相正弦曲线沿圆周闭合交织，组成连续的无穷符号链。六个周期的整数相位保证首尾位置、切线平滑衔接，青蓝/淡紫配色保留。
- 死亡四角星仍位于无穷链内部，两标记共存的同心布局和光晕抑制参数不变。通过实际 XNA 加载两个标记 XNB，并比较单标记/共存及明暗背景预览。


### 标记对比度与修正方向的延伸刺击

- NPC 标记的轮齿、内边、无穷链和倒计时采用冷白青紫亮芯加深色窄描边，略增加线宽，降低齿体填充。深色描边在 NPC 高频纹理上隔开轮廓，亮芯提高深色 NPC 上的可读性；仍保留弱外光及死亡星体共存布局。
- 横抽结束时由持有者再读取一次光标并同步新的刺击方向；其他端在 Turn 末尾等待这一同步，不自行使用旧瞄准进入后撤。额外 AI 同步横抽结束方向，Pullback 用 5 帧把矛方向平滑修正到新方向并后撤 48 像素，然后进入刺击，玩家本体不位移。
- 刺击先用前三成进度的快速缓出将矛从后撤位置伸至前方 180 像素，短暂保持后收回。青紫矛光从实体矛尖后方 180 像素的刃根开始，采用连续柔边矛形纹理包覆整个矛刃，并向矛尖外延伸最多 280 像素。使用 SpriteBatch 的零 alpha 自发光颜色，紧随实体矛绘制青色亮芯与柔和外光，不依赖因果线 shader。虚影与实体矛同步淡入、延伸及淡出。ThrustReach / ThrustGlow / ThrustTipDistance 集中在 VisualGeometry，服务器伤害代理与视觉共同使用，最大前向范围约 680 像素，线形碰撞宽度 42 像素（原为 28）。
- 只在 Thrust 创建伤害代理，Pullback 不造成伤害或提前标记；仍在整次刺击结束后从实际命中集合中择优标记。因果线继续跟随实体矛尖，并采样后撤、方向修正及新刺击曲线。
- 几何测试逐帧验证后撤/刺击衔接、快速出矛曲线、矛光与碰撞端点一致、峰值范围及光效首尾为零。

- 轮回转矛与横抽由 Samsara/EventideReunionSamsaraSweep.cs 提供独立服务器伤害代理，每阶段对同一 NPC 最多命中一次；伤害倍率 3 倍（刺击为 4 倍），不收集标记候选、不施加轮回标记。碰撞复用实际 SpinVector / TurnVector，每帧细分 32 段覆盖挥动弧线，横抽仍为 6 帧。

- 标记结算齿轮统一调用 EventideReunionMarkDrawing.DrawSamsara，与持续标记共用十八齿 shader、无穷内环、全局旋转时钟及双标记配色；剩余时间传零，保留倒计时耗尽状态。初始半径复用 SamsaraRadius 并继承死亡标记共存倍率，结算只额外放大与淡出，不再调用施法用的线段 DrawGear。

- 玩家身后齿轮也统一调用 DrawSamsara，与标记及结算共用新版 shader。PostDrawTiles 在残影和玩家之前绘制，位置为玩家中心加 gfxOffY；保留蓄力/收招缩放淡入淡出并补偿 shader 网格留白以保持原有外径。移除旧线段绘制器及 GearLayer，避免重复齿轮和玩家缓存重入。


- 刺击矛光在 PreDraw 中紧随实体矛提交同一 SpriteBatch，移除原有 PostDraw 顶点带路径。运行时生成并缓存 512×128 的连续柔边矛形纹理，卸载时在主线程释放；刃根到远端尖点为一体，最大延伸仍为 280 像素。GPU 测试直接提取生产纹理生成方法，验证实际 SpriteBatch 绘制的刃身覆盖、前向延伸和透明边界。


- 死亡突进逐步检测地形；任一步位移被截断时强制结束突进，不能因与 NPC 重叠而继续冲墙；有安全位置时先退回安全位置，随后回斩并保持原有保护帧。
- 死亡印记仅使用 `AnomalyGlobalNPC.EventideReunion_DeathMarkTime` 与归属字段存储，持续 360 游戏帧。通过 Behavior 的 ExtraAI 同步计时，客户端逐帧预测，服务器负责刷新、到期和伤害结算；所有 NPC 的 Buff 免疫或清除均不影响印记。保留全体目标受到 1.15 倍伤害、非 Boss 低血斩杀和击杀治疗球机制。不得注册死亡印记 Buff、图标或其本地化。