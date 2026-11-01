namespace Anomalies.Bosses.EaterofWorlds;

public sealed partial class EaterofWorldsHead
{
    private void Phase1CombatAI()
    {
        List<NPC> heads = GetEncounterHeads();
        if (heads.Count == 0)
            return;
        Timer1++;
        if (!Main.dedServ && Timer1 == 1 && CurrentLocalPhase == 2
            && CurrentBehavior is Behavior.Phase1_BurrowCharge or Behavior.Phase1_TripleHunt or Behavior.Phase1_Split or Behavior.Phase1_Combine)
            SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
        switch (CurrentBehavior)
        {
            case Behavior.Phase1_Hunt:
                Hunt();
                break;
            case Behavior.Phase1_BurrowCharge:
                BurrowCharge();
                break;
            case Behavior.Phase1_Split:
                TransformAttack(false);
                break;
            case Behavior.Phase1_TripleHunt:
                TripleHunt();
                break;
            case Behavior.Phase1_CrossSweep:
                CrossSweep();
                break;
            case Behavior.Phase1_Combine:
                TransformAttack(true);
                break;
            default:
                ChangeAttack(Behavior.Phase1_Hunt);
                break;
        }
        foreach (NPC head in heads)
        {
            // Periodic correction supplements authoritative boundary snapshots without per-frame packets.
            if (TONetworkEnvironment.NotClient && Timer1 % 45 == 0)
                head.netUpdate = true;
        }

        void Hunt()
        {
            // Keep moving even against a stationary target, so the spawn coil is continuously pulled out.
            PursueTarget(NPC);
            int shotTime = Timer1 % 110;
            if (shotTime == 86)
                LockDirection(NPC, Target.Center);
            if (shotTime == 86 && Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1))
                ShootFlames(NPC, LaunchDirection, speed: 12f);
            if (Timer1 >= (Phase1_2 ? 210 : HuntDuration))
                ChangeAttack(Behavior.Phase1_BurrowCharge);
        }

        void BurrowCharge()
        {
            // 0: approach; 1: tracking warning; 2: locked charge, then immediately reposition.
            int side = AttackSide * (AttackIndex % 2 == 0 ? 1 : -1);
            switch (CurrentLocalPhase)
            {
                case 0:
                    Vector2 staging = AttackCenter + new Vector2(side * FormationDistance, 300f);
                    MoveTowards(NPC, staging, 22f, 2f);
                    if (Timer1 >= 45 && NPC.Distance(staging) < 90f || Timer1 >= FormationTimeout)
                    {
                        ChangeStep(1);
                    }
                    break;
                case 1:
                    PursueTarget(NPC);
                    if (Timer1 >= ChargeWarningTime)
                    {
                        LockDirection(NPC, Target.Center);
                        ChangeStep(2);
                    }
                    break;
                case 2:
                    AccelerateCharge(NPC, LaunchDirection, ChargeSpeed, Timer1);
                    if (Timer1 >= ChargeDuration && TONetworkEnvironment.NotClient)
                    {
                        if (++AttackIndex >= 2)
                            ChangeAttack(Behavior.Phase1_Split);
                        else
                        {
                            AttackCenter = Target.Center;
                            ChangeStep(0);
                        }
                    }
                    break;
            }
        }

