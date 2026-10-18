# 克苏鲁之脑战斗移植

来源为本地 CalamityMod 的 `NPCs/VanillaNPCAIOverrides/Bosses/BrainOfCthulhu`、`Projectiles/Boss/BrainOfCthulhu` 及所用的粒子、效果和音效。实体名与幻象死亡提示沿用 CalamityCN 的英中本地化，统一放在 `Mods.Anomalies.Bosses.BrainofCthulhu`。

## 组织与状态

- `BrainofCthulhu.cs`：参数、属性、命中和死亡处理。
- `BrainofCthulhu.State.cs`：Phase / Behavior / CurrentLocalPhase 与同步 AI 数组映射。
- `BrainofCthulhu.AI.cs`：阶段分派、初始化、目标选择与离场；入场和死亡演出为 `PreAI` 内的本地函数。
- `BrainofCthulhu.Phase1AI.cs`、`BrainofCthulhu.PhaseChangeAI.cs`、`BrainofCthulhu.Phase2AI.cs`：一阶段（含眩晕）、转阶段、二阶段的私有入口，各自包含本阶段的本地行为函数。
- `BrainofCthulhu.Drawing.cs`：触须、主脑与幻象绘制。
- `CreeperAI`：飞眼怪 Behavior；它的阶段由主脑 Phase 驱动，LocalPhase 计时保存在自身 AI 数组。
- `Visuals`、`Sounds`、`Assets`：克脑专用演出与纹理选择、音效及资源。
- `Anomalies/Assets/Effects/ScreenFilters`：克脑护盾与径向模糊；`Primitives/CircularAoETelegraph`：范围预警。统一由 `AnomalyEffects` 注册，遵循着色器配置开关。

灾厄 Death 的参数对应普通 Anomaly（异象），Ultra 暂不额外改变克脑 AI。飞眼怪的生成、再生、血量比例与触须数组统一使用 `CreeperCount = 30`。未启用异象模式时，原版或灾厄自己的 AI 保持原有入口。灾厄是可选依赖，相关调用隔离在带 `ExtendsFromMod` 的兼容类中；安装灾厄时还会保留肾上腺素重置和防御损伤的原有交互。

## 通用 API 与资源复用

粒子更新、分组绘制、上限与生命周期统一使用 Transoceanic 的 `Particle` / `ParticleHandler`。普通和定向圆环统一复用已有 `PulseRing`；BloomCircle、HollowCircleHardEdge 与透明纹理不再保留重复副本。

血液粒子和星芒粒子归入 `Transoceanic/DataStructures/Particles`，并共享该目录的纹理。`TexturePulseParticle` 提供任意纹理的扩散、非等比缩放、混合模式和淡出；克脑只保留纹理和种子特殊表现的薄封装。`PathAfterimageParticle` 复用现有残影基类，提供按路径移动且由调用者决定绘制顺序的残影。

`Transoceanic/DataStructures/BezierCurve` 提供包含两个端点、数量确定的采样；`VerletSimulatedSegment` 提供带端点锁定、重力和可选惯性衰减的链模拟。克脑直接调用这些通用 API。

Behavior 不保存实体实例字段。主脑的普通状态使用 NPC.ai 与 AnomalyAI32/64；攻击队列和玩家目标历史通过 BrainByteList/BrainTargetSet 打包到同一套快照。触须、动画帧和残影是客户端视觉数据。具体数组布局写在 State 文件中。

本目录声明的属性与索引器统一为 public。主脑的 AI 行为函数收在对应阶段入口内，飞眼怪的 8 个 AI 专用函数收在 `PreAI` 内；同时服务于 AI、命中和死亡回调的状态重置、死亡触发等方法保留为类成员，绘制与同步回调保留框架要求的入口。

## 验证与游戏内复核

先编译通用库：`dotnet msbuild Transoceanic/Transoceanic.csproj /t:Build /p:BuildProjectReferences=false /p:BuildMod=false`；再编译克脑代码：`dotnet msbuild Anomalies/Anomalies.csproj /t:ResolveReferences,Compile /p:BuildProjectReferences=false`。

存储回归检查位于 `Tests/BrainofCthulhu.State`，覆盖跨 32/64 位数组边界的队列、玩家目标位图、实体隔离、快照恢复及同步保留槽，并包含 Bézier 采样与 Verlet 模拟的边界检查。执行 `dotnet run --project Tests/BrainofCthulhu.State/BrainofCthulhu.State.csproj`；同目录的 `verify_assets.py` 检查本地资源引用、统一着色器路径、本地化键及难度/数量约定。

仍需游戏内复核：普通异象与 Ultra 各完整战斗一次；不同高度及血腥脊椎入场（统一使用地表行为）；飞眼怪死亡后断裂触须及再生；眩晕中压到 70% 转阶段；25% 强制幻象；真假脑命中与超时惩罚；死亡演出；所有玩家死亡或远离后的收场；猩红外召唤与战斗；两类幻象均不显示血条。联机应覆盖两名玩家、战斗中途加入、真假脑同时命中及无灾厄环境。

护盾噪声统一使用 `AnomalyTextures.Extra.Vein2` 与 `AnomalyTextures.Extra.Voronoi`。音效文件不使用灾厄前缀或下划线，对应代码字段统一采用 `Sound` 后缀。幻象惩罚不施加血液沸腾，额外 Buff、玩家状态类、纹理与本地化均已移除。

战斗和血腥脊椎召唤不要求猩红环境；入场、飞眼怪攻击方向与 CrimsonEyes 追击速度均固定采用地表行为。

`BrainBloodShot` 与 `CreeperBloodShot` 独立实现灾厄对两类原版血弹的 AI 覆盖及特殊绘制，不再依赖 `AIType` 继承类型判断。血弹保留转向 AI 数组，使用原版缓存长度和朝向拖尾、灾厄血尘与颜色；所有尘埃使用 Action 生成接口。真假脑贴图选择同时考虑资源包检测和灾厄换皮配置，冲刺幻象按实际主脑贴图尺寸取帧，缩放仍跟随主脑。
