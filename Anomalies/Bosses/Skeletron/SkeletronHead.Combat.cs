namespace Anomalies.Bosses.Skeletron;

public sealed partial class SkeletronHead
{
    private void BeginSpin(Behavior behavior, int localPhase)
    {
        Timer = AttackTimer = 0f;
        CurrentBehavior = behavior;
        CurrentLocalPhase = localPhase;
        ChargeReadiness = 1f;
        ChargeTelegraphTimer = ChargeGateValue;
    }

    private void FireHoverSkulls(int interval, bool handsDead)
    {
        if (!TONetworkEnvironment.NotClient)
            return;

        int age = (int)AttackTimer;
        if (!handsDead)
            interval *= 2;
        if (age <= SkullInterval || age % interval != 0)
            return;

        Player target = Target;
        SkeletronCombat.FireSkull(NPC, target, HoverDoubleSkullChanceDenominator > 0 && Main.rand.NextBool(HoverDoubleSkullChanceDenominator) ? 2 : 1, TOMathUtils.PiOver10, HeadSkullSpeed,
            SkullDamage, addNPCVelocity: false);
        if (Phase2_2)
        {
            float speed = HoverCrossboneSpeed;
            Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            Vector2 position = NPC.Center + direction * HeadSkullSpeed
                + new Vector2(0f, 25f * NPC.scale).RotatedBy(NPC.rotation);
            Crossbone.Fire(NPC, position, direction * speed, SkullDamage,
                CrossboneMaxSpeed);
        }
    }

    private void MoveWhileHovering()
    {
        float headYAcceleration = HoverBaseAccelerationY + HoverLowHealthAccelerationY * (1f - (float)NPC.LifeRatio);
        float headYTopSpeed = headYAcceleration * 100f;
        float headXAcceleration = HoverBaseAccelerationX + HoverLowHealthAccelerationX * (1f - (float)NPC.LifeRatio);
        float headXTopSpeed = headXAcceleration * 100f;
        float rotation = NPC.velocity.X / 15f;
        SkeletronKinematics.NormalMovement(NPC, Target, headXTopSpeed, headYTopSpeed,
            headXAcceleration, headYAcceleration, 250f);
        NPC.rotation = rotation;
    }

    private void BeginSpinFrame(SoundStyle roar)
    {
        if (Aroma && TONetworkEnvironment.NotClient && Timer == 0f && CompletedCharges == 0)
        {
            if (NPC.CountNPCS(NPCID.DarkCaster) < 6)
                TrySpawnGroundEnemy(NPCID.DarkCaster);
            SummonAromaDungeonEnemies();
        }

        NPC.defense = NPC.defDefense - 10;
        NPC.damage = (int)Math.Round(NPC.defDamage * SpinDamageMult);

        Timer++;

        AttackTimer += 1f;
        if (AttackTimer == 2f)
            SoundEngine.PlaySound(roar, NPC.Center);

        void SummonAromaDungeonEnemies()
        {
            // All groups run in the same frame; each flying enemy rolls independently.
            if (Main.rand.NextBool())
            {
                TrySpawnGroundEnemy(Main.rand.NextFromList(NPCID.BlueArmoredBones, NPCID.BlueArmoredBonesMace, NPCID.BlueArmoredBonesNoPants, NPCID.BlueArmoredBonesSword));
                TrySpawnGroundEnemy(Main.rand.NextFromList(NPCID.RustyArmoredBonesAxe, NPCID.RustyArmoredBonesFlail, NPCID.RustyArmoredBonesSword, NPCID.RustyArmoredBonesSwordNoArmor));
                TrySpawnGroundEnemy(Main.rand.NextFromList(NPCID.HellArmoredBones, NPCID.HellArmoredBonesMace, NPCID.HellArmoredBonesSpikeShield, NPCID.HellArmoredBonesSword));
            }
            else
            {
                int casterType = Main.rand.Next(3) switch
                {
                    0 => Main.rand.NextFromList(NPCID.Necromancer, NPCID.NecromancerArmored),
                    1 => Main.rand.NextFromList(NPCID.RaggedCaster, NPCID.RaggedCasterOpenCoat),
                    _ => Main.rand.NextFromList(NPCID.DiabolistRed, NPCID.DiabolistWhite),
                };
                TrySpawnGroundEnemy(casterType);
                TrySpawnGroundEnemy(Main.rand.NextFromList(NPCID.SkeletonCommando, NPCID.SkeletonSniper, NPCID.TacticalSkeleton));
                TrySpawnGroundEnemy(NPCID.BoneLee);
            }

            if (Main.rand.NextBool())
                SpawnFlyingEnemy(NPCID.GiantCursedSkull);
            if (Main.rand.NextBool())
                SpawnFlyingEnemy(NPCID.DungeonSpirit);
            if (Main.rand.NextBool(20))
                TrySpawnGroundEnemy(NPCID.Paladin);

            void SpawnFlyingEnemy(int type) => NPC.NewNPCAction(SourceAI, NPC.Center, type, action: npc =>
            {
                npc.Center = NPC.Center;
                npc.target = NPC.target;
            });
        }
    }

