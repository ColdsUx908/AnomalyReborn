namespace Anomalies.Bosses.BrainofCthulhu;

[AutoloadBossHead]
public class FalseBrain : AnomalyModNPC
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_23";

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults()
    {
        SpawnTime = 60;
        NPC.width = 160;
        NPC.height = 110;
        NPC.damage = 0;
        NPC.lifeMax = 1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
        NPC.BossBar = ModContent.GetInstance<BrainIllusionBossBar>();
        NPC.immortal = true;
        NPC.dontTakeDamage = true;
        NPC.chaseable = false;
        NPC.npcSlots = 0f;
        NPC.netAlways = true;
        NPC.ShowNameOnHover = false;
        Music = MusicID.Boss3;
        SceneEffectPriority = (SceneEffectPriority)(-1);
        NPC.localAI[0] = Main.rand.Next(6);
        NPC.localAI[1] = 1 + Main.rand.NextFloat(-0.25f, 0.25f);
    }

    public int Variant => (int)NPC.localAI[0];
    public float Angle => NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public int AttackTime { get => (int)NPC.ai[2]; set => NPC.ai[2] = value; }

    public bool BeenHit
    {
        get => AnomalyNPC.AnomalyAI32[0].i != 0;
        set
        {
            AnomalyNPC.AnomalyAI32[0].i = value ? 1 : 0;
            AnomalyNPC.AIChanged32[0] = true;
        }
    }

    public int SpawnTime
    {
        get => AnomalyNPC.AnomalyAI32[1].i;
        set
        {
            AnomalyNPC.AnomalyAI32[1].i = value;
            AnomalyNPC.AIChanged32[1] = true;
        }
    }

    public static float TimeDivisor => 360f;
    public float DrawPriority => (float)Math.Cos(-Time * MathHelper.TwoPi / TimeDivisor);

    internal static List<int> blackListedProjectiles = [];
    public override void AI()
    {
        if (!BrainBehaviorAccess.ActiveBrain)
        {
            NPC.active = false;
            return;
        }

        NPC brain = Main.npc[NPC.crimsonBoss];
        NPC.scale = brain.scale;
        NPC.GivenName = brain.GivenOrTypeName + $": {brain.life}/{brain.lifeMax}";
        if (brain.GetBrainBehavior().AttackFlag)
            BeenHit = true;
        if (BeenHit)
        {
            NPC.dontTakeDamage = true;
            if (AttackTime++ >= 60)
                NPC.active = false;
        }
        else
        {
            float lerp = TOMathUtils.Interpolation.SineEaseInOut(MathHelper.Clamp(SpawnTime / -30f, 0f, 1f), clamped: false);
            float baseDist = 240;
            float circleDist = 480;
            if (SpawnTime != -30)
            {
                baseDist = MathHelper.Lerp(480, 240, lerp);
                circleDist = MathHelper.Lerp(240, 480, lerp);
            }

            NPC.Center = brain.GetBrainBehavior().AttackPosition + Vector2.UnitX.RotatedBy(Angle) * (baseDist + (circleDist * ((float)Math.Sin(-Time * MathHelper.TwoPi / TimeDivisor) / 2f + 0.5f)));
            NPC.Center += Vector2.UnitX.RotatedBy(Angle + MathHelper.PiOver2) * (90 * (float)Math.Cos(Time * MathHelper.TwoPi / TimeDivisor));
        }

        if (SpawnTime > 0)
            SpawnTime--;
        else if (SpawnTime >= -30)
        {
            Time += --SpawnTime / -30f;
            if (SpawnTime <= -30)
                NPC.dontTakeDamage = false;
        }
        else
            Time++;
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.Write(BeenHit);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        BeenHit = reader.ReadBoolean();
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (BeenHit)
            return;
        if (Main.netMode == NetmodeID.MultiplayerClient)
            BrainIllusionHit.Send(NPC.whoAmI, player.whoAmI);
        else
            BrainIllusionHit.Apply(NPC.whoAmI, player.whoAmI);
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (BeenHit || projectile.Anomaly.BrainOfCthulhu_IgnoreIllusions || (uint)projectile.owner >= Main.maxPlayers)
            return;
        if (Main.netMode == NetmodeID.MultiplayerClient)
            BrainIllusionHit.Send(NPC.whoAmI, projectile.owner);
        else
            BrainIllusionHit.Apply(NPC.whoAmI, projectile.owner);
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.2f * NPC.localAI[1];
        if ((int)NPC.frameCounter > 3)
            NPC.frameCounter = 0;
    }

    internal void DrawSelf(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // This sheet contains seven subtly different exposed-brain variants.
        // Boss texture/config overrides must not replace them with closed-brain frames.
        Texture2D tex = TextureAssets.Npc[Type].Value;
        Rectangle frame = tex.Frame(7, 4, Variant, (int)NPC.frameCounter);
        Vector2 scaleDistort = new((float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 2) / 2f);
        float endLerp = MathHelper.Clamp(AttackTime / 60f, 0f, 1f);
        float startLerp = MathHelper.Clamp(SpawnTime / 60f, 0f, 1f);
        if (SpawnTime > 0)
        {
            drawColor *= 1 - startLerp;
            scaleDistort *= startLerp;
        }
        else if (AttackTime > 0)
        {
            drawColor = Color.Lerp(drawColor, Color.Red, endLerp) * (1 - endLerp);
            scaleDistort *= endLerp;
        }
        else
            scaleDistort = Vector2.Zero;
        spriteBatch.Draw(tex, NPC.Center + (Vector2.UnitY * 16) - screenPos, frame, drawColor, NPC.rotation, frame.Size() * 0.5f, (Vector2.One + scaleDistort) * NPC.scale, 0, 0);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
}