        void TransformAttack(bool combine)
        {
            // 0: retreat; 1: enter while the exit rift charges; 3: emerge in the new topology.
            switch (CurrentLocalPhase)
            {
                case 0:
                    if (Timer1 == 1 && TONetworkEnvironment.NotClient)
                    {
                        Vector2 away = (NPC.Center - Target.Center).SafeNormalize(new Vector2(AttackSide, 0f));
                        AttackCenter = Target.Center + away * PortalRetreatDistance;
                    }
                    foreach (NPC head in heads)
                        MoveTowards(head, AttackCenter, 38f, 6f);
                    if (Timer1 >= 45 && heads.All(head => head.Distance(Target.Center) >= PortalSafeDistance)
                        && TONetworkEnvironment.NotClient)
                    {
                        // Fix the destination for the entire warning, so a moving player can dodge it.
                        CoilingCenter = Target.Center + Target.velocity.SafeNormalize(new Vector2(AttackSide, 0f)) * PortalExitDistance;
                        foreach (NPC head in heads)
                        {
                            Vector2 direction = (head.Center - Target.Center).SafeNormalize(Vector2.UnitY);
                            GetInstance(head).LaunchDirection = direction;
                            SpawnPortal(head.Center + direction * 120f, true);
                            head.netUpdate = true;
                        }
                        SpawnPortal(CoilingCenter, false);
                        ChangeStep(1);
                    }
                    else if (Timer1 >= FormationTimeout * 3)
                        ChangeAttack(combine ? Behavior.Phase1_CrossSweep : Behavior.Phase1_Hunt);
                    break;
                case 1:
                    foreach (NPC head in heads)
                        head.velocity = GetInstance(head).LaunchDirection * 28f;
                    if (Timer1 >= PortalChargeDuration && TONetworkEnvironment.NotClient)
                    {
                        // Hidden links are changed on the same boundary as the rift explosion and launch.
                        List<NPC> transformed = TransformToWorms(combine ? 1 : 3);
                        if (transformed.Count == 0)
                        {
                            RemoveEncounter();
                            return;
                        }
                        foreach (NPC head in transformed)
                        {
                            int group = (GetInstance(head).SegmentIndex - 1) / (TotalSegmentCount / 3);
                            Vector2 direction = CoilLaunchDirection(group, combine);
                            GetInstance(head).LaunchDirection = direction;
                            head.velocity = direction * OutwardChargeSpeed;
                        }
                        foreach (EaterofWorldsSegmentContainer segment in AllSegments)
                        {
                            NPC entity = segment.NPC;
                            entity.Center = CoilingCenter;
                            entity.Opacity = 0f;
                            if (entity.type != NPCID.EaterofWorldsHead)
                                entity.velocity = Vector2.Zero;
                            Array.Fill(entity.oldPos, entity.position);
                            Array.Fill(entity.oldRot, entity.rotation);
                            entity.netUpdate = true;
                        }
                        ChangeStep(3);
                    }
                    break;
                case 3:
                    foreach (NPC head in heads)
                        head.velocity = GetInstance(head).LaunchDirection * OutwardChargeSpeed;
                    // Wait for the final tail to emerge before scheduling the next attack.
                    if (TONetworkEnvironment.NotClient && Timer1 >= 12
                        && heads.All(head => head.Distance(CoilingCenter) >= OutwardChargeDistance)
                        && AllSegments.All(segment => segment.NPC.Distance(CoilingCenter) >= 60f))
                    {
                        if (combine)
                        {
                            CycleIndex++;
                            ChangeAttack(Behavior.Phase1_Hunt);
                        }
                        else
                            ChangeAttack(CycleIndex % 2 == 0 ? Behavior.Phase1_TripleHunt : Behavior.Phase1_CrossSweep);
                    }
                    break;
            }
        }

