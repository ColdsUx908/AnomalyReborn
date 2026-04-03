using CalamityMod.NPCs.NormalNPCs;
using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlime_Handler : IContentLoader
{
    public const string AnomalyKingSlimePath = "CalamityAnomalies/Anomaly/KingSlime/";

    [LoadTexture("CalamityMod/NPCs/NormalNPCs/KingSlimeJewelFlash")]
    private static Asset<Texture2D> _flashTexture;
    public static Texture2D FlashTexture => _flashTexture.Value;

    [LoadTexture("CalamityMod/Particles/KingSlimeRubyShards")]
    private static Asset<Texture2D> _rubyShardTexture;
    public static Texture2D RubyShardTexture => _rubyShardTexture.Value;

    [LoadTexture(AnomalyKingSlimePath + "KingSlimeEmeraldShards")]
    private static Asset<Texture2D> _emeraldShardTexture;
    public static Texture2D EmeraldShardTexture => _emeraldShardTexture.Value;

    [LoadTexture(AnomalyKingSlimePath + "KingSlimeSapphireShards")]
    private static Asset<Texture2D> _sapphireShardTexture;
    public static Texture2D SapphireShardTexture => _sapphireShardTexture.Value;

    [LoadTexture(AnomalyKingSlimePath + "KingSlimeRainbowShards")]
    private static Asset<Texture2D> _rainbowShardTexture;
    public static Texture2D RainbowShardTexture => _rainbowShardTexture.Value;

    public static readonly SoundStyle SpawnSound = new("CalamityMod/Sounds/Custom/KingSlimeJewelSpawn") { Volume = 1f };
    public static readonly SoundStyle ShatterSound = new("CalamityMod/Sounds/NPCKilled/CrownJewelShatter") { Volume = 0.3f };
    public static readonly SoundStyle ShootSound = new("CalamityMod/Sounds/Custom/RedJewelFire");

    public static Color RubyColor => Main.zenithWorld ? Color.Cyan : Color.Red;
    public static Color EmeraldColor => Main.zenithWorld ? Color.Purple : Color.Lime with { B = 40 };
    public static Color SapphireColor => Main.zenithWorld ? Color.Yellow : Color.Blue;
    public static Color RainbowColor => Main.DiscoColor;
    public static Color RubyFinalColor => Main.zenithWorld ? new(175, 255, 255) : new(255, 175, 175);
    public static Color EmeraldFinalColor => Main.zenithWorld ? new(255, 175, 255) : new(175, 255, 175);
    public static Color SapphireFinalColor => Main.zenithWorld ? new(255, 255, 175) : new(175, 175, 255);
    public static Color RainbowFinalColor => Color.Lerp(Main.DiscoColor, Color.White, 0.7f);

    /// <summary>
    /// 获取宝石对应的 <see cref="IKingSlimeJewel"/> 实例。
    /// </summary>
    public static IKingSlimeJewel GetKingSlimeJewel(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelRuby => new KingSlimeJewelRuby_Anomaly() { _entity = jewel },
        KingSlimeJewelEmerald emerald => emerald,
        KingSlimeJewelSapphire sapphire => sapphire,
        KingSlimeJewelRainbow rainbow => rainbow,
        _ => null
    };

    public static Color GetColor(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelRuby => RubyColor,
        KingSlimeJewelEmerald => EmeraldColor,
        KingSlimeJewelSapphire => SapphireColor,
        KingSlimeJewelRainbow => RainbowColor,
        _ => Color.White
    };

    public static Color GetFinalColor(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelRuby => RubyFinalColor,
        KingSlimeJewelEmerald => EmeraldFinalColor,
        KingSlimeJewelSapphire => SapphireFinalColor,
        KingSlimeJewelRainbow => RainbowFinalColor,
        _ => Color.White
    };

    public static Color GetFlashColor(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelRuby => Color.Pink,
        KingSlimeJewelEmerald => Color.LimeGreen,
        KingSlimeJewelSapphire => Color.CornflowerBlue,
        KingSlimeJewelRainbow => Color.White,
        _ => Color.White
    };

    /// <summary>
    /// 宝石脱战通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    public static void Despawn(NPC jewel)
    {
        jewel.life = 0;
        jewel.HitEffect();
        jewel.active = false;
        jewel.netUpdate = true;
    }

    public static void Kill(NPC jewel)
    {
        jewel.life = 0;
        jewel.HitEffect();
        jewel.ModNPC?.OnKill();
        jewel.active = false;
        jewel.netUpdate = true;
    }

    /// <summary>
    /// 宝石跟随目标通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    /// <param name="destination">目标点。</param>
    /// <param name="maxVelocityX">水平速度最大值。</param>
    /// <param name="maxVelocityY">竖直速度最大值。</param>
    /// <param name="accelerationX">水平加速度。</param>
    /// <param name="accelerationY">竖直加速度。</param>
    /// <param name="safeDistanceXMax">水平安全距离（宝石中心X坐标减目标中心X坐标）最大值。当水平距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceXMin">水平安全距离（宝石中心X坐标减目标中心X坐标）最小值。当水平距离小于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMax">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最大值。当竖直距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMin">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最小值。当竖直距离小于此值时触发反向移动。</param>
    public static void Move(NPC jewel, Vector2 destination, float maxVelocityX, float maxVelocityY, float accelerationX, float accelerationY, float safeDistanceXMax, float safeDistanceXMin, float safeDistanceYMax, float safeDistanceYMin)
    {
        jewel.damage = 0;
        jewel.knockBackResist = 0.4f;
        jewel.rotation = jewel.velocity.X / 15f;
        Vector2 distance = jewel.Center - destination;
        float adjustedX = distance.X - (safeDistanceXMax + safeDistanceXMin) / 2f;
        float safeX = (safeDistanceXMax - safeDistanceXMin) / 2f;
        float adjustedY = distance.Y - (safeDistanceYMax + safeDistanceYMin) / 2f;
        float safeY = (safeDistanceYMax - safeDistanceYMin) / 2f;
        maxVelocityX = Utils.Remap(Math.Abs(adjustedX), safeX, safeX * 7f, maxVelocityX * 0.65f, maxVelocityX);
        maxVelocityY = Utils.Remap(Math.Abs(adjustedY), safeY, safeY * 7f, maxVelocityY * 0.65f, maxVelocityY, true);
        if (adjustedX > safeX)
        {
            if (jewel.velocity.X > 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X - accelerationX, -maxVelocityX, maxVelocityX);
        }
        else if (adjustedX < -safeX)
        {
            if (jewel.velocity.X < 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X + accelerationX, -maxVelocityX, maxVelocityX);
        }
        if (adjustedY > safeY)
        {
            if (jewel.velocity.Y > 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y - accelerationY, -maxVelocityY, maxVelocityY);
        }
        else if (adjustedY < -safeY)
        {
            if (jewel.velocity.Y < 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y + accelerationY, -maxVelocityY, maxVelocityY);
        }
    }

    /// <summary>
    /// 宝石绘制通用逻辑。
    /// </summary>
    /// <param name="spriteBatch"><c>PreDraw</c> 方法中的 <c>spriteBatch</c> 参数。</param>
    /// <param name="screenPos"><c>PreDraw</c> 方法中的 <c>screenPos</c> 参数。</param>
    /// <param name="jewel">宝石。</param>
    /// <param name="ratio">插值比例。<br/>决定闪烁效果强度。</param>
    public static void DrawJewel(SpriteBatch spriteBatch, Vector2 screenPos, NPC jewel, float ratio)
    {
        bool isRainbowJewel = jewel.ModNPC is KingSlimeJewelRainbow;
        Color jewelColor = GetColor(jewel);
        if (isRainbowJewel)
            TODrawUtils.DrawBorderTextureFromCenter(spriteBatch, jewel.Texture, jewel.Center - screenPos, null, jewelColor, jewel.rotation, jewel.scale, borderWidth: 3f + TOMathUtils.TimeWrappingFunction.GetTimeSin(1f, unsigned: true));
        spriteBatch.DrawFromCenter(jewel.Texture, jewel.Center - screenPos, null, isRainbowJewel ? jewelColor : Color.White with { A = jewel.GraphicAlpha }, jewel.rotation, jewel.scale);
        if (ratio > 0f) //攻击前的闪烁效果
        {
            Color flashColor = Color.Lerp(jewelColor, GetFlashColor(jewel), ratio).MultiplyRGBA(new Color(ratio, ratio, ratio, 0f)) * ratio;
            spriteBatch.DrawFromCenter(FlashTexture, jewel.Center - screenPos, null, flashColor, jewel.rotation, jewel.scale * ratio * (isRainbowJewel ? 1.4f : 1.25f));
        }
    }

    /// <summary>
    /// 宝石攻击效果（提示圈）绘制通用逻辑。
    /// </summary>
    /// <param name="spriteBatch"><c>PreDraw</c> 方法中的 <c>spriteBatch</c> 参数。</param>
    /// <param name="screenPos"><c>PreDraw</c> 方法中的 <c>screenPos</c> 参数。</param>
    /// <param name="jewel">宝石。</param>
    /// <param name="ratio">插值比例。<br/>决定提示圈最终半径、宽度和透明度。</param>
    /// <param name="radius">提示圈最大半径。</param>
    /// <param name="scale"></param>
    public static void DrawAttackEffect(SpriteBatch spriteBatch, Vector2 screenPos, NPC jewel, float ratio, float radius, float scale)
    {
        if (!CAClientConfig.Instance.AuxiliaryVisualEffects || ratio <= 0f)
            return;

        bool isRainbowJewel = jewel.ModNPC is KingSlimeJewelRainbow;
        Color jewelColor = GetColor(jewel);
        Color color = jewelColor with { A = 0 } * Math.Clamp(ratio * 1.5f, 0f, 1f);
        float interpolation = TOMathUtils.Interpolation.QuadraticEaseOut(1f - ratio);
        float interpolation2 = TOMathUtils.Interpolation.QuadraticEaseOut(Math.Clamp(1f - ratio, 0f, 0.2f) * 5f);

        for (int i = 0; i < 300; i++)
        {
            Color color2 = isRainbowJewel ? Color.LerpMany(Color.RainbowColors, (ratio + i / 300f + TOMathUtils.TimeWrappingFunction.GetTimeSin(0.5f, 0.5f, 0f, true)) % 1f) with { A = 0 } * ratio * 1.5f : color;
            spriteBatch.DrawFromCenter(ParticleHandler.GetTexture<OrbParticle>(), jewel.Center + new PolarVector2(radius * interpolation, MathHelper.TwoPi / 300 * i) - screenPos, null, color2, 0f, scale * interpolation2);
        }
    }

    /// <summary>
    /// 宝石粒子生成通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    /// <param name="velocity">粒子速度大小。</param>
    /// <param name="lifetime">粒子存在时长。</param>
    /// <param name="scale">粒子大小。</param>
    public static void SpawnOrbParticle(NPC jewel, float velocity, int lifetime, float scale)
    {
        Color color = jewel.ModNPC switch
        {
            KingSlimeJewelRainbow => Color.GetRandomRainbowColor(),
            _ => GetColor(jewel)
        };
        ParticleHandler.SpawnParticle(new OrbParticle(jewel.Center, Main.rand.NextPolarVector2(velocity), lifetime, scale, color, lifeEndRatio: 0.925f));
    }

    public static void SpawnPointingParticle(NPC jewel, int amount, bool extraParticle)
    {
        bool isRainbowJewel = jewel.ModNPC is KingSlimeJewelRainbow;
        Color color = GetColor(jewel);
        Color flashColor = GetFlashColor(jewel);

        for (int i = 0; i < amount; i++)
        {
            ParticleHandler.SpawnParticle(new PointingParticle(jewel.Center, new Vector2(Main.rand.NextFloat(20), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.8f, 1.5f), isRainbowJewel ? Color.GetRandomRainbowColor() with { A = 0 } : color));
            if (extraParticle)
                ParticleHandler.SpawnParticle(new PointingParticle(jewel.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.8f, 1.5f), isRainbowJewel ? Color.GetRandomRainbowColor() : flashColor));
        }
    }

    public static void CreateDustFromJewelTo(NPC jewel, Vector2 destination, int type)
    {
        int maxDustIterations = (int)Vector2.Distance(jewel.Center, destination); //distance
        int maxDust = 100;
        int dustDivisor = Math.Max(maxDustIterations / maxDust, 2);

        Vector2 start = jewel.Center;
        Vector2 spinningpoint = Vector2.UnitX.RotatedByRandom();
        for (int i = 0; i < maxDustIterations; i++)
        {
            if (i % dustDivisor == 0)
            {
                Vector2 position = Vector2.Lerp(start, destination, i / (float)maxDustIterations);
                Dust.NewDustAction(position, 0, 0, type, Vector2.Zero, d =>
                {
                    d.position = position;
                    d.velocity = spinningpoint.RotatedBy(MathHelper.TwoPi * i / maxDustIterations) * (0.9f + Main.rand.NextFloat() * 0.2f);
                    d.noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        d.scale = 0.5f;
                        d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                    Dust d2 = Dust.CloneDust(d);
                    d2.scale *= 0.5f;
                    d2.fadeIn *= 0.5f;
                });
            }
        }
    }

    public static bool CheckIfPhase2(NPC jewel) => GetKingSlimeJewel(jewel).HasEnteredPhase2;

    public static void EnterPhase2(NPC jewel)
    {
        for (int i = 0; i < 20; i++)
            SpawnOrbParticle(jewel, Main.rand.NextFloat(2f, 2.5f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
        SoundEngine.PlaySound(jewel.DeathSound, jewel.Center);
        GetKingSlimeJewel(jewel)?.HasEnteredPhase2 = true;
        jewel.dontTakeDamage = true;
        jewel.netUpdate = true;
    }

    public static void DisableAttack(NPC jewel) => GetKingSlimeJewel(jewel)?.CanAttack = false;

    public static int GetRandomDustID() => Main.rand.Next(7) switch
    {
        0 => DustID.GemAmethyst,
        1 => DustID.GemTopaz,
        2 => DustID.GemSapphire,
        3 => DustID.GemEmerald,
        4 => DustID.GemRuby,
        5 => DustID.GemDiamond,
        6 => DustID.GemAmber,
        _ => DustID.TintableDust
    };

    public static void HitEffect(NPC jewel) => SpawnPointingParticle(jewel, 6, false);

    public static void OnKill(NPC jewel)
    {
        bool isRainbowJewel = jewel.ModNPC is KingSlimeJewelRainbow;
        SpawnPointingParticle(jewel, 6, true);

        Texture2D shardTexture = jewel.ModNPC switch
        {
            KingSlimeJewelRuby => RubyShardTexture,
            KingSlimeJewelEmerald => EmeraldShardTexture,
            KingSlimeJewelSapphire => SapphireShardTexture,
            KingSlimeJewelRainbow => RainbowShardTexture,
            _ => null
        };

        float start = Main.rand.NextFloat(MathHelper.TwoPi);
        for (int i = 0; i < 3; i++)
        {
            int iClone = i;
            CustomSpriteParticle particle = isRainbowJewel
                ? new(shardTexture, jewel.Center, new Vector2(0, -2).RotatedByRandom(start + MathHelper.ToRadians(20f)).RotatedBy(MathHelper.ToRadians(i * 125)), 120, 0f, 1f, Color.GetRandomRainbowColor(), Main.rand.NextFloat(0.2f, 0.6f), BlendState.Additive, customGetFrameAction: p => p.Texture.Frame(1, 3, 0, iClone))
                : new(shardTexture, jewel.Center, new Vector2(0, -2).RotatedByRandom(start + MathHelper.ToRadians(20f)).RotatedBy(MathHelper.ToRadians(i * 125)), 120, 0f, 1f, new Color(255, 255, 255), Main.rand.NextFloat(0.2f, 0.6f), BlendState.Additive, customGetFrameAction: p => p.Texture.Frame(1, 3, 0, iClone));
            ParticleHandler.SpawnParticle(particle);
        }

        SoundEngine.PlaySound(ShatterSound, jewel.Center);
    }
}