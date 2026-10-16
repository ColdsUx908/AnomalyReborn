namespace Anomalies.DataStructures;

/// <summary>
/// 在 AI 结束时同步离散状态、瞬移和速度突变，不同步普通的逐帧移动。
/// </summary>
public readonly struct BossNetUpdateScope : IDisposable
{
    private readonly NPC _NPC;
    private readonly Vector2 _Velocity;
    private readonly Vector2 _Position;
    private readonly int _State;
    private readonly int _Target;
    private readonly bool _Active;

    /// <summary>记录 NPC 执行本次 AI 前的运动状态、目标及离散状态。</summary>
    /// <param name="npc">需要监测同步时机的 NPC。</param>
    public BossNetUpdateScope(NPC npc)
    {
        _NPC = npc;
        _Velocity = npc.velocity;
        _Position = npc.position;
        // ai[0] 可能是 Union32 打包数据，必须按位比较（包括 NaN 位型）。
        _State = BitConverter.SingleToInt32Bits(npc.ai[0]);
        _Target = npc.target;
        _Active = npc.active;
    }

    /// <summary>比较 AI 执行前后的状态，在非客户端检测到突变时请求同步 NPC。</summary>
    public void Dispose()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        // 速度每帧变化至少 4 像素，或高速运动明显转向时，发送新的运动起点。
        float speedSquared = _Velocity.LengthSquared();
        float newSpeedSquared = _NPC.velocity.LengthSquared();
        bool sharpTurn = speedSquared >= 16f && newSpeedSquared >= 16f
            && Vector2.Dot(_Velocity, _NPC.velocity) < 0.8660254f * MathF.Sqrt(speedSquared * newSpeedSquared);
        if (Vector2.DistanceSquared(_Velocity, _NPC.velocity) >= 16f || sharpTurn
            || Vector2.DistanceSquared(_Position, _NPC.position) >= 64f
            || _State != BitConverter.SingleToInt32Bits(_NPC.ai[0])
            || _Target != _NPC.target || _Active != _NPC.active)
            _NPC.netUpdate = true;
    }
}
