# Boss 模块隔离检查

在仓库根目录使用 PowerShell 7 运行 `Tests/BossIsolation/Verify.ps1`。脚本依次在 MSBuild 中排除每个 Boss 直属子文件夹的源码和 AdditionalFiles，再构建完整项目恢复正常输出；不移动或删除工作区文件，也不执行 tModLoader 打包或写入游戏目录。

`ExcludeBoss.targets` 在编译依赖缓存生成前排除模块，确保切换被排除的 Boss 时会重新编译，避免增量构建误报通过。专属 GlobalNPC 数据、掉落、血条和网络部分类也必须随模块排除。

每次构建后读取程序集元数据，确认被排除的 Boss 命名空间不存在，防止错误复用完整构建产物。

运行 `python Tests/BossIsolation/verify_dependencies.py` 检查源码中的跨模块命名空间与资源路径引用。该检查保留本地化分类字符串，不要求删除公共本地化条目。

编译与静态检查不覆盖游戏内动态资源加载、完整战斗或多人同步；这些仍需在游戏内验证。
