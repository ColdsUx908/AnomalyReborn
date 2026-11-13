# Overlay 加载与绘制

`CustomOverlay` 与 `PostTilesOverlay` 均继承 `RegisteredContent<T>`，使用 tModLoader 内容注册和生命周期。可以重写 `IsLoadingEnabled(Mod)`；返回 false 的类型不进入绘制与更新集合。

管理器在 PostSetupContent 中只收集受框架支持模组的实际注册实例，不反射重建实例，也不绕过构造函数。内容初始化和绘制使用同一实例；不重写封闭的 Register 或 SetupContent。卸载时管理器清空集合，自有资源仍按原生命周期释放。

落日遗逢的 SunFlash 和 DeathVignette 使用 `AnomalySpecialContentToggler.EventideReunion`。其他屏幕效果沿用默认加载条件。共享资源路径和现有绘制层次保持不变。
