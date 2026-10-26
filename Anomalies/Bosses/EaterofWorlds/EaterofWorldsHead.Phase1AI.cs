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
            // 0: fast entry; 1: orbit until the entire chain follows inside; 2: coil and telegraph;
            // 3: staggered split launch or combined charge; chain directly into the next attack.
            switch (CurrentLocalPhase)
            {
                case 0:
                    if (Timer1 == 1 && TONetworkEnvironment.NotClient)
                    {
                        CoilingCenter = Target.Center + new Vector2(AttackSide * FormationDistance, combine ? 300f : 120f);
                        NPC.netUpdate = true;
                    }
                    bool ready = true;
                    for (int i = 0; i < heads.Count; i++)
                    {
                        if (heads[i].Distance(CoilingCenter) < 125f)
                            Orbit(heads[i]);
                        else
                            MoveTowards(heads[i], CoilingCenter, 38f, 6f);
                        ready &= heads[i].Distance(CoilingCenter) < 125f;
                    }
                    if (Timer1 >= 24 && ready)
                        ChangeStep(1);
                    break;
                case 1:
                    for (int i = 0; i < heads.Count; i++)
                        Orbit(heads[i]);
                    if (Timer1 >= 36 && TONetworkEnvironment.NotClient
                        && AllSegments.All(segment => segment.NPC.Distance(CoilingCenter) < 170f))
                    {
                        if (TONetworkEnvironment.NotClient)
                        {
                            AimPoint = Target.Center;
                            CoilingStartAngle = (AimPoint - CoilingCenter).ToRotation();
                        }
                        ChangeStep(2);
                    }
                    else if (Timer1 >= TotalSegmentCount * 12)
                        ChangeAttack(combine ? Behavior.Phase1_CrossSweep : Behavior.Phase1_Hunt);
                    break;
                case 2:
                    for (int i = 0; i < heads.Count; i++)
                        Orbit(heads[i]);
                    if (Timer1 >= CoilDuration && TONetworkEnvironment.NotClient)
                    {
                        List<NPC> transformed = TransformToWorms(combine ? 1 : 3);
                        if (transformed.Count == 0)
                        {
                            RemoveEncounter();
                            return;
                        }
                        // Rewire only: every segment keeps the position it reached by following its predecessor.
                        ChangeStep(3);
                    }
                    break;
                case 3:
                    int launchWarning = combine ? 42 : 30;
                    for (int i = 0; i < heads.Count; i++)
                    {
                        int delay = launchWarning + (combine ? 0 : i * SplitLaunchDelay);
                        if (Timer1 <= delay)
                        {
                            Orbit(heads[i]);
                            if (Timer1 == delay)
                            {
                                // Predict the same launch on clients; the server snapshot corrects latency differences.
                                Vector2 direction = CoilLaunchDirection(i, combine);
                                GetInstance(heads[i]).LaunchDirection = direction;
                                heads[i].Center = CoilingCenter;
                                heads[i].velocity = direction * OutwardChargeSpeed;
                                heads[i].rotation = direction.ToRotation() + MathHelper.PiOver2;
                                Array.Fill(heads[i].oldPos, heads[i].position);
                                Array.Fill(heads[i].oldRot, heads[i].rotation);
                                if (TONetworkEnvironment.NotClient)
                                    heads[i].netUpdate = true;
                            }
                        }
                        else
                            heads[i].velocity = GetInstance(heads[i]).LaunchDirection * OutwardChargeSpeed;
                    }
                    if (Timer1 > launchWarning + (combine ? 0 : 2 * SplitLaunchDelay)
                        && heads.All(head => head.Distance(CoilingCenter) >= OutwardChargeDistance)
                        && TONetworkEnvironment.NotClient)
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

            void Orbit(NPC head)
            {
                Vector2 radial = (head.Center - CoilingCenter).SafeNormalize(Vector2.UnitX);
                Vector2 point = CoilingCenter + (radial * CoilRadius).RotatedBy(0.55f);
                Vector2 delta = point - head.Center;
                Vector2 desired = delta.SafeNormalize(Vector2.UnitY) * Math.Min(32f, delta.Length() * 0.9f);
                head.velocity = Vector2.Lerp(head.velocity, desired, 0.35f);
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
