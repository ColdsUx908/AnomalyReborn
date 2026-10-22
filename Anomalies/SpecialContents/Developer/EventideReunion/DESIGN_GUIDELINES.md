# EventideReunion 文件组织指导

本文件约定落日遗逢的文件职责、技能扩展方式和资源路径。通用代码规范同时遵循仓库根目录的 `DESIGN_GUIDELINES.md`。

## 目录与职责

根目录保留武器入口、技能选择和跨技能共享模块；普通攻击代码与音效放入 `NormalAttack/`，每个技能的专属实现与资源放入以技能名命名的子目录。

```text
EventideReunion/
├── DESIGN_GUIDELINES.md
├── EventideReunion.cs                 # 物品定义、使用入口与技能释放分派
├── EventideReunionAspect.cs           # 技能枚举
├── EventideReunionSelection.cs        # 选择输入、技能轮盘及其绘制
├── EventideReunionBladeTrail.cs       # 普通攻击与技能共用的刀光渲染
├── EventideReunionStreamers.cs        # 飘带实现（当前暂时禁用）
├── EventideReunion.png
├── EventideReunion_NoStreamer.png
├── SpearTrail.png                    # 共享刀光材质
├── SamsaraResonance.png
├── NormalAttack/
│   ├── EventideReunionHoldout.cs           # 普通攻击主逻辑；显式引用根目录武器材质
│   ├── EventideReunionHoldout.Downward.cs  # 普通攻击下劈
│   ├── EventideReunionHoldout.Drawing.cs   # 普通攻击绘制
│   ├── EventideReunionHoldout.Particles.cs # 普通攻击粒子
│   ├── EventideReunionSounds.cs            # 普通攻击音效定义（Sun 起手也复用 Thrust）
│   ├── Thrust.ogg
│   ├── SweepUpward.ogg
│   └── DownwardSlash.ogg
├── Runes/                            # 选择轮盘使用的全部技能符文
│   ├── EventideReunionRunes.cs        # 六个符文材质加载及技能枚举映射
│   ├── Sun.png
│   ├── Samsara.png
│   ├── Life.png
│   ├── Death.png
│   ├── Sacredness.png
│   └── Star.png
├── Death/
│   ├── EventideReunionDeathSkill.cs         # 蓄力、瞬步、回身横斩与同步状态
│   ├── EventideReunionDeathSkill.Drawing.cs # 武器绘制、黑雾与粒子入口
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
│   ├── EventideReunionDeathMark.png         # 从灾厄复制的印记贴图，本模组独立加载
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
    ├── EventideReunionSunSounds.cs          # Sun 专属音效定义
    ├── SunSweep.ogg
    ├── SunDashHit.ogg
    └── SunFollowUpSlash.ogg
```

`SamsaraResonance.png` 是当前已有资源；后续实现 Samsara 时，按实际使用范围确定是否迁入其技能目录。

## 通用模块与技能专属模块

- 判断文件归属时，以职责和使用范围为准。只服务于 Sun 的 AI、几何、玩家行为、图层、粒子、特效和音效放入 `Sun/`；其他技能采用同样规则。
- 普通攻击 Holdout 的主逻辑、下劈、绘制、粒子及音效定义与文件统一放在 `NormalAttack/`。Sun 起手复用其中的 `Thrust` 音效，引用继续通过 `EventideReunionSounds.Thrust` 访问。选择轮盘、技能枚举、武器贴图、共享刀光渲染与材质保留在根目录。通用文件调用某个技能类型，并不意味着该文件属于该技能。
- `Runes/` 为技能选择界面提供统一资源，包含 Sun 在内的所有符文均保留在此，不能因为符文名称与某个技能相同就将其迁入技能目录。
- 技能可以复用根目录的公共模块；跨技能复用逻辑应放在公共模块，避免一个技能依赖另一个技能的内部实现。
- 玩家实例数据继续放在 `AnomalyGlobalInstances.AnomalyPlayer` 中，技能目录中的 `AnomalyPlayerBehavior` 仅实现行为，不另建玩家数据容器。

## 命名空间与资源加载

所有 C# 文件统一使用以下命名空间，包括 `NormalAttack/`、`Sun/`、`Death/` 及后续技能子目录中的文件：

```csharp
namespace Anomalies.SpecialContents.Developer.EventideReunion;
```

子目录仅用于文件组织，不追加 `.NormalAttack`、`.Sun` 等命名空间后缀。同名技能辅助类应带上技能名称，例如 `EventideReunionSunGeometry`、`EventideReunionSunSounds`，避免统一命名空间内的类型冲突。

命名空间与文件夹结构不一致可能使 tModLoader 推导出错误的默认材质路径。每个受影响的物品、弹幕、NPC、物块等内容类必须显式重写 `Texture`，指向资源的实际位置；不能因自行绘制或使用 `PreDraw` 就忽略默认材质加载。无材质加载职责的辅助类、玩家行为和绘制图层无需添加材质属性。

当前资源路径约定：

