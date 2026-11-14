# 粒子与图形资源

粒子定义在客户端与专用服务器均注册并初始化；资源加载完成后，两端都可以正常调用粒子构造函数和请求纹理资产。专用服务器的纹理请求沿用 tModLoader 的占位资源行为，不创建 GPU 纹理。ParticleHandler、ParticleSet 和实体残影提交入口统一拒绝服务器提交，更新和绘制入口跳过服务器，因此普通调用点无需为了构造粒子额外判断 Main.dedServ。直接访问 GraphicsDevice、创建纹理或执行自定义绘制仍须只在客户端进行。运行 Particle 与注册 ParticleDefinition 分离；所属模组由定义提供。定义由 tML 自动加载流程注册，禁用类型不会进入缓存；缓存就绪前或定义禁用时，构造仍会抛出明确异常。

## 全局粒子

ParticleHandler.TrySpawnParticle 返回是否成功提交：容量不足、专用服务器、未初始化或 PreSpawn 否决时不提交。PostSpawn 是提交到缓冲后的通知，抛错不撤销已经提交的对象。

全局活动、待加入及回调预留共同计入 ParticleLimit；重要粒子的单项豁免保留。普通批量生成逐项提交，false 可能代表部分成功；需要整组提交时使用对应的原子批量选项。PreSpawn 自己造成的外部副作用不回滚。

每次 PostUpdateEverything 开始时并入待加入对象，再更新活动列表。Update 中生成的子粒子到下一次更新才开始更新，避免遍历中修改活动列表。Timer 先加一，然后 Update，最后按 AutoUpdatePosition 应用更新后的 Velocity。更新结束移除到期/死亡对象。

世界退出清空全局活动和缓冲列表；卸载释放类型缓存。不要把同一个可变粒子实例交给不同管理者。

绘制检查使用 Main.Camera 的缩放后世界视野，以屏幕中心为基准扩大到两倍宽高，再向四周添加粒子实例的 DrawScreenCheckFluff（世界像素，默认零）。该字段类似原版弹幕的同名屏外绘制边距；大粒子应设置覆盖自身绘制范围的边距，正无穷可禁用剔除。PulseRing 按实际纹理、缩放、双轴形变和旋转包围半径自动更新边距。剔除发生在 PreDraw/PostDraw 前，仅跳过绘制，不停止更新或光照更新。ParticleSet 的 basePosition 参与检查；实体自行绘制的残影仍沿用所属实体的绘制路径。全局粒子分类和每个 ParticleSet.DrawSet 在本批开始时读取一次摄像机范围；绘制回调对摄像机的修改从下一批生效。

## 局部 ParticleSet

ParticleSet 使用局部坐标，由调用方管理集合寿命、调用 Update 和 DrawSet。它不计入全局容量；自己的 ParticleLimit 默认 5000，活动、待加入和 PreSpawn 预留均计入容量，重要粒子允许豁免。

```csharp
bool accepted = set.TrySpawnParticle(particle);
set.Update();
set.DrawSet(basePosition);
// 拥有者退出或不再使用集合时：
set.Clear();
```

Particles 是只读活动视图，不包含缓冲。TrySpawnParticle 拒绝 null（抛 ArgumentNullException）、重复对象、容量不足、PreSpawn 否决或服务器提交；后四种情况返回 false。PreSpawn/PostSpawn 都会执行，继承类须据此管理副作用。集合不提供粒子对象复用池，不重置已提交对象的状态。

DrawSet 按接受提交的先后顺序绘制，不再按 Timer 排序，OrderFunction 已移除。全局粒子仍先按混合状态分组，每组保持提交顺序。

自动生成仍按 ParticleSpawnRate 和 SetLifetime 判断，先提交再合并缓冲，保持新自动粒子当帧更新；更新或绘制回调中新提交的粒子推迟至下一次 Update。手动提交不受自动生成寿命截止限制。重复提交同一集合已经持有的对象返回 false。

更新/绘制期间禁止递归 Update、DrawSet 或 Clear，抛 InvalidOperationException；允许 TrySpawnParticle。Update 异常时仍解除保护、清理死亡对象，但不递增集合 LocalTimer，未执行的粒子不会被补更新。绘制异常也解除保护。PreSpawn 清空集合会取消该次提交；PostSpawn 清空则移除已提交对象。

Clear 清空两个列表和占用记录，不重置 LocalTimer；世界退出不会自动清理消费方自己持有的集合，所有者负责 Clear 或释放引用。Particles 视图不是线程安全快照。

## 实体残影

NPC、弹幕默认不分配残影列表。显式访问 Ocean.Afterimages 或调用 SpawnAfterimage 才启用；框架的更新和绘制仅查询已经启用的列表。DisableAfterimages 清空并关闭；Clone 不继承视觉缓存。残影不进入网络状态，接受寿命和颜色参数的 SpawnAfterimage 在服务器上于纹理读取和对象构造前退出；接受已有粒子的重载也只跳过服务器提交，允许调用方先构造粒子；定义仍须已完成初始化。

默认 AfterimageParticle.Draw 在光照查询和精灵提交前进行保守屏外检查；边距覆盖帧尺寸、缩放（含负值）、任意旋转及 DrawOffset，另叠加实例 DrawScreenCheckFluff 的更大边距，正无穷可禁用剔除。NPC 和弹幕各自的默认残影绘制批次共用一次视野快照。残影更新时序、寿命边界和提交顺序保持不变，自定义绘制仍由调用方管理。

ParticleSet 为死亡清理复用构造时创建的谓词，避免逐次更新创建绑定委托；仍先更新全部粒子再清理，允许后续粒子改变先前粒子的死亡状态。

