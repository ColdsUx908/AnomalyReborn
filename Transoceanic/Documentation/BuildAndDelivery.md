# 构建与交付

框架目标 net8.0，代码辅助项目目标 netstandard2.0；需要支持当前 preview 语法的 SDK，以及 tModLoader 开发环境和仓库上级的 tModLoader.targets。

从仓库根目录构建：

```powershell
dotnet build Transoceanic/Transoceanic.csproj -p:BuildMod=false -m:1
```

Build 后，框架的 XML API 文档会自动复制为项目根目录的 Transoceanic.xml，bin 中也保留原文件。它是生成产物；交付时使用同次构建的 DLL 与 XML，实际游戏使用前还须完成模组打包。

产品源码、设计规范和使用指南随 Git 提供。自动化测试、审计、验证覆盖记录及历史资料保存在 Git 工作区之外的独立资料包，不复制回源码目录，也不通过 Git 忽略规则隐藏。取得资料包后，核对其文件清单、SHA256、源码版本和构建信息，并按资料包说明配置被测源码与输出路径。没有资料包时不能宣称执行了其中的检查。

构建不能替代真实 tModLoader 加载、多人连接、GPU 绘制与工坊交付检查。公共兼容约定见 [1.3 兼容性契约](Compatibility-1.3.md)，消费方迁移见 [迁移说明](Migration-1.3.md)。
