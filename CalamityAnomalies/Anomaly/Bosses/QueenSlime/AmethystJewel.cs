// Developed by ColdsUx

using CalamityAnomalies.Anomaly.Bosses.KingSlime;
using CalamityAnomalies.DataStructures;

namespace CalamityAnomalies.Anomaly.Bosses.QueenSlime;

public sealed class AmethystJewel : JewelNPC
{
    public enum Behavior : byte
    {
        None = 0,
        Normal,
        Heart,
    }

    public static int ShootCooldownTime => 240;

    private static readonly ProjectileDamageContainer _JewelProjectileRainbowDamage = new(40, 60, 90, 120, 90, 120);
    public static int JewelProjectileRainbowDamage => _JewelProjectileRainbowDamage.Value;

    public const float MaxProjectileSpeed = 18f;

    public Behavior CurrentBehavior
    {
        get => (Behavior)AI_Union_0.byte0;
        set
        {
            Union32 union = AI_Union_0;
            union.byte0 = (byte)value;
            AI_Union_0 = union;
        }
    }

    public override string LocalizationCategory => "Anomaly.QueenSlime";

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.damage = 25;
        NPC.width = 28;
        NPC.height = 28;
        NPC.defense = 10;

        NPC.lifeMax = 7000;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.2f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = JewelHandler.HitSound;
        NPC.DeathSound = JewelHandler.ShatterSound;
        CalamityNPC.VulnerableToSickness = false;

        NPC.IsImportantBossMinion = true;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (MasterDead)
        {
            JewelHandler.Kill(NPC);
            return;
        }

        if (!NPC.TryGetMaster(NPCID.QueenSlimeBoss, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return;
        }

        Lighting.AddLight(NPC.Center, 0.8f, 0f, 0.8f);

        NPC.damage = 0;

        if (CanAttack)
            JewelHandler.Move(NPC, Target.Center, 15f, 15f, 0.175f, 0.125f, 250f, -250f, -250f, -400f);
        else
            JewelHandler.Move(NPC, master.Center, 20f, 20f, 0.2f, 0.15f, 150f, -150f, 0f, -200f);

        QueenSlime masterBehavior = QueenSlime.GetInstance(master);

        if (CanAttack)
        {
            switch (CurrentBehavior)
            {
            }
        }
        else
        {
            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }

        NPC.netUpdate = true;

        return;

        void Attack_Normal()
        {
            SoundEngine.PlaySound(JewelHandler.ShootSound, NPC.Center);
            for (int i = 0; i < 20; i++)
                JewelHandler.SpawnOrbParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 50), Main.rand.NextFloat(0.4f, 0.7f));
            JewelHandler.SpawnPointingParticle(NPC, 6, true);

            JewelHandler.CreateDustFromJewelTo(NPC, master.Center, -1, true);

            if (TOSharedData.NotClient)
            {
                int amount = 9;
                float singleRadian = MathHelper.TwoPi / amount;
                Vector2 originalVelocity = (PolarVector2)NPC.GetVelocityTowards(Target, MaxProjectileSpeed * 0.85f);
                Projectile.NewProjectilesArc<RainbowJewelProjectile>(amount, singleRadian, SourceAI, NPC.Center, originalVelocity, JewelProjectileRainbowDamage, 0f, action: p => p.ai[0] = RainbowJewelProjectile.TextureType_Circle);
            }

            CurrentBehavior = Behavior.None;
            Timer1 = 0;
        }
    }

    public static bool CheckMasterJump(QueenSlime behavior) =>
        behavior.CurrentBehavior is QueenSlime.Behavior.Phase1_FirstJump or QueenSlime.Behavior.Phase1_HighJump
        && behavior.CurrentAttackPhase == 0;

    public static bool CheckShoot(QueenSlime behavior) => CheckMasterJump(behavior) && behavior.Timer1 == QueenSlime.JumpDelay;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC);
        return false;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        JewelHandler.HitEffect(NPC);
    }

    public override void OnKill()
    {
        JewelHandler.OnKill(NPC);
    }
}
