# 构建、文档与验证交付

## Git 与独立资料

产品源码、设计规范和本项目下的使用文档随 Git 保存。仓库根 Tests/ 和 docs/ 是本地验证及改动记录资料，不作为 Git 项目成员，由项目维护者通过独立验证资料包交付。没有资料包的 Git 检出不应被描述为测试文件缺失缺陷，也不能宣称运行了这些测试。

取得与源码对应的资料包后，校验 manifest.json 的文件 SHA256、源码基础提交和二进制构建信息，将 Tests/、docs/ 解压到仓库根目录。资料包不包含 tModLoader 等外部依赖，也不包含 bin/obj；按本地开发环境恢复和构建。

维护者在本地通过 `pwsh -File Tests/Export-ValidationBundle.ps1` 导出资料包，默认输出到被 Git 忽略的 artifacts/validation/。manifest 记录基础提交、工作区差异摘要及当前构建 DLL 的哈希；未提交源码需与对应源码交付配套，单靠提交号不能还原它。

## 验证命令

从仓库根目录运行：

```powershell
dotnet build Transoceanic/Transoceanic.csproj -p:BuildMod=false -m:1
dotnet run --project Tests/Transoceanic/ReleaseReadiness/ReleaseReadiness.Tests.csproj
```

ReleaseReadiness 只读取已构建的框架 DLL/XML 和生成器 DLL，覆盖实际几何/数学性质、宿主 IL 结构、独立消费方、API 基线及固定网络字节检查，不需要任何具体消费模组。

仓库维护用入口 `pwsh -File Tests/Transoceanic/Verify.ps1` 还覆盖 XML、输入托管、基础集合、生命周期/绑定/克隆、模拟粒子/资源池、Behavior 生成和网络编解码；其中包含消费方集成用例，需要相应测试源码与依赖。此入口及其 `-IncludeConsumers` 扩展检查不作为独立使用框架的环境要求。

独立发布契约测试需要 tModLoader 安装目录，默认从仓库上级 tModLoader.targets 的 Import 路径解析，也可通过 TML_GAME_PATH 环境变量提供。缺少依赖应明确失败，不能跳过检查或将环境失败计入通过。

测试使用现代 C# 语法，部分模拟项目目标 net10.0；产品仍目标 net8.0，生成器仍目标 netstandard2.0。

## XML 交付

Transoceanic 项目保持 GenerateDocumentationFile，Build 后自动复制到项目根 Transoceanic.xml，bin 中的原文件也保留。不要把生成 XML 提交 Git。开发者交付时，将同构建 XML 与 DLL 一同提供；仅生成 XML 不代表工坊已经交付该文件。

XML 编译检查验证引用、参数和结构，使用指南补充模块导航、最小接入、同步布局、资源生命周期及失败语义。新增公共 API 时同步更新 XML 和相关指南；行为变化补入当前迁移说明。

## 尚需宿主验证

模拟验证不能代替：框架单独安装、真实加载/卸载与热重载；世界切换和战斗退出；专用服务器双客户端、后加入、重连及不同构建诊断；原生 MonoMod Hook/IL；GPU 绘制、设备与分辨率变化；实际工坊打包和交付清单。这些留作后续真实客户端验证，不记入模拟测试通过结果。

## ModType 内容回归

Runtime 测试还链接实际 RegisteredContent、ContentRegistration、TOCleanup、TypeDetour 与 TODetourHandler，使用模拟 tML 查询表检查所属模组、禁用/未注册内容排除、手动闭合泛型内容、注册实例复用、失败清理、重复加载、提供者优先级和 ShouldApplyDetour。Graphics 测试覆盖 ParticleDefinition 注册、带参运行构造、模板提交拒绝、专用服务器排除和定义清理；这些不表示已经通过真实 tML 自动加载或原生钩子验证。

## 原生加载迁移回归

Runtime 测试验证实际 On_TOExtensions 原生加载后的消费方订阅不会被 PostSetup 清空、卸载与重载清理、TOBossChallenge 事件以及标识符和可选模组引用。NetworkSync 额外验证主实例赋值前使用入口 Mod 登记同步，载荷往返检查保持不变。

## 独立发布契约测试

`dotnet run --project Tests/Transoceanic/ReleaseReadiness/ReleaseReadiness.Tests.csproj` 只读取实际框架 DLL/XML 和生成器 DLL，不引用产品源码项目，不加入产品项目。`--release` 选择先行构建的 Release 产物。`--update-api` 仅在审查兼容性变更及迁移说明后显式更新候选 API 基线；统一验证不自动更新。测试与基线随独立验证资料包交付。公开承诺见 [1.3 兼容性契约](Compatibility-1.3.md)。
