namespace Anomalies.Bosses.BrainofCthulhu;

[AutoloadBossHead]
public class BrainIllusion : AnomalyModNPC
{
    public override string LocalizationCategory => "Bosses.BrainofCthulhu";
    public override string Texture => "Terraria/Images/NPC_266";
    public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_23";
    public ref float Time => ref NPC.ai[0];
    public ref float AttackValue => ref NPC.ai[1];
    public ref float Angle => ref NPC.ai[2];

    public float TeleportDuration
    {
        get => AnomalyNPC.AnomalyAI32[0].f;
        set
        {
            AnomalyNPC.AnomalyAI32[0].f = value;
            AnomalyNPC.AIChanged32[0] = true;
        }
    }

    public ref float TeleportTime => ref NPC.ai[3];

    public Vector2 OldPos
    {
        get => AnomalyNPC.AnomalyAI64[0].GetValue<Vector2>();
        set
        {
            AnomalyNPC.AnomalyAI64[0].SetValue(value);
            AnomalyNPC.AIChanged64[0] = true;
        }
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 8;
    }

    public override void SetDefaults()
    {
        NPC.width = 160;
        NPC.height = 110;
        NPC.damage = 20;
        NPC.lifeMax = 1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.boss = true;
        NPC.BossBar = ModContent.GetInstance<BrainIllusionBossBar>();
        NPC.dontTakeDamage = true;
        NPC.chaseable = false;
        NPC.npcSlots = 0f;
        NPC.netAlways = true;
        Music = MusicID.Boss3;
        SceneEffectPriority = (SceneEffectPriority)(-1);
    }

    public override void OnSpawn(IEntitySource source)
    {
        TeleportDuration = AttackValue;
        AttackValue = 0;
        TeleportTime = Time;
        NPC.netUpdate = true;
    }

    public override void AI()
    {
        if (!BrainBehaviorAccess.ActiveBrain)
        {
            NPC.active = false;
            return;
        }

        NPC brain = Main.npc[NPC.crimsonBoss];
        NPC.scale = brain.scale;
        if (brain.GetBrainBehavior().CurrentBehavior == BrainofCthulhu.Behavior.DeathAnimation)
        {
            NPC.damage = 0;
            NPC.Opacity -= 0.1f;
            if (NPC.Opacity <= 0)
                NPC.active = false;
        }

        Player target = Main.player[NPC.target];
        NPC.GivenName = brain.GivenOrTypeName + $": {brain.life}/{brain.lifeMax}";
        #region Attack Start
        if (Time < 30)
        {
            TeleportDuration = 30;
            if (Time == (TeleportDuration / 2f) && AttackValue == 0)
            {
                AttackValue = 1;
            }
            else
            {
                TeleportTime--;
            }

            NPC.Opacity = 1 - (TeleportTime / (TeleportDuration / 2f));
        }
        #endregion
        else
        {
            if (Time == 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    OldPos = NPC.Center;
                    TeleportTime = 0;
                    AttackValue = 0;
                    NPC.netUpdate = true;
                }

                NPC.Opacity = 1f;
            }

            if (Time < 60)
            {
                float lerp = (Time - 30) / 30f;
                float circleDist = MathHelper.Lerp(BrainofCthulhu.IllusionDashTeleportDistance, BrainofCthulhu.IllusionDashCloseInDistance, BrainCombatUtils.SineInOutEasing(lerp, 1));
                NPC.Center = Vector2.Lerp(OldPos, target.Center + (Angle.ToRotationVector2() * circleDist), lerp);
                Angle += MathHelper.Lerp(0f, BrainofCthulhu.IllusionDashStartingSpinSpeed, BrainCombatUtils.SineInEasing(lerp, 1));
            }
            else if (Time <= 60f + BrainofCthulhu.IllusionDashSpinDuration)
            {
                NPC.Center = target.Center + Angle.ToRotationVector2() * BrainofCthulhu.IllusionDashCloseInDistance;
                Angle += MathHelper.Lerp(BrainofCthulhu.IllusionDashStartingSpinSpeed, 0f, BrainCombatUtils.SineOutEasing((Time - 60) / BrainofCthulhu.IllusionDashSpinDuration, 1));
            }
            else if (Time <= 90 + BrainofCthulhu.IllusionDashSpinDuration)
            {
                float reelBackSpeedExponent = 2.6f;
                float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time - (60 + BrainofCthulhu.IllusionDashSpinDuration), true);
                float reelBackSpeed = MathHelper.Lerp(4f, 20f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                Vector2 reelBackVelocity = (Angle + MathHelper.Pi).ToRotationVector2() * -reelBackSpeed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
            }
            else if (Time == 91 + BrainofCthulhu.IllusionDashSpinDuration)
                NPC.velocity = (Angle + MathHelper.Pi).ToRotationVector2() * BrainofCthulhu.IllusionDashVelocity;
            else if (Time <= 91 + BrainofCthulhu.IllusionDashSpinDuration + BrainofCthulhu.IllusionDashTeleportDuration)
            {
                NPC.Opacity = MathHelper.Lerp(1f, 0.25f, BrainCombatUtils.SineInEasing((Time - (91 + BrainofCthulhu.IllusionDashSpinDuration)) / BrainofCthulhu.IllusionDashTeleportDuration, 1));
                if (NPC.Opacity < 0.666f)
                    NPC.damage = 0;
            }
            else
                NPC.active = false;
        }

