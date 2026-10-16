namespace Anomalies.DataStructures;

/// <summary>
/// 弹幕槽位 whoAmI 是本地索引；跨端引用必须使用 owner + identity。
/// </summary>
public struct ProjectileNetReference
{
    private int _Owner;
    private int _Identity;
    private int _LocalIndex;
    private bool _HasValue;

    /// <summary>获取可存入 AnomalyAI64 的引用值；高、低 32 位分别保存加一后的所有者和标识，零表示空引用。</summary>
    public readonly long PackedValue => _HasValue ? (long)(((ulong)(uint)(_Owner + 1) << 32) | ((uint)_Identity + 1u)) : 0L;

    /// <summary>从打包值更新引用；值改变时清除旧的本地槽位缓存。</summary>
    /// <param name="value">由 <see cref="PackedValue"/> 获取的引用值，或表示空引用的零。</param>
    /// <exception cref="InvalidDataException">所有者或弹幕标识无效。</exception>
    public void SetPackedValue(long value)
    {
        if (PackedValue == value)
            return;
        if (value == 0)
        {
            this = default;
            return;
        }
        int owner = unchecked((int)((ulong)value >> 32)) - 1;
        int identity = unchecked((int)((uint)value - 1u));
        if ((uint)owner > Main.maxPlayers || identity < 0)
            throw new InvalidDataException("Invalid packed projectile reference.");
        this = new ProjectileNetReference { _HasValue = true, _Owner = owner, _Identity = identity, _LocalIndex = -1 };
    }

    /// <summary>记录弹幕的所有者、跨端标识和本地槽位；空或未激活的弹幕会清空引用。</summary>
    /// <param name="projectile">要引用的弹幕，可为空。</param>
    public void Set(Projectile projectile)
    {
        _HasValue = projectile is { active: true };
        _Owner = _HasValue ? projectile.owner : -1;
        _Identity = _HasValue ? projectile.identity : -1;
        _LocalIndex = _HasValue ? projectile.whoAmI : -1;
    }

    /// <summary>通过所有者和标识查找当前端的弹幕，并更新本地槽位缓存。</summary>
    /// <returns>匹配的活动弹幕；引用为空或弹幕尚未到达时返回占位弹幕。</returns>
    public Projectile Resolve()
    {
        if (!_HasValue)
            return Projectile.DummyProjectile;
        if ((uint)_LocalIndex < Main.maxProjectiles && Matches(Main.projectile[_LocalIndex]))
            return Main.projectile[_LocalIndex];
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (!Matches(Main.projectile[i]))
                continue;
            _LocalIndex = i;
            return Main.projectile[i];
        }
        return Projectile.DummyProjectile;
    }

    private readonly bool Matches(Projectile projectile) => projectile.active && projectile.owner == _Owner && projectile.identity == _Identity;

    /// <summary>依次写入所有者和弹幕标识，不发送本地槽位索引。</summary>
    /// <param name="writer">接收引用数据的二进制写入器。</param>
    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(_HasValue ? _Owner : -1);
        writer.Write(_HasValue ? _Identity : -1);
    }

    /// <summary>按 <see cref="Write"/> 的顺序读取并校验弹幕引用。</summary>
    /// <param name="reader">提供引用数据的二进制读取器。</param>
    /// <returns>未缓存本地槽位的引用；两个值均为负一时返回空引用。</returns>
    /// <exception cref="InvalidDataException">所有者或弹幕标识无效。</exception>
    public static ProjectileNetReference Read(BinaryReader reader)
    {
        int owner = reader.ReadInt32();
        int identity = reader.ReadInt32();
        if (owner == -1 && identity == -1)
            return default;
        if ((uint)owner > Main.maxPlayers || identity < 0)
            throw new InvalidDataException("Invalid projectile reference.");
        return new ProjectileNetReference { _HasValue = true, _Owner = owner, _Identity = identity, _LocalIndex = -1 };
    }
}