| 用途 | 路径或入口 |
| --- | --- |
| 武器及共享贴图 | `EventideReunionHoldout.AssetPath`，即 `Anomalies/SpecialContents/Developer/EventideReunion/`；Holdout 文件移入 `NormalAttack/` 不改变此共享路径 |
| 选择符文 | `EventideReunionRunes.AssetPath` 加 `<技能名>`；加载与枚举映射统一由 `Runes/EventideReunionRunes.cs` 维护 |
| 普通攻击音效 | `EventideReunionSounds.AssetPath`，指向 `Anomalies/SpecialContents/Developer/EventideReunion/NormalAttack/` |
| Sun 专属音效 | `EventideReunionSunSounds.SunAssetPath`，指向 `Anomalies/SpecialContents/Developer/EventideReunion/Sun/` |

资源标识使用 `/` 分隔，省略扩展名。移动资源或修改命名空间时，同步核对 `Texture`、`LoadTexture`、`ModContent.Request`、`SoundStyle` 以及发光和其他附属材质的引用；路径必须对应真实文件，不能凭目录惯例添加不存在的 `Sounds/` 等层级。

Anomalies 不强依赖灾厄等可选模组。技能所需的外部贴图必须复制到本模组对应技能目录，并显式指向 `Anomalies/` 下的本地资源，不能通过无条件外部资源路径要求玩家启用可选模组。

## 本地化归属

- EventideReunion 管辖的全部本地化内容统一位于 `Mods.Anomalies.SpecialContents.Developer.EventideReunion`，在中英文 `*_Mods.Anomalies.SpecialContents.hjson` 中沿用已有的 `Developer.EventideReunion` 分组格式补充，不分散到默认 `Buffs`、`Projectiles` 或其他根键。
- 武器的 `LocalizationCategory` 为 `SpecialContents.Developer`：tModLoader 会自动追加物品名 `EventideReunion`，对应分组中已有的 `DisplayName` 和 `Tooltip`。`LocalizationPrefix` 直接指向统一分组，避免重复追加物品名。
- 普通攻击、选择轮盘、各技能弹幕及专属 Buff 的 `LocalizationCategory` 统一返回 `EventideReunion.LocalizationCategoryForEventideReunion`（`SpecialContents.Developer.EventideReunion`）。例如死亡印记使用该分组内的 `EventideReunionDeathMark.DisplayName` 和 `Description`；无本地化职责的辅助类、玩家行为和绘制层无需添加该属性。
- 移动文本或新增技能时同步核对所有语言、`LocalizationCategory`、`LocalizationPrefix` 和显式键引用，并移除旧位置的重复文本。
- 具有独立效果身份的弹幕使用自身的默认 `DisplayName` 本地化入口，不重写为武器名称：当前包括 `EventideReunionSunExplosion`（日冕耀斑）、`EventideReunionDeathWave`（寂灭镰波）、`EventideReunionDeathSoul`（收割之魂）。文本按现有 `<类型名>.DisplayName` 格式补充到统一分组。矛身、技能动作载体、选择时持矛及不可见伤害代理继续共用武器显示名，不为这些实现实体增设独立名称。

## 后续技能扩展

1. 新建与技能枚举名称一致的目录，例如 `Samsara/`、`Life/`、`Death/`、`Sacredness/`、`Star/`，集中放置该技能专属代码和资源。
2. 使用 `EventideReunion<技能名>...` 命名类型和代码文件，继续采用根命名空间。按逻辑规模拆分 AI、绘制、粒子、几何等职责，无需为尚不存在的职责创建空文件。
3. 在物品入口与选择模块中接入技能，专属战斗细节仍放在技能目录；通用选择、普通攻击和共享渲染不随技能迁移。
4. 为受目录与命名空间差异影响的内容类显式指定材质，并核对专属音效和共享资源的路径。
5. 检查外部引用和测试工程的源文件链接。Sun 几何测试位于仓库的 `Tests/Eventide.Sun/`，移动其被引用源文件时同步更新项目路径。

调整文件组织后同步维护本文，确保目录树和职责说明与实际代码一致。

## Death 技能

- 死亡印记通过 `ModifyIncomingHit` 将目标受到的最终命中伤害乘以 1.15，对普通敌人和 Boss 均生效；仅斩杀判定排除 Boss。

