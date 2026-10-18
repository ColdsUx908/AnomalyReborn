using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Bosses.Skeletron.LegendaryNight.Misc;

/// <summary>
/// 每只手独立持有的血尘，不注册到 Main.dust，由手的 PreDraw 更新和绘制。
/// </summary>
public sealed class SkeletronHandBloodDust
{
    private const int MaxDustCount = 96;
    private readonly List<Dust> _Dust = [];
    private ulong _LastUpdate;
    private ulong? _LastSpawn;
    private bool _Initialized;

    public void Spawn(Vector2 position, int width, int height, float speedX, float speedY, int alpha, Color color, float scale)
    {
        if (Main.gamePaused || Main.gameMenu || _LastSpawn == Main.GameUpdateCount)
            return;
        _LastSpawn = Main.GameUpdateCount;
        if (!_Initialized)
        {
            _LastUpdate = Main.GameUpdateCount;
            _Initialized = true;
        }

        // 复用原版血尘的随机位移、速度、动画帧和尺寸，但不做未投影世界坐标的视野裁剪。
        Dust dust = new()
        {
            active = true,
            type = DustID.Blood,
            position = position + new Vector2(Main.rand.Next(Math.Max(5, width) - 4) + 4f,
                Main.rand.Next(Math.Max(5, height) - 4) + 4f),
            velocity = new Vector2(Main.rand.Next(-20, 21) * 0.1f + speedX, Main.rand.Next(-20, 21) * 0.1f + speedY),
            frame = new Rectangle(10 * DustID.Blood, 10 * Main.rand.Next(3), 8, 8),
            scale = scale * (1f + Main.rand.Next(-20, 21) * 0.01f),
            alpha = alpha,
            color = color,
            noGravity = true
        };

        // 保留原版的血腥效果过滤行为。
        if (!ChildSafety.Disabled && ChildSafety.DangerousDust(dust.type))
        {
            if (Main.rand.Next(2) != 0)
                return;
            dust.type = DustID.Cloud;
            dust.scale = Main.rand.NextFloat() * 1.6f + 0.3f;
            dust.color = Color.Transparent;
            dust.frame = new Rectangle(10 * dust.type, 10 * Main.rand.Next(3), 8, 8);
        }
        if (_Dust.Count >= MaxDustCount)
            _Dust.RemoveAt(0);
        _Dust.Add(dust);
    }

    public void UpdateAndDraw(SpriteBatch spriteBatch, Vector2 drawOffset, float opacity)
    {
        Update(Main.GameUpdateCount, Main.gamePaused);
        foreach (Dust dust in _Dust)
        {
            Color light = Lighting.GetColor((int)((dust.position.X + 4f) / 16f), (int)((dust.position.Y + 4f) / 16f));
            Color color = dust.GetAlpha(light) * opacity;
            spriteBatch.Draw(TextureAssets.Dust.Value, dust.position + drawOffset, dust.frame, color,
                dust.GetVisualRotation(), new Vector2(4f), dust.GetVisualScale(), SpriteEffects.None, 0f);
            if (dust.color.PackedValue != 0)
            {
                Color tint = dust.GetColor(dust.GetAlpha(light)) * opacity;
                spriteBatch.Draw(TextureAssets.Dust.Value, dust.position + drawOffset, dust.frame, tint,
                    dust.GetVisualRotation(), new Vector2(4f), dust.GetVisualScale(), SpriteEffects.None, 0f);
            }
        }
    }

    internal void Update(ulong updateCount, bool paused)
    {
        if (!_Initialized)
        {
            _LastUpdate = updateCount;
            _Initialized = true;
            return;
        }
        ulong elapsed = updateCount >= _LastUpdate ? updateCount - _LastUpdate : 0;
        _LastUpdate = updateCount;
        if (paused || elapsed == 0)
            return;
        // 若长时间没有绘制，直接淘汰已超过自然寿命的粒子，不补跑无限多帧。
        if (elapsed > 60)
        {
            _Dust.Clear();
            return;
        }
        for (ulong frame = 0; frame < elapsed; frame++)
        {
            for (int i = _Dust.Count - 1; i >= 0; i--)
            {
                Dust dust = _Dust[i];
                // 对应原版无重力 Blood 的更新分支（Cloud 是血腥过滤后的替代效果）。
                dust.position += dust.velocity;
                if (dust.type == DustID.Blood)
                    dust.scale -= 0.04f;
                dust.velocity.X *= 0.99f;
                dust.rotation += dust.velocity.X * 0.5f;
                dust.scale -= 0.01f;
                dust.velocity *= 0.92f;
                dust.scale -= 0.04f;
                if (!dust.active || dust.scale < 0.1f)
                    _Dust.RemoveAt(i);
            }
        }
    }
}

public sealed class SkeletronHandBloodDustSystem : ModSystem
{
    public override void Load()
    {
        if (!Main.dedServ)
        {
            IL_Main.DrawNPCDirect += RedirectHandBloodDust;
        }
    }

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            IL_Main.DrawNPCDirect -= RedirectHandBloodDust;
        }
    }

    private static void RedirectHandBloodDust(ILContext il)
    {
        ILCursor cursor = new(il);
        // 仅定位原版 type == SkeletronHand 分支内的血尘生成，不拦截其他实体或受击血尘。
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(NPCID.SkeletronHand),
                i => i.OpCode.FlowControl == FlowControl.Cond_Branch)
            || !cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Dust>(nameof(Dust.NewDust))))
        {
            AnomalyUtils.ILFailure(nameof(SkeletronHandBloodDustSystem), "Could not locate Skeletron hand blood dust creation.");
            return;
        }
        cursor.Remove();
        cursor.Emit(OpCodes.Ldarg_2); // DrawNPCDirect 的 rCurrentNPC。
        cursor.EmitDelegate(CreateHandBloodDust);
    }

    private static int CreateHandBloodDust(Vector2 position, int width, int height, int type, float speedX, float speedY,
        int alpha, Color color, float scale, NPC npc)
    {
        if (npc.type != NPCID.SkeletronHand || npc.Anomaly.CustomAIType != 1 || type != DustID.Blood)
            return Dust.NewDust(position, width, height, type, speedX, speedY, alpha, color, scale);

        SkeletronHandBloodDust blood = npc.Anomaly.HandBloodDust ??= new();
        blood.Spawn(position, width, height, speedX, speedY, alpha, color, scale);
        // 原版紧接着会访问返回槽位的 noGravity。使用 NewDust 自带的失败哨兵，绝不占用全局粒子槽。
        return Main.maxDust;
    }
}
