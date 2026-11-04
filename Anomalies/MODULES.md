# Anomalies 构建模块

模块使用构建时开关：MSBuild 属性默认全部为 `true`，启用时加入对应的条件编译标签。修改 `AnomaliesModules.props` 中的默认值，或通过 `dotnet build` 的 `-p:` 参数覆盖；属性只接受 `true` / `false`。切换后需要重新构建并打包模组，不是游戏运行时配置。

| 标签 | 构建属性 | 内容 |
| --- | --- | --- |
| `ANOMALY_MODE` | `AnomaliesEnableAnomalyMode` | 异象难度、裂天金冕、Boss 修改及其专属实体、难度兼容、故事/极限模式、相关模式机制 |
| `TWEAK` | `AnomaliesEnableTweak` | `Tweaks/` 下的武器修改和修改标识（目前为太空枪），以及玩家基础属性配置和对应行为 |
| `GAME_CONTENTS` | `AnomaliesEnableGameContents` | `GameContents/` 的具体内容及旅行商店接入；共享基类除外 |
| `SPECIAL_CONTENTS` | `AnomaliesEnableSpecialContents` | `SpecialContents/` 的内容及其出生物品、玩家同步接入 |

例如，保留其他模块并关闭异象难度：

```powershell
dotnet build Anomalies/Anomalies.csproj -p:AnomaliesEnableAnomalyMode=false
```

全部关闭：

```powershell
dotnet build Anomalies/Anomalies.csproj -p:AnomaliesEnableAnomalyMode=false -p:AnomaliesEnableTweak=false -p:AnomaliesEnableGameContents=false -p:AnomaliesEnableSpecialContents=false
```

C# 检查可以追加 `-p:BuildMod=false`，实际进入游戏前须完成正常打包。

`AnomaliesModules.props` 只按目录筛选整组源码，不列举单文件例外；单文件归属和跨模块接入统一使用源码 `#if`。SpecialContents 内的普通内容使用 `SPECIAL_CONTENTS`，测试物品仅使用 `DEBUG`。新增模块内容应放入对应目录；新增公共接入时应显式处理模块关闭的情况。四个模块互不要求对方开启。菜单、星空和血条为默认内容，不另设模块；公共层原则上不自行产生可见副作用。除明确的默认内容和测试物品外，所有额外内容必须归属现有模块。新增模块只能由项目所有者 ColdsUx 最终决定并提交，代理不得自行新增或代为提交。

跨模块内容的归属：

- HoneyedWildbloom、宠物弹幕与 Buff 由 `GAME_CONTENTS` 控制；蜂后掉落入口同时要求 `GAME_CONTENTS` 和 `ANOMALY_MODE`，并保留世界已开启异象模式的运行时条件。仅关闭难度时，物品仍存在，原有物品仍可使用，但不会注册其掉落。
- 异象掉落条件位于 `Common/AnomalyLootConditions.cs`，由 `ANOMALY_MODE` 控制。传奇骷髅王不依赖普通游戏内容的掉落入口。
- `GameContents/Base/` 仅保留共享基类。跨模块的物品标识契约、稀有度和提示绘制支持位于 `Common/Items/`，保留原命名空间；具体稀有度与提示行为只在至少一个内容模块开启时编译，单独开启 `TWEAK` 不会引入它们。
- 测试物品是例外，只要求 `DEBUG`，不叠加功能模块开关。测试物品的骷髅王专属操作通过 Boss 模块内的可选部分类方法接入；关闭 `ANOMALY_MODE` 或移除骷髅王模块时，测试物品仍存在，但不执行这些操作。
- 关闭 `ANOMALY_MODE` 时，世界难度查询始终为 `false`，存档读取、网络收包与 Mod.Call 不能激活难度。公共网络入口仍消费已定义消息的固定载荷。
- 全关时除 `DEBUG` 测试物品外，不编译新增物品、NPC、弹幕、Buff 或具体实体修改行为；菜单、星空和血条保持默认开启，保留必要的公共数据容器、接口及视觉支持。

验证脚本随独立验证资料包提供：`pwsh -NoProfile -File Tests/Modules/Verify.ps1`。它构建全部 16 种组合，检查实际输出 DLL 中的类型，并恢复默认全开构建。构建和元数据检查不替代游戏内加载、战斗和联机验证。
