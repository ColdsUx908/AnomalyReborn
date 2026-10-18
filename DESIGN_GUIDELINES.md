# AnomalyReborn 设计规范

## 着色器入口命名

- `Anomalies` 项目中所有着色器文件的主函数统一命名为 `Main`，大小写必须保持一致。
- `technique` / `pass` 中的着色器编译入口必须同步引用 `Main`，例如：`PixelShader = compile ps_3_0 Main();`。着色器模型版本按实际需求保留。
- `technique` 名称统一为 `Technique1`，`pass` 名称统一为 `Pass0`，大小写必须保持一致。
- C# 中注册、加载或应用着色器时，相关名称引用必须与着色器文件一致，pass 名称使用 `Pass0`。
- 新增或修改着色器时遵循以上规范；辅助函数可按其用途命名。

## 实体生成

- 生成 NPC、弹幕、Dust、Gore 等实体或视觉对象时，均使用现有的 Action 尾缀接口（如 `NPC.NewNPCAction`、`Projectile.NewProjectileAction`、`Dust.NewDustAction`、`Dust.NewDustPerfectAction`、`Gore.NewGoreAction`），复用各接口的生成检查、初始化及适用的同步逻辑。
- 替换生成接口时核对坐标语义（中心或左上角）、默认速度和缩放等差异，保持原有表现；附加属性统一在 Action 回调内设置。
- AI 参数、目标、速度、寿命等生成后的初始化统一放在 Action 回调内，在实体同步前完成。保留具有专用网络协议或召唤语义的框架入口，如 `NPC.SpawnOnPlayer`。

## AI 行为组织

- 行为函数尽量用本地函数，放在直接调用它们的 AI 方法内；跨阶段或同时被其他回调使用的逻辑保留为类方法。
- 较大的阶段 AI 可拆为独立文件中的私有阶段方法，在主 AI 入口分派；每个阶段的具体行为和阶段内辅助函数放在该阶段方法内。

## 实体数据存放

- 只在 `AnomalyGlobalInstances` 的 `GlobalType` 类型内存放对应类型的数据：NPC 数据归入 `AnomalyGlobalNPC`，弹幕数据归入 `AnomalyGlobalProjectile`，其他实体按对应类型归属。
- 客户端视觉数据同样遵循该规则，不另建仅用于存储数据的 GlobalType；Behavior 通过实体的对应 GlobalType 访问数据。
- 可变容器在实体克隆时必须独立处理，避免不同实体共享状态；仅用于绘制的数据不加入网络快照。

- 上述存放约定针对继承 `GlobalNPC`、`GlobalProjectile`、`GlobalItem` 等 GlobalType 的数据容器；`ModNPC`、`ModProjectile`、`ModItem` 等实体自身类型可以正常持有各自数据。
- 仅提供行为、没有存储数据的 GlobalType，如果存在对应的 Behavior 基类，应重构为对应的 Behavior；不存在对应基类时可保留 GlobalType。
