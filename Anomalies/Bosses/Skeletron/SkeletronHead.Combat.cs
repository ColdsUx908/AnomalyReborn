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
                TrySpawnCaster(NPCID.DarkCaster);
            if (!NPC.AnyNPCs(NPCID.DiabolistWhite))
                TrySpawnCaster(NPCID.DiabolistWhite);
        }

        NPC.defense = NPC.defDefense - 10;
        NPC.damage = (int)Math.Round(NPC.defDamage * SpinDamageMult);

        Timer++;

        AttackTimer += 1f;
        if (AttackTimer == 2f)
            SoundEngine.PlaySound(roar, NPC.Center);
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
        if (Aroma && TONetworkEnvironment.NotClient && NPC.CountNPCS(NPCID.DarkCaster) < 6)
            TrySpawnCaster(NPCID.DarkCaster);

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

    private void TrySpawnCaster(int npcType)
    {
        const int MaxAttempts = 1000;
        const int SearchRadius = 50;
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            int tileX = (int)(NPC.Center.X / 16f) + Main.rand.Next(-SearchRadius, SearchRadius + 1);
            int tileY = (int)(NPC.Center.Y / 16f) + Main.rand.Next(-SearchRadius, SearchRadius + 1);
            for (; tileY < Main.maxTilesY - 10 && !WorldGen.SolidTile(tileX, tileY); tileY++)
                continue;
            tileY--;
            if (WorldGen.SolidTile(tileX, tileY))
                continue;
            NPC.NewNPCAction(SourceAI, new Vector2(tileX * 16 + 8, tileY * 16), npcType);
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
