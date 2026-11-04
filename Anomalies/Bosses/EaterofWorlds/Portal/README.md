# 神明吞噬者传送视效来源

本传送效果最初的代码、着色器、纹理与音效移植自 CalamityMod，原导入内容版权归 Azafure, LLC。当前独立程序生成的替换纹理见下文，不能将历史来源说明理解为这些新图仍是原始资源副本。
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

Cloud、Harsh、Voronoi2 已替换为本项目生成的 512×512 周期噪声；Cloud/Harsh 使用扭曲分形噪声，Voronoi2 使用胞元距离差。雾与入口旋涡的采样已补偿均值及对比度，原图与原着色器保存在仓库 Backup/NoiseMigration。各噪声参数、复现入口和迁移日志见 Tools/NoiseGenerator/<噪声名>/。上表记录原始导入对应关系，其他资源及改编代码的来源与许可保持不变。

本次补齐Melt2、Neuron、Pebble、Sharp，至此Noise目录全部18张已替换为512×512程序生成图。风的四个采样源已分别恢复原图亮度范围及对比度；原始资源对应表仅为来源历史，当前图不再是导入副本。完整记录及预览见Tools/NoiseGenerator对应噪声目录。

## 程序生成裂纹（2026-11-02）

CrackedGlass_Glowing.png 已替换为本项目独立几何算法生成的 960×640 灰度图，不再使用灾厄原图副本。生成器以固定种子构造放射路径、分叉、玻璃碎片连接和多尺度光晕，不读取或描摹原图。保留现有三层旋转叠加与着色器显露逻辑。候选、参数、复现入口与静态对比见 Tools/TextureGenerator/CrackedGlass/README.md；原图与许可证备份至 Backup/TextureMigration/CrackedGlass-2026-11-02。静态对比已完成，游戏内动态视觉仍待验收。本文开头来源说明对其余导入资源及改编代码仍然适用。

后续中心强化：用不规则明暗多边形碎块与黑色缝隙替代短线堆叠的撞击核，软化块边，并调整主裂纹曲率、强弱和中心位置。当前生成参数为 seed 7319 / 19 条主路径 / Glow 1.3；初版 B 与局部放大对照保存在同一工具目录。游戏内动态效果仍待验收。

## 程序生成爆发纹理（2026-11-02）

PlasmaExplosion.png、ShineExplosion1.png、ShineExplosion2.png 已替换为本项目独立程序生成的 2048×2048、不透明、严格灰度 RGB 图片（逐像素 R=G=B）。保留原有资源路径、三层随机旋转、颜色调制、扩散与淡出逻辑。生成器不读取或描摹原图；固定种子、九轮优化记录、单图对比和离线爆发/蓄力预览见 [生成说明](../../../../Tools/TextureGenerator/PortalExplosions/README.md)。原图与许可证位于 Backup/TextureMigration/PortalExplosions-2026-11-02。已完成离线对照，具体孔洞和亮片布局不同；游戏内动态视觉仍需验收。音效、公共层与框架中的其他导入资源及改编代码不因本次三张纹理替换而改变来源。

后续第十轮优化：两张 Shine 加入独立的直线外围尖峰及带缺口宽尖片，减弱拖影并取消极坐标弯曲；Shine1 调整中心星芒与碎片分布，Shine2 将实心菱形改为内凹十字光束。Plasma 主体外环改为固定半径正圆，外围撕裂尖峰独立保留。仍为 2048×2048、不透明严格灰度 RGB，资源路径和动画不变。复现源码、修改前图片及静态/叠加对照位于上述工具目录的 Round10；游戏内动态效果尚未验证。


## 高清裂隙星芒

蓄力十字星改用本模块 RiftStar.png（256×2048 白色 RGBA 透明底，亮度写入 Alpha），由 Tools/TextureGenerator/RiftStar/Generate.ps1 独立解析生成。单束由加宽的渐细高斯亮核与宽柔光晕构成（中心宽度参数 0.36，中段约 0.10；外晕权重 0.32），末端平滑归零；两束旋转 90° 等长叠加，保留白色及紫色层、当前长度系数 6 与蓄力时序。按 64×64 逻辑尺寸归一化，星芒单独使用 LinearClamp，裂纹和其他粒子采样不变。无需着色器，沿用模块客户端加载与卸载。上文 ThePerfectGlow 的星芒引用为历史记录，火花仍使用原版材质。游戏内动态视觉待验收。
