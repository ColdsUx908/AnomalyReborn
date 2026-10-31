# 粒子与图形资源

所有粒子和 GPU 操作仅用于客户端。创建纹理、读取 Entity.Texture、构造依赖纹理的粒子之前，先跳过 Main.dedServ。资源加载完成前不要构造依赖类型缓存的粒子。运行 Particle 与注册 ParticleDefinition 分离；所属模组由定义提供，带参构造不变。定义由 tML 自动加载流程注册，禁用类型不会进入缓存。

## 全局粒子

ParticleHandler.TrySpawnParticle 返回是否成功提交：容量不足、专用服务器、未初始化或 PreSpawn 否决时不提交。PostSpawn 是提交到缓冲后的通知，抛错不撤销已经提交的对象。

全局活动、待加入及回调预留共同计入 ParticleLimit；重要粒子的单项豁免保留。普通批量生成逐项提交，false 可能代表部分成功；需要整组提交时使用对应的原子批量选项。PreSpawn 自己造成的外部副作用不回滚。

每次 PostUpdateEverything 开始时并入待加入对象，再更新活动列表。Update 中生成的子粒子到下一次更新才开始更新，避免遍历中修改活动列表。Timer 先加一，然后 Update，最后按 AutoUpdatePosition 应用更新后的 Velocity。更新结束移除到期/死亡对象。

世界退出清空全局活动和缓冲列表；卸载释放类型缓存。不要把同一个可变粒子实例交给不同管理者。

绘制检查使用 Main.Camera 的缩放后世界视野，以屏幕中心为基准扩大到两倍宽高，再向四周添加粒子实例的 DrawScreenCheckFluff（世界像素，默认零）。该字段类似原版弹幕的同名屏外绘制边距；大粒子应设置覆盖自身绘制范围的边距，正无穷可禁用剔除。PulseRing 按实际纹理、缩放、双轴形变和旋转包围半径自动更新边距。剔除发生在 PreDraw/PostDraw 前，仅跳过绘制，不停止更新或光照更新。ParticleSet 的 basePosition 参与检查；实体自行绘制的残影仍沿用所属实体的绘制路径。

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

NPC、弹幕默认不分配残影列表。显式访问 Ocean.Afterimages 或调用 SpawnAfterimage 才启用；框架的更新和绘制仅查询已经启用的列表。DisableAfterimages 清空并关闭；Clone 不继承视觉缓存。残影不进入网络状态，接受寿命和颜色参数的 SpawnAfterimage 在服务器上于纹理读取和对象构造前退出；接受已有粒子的重载只跳过提交，调用方仍须避免服务器上构造粒子。

## 渲染目标与 SpriteBatch

RenderTargetPool.Shared 按设备、尺寸和描述符复用目标，目标可能带旧图像，按需清屏。租约用 using 管理，解除目标绑定后归还；不能直接 Dispose Target，不能归还后继续绘制。错池归还抛错，同租约重复归还无效。

ReturnNextFrame 仅用于必须交出本帧所有权的情况；下一帧 Draw 开始归还，卸载时排空。不要把返回计划当作延长租约的可撤销承诺。所有操作在图形主线程执行。

共享池限制每键 4 个、总计 128 个缓存目标，并设置 128 MiB 估算缓存显存预算；超预算归还的目标直接释放。估算包含颜色、mipmap、多重采样和深度缓冲，非 Color 格式按每像素 16 字节保守估计，不计驱动分配对齐。正在租用的目标不受缓存预算限制。租用及每帧绘制开始均可触发过期清理，每秒至多扫描一次，空闲超过五秒的缓存释放。延迟归还复用双缓冲，归还中新登记的租约留到下一轮，异常不阻止其他租约归还。

RenderTargetScope、SpriteBatchScope 用于局部保存和恢复状态；调用方明确坐标空间、采样状态和 Effect 参数。使用 LoadTexture 等特性的静态资源引用由加载系统管理，自建纹理/目标由所有者释放。设备变化、原生资源释放和共享 Effect 状态仍须真实 GPU 验证。

## 粒子定义与清理

通过 ParticleHandler.GetDefinition<T>() 或 particle.Definition 查询已注册定义；内容全名为 ModName/运行粒子完整CLR类型名。GetTemplateInstance<T>() 只提供旧式配置模板，不执行运行构造，也不可提交到任何粒子队列。类型配置 getter 必须不依赖运行构造设置的字段。

ParticleDefinition 的模板与资产引用由 TOCleanup 清理；ParticleHandler 的活动队列与缓存仍沿用 IContentLoader 清理。局部 ParticleSet 继续由拥有者负责，未改为 ModType，也不会因内容注册自动清空外部持有的集合。

## 原生内容加载中的资源

消费方可以在原生 ILoadable.Load 中通过所属 Mod.Assets 请求字体和自身着色器；资产仓库已就绪，但主 Mod.Load 的静态单例可能尚未赋值。专用服务器应跳过资源请求；消费方提供关闭效果的配置时，应在加载入口应用该配置。初始化前登记容器清理，每个筛选器在调用 Load 前单独登记失活，确保部分初始化失败后其余资源仍能释放。
