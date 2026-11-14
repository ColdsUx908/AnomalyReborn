# 功能与 API 导航

本文提供模块索引。具体类型、方法、参数、单位、异常及生命周期约定见同构建的源码 XML 注释和 Transoceanic.xml。

| 目录 / 命名空间 | 基本功能 |
| --- | --- |
| Behaviors | 实体行为、调度与查询 |
| Core/Loading | 加载生命周期与优先级 |
| Core/ModIntegration | 模组接入与可选依赖引用 |
| Game/Networking | 网络环境与 AI 数据编解码 |
| GlobalInstances | 框架实体与玩家数据 |
| Game/Targeting | NPC、玩家与 PvP 目标检索 |
| Game/Combat | 战斗工具、寻敌选项与持续伤害 |
| Game/Progression | Boss 挑战与击败进度 |
| Game/Timing、World、Entities | 游戏计时、世界状态与实体遍历 |
| Primitives | 集合、数值、几何、数学、计时与物理基础类型 |
| Extensions | System、Terraria 与 Xna 扩展 |
| Graphics/Particles | 全局与局部粒子系统 |
| Graphics/Rendering | 渲染目标、绘制状态、LightweightBloom 与 IBloomLayer 图层提交 |
| Graphics/Assets | 图形资源加载与公共资源 |
| Graphics/Drawing | 绘制工具、WorldView2D 坐标转换与 SpriteDrawing 像素精灵 |
| Runtime | 反射、Hook、Detour、IL 编辑与成员访问辅助 |
| Input/AutoHosting | 本地输入托管 |
| Commands、Localization、UI | 命令、本地化与界面控件 |

接入步骤见 [接入指南](GettingStarted.md)，构建与验证见 [验证说明](Validation.md)，版本变化见 [迁移说明](Migration-1.3.md)。

1.3 的 API、网络、生成器及数学边界承诺见 [兼容性契约](Compatibility-1.3.md)。

确定性视觉配方可使用 `Primitives.Mathematics.Mulberry32Random`；结构体复制保留序列位置，`NextSingle` 的浮点舍入可能产生 1。缓动入口 `TOMathUtils.Interpolation.BinaryExponentialEaseOut` 表示以 2 为底的缓出，与接受幂次的 `ExponentialEaseOut` 不同。Bloom 图层的完整示例、主线程和资源约定见[图形资源](Graphics.md)。
