using Terraria.GameContent.UI.BigProgressBar;

namespace Anomalies.Visuals.BossBar;

public sealed class BossBarStyleRetro : ModBossBarStyle
{
    public override string DisplayName => "Anomalies (Retro)";
    public override bool PreventDraw => true;

    /// <summary>
    /// 每帧更新血条系统，清理无效条目，为符合条件的 NPC 创建新血条并推进动画。
    /// </summary>
    /// <param name="currentBar">原版大进度条接口，此处未使用。</param>
    /// <param name="info">进度条信息，此处未使用。</param>
    public override void Update(IBigProgressBar currentBar, ref BigProgressBarInfo info) => BossBarHandler.Update();

    /// <summary>
    /// 绘制所有活跃的血条，按有效状态排序，并自动调整纵向布局。
    /// Retro 风格在绘制时传入不同标志以切换外观。
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch。</param>
    /// <param name="currentBar">原版大进度条接口，此处未使用。</param>
    /// <param name="info">进度条信息，此处未使用。</param>
    public override void Draw(SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info) => BossBarHandler.Draw(spriteBatch, false);
}
