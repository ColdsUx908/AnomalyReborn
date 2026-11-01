# 神明吞噬者传送视效来源

代码、着色器、纹理与音效移植自 CalamityMod，版权归 Azafure, LLC。
来源：https://github.com/CalamityTeam/CalamityModPublic 。许可证原文见 Calamity-LICENSE.md。

对应原实现：DoGTeleportRift、DoGP1EndPortal、DoGRiftCrack、DoGDistortionMetaball、DoGVisualsManager 的前景绘制，以及 SparkParticle、SquishyLightParticle、CustomPulse、StrongBloom。仅替换本项目实体、粒子、渲染目标和同步接口。伪裂隙与神明吞噬者天空不参与此攻击。

着色器统一由 AnomalyEffects 管理，入口为 Main / ps_3_0 / Technique1 / Pass0。屏幕 UV 的云雾和风增加以 16:9 为基准的比例修正，裂纹与旋涡为精灵局部坐标。

## 本地复用

- 火花直接使用 Transoceanic 的 PointingParticle，并指定原版 TextureAssets.Extra[ExtrasID.ThePerfectGlow]（额外材质 89）；不再保留 EaterRiftSpark。
- 扩散脉冲直接使用 TexturePulseParticle；不再保留 EaterRiftPulse。
- EaterRiftBloom 继承 BloomParticle，只保留原效果的正弦闪光、速度衰减与双重透明度；它与普通 BloomParticle 的淡出曲线不同。
- EaterRiftLight 使用框架 Light、BloomCircle 纹理；其速度形变和淡出曲线不同于现有 SquashParticle，保留专属行为。
- BasicCircle 使用 TOTextures.Extra.BasicCircle，纯白 Square 使用 TOTextures.FillerTexture，BloomCircle 使用框架 BloomParticle 纹理。
- BloomRing 从克脑模块移至 Anomalies/Assets/Textures/Extra/BloomRing.png；两个 Boss 均从公共层引用，不相互依赖。
- StarProj、原先的 ImmaculateBolt 纹理实际对应原版 ThePerfectGlow。已删除 Extra/Star.png 及其加载字段；传送星芒直接使用 TextureAssets.Extra[ExtrasID.ThePerfectGlow]，ImmaculateBolt 的 Texture 指向对应原版 Extra 资源。
- 已删除 Portal 中的 BasicCircle、BloomCircle、BloomRing、Light、Square、StarProj 六张副本。其余独有纹理予以保留；噪声统一移至公共 Noise 目录。

## 噪声资源

噪声统一由 AnomalyTextures.Noise 的 LoadTexture 字段加载，文件名、_Name 字段和 Name 属性对应，按字母排序。原始文件与本地命名：

| 原始文件 | Anomalies/Assets/Textures/Noise |
| --- | --- |
| HarshNoise | Harsh.png |
| MeltyNoise | Melt2.png |
| Neurons2 | Neuron.png |
| Pebbles | Pebble.png |
| RealisticClouds | Cloud.png |
| SharpNoise | Sharp.png |
| VoronoiShapes | Voronoi2.png |

Melt2、Voronoi2 使用已有噪声族的数字后缀，保持原有 Melt、Voronoi 资源及调用。Portal 的局部缓存仅管理裂纹与爆裂精灵，不再重复加载噪声。
