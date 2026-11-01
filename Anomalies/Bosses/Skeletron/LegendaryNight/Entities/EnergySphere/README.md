# 传说之夜群星物质光球

蓄力时手掌持续产生星芒，沿弧线汇入光球。光球保持原版的金色窄亮边与集中白色光核，内部的赤金底纹、金白光带和星芒共用持续旋转的对数螺旋坐标，形成银河旋臂；关闭着色器时使用旋转星芒与分层光晕回退。

投掷飞行时从球体后半部持续喷发金橙色星芒和光点。抵达竞技场边缘时在圆周接触点产生一次较强撞击火花，停留期间持续沿圆周切线向两侧喷出摩擦火星与橙红火光。接触粒子恢复全局粒子绘制顺序，竞技场遮罩仅保留半透明波纹迷雾，不再绘制不透明色底。光球使用包含着色器亮边偏移的接触半径，抵住边缘后由 ai[2]=2 表示停靠，并逐帧约束圆心至内切圆；没有新增网络载荷字段。

三次方蓄力增长和分层光晕参考 FargowiltasCrossmod 的 AresBodyEternity.DoBehavior_LargeTeslaOrbBlast_ManageSphere 与 LargeTeslaSphere。着色器保留 LargeTeslaSphereShader 的光球边缘构思，内部改为本项目的流动星辰，不再使用电噪声；参考 MEAC 羲和 Sun.fx 的径向亮度和角向光芒分层思路，自行实现金白色光带，没有复制其代码或资源。

导入的 Electric.png、Perlin2.png 已集中放入 Assets/Textures/Noise，通过 AnomalyTextures.Noise 访问。Electric 保留为通用资源，本光球不使用它。TeslaSphereLoop.ogg 保留在当前目录。导入资源及改编代码的 MIT 许可位于 Licenses/FargowiltasCrossmod-MIT.txt。

着色器使用局部方形精灵 UV，不需要屏幕长宽比修正。
