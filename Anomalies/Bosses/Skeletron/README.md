# 普通骷髅王

本目录直属源码实现普通异象与 Ultra 骷髅王。`LegendaryNight/` 是传说之夜，本索引只说明其与普通实现共用的接口。

## 阅读顺序

1. `SkeletronHead.cs`：行为启用条件、属性、命中、锁血与死亡回调。
2. `SkeletronHead.State.cs`：阶段/行为枚举、同步槽位、阶段条件与转场重置。
3. `SkeletronHead.Parameters.cs`：头部攻击参数与兼容参数入口。
4. `SkeletronHead.AI.cs`：同步、目标、初始化、保护、阶段分派、狂暴与退场。
5. `SkeletronHead.Phase1AI.cs`、`.Phase2AI.cs`、`.PhaseChangeAI.cs`、`.Phase3AI.cs`、`.FinalAI.cs`：各阶段实现；具体攻击优先作为阶段方法的本地函数。
6. `SkeletronHead.Combat.cs`：跨阶段攻击切换、悬停运动、特殊世界生成及竞技场查询。
7. `SkeletronHead.Drawing.cs`、`.FinalDrawing.cs`：头部预警、粒子、血条与最终蓄力绘制。

阶段流程：初始化 → 有手阶段 → 无手阶段及强化阶段。Ultra 在无手且达到血量门槛后进入三阶段过渡，再循环雨幕、巡卫与骨潮。普通异象在前三种战斗阶段、Ultra 在三阶段达到最终血量门槛后进入静默/蓄力，随后进入最终追击。

头部在仍有存活手部时可被命中但不可被追踪，防御固定为 9999（包括旋转期间），血条通过独立免疫标记保持锁定；全部手部清除后解除该标记、恢复可被追踪并恢复对应行为的防御。转阶段本身的无敌仍按阶段逻辑处理。

一阶段悬停期间（普通射击或静默期）清空全部手部后，停止射击并重新静默 90 帧，随后直接进入二阶段常态攻击；旋转期间清空手部仍沿用无手旋转收尾。一阶段常态总时长保持 480 帧，最后 60 帧停止发射弹幕，蓄力计时继续推进。

## 共用支持

| 文件 | 职责 |
| --- | --- |
| `SkeletronKinematics.cs` | 头部悬停/旋转、手部待机/蓄势运动；保留原有逐轴制动及限速语义 |
| `SkeletronCombat.cs` | 头骨齐射与加速扇形发射 |
| `SkeletronCombatVisuals.cs` | 骨色与最终冲击波爆发粒子 |
| `SkeletronHandler.cs` | 手部死亡 IL 接入 |
| `SkeletronPhase3Patterns.cs` | 头部嵌套 `Phase3Patterns` 的参数、时间表与缺口几何，供攻击、弹幕和预警共同读取 |

普通骷髅王和传说之夜直接调用 `SkeletronKinematics` 与 `SkeletronCombat`，不经过头部、手部或 Handler 转发。修改共用方法时必须同时检查传说之夜调用，但不要把其独立攻击逻辑并入普通状态机。

## 实体与绘制

- `SkeletronHand.cs` 保持单文件，包含手部状态机、命名回调与预警。
- `Skull.cs` 提供飞行协议与初始化；`Skull.AI.cs` 负责运动；`Skull.Drawing.cs` 负责光束、回退绘制与粒子。保留原有模式数值和七个 AI32 槽的容量，第六号槽不再使用。
- `Crossbone.cs`、`BoneShockwave.cs`、`SkeletronSkullWarning.cs` 和 `SkeletronPhase3Guardian.cs` 各自包含完整的小型实体实现。
- `SkeletronBoneArena.cs` 提供普通与传说之夜共用的十六边形场地、边界碰撞、同步和视觉边界快照。
- `SkeletronPhase3Arena.cs` 保存普通三阶段场地与客户端预警快照。场地边界/雾幕由 `SkeletronPhase3ArenaOverlay.cs` 绘制，场内预警由 `SkeletronPhase3WarningOverlay.cs` 在物块之后调用 `SkeletronPhase3Warnings.cs` 绘制。
- `SkeletronArenaDrawing.cs` 保留圆形和多边形雾幕入口，资源路径保持不变。
- `SkeletronLoot.cs`、`BossHealthBar.Skeletron.cs` 是模块内集成入口；`AnomalyGlobalNPC.Skeletron.cs` 当前保存传说之夜客户端实例数据。

未接入的竞技场骨节发射头骨、独立发射/呼吸时钟、骨节发光及专属场心吸收飞行已删除。场地边界伤害、移动限制和传说之夜使用的死亡特效抑制标记仍各自保留。

## 修改约束与验证

同步槽位、枚举值与 ExtraAI 读写顺序不能因文件拆分而改变。头部 `Byte2` 的零值同时表示首轮悬停尚未结束；后续值表示下一次强化旋转，不得拆成未同步的字段。旧传送槽位保留布局与清零行为，当前普通 AI 不执行传送。

保留同帧 `goto case`、三连冲锋结束时立即开始下一次后撤、先/后递增时钟及扇形末端原有采样边界。战斗平衡修改应与结构整理分别评估。

验证资料由独立 `Validation/` 资料包提供：普通骷髅王运行时回归入口为 `Validation/Tests/Anomalies/Bosses/Skeletron.Phase3/`；模块依赖与排除构建入口为 `Validation/Tests/Anomalies/Bosses/BossIsolation/`。编译及模拟宿主不能替代实际游戏的战斗、动态资源、视觉和联机验收。