- 继续使用当前右键轮盘的松键释放入口，冷却为 600 帧，印记持续 360 帧。文档未定的伤害集中为 `EventideReunionDeathSkill` 的 `DashDamageMultiplier`、`SlashDamageMultiplier`、`WaveDamageMultiplier`（当前分别为武器伤害的 3、5、5 倍）。三者独立使用每目标一次的局部命中免疫，回身斩与镰刀波均施加印记，突进矛仅造成伤害。
- 技能按蓄力、瞬步、回身横斩和收招组织；蓄力期间跟随持有者光标，瞬步开始时锁定并同步最终方向，行为与太阳技能一致。瞬步使用玩家尺寸进行地形碰撞，不穿墙。专属表现复用已有武器、烟雾、暗影焰，暗屏使用 Death 专属边缘遮罩。
- 瞬步以不超过 4 像素的小步检查地形和敌人，记录首个穿过的有效敌人。未命中时持续 12 帧，以每帧 48 像素移动共 576 像素。矛或玩家碰撞箱首次命中目标后，从该时刻重新计时，给予 240 帧（4 秒）的穿身预算；目标后方至少留出 96 像素的碰撞箱间距，再结束突进。遇到地形阻挡或持续过长时，若仍重叠则退回最近的无重叠安全位置；没有安全落点时保持保护，不能在敌人体内强制结束。
- 突进期间通过玩家行为提供全伤害免疫，结束后再保留 10 帧，状态存入 `AnomalyPlayer.EventideReunionDeathProtection`。已有更长的无敌时间不被缩短。
- 残影保存生成时的矛角度、缩放、人物方向、手臂姿态和身体帧，不读取真实矛之后的横斩方向。横斩开始后按生成顺序间隔 10 帧回收：先用 8 帧平滑立矛，再用 20 帧加速移向玩家并缩小；每次回收在玩家处产生紫黑星光，最后一枚触发较大的爆发。收招延长至 42 帧以容纳完整回收动画。
- 结束突进时朝首个穿过目标碰撞箱的最近部位锁定横斩方向，保证巨大或狭长目标也能进入斩击范围；目标失效时使用其最后记录的碰撞箱，未接触敌人时向突进后方回斩。方向及挥动符号随技能弹幕同步。
- 横斩和同步释放的镰刀波共用 `EventideReunionDeathGeometry.SlashVector` 的近圆弧与轻微透视投影（半径 320、横向比例 0.92），武器姿态、弧面推进和伤害碰撞使用一致方向。镰刀波不使用原版死神镰刀贴图，使用本项目 `Assets/Effects/Primitives/EventideDeathSweep.fx/.xnb` 绘制明艳紫色亮刃和内部星空。参考 MEAC 永夜刃夜间 `Effects/Screen/Cosmic.fx` 的遮罩星空采样，将 `Images/Cosmic.png` 复制为本地 `Death/Starfield.png`；世界像素采样避免随屏幕比例或圆弧 UV 压扁星空，不需要启用 MEAC。
- 刀光两端、内侧和外缘使用平滑透明度过渡，星空随同一遮罩消失。飞行波沿前后两帧之间的世界轨迹补发紫色细长光点与白紫星屑，粒子脱离波体后向后缓慢漂移并自行消散；拖尾历史仅存于客户端，寿命末期同步减少发射并平滑淡出。
- 镰刀波及突进、横斩伤害代理由单人端或服务器生成并拥有；伤害代理通过 `ProjectileNetReference` 记录技能的 owner + identity，不能跨端使用本地弹幕槽位。印记归属存入 `AnomalyGlobalNPC.EventideReunionDeathOwner`。服务器负责施加印记、斩杀及每次击杀唯一的灵魂生成，避免客户端重复结算。
- 回斩和波在 `ModifyHitNPC` 中登记本次命中的临时归属 `EventideReunionDeathHitOwner`，并在 `OnHitNPC` 中清除。`OnKill` 先于命中后回调执行，直接击杀因此不依赖 Buff 成功挂上（包括印记免疫目标）；分节目标同步登记共享生命根实体，但只有根实体死亡时生成一次治疗球。未致死且未挂上印记的攻击不能留下永久治疗资格。
- 标记 NPC 使用 `Assets/Effects/Primitives/EventideDeathMark.fx/.xnb` 在身体处以纯程序形状绘制深紫外晕、白色亮芯的凹弧四角星与断裂封印环，星体随时间平滑呼吸缩放并发光；仅在 Buff 存在时绘制，大小按 NPC 尺寸限制，不引入额外网络视觉状态。
- 印记目标生命严格低于 25% 才执行斩杀，排除 Boss 及其多节部件。带印记目标被击杀后，治疗灵魂归属于施加印记的玩家。灵魂保留原版幽魂粒子，但用独立追踪 AI，以玩家速度前馈加距离自适应速度追踪；路径碰撞检查防止高速越过玩家。所属客户端实际增加最多 5 点生命（不超过上限，遵循月噬限制），通过原版 `SpiritHeal` 消息同步治疗，并立即销毁灵魂以防重复结算。
- 死亡印记使用从灾厄 `MarkedforDeath` 复制到 `Death/EventideReunionDeathMark.png` 的本地贴图，不需要启用灾厄，也不施加灾厄对应减益；独立 Buff 提供中英文显示。

突进边缘暗屏使用 `Assets/Effects/Overlays/EventideDeathVignette.fx/.xnb` 的解析遮罩，屏幕中心透明，四边按屏幕高度统一尺度平滑变暗，进出阶段平滑淡入淡出；不再写入通用全屏 `DarknessIntensity`。

`Tests/Eventide.Death/` 验证薄目标扫描、宽目标延长穿身、迟命中、空挥距离、地形阻挡、安全退回、96 像素间距、目标后方判定、残影回收顺序与加速缩小、扩大近圆弧与投影旋转一致性，以及静止和高速玩家、远距离传送后的灵魂追踪、治疗上限与月噬限制。
