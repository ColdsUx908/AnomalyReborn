# Transoceanic

面向 tModLoader 的通用框架，提供实体 Behavior、可选 ExtraAI 同步、几何与数学工具、粒子、渲染目标租约、资源加载和运行时扩展。框架不依赖特定内容模组。

当前源码版本为 1.3，包含源码、二进制和网络协议变更。消费方应重新编译；启用同步的各端使用相同构建。版本号不表示已经完成工坊发布或真实客户端验证。

## 开始使用

- [接入与最小示例](Documentation/GettingStarted.md)：依赖、程序集标记、Behavior 和同步。
- [功能与 API 导航](Documentation/API.md)：各模块入口及使用限制。
- [加载生命周期与实体绑定](Core/Loading/README.md)。
- [网络约定与 ExtraAI 布局](Game/Networking/README.md)。
- [粒子与绘制资源](Documentation/Graphics.md)。
- [1.3 兼容性契约](Documentation/Compatibility-1.3.md)。
- [1.3 迁移说明](Documentation/Migration-1.3.md)。
- [构建、文档和验证资料](Documentation/Validation.md)。

## 构建要求

框架目标 net8.0，语言版本 preview；代码辅助项目目标 netstandard2.0。当前源码使用 C# 扩展成员和 field 访问器，需要支持这些语法的 SDK（验证环境使用 .NET SDK 10）。

项目导入仓库上级的 tModLoader.targets；按 tModLoader 开发环境提供此文件和游戏依赖。

构建后，项目根目录自动取得 `Transoceanic.xml`。它是构建产物，和 bin 中的 XML 内容相同，不参与 Git。