        Time++;
    }

    protected override void SendAnomalyExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(OldPos);
    }

    protected override void ReceiveAnomalyExtraAI(BinaryReader reader)
    {
        OldPos = reader.ReadVector2();
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter > 6.0)
        {
            NPC.frameCounter = 0.0;
            NPC.frame.Y += frameHeight;
        }

        if (NPC.frame.Y < frameHeight * 4)
        {
            NPC.frame.Y = frameHeight * 4;
        }

        if (NPC.frame.Y > frameHeight * 7)
        {
            NPC.frame.Y = frameHeight * 4;
        }
    }

    public override bool? CanBeHitByItem(Player player, Item item) => false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => false;
    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => target.whoAmI == NPC.target;
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 drawPos = NPC.Center + (Vector2.UnitY * 16) - screenPos;
        Texture2D texture = TextureAssets.Npc[NPCID.BrainofCthulhu].Value;
        // The displayed boss texture may have been replaced independently of this NPC's texture.
        Rectangle frame = texture.Frame(1, 8, 0, NPC.frame.Height > 0 ? NPC.frame.Y / NPC.frame.Height : 4);
        Vector2 scale = Vector2.One;
        float opacityMult = Main.LocalPlayer.whoAmI == NPC.target ? 1f : 0.25f;
        if (TeleportTime != 0)
        {
            //Color glowColor = Color.White * (teleportCounter / 30f);
            scale = Vector2.Lerp(Vector2.One, new Vector2(0.5f + ((float)Math.Cos(Time / (TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f), 0.5f + ((float)Math.Sin(Time / (TeleportDuration / 2f) * MathHelper.TwoPi) / 2f + 0.5f)), BrainCombatUtils.SineInOutEasing(TeleportTime / (TeleportDuration / 2f), 1));
            spriteBatch.Draw(texture, drawPos, frame, Lighting.GetColor(NPC.Center.ToTileCoordinates()) * NPC.Opacity * opacityMult, NPC.rotation, frame.Size() * 0.5f, scale * NPC.scale, 0, 0);
            //spriteBatch.Draw(GetBrainGlow(), drawPos, npc.frame, glowColor, npc.rotation, npc.frame.Size() * 0.5f, scale, 0, 0);
        }
        else
            spriteBatch.Draw(texture, drawPos, frame, Lighting.GetColor(NPC.Center.ToTileCoordinates()) * NPC.Opacity * opacityMult, NPC.rotation, frame.Size() * 0.5f, scale * NPC.scale, 0, 0);
        return false;
    }
}