        void TripleHunt()
        {
            // 0: arrange; 1: tracking windup; 2: charge, then immediately hand off with retained momentum.
            int hunterIndex = AttackIndex % heads.Count;
            NPC hunter = heads[hunterIndex];
            for (int i = 0; i < heads.Count; i++)
            {
                if (i == hunterIndex)
                    continue;
                PursueTarget(heads[i]);
            }
            switch (CurrentLocalPhase)
            {
                case 0:
                    PursueTarget(hunter);
                    if (Timer1 >= 24)
                    {
                        ChangeStep(1);
                    }
                    break;
                case 1:
                    PursueTarget(hunter);
                    // Fire towards the approach side, never across the hunter's perpendicular escape lane.
                    if (Timer1 == 30 && heads.Count >= 3)
                    {
                        NPC support = heads[(hunterIndex + 1) % heads.Count];
                        Vector2 destination = Target.Center - WarningDirection(hunter) * 260f;
                        Vector2 direction = (destination - support.Center).SafeNormalize(Vector2.UnitY);
                        Rectangle escapeArea = new((int)Target.Center.X - 150, (int)Target.Center.Y - 150, 300, 300);
                        bool clear = true;
                        for (int shot = -1; shot <= 1; shot++)
                            clear &= !RotatedRectangle.FromLineSegment(support.Center,
                                support.Center + direction.RotatedBy(shot * 0.19f) * 1050f, 30f).Collides(escapeArea);
                        if (clear && support.Distance(Target.Center) > 320f
                            && Collision.CanHitLine(support.Center, 1, 1, destination, 1, 1))
                            ShootFlames(support, destination - support.Center, 3, 7f);
                    }
                    if (Timer1 >= TripleWarningTime)
                    {
                        LockDirection(hunter, Target.Center);
                        ChangeStep(2);
                    }
                    break;
                case 2:
                    AccelerateCharge(hunter, GetInstance(hunter).LaunchDirection, ChargeSpeed + 4f, Timer1);
                    if (Timer1 == 12)
                        ShootFlames(hunter, GetInstance(hunter).LaunchDirection, 3, 11f);
                    if (Timer1 >= ChargeDuration && TONetworkEnvironment.NotClient)
                    {
                        if (++AttackIndex >= TripleRounds)
                            ChangeAttack(CycleIndex % 2 == 0 ? Behavior.Phase1_CrossSweep : Behavior.Phase1_Combine);
                        else
                        {
                            AttackCenter = Target.Center;
                            ChangeStep(0);
                        }
                    }
                    break;
            }
        }

        void CrossSweep()
        {
            // 0: arrange; 1: tracking warning; 2: sweep, then reposition without a rest window.
            if (heads.Count != 3)
            {
                ChangeAttack(Behavior.Phase1_Combine);
                return;
            }
            int side = AttackSide * (AttackIndex % 2 == 0 ? 1 : -1);
            switch (CurrentLocalPhase)
            {
                case 0:
                    foreach (NPC head in heads)
                        PursueTarget(head);
                    if (Timer1 >= 36)
                    {
                        // A corridor filled with solid tiles is not a fair movement check.
                        if (Collision.SolidCollision(AttackCenter - new Vector2(100f, 70f), 200, 140))
                            FinishSweep();
                        else
                            ChangeStep(1);
                    }
                    break;
                case 1:
                    foreach (NPC head in heads)
                        PursueTarget(head);
                    if (Timer1 >= SweepWarningTime)
                    {
                        LockDirection(heads[0], Target.Center - new Vector2(0f, SweepHalfGap));
                        ChangeStep(2);
                    }
                    break;
                case 2:
                    AccelerateCharge(heads[0], GetInstance(heads[0]).LaunchDirection, 20f, Timer1);
                    if (Timer1 <= 24)
                    {
                        PursueTarget(heads[1]);
                        if (Timer1 == 24)
                            LockDirection(heads[1], Target.Center + new Vector2(0f, SweepHalfGap));
                    }
                    else
                        AccelerateCharge(heads[1], GetInstance(heads[1]).LaunchDirection, 20f, Timer1 - 24);
                    PursueTarget(heads[2]);
                    if (Timer1 == 24)
                        ShootFlames(heads[2], new Vector2(side, 0f), 3, 8f);
                    if (Timer1 >= 100 && TONetworkEnvironment.NotClient)
                    {
                        if (++AttackIndex >= 2)
                            FinishSweep();
                        else
                        {
                            AttackCenter = Target.Center;
                            ChangeStep(0);
                        }
                    }
                    break;
            }

            void FinishSweep() => ChangeAttack(CycleIndex % 2 == 0 ? Behavior.Phase1_Combine : Behavior.Phase1_TripleHunt);
        }
    }
}
