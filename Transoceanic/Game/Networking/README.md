# NoSync 与同步能力使用约定

Transoceanic 保留 `side = NoSync`。只使用数学、粒子或本地绘制能力的模组可以保持单端使用，默认不启用框架 ExtraAI。

需要 Ocean AI 网络数据的消费模组必须满足以下约定：

1. 消费模组使用 `side = Both`，通过正式的强依赖引用 Transoceanic。tModLoader 会将 Both 模组的 NoSync 依赖纳入连接时的模组同步检查；不得只靠反射软引用假定对端已经安装框架。
2. 服务端、主机和所有客户端使用相同构建的 Transoceanic 与消费模组。框架不要求消费方显式指定最低依赖版本。网络协议改变时必须一起升级。
3. 在消费模组的 `Mod.Load` 或其原生内容 Load 中，各端无条件调用 `TONetworkEnvironment.EnableSync(consumerMod)`，在实体同步开始前完成。从 `ILoadable.Load(Mod mod)` 登记时直接使用参数 mod，不依赖可能尚未初始化的消费方静态主实例。
4. 禁止以本地配置、图形设备、netMode、进入世界条件或可选客户端模组是否存在来决定该开关。一个消费模组启用后，整个加载期间所有 NPC/弹幕的框架 ExtraAI 都采用相同布局；任一端不一致都可能导致整个 ExtraAI 流错位。
5. 开关只允许由 false 变为 true，卸载时由框架复位。消费方不自行关闭，也不把它作为逐世界或逐实体的控制项。
6. 服务器负责 NPC 和服务器所有实体的权威状态；玩家所属弹幕按 tML 的所有者规则生成和更新。开启框架同步不会自动同步任意字段、执行行为或判定生成权限。
7. 物品 Behavior（GlobalItemBehavior、SingleItemBehavior 及其派生类型和 Handler）不支持 NetSend、NetReceive 网络方法，也不会生成或分发这些回调。物品数据应在其实际所属 ModItem 或 GlobalItem 中实现成对的发送与接收；EnableSync 不会启用物品 Behavior 网络方法。
8. PlayerBehavior 不提供玩家网络回调。玩家数据在其实际所属 ModPlayer 中完成成对的状态复制、发送和接收。

本接口不增加运行时握手或动态协议协商。偏离上述约定时，不保证联机兼容。发布前至少实测专用服务器、两个客户端、后加入与重连，不能仅以单机或序列化测试通过代替。

统一入口 EnableSync(Mod consumer) 校验当前加载阶段、消费方注册身份与 Both 侧，记录消费方名称和版本；内容设置完成时输出本地启用状态。旧 SyncEnabled setter 暂时兼容，但未登记启用方会输出警告。设置完成后禁止首次启用，卸载时清空登记与阶段标记。tML 未提供本入口可用的公开强依赖元数据查询，因此 modReferences 强依赖仍由消费方负责，本入口不声称已经验证依赖或对端状态。


2026-10-29 实体 AI 容量调整：Ocean ExtraAI 现在只写一段 AI32 稀疏快照，NPC 为 12 数据槽、弹幕为 7 数据槽；OceanAI64 段及数组已完全删除。计时器槽位也已重排，必须各端一起升级，不能与旧构建混用。通用 AI64 编解码工具仍供消费方使用。详见 [1.3 迁移说明](../../Documentation/Migration-1.3.md)。

1.3 固定字节布局、错误原子性、setter 兼容期限与版本规则见 [兼容性契约](../../Documentation/Compatibility-1.3.md)。
