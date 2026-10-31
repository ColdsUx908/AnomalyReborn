# 接入 Transoceanic

## 最小消费模组

在消费模组的 build.txt 中建立正式依赖：

```ini
displayName = ExampleConsumer
author = Example
version = 0.1
modReferences = Transoceanic
side = Both
```

普通只使用数学或本地视觉功能的模组不需要开启框架同步；选择符合自身实际用途的 side。

需要框架发现 Behavior、后置加载器或资源特性的程序集，添加：

```csharp
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.Behaviors;
using Transoceanic.Core.ModIntegration;
using Transoceanic.Game.Networking;

[assembly: SupportedByTransoceanic]

namespace ExampleConsumer;

public sealed class ExampleConsumer : Mod
{
    public override void Load()
    {
        // Both 消费方必须添加本行，并在各端无条件执行。
        TONetworkEnvironment.EnableSync(this);
    }
}

public sealed class BlueSlimeBehavior : SingleNPCBehavior
{
    public override int ApplyingType => NPCID.BlueSlime;

    public override void AI()
    {
        // Entity 为当前 NPC；仅演示读取，不改变原版行为。
        _ = Entity.Center;
    }
}
```

源码开发时引用同构建的 Transoceanic 项目或 DLL；若消费方使用框架的源生成能力，还须将 TransoceanicCodeAssist 配为 Analyzer，不能把它作为普通运行时依赖。参考本仓库项目中的 ProjectReference 配置。不要依靠框架的 GlobalUsings，它们不会传播到消费方。

## 状态和回调

Behavior 按类型共享，实例字段不能保存某个实体的计时、缓存或网络状态。数据归属 ModNPC/ModProjectile、对应的 Global 实例或 ModPlayer。第三方继承公开的 GlobalNPCBehavior、GlobalProjectileBehavior、GlobalItemBehavior、PlayerBehavior 或 Single 类型；Mod 由 tML 注册时赋值，不再重写。

框架调度负责实体绑定和锁定。回调期间不能通过同一个单例切换到其他实体；手工 new 的 Behavior 没有 Mod 或内容初始化，仅可用于不依赖内容生命周期的独立逻辑；不要手工调用 ILoadable.Load，否则会重复注册。直接查询后调用时以 `using var entityLock = behavior.LockEntity();` 包围调用，不缓存共享 Behavior。

全局业务枚举使用 BehaviorQueries；明确传入实体和回调名，避免自定义辅助函数的 CallerMemberName 意外筛选错误方法。ShouldProcess、优先级和方法分组仍生效。

## 联机与权限

EnableSync 只启用框架实体数据的 ExtraAI 段，不会自动同步任意字段、玩家数据，也不授权客户端生成服务器实体。服务器管理 NPC 和服务器所有实体；玩家所属弹幕遵循 tML 所有者规则。

物品 Behavior（包括 GlobalItemBehavior、SingleItemBehavior 及其派生类型和 Handler）不支持 NetSend、NetReceive；物品网络数据自行在所属 ModItem 或 GlobalItem 中成对实现。

玩家数据自行在所属 ModPlayer 中实现 CopyClientState、SyncPlayer、SendClientChanges。PlayerBehavior 不提供这些接口。所有新增载荷须成对写入/读取，保持顺序和类型一致；中途加入需要完整可恢复状态。

## 常见错误

- 自动发现不到类型：先核对程序集标记、抽象/开放泛型过滤、正式依赖、有效无参构造函数，以及 Autoload/IsLoadingEnabled 是否允许注册。
- 实体绑定异常：检查是否跨实体重入共享 Behavior；不要禁用绑定保护。
- 仅服务器资源为空：图形资源和粒子只用于客户端，先检查 Main.dedServ。
- 联机 ExtraAI 错位：核对所有端的同构建和 Load 阶段无条件启用，不按本地配置切换。
- 找不到测试脚本：测试通过独立验证资料包提供，见验证文档；不是 Git 源码包缺失。

## 选择加载入口

仅安装独立钩子、初始化常量数据或请求自身资产时，使用 tML 原生 Load：ModType/ModSystem 重写无参 Load，普通服务实现 ILoadable.Load(Mod mod)。内容自动加载早于 Mod.Load，使用 tML 提供的 Mod，不依赖主 Mod.Load 才赋值的静态单例。

需要完整内容 ID、跨模组集合或其他加载器结果时，继续使用 PostSetupContent 等正确阶段；LoadPriority=0 不表示可以提前。自有引用在初始化前登记 TOCleanup，On/IL 钩子交由 tML 管理。