    private void FadeChargeTelegraph()
    {
        // Reset telegraph timer to create color fade
        if (ChargeTelegraphTimer > 0f)
        {
            ChargeTelegraphTimer -= 2f;
            if (ChargeTelegraphTimer <= 0f)
            {
                ChargeTelegraphTimer = 0f;
                NPC.netUpdate = true;
            }
        }
    }

    private void FinishSpin(bool handsDead)
    {
        // Start counting the alternating spins from the next complete hover round.
        if (handsDead)
        {
            InitializePhase2Hover();
        }

        CompletedCharges = 0;
        Timer = 0f;
        CurrentBehavior = Behavior.Hover;
        CurrentLocalPhase = 0;
        ChargeReadiness = 0f;
        ChargeTelegraphTimer = 0f;
        AttackTimer = -PostSpinHoverQuietDuration;

        NPC.TargetClosest();
        NPC.netUpdate = true;
        NPC.rotation += NPC.direction * SpinSpeed;
    }

    private void InitializePhase2Hover()
    {
        if (CurrentPhase == Phase.Phase1)
            CurrentPhase = Phase.Phase2;
        AdvancePhase2Subphase();
        HoverLifeRatio = MathHelper.Clamp((float)NPC.LifeRatio, 0f, 1f);
        RapidSkullDuration = (int)MathF.Round(MathHelper.Lerp(MinRapidSkullDuration, MaxRapidSkullDuration, NPC.LifeRatio));
    }

    private void AdvancePhase2Subphase()
    {
        if (ShouldEnterPhase2_2)
            CurrentPhase = Phase.Phase2_2;
    }

    private void TrySpawnGroundEnemy(int npcType)
    {
        const int MaxAttempts = 1000;
        const int SearchRadius = 50;
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            int tileX = Math.Clamp((int)(NPC.Center.X / 16f) + Main.rand.Next(-SearchRadius, SearchRadius + 1), 10, Main.maxTilesX - 11);
            int tileY = Math.Clamp((int)(NPC.Center.Y / 16f) + Main.rand.Next(-SearchRadius, SearchRadius + 1), 10, Main.maxTilesY - 11);
            for (; tileY < Main.maxTilesY - 10 && !WorldGen.SolidTile(tileX, tileY); tileY++)
                continue;
            if (tileY >= Main.maxTilesY - 10)
                continue;
            NPC sample = ContentSamples.NpcsByNetId[npcType];
            Vector2 bottom = new(tileX * 16 + 8, tileY * 16);
            Vector2 topLeft = bottom - new Vector2(sample.width * 0.5f, sample.height);
            if (Collision.SolidCollision(topLeft, sample.width, sample.height))
                continue;
            NPC.NewNPCAction(SourceAI, bottom, npcType, action: npc =>
            {
                npc.Bottom = bottom;
                npc.target = NPC.target;
            });
            break;
        }
    }

    private SkeletronPhase3Arena FindActivePhase3Arena()
    {
        foreach (Projectile projectile in Projectile.ActiveProjectiles)
        {
            if (projectile.ModProjectile is SkeletronPhase3Arena { IsActivated: true } arena && arena.Master == NPC)
                return arena;
        }
        return null;
    }
}