## 渲染目标与 SpriteBatch

RenderTargetPool.Shared 按设备、尺寸和描述符复用目标，目标可能带旧图像，按需清屏。租约用 using 管理，解除目标绑定后归还；不能直接 Dispose Target，不能归还后继续绘制。错池归还抛错，同租约重复归还无效。

ReturnNextFrame 仅用于必须交出本帧所有权的情况；下一帧 Draw 开始归还，卸载时排空。不要把返回计划当作延长租约的可撤销承诺。所有操作在图形主线程执行。

共享池限制每键 4 个、总计 128 个缓存目标，并设置 128 MiB 估算缓存显存预算；超预算归还的目标直接释放。估算包含颜色、mipmap、多重采样和深度缓冲，非 Color 格式按每像素 16 字节保守估计，不计驱动分配对齐。正在租用的目标不受缓存预算限制。租用及每帧绘制开始均可触发过期清理，每秒至多扫描一次，空闲超过五秒的缓存释放。延迟归还复用双缓冲，归还中新登记的租约留到下一轮，异常不阻止其他租约归还。

RenderTargetScope、SpriteBatchScope 用于局部保存和恢复状态；调用方明确坐标空间、采样状态和 Effect 参数。使用 LoadTexture 等特性的静态资源引用由加载系统管理，自建纹理/目标由所有者释放。设备变化、原生资源释放和共享 Effect 状态仍须真实 GPU 验证。

## Bloom 与像素精灵

`LightweightBloom.Draw` 在当前视口捕获一个图层，使用半分辨率近光晕和可选的四分之一分辨率远光晕，再合成回原场景。`BloomSettings.Default` 的半径为最终画面像素的高斯标准差；相机缩放、世界半径和昼夜合成策略由内容侧换算，不由框架猜测。非有限/负半径按零处理，非正/非有限强度不合成对应光晕。

```csharp
BloomSettings settings = BloomSettings.Default;
// 调用前 SpriteBatch 未 Begin，回调自行配对 Begin/End。
LightweightBloom.Draw(spriteBatch, enabled, settings, () =>
{
    spriteBatch.Begin(SpriteSortMode.Deferred, LightweightBloom.AdditiveBlend);
    SpriteDrawing.DrawGlow(spriteBatch, texture, position, new Vector2(64f), Color.White, 0f, 1.5f);
    spriteBatch.End();
});
```

热路径可以实现值类型 `IBloomLayer`，使用泛型 `Draw` 避免捕获委托；调用同步完成，不保留图层或纹理引用。自动资产入口使用框架管理的 Effect，显式 Effect 入口方便独立宿主；调用方持有的 Effect 不会被释放。框架在卸载时释放自己的混合状态，独立宿主可调用 `ReleaseResources`。

所有入口须在图形主线程运行，SpriteBatch 必须未开始，输入资源属于同一设备且未释放。不能在回调中切换目标、重建设备或释放使用中的资源；无内容时不要调用。透明图层使用预乘 RGBA；`AdditiveBlend` 同时累加颜色与覆盖度。关闭效果、资产未加载或显式 Effect 为 null 时执行原图层，不租用捕获目标。

`Draw` 支持非零视口原点，按视口尺寸捕获；回调仍按视口相对像素与自行提供的相机矩阵绘制。恢复原目标、视口、裁剪、混合/深度/光栅状态与纹理/采样槽 0；回调异常会关闭遗留批次后传播，已绘制的场景内容不提供事务回滚。其他纹理槽、顶点和索引缓冲由调用方负责。原 RT 的 Usage 会持久改为 PreserveContents，回缓冲的 Usage 则在调用后恢复。设备变化后应重新提供有效资源，资源池按设备和尺寸分别租用。

`Blur` 和 `Composite` 是低层入口，不保存设备状态。Blur 要求源、scratch、result 各不相同，两个工作目标等尺寸；返回时 result 仍绑定。Composite 不切换目标，拒绝输入与目标别名，目标矩形非正尺寸时不提交。图层入口内部的租约在解除目标绑定后归还。低层绘制异常时由调用方恢复 SpriteBatch 和设备状态。

`WorldView2D` 接受显式的世界偏移、相机矩阵与有限正视口尺寸，提供像素/UV 换算与相机尺度；不读取全局相机，不重复做长宽比修正。`SpriteDrawing.DrawSized/DrawGlow` 只提交中心对齐的完整纹理，不 Begin/End、不加载资源；不透明度不超过 0.002 或非正尺寸不提交，Glow 将不大于 64 的有限强度分为若干次提交，调用方选择合适的混合状态。

## 粒子定义与清理

通过 ParticleHandler.GetDefinition<T>() 或 particle.Definition 查询已注册定义；内容全名为 ModName/运行粒子完整CLR类型名。GetTemplateInstance<T>() 只提供旧式配置模板，不执行运行构造，也不可提交到任何粒子队列。类型配置 getter 必须不依赖运行构造设置的字段。

ParticleDefinition 的模板与资产引用由 TOCleanup 清理；ParticleHandler 的活动队列与缓存仍沿用 IContentLoader 清理。局部 ParticleSet 继续由拥有者负责，未改为 ModType，也不会因内容注册自动清空外部持有的集合。

## 原生内容加载中的资源

消费方可以在原生 ILoadable.Load 中通过所属 Mod.Assets 请求字体和自身着色器；资产仓库已就绪，但主 Mod.Load 的静态单例可能尚未赋值。专用服务器应跳过资源请求；消费方提供关闭效果的配置时，应在加载入口应用该配置。初始化前登记容器清理，每个筛选器在调用 Load 前单独登记失活，确保部分初始化失败后其余资源仍能释放。
