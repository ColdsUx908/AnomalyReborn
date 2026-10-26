namespace Transoceanic.Game.Combat;

public enum NPCTargetPreference
{
    Anyone,
    PreferSame,
    ForceSwitch
}

public struct NPCTargetingParameters : IEquatable<NPCTargetingParameters>
{
    // Vanilla argument to TargetClosest. Defaults to true as it does in vanilla.
    // If true, the NPC will turn to face the target.
    public bool FaceTarget = true;
    // Vanilla argument to TargetClosestUpgraded. That function is never used, but the flexibility is implemented here.
    // Allows targeting distance calculations to be measured from a different position than the NPC's center, if desired.
    public Vector2? TargetingCenter = null;
    // Hard maximum range to search for targets. Players outside this physical Manhattan distance will always be ignored.
    //
    // Vanilla always uses infinity for this, leading to behavior like Queen Bee crossing the world to spawncamp you.
    // Calamity defaults to a very high but not infinite value.
    public float MaxSearchRange = 9600f; // 600 tiles
    // Targeting preference enum.
    // Anyone = Target the "closest" player, no other considerations. Vanilla behavior.
    // PreferSame = Always pick the same player if they're within the search range, even if another player is closer or has more aggro.
    // ForceSwitch = Try to pick any other player but the current player, if possible. Similar to an "aggro drop" in MMOs.
    //
    // ForceSwitch intentionally does nothing in single player, because there is nobody to switch to.
    public NPCTargetPreference TargetType = NPCTargetPreference.Anyone;
    // The ratio at which to consider aggro bonuses from player gear.
    // 1f is vanilla. Set to 0f to ignore aggro bonuses entirely.
    // Set to a negative value to make the NPC intentionally avoid tanks and preferentially go after other players.
    public float AggroRatio = 1f;
    // Whether or not players must have line of sight to the NPC to be considered valid targets.
    // This is always line of sight to the NPC itself, even if a different targeting center for range finding is specified.
    public bool RequireLineOfSight = false;
    // If true, the targeting algorithm counts missing health as a gigantic boost to aggro.
    // This makes for a "merciless" or "bloodthirsty" NPC which is focused on killing the lowest health players.
    public bool FinishThemOff = false;
    internal const float FinishThemOffMaxAggroBoost = 4000f;
    // If true, this NPC ignores the Stardust armor set bonus "JoJo Tank Minion" (or "Algalon the Observer" according to the wiki).
    //
    // This is set to false by default, because that's vanilla behavior.
    // As Stardust armor is postgame in vanilla, no vanilla bosses ignore the Stardust Guardian.
    // It is highly recommended to set this to true for all bosses, or their aggro can be abusively manipulated.
    public bool IgnoreTankMinions = false;
    // If true, this NPC ignores players who have less than zero net aggro and are not actively using items.
    //
    // This is set to true by default, because it's (undocumented) vanilla behavior.
    // Bosses will automatically attack stealthed players anyway -- you don't need to set this to false for that to occur.
    public bool IgnoreStealthedPlayers = true;
    // If true, this targeting change forces a net update.
    // In vanilla, targeting updates cause net updates if direction changed or the target player changed,
    // but NEVER if the NPC has collideX or collideY set to true.
    //
    // Generally this doesn't need to be set to true, as bosses will never have collideX or collideY set to true.
    public bool ForceNetUpdate = false;
    // Player indexes put into this list will not be considered for targetting.
    public HashSet<int> ExcludedPlayers = [];
    public NPCTargetingParameters()
    {
    }

    // Quick defaults for recommended boss settings.
    public NPCTargetingParameters(bool isBoss) => IgnoreTankMinions = isBoss;
    // Using the default keyword on structs produces garbage. Please use the below instead, or define your own parameters.
    public static NPCTargetingParameters Defaults => new();
    public static NPCTargetingParameters BossDefaults => new(true);

    #region Equality Operators
    public static bool operator ==(NPCTargetingParameters ctp1, NPCTargetingParameters ctp2)
    {
        bool targetingCentersEqual = (ctp1.TargetingCenter is null && ctp2.TargetingCenter is null) || ctp1.TargetingCenter == ctp2.TargetingCenter;
        if (!targetingCentersEqual)
            return false;
        return ctp1.FaceTarget == ctp2.FaceTarget && ctp1.MaxSearchRange == ctp2.MaxSearchRange && ctp1.TargetType == ctp2.TargetType && ctp1.AggroRatio == ctp2.AggroRatio && ctp1.RequireLineOfSight == ctp2.RequireLineOfSight && ctp1.FinishThemOff == ctp2.FinishThemOff && ctp1.IgnoreTankMinions == ctp2.IgnoreTankMinions && ctp1.IgnoreStealthedPlayers == ctp2.IgnoreStealthedPlayers && ctp1.ForceNetUpdate == ctp2.ForceNetUpdate;
    }

    public static bool operator !=(NPCTargetingParameters ctp1, NPCTargetingParameters ctp2) => !(ctp1 == ctp2);
    public readonly bool Equals(NPCTargetingParameters other) => this == other;
    public override readonly bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not NPCTargetingParameters)
            return false;
        return this == (NPCTargetingParameters)obj;
    }

    // Visual Studio complains if this is not here. I do not know why.
    public override readonly int GetHashCode() => base.GetHashCode();
    #endregion
}

public static class TOCombatUtils
{
    public static int TargetWithOptions(this NPC npc, NPCTargetingParameters options)
    {
        // 05JUN2024: Ozzatron: Struct defaults are always memset to all-zero, giving garbage parameters.
        // If you actually call this function with the default keyword for the struct,
        // change the options on the spot to valid default / intended default parameters.
        if (options == default)
            options = new NPCTargetingParameters();
        float distance = 0f;
        // float realDist = 0f; // Defined but not used by vanilla. Commented out here.
        bool anyTargetAvailable = false;
        int tankMinionProjectileID = -1;
        // The setup and initial loop is equivalent to vanilla NPC.TargetClosest, but optimized.
        foreach (Player p in Main.ActivePlayers)
        {
            bool playerDead = p.dead || p.ghost;
            if (playerDead)
                continue;
            // ForceSwitch targeting. If the same player from last time is iterated over, just ignore them.
            // Player exclusion. If the player is to be excluded, do not consider them.
            bool sameTargetAsLastTime = p.whoAmI == npc.oldTarget;
            bool notSinglePlayer = Main.netMode != NetmodeID.SinglePlayer;
            if (notSinglePlayer && (options.ExcludedPlayers.Contains(p.whoAmI) || (options.TargetType == NPCTargetPreference.ForceSwitch && sameTargetAsLastTime)))
                continue;
            //
            // The below code is implemented in vanilla as a separate method. Here, it's inlined for efficiency.
            //
            Vector2 pCenter = p.Center;
            Vector2 targetCenter = options.TargetingCenter ?? npc.Center;
            float manhattanDist = Math.Abs(targetCenter.X - pCenter.X) + Math.Abs(targetCenter.Y - pCenter.Y);
            // Hard cutoff range specified in options. If the player is further, completely ignore them.
            if (manhattanDist > options.MaxSearchRange)
                continue;
            // Line of sight requirement specified in options. Please don't use this without reducing the max search range.
            if (options.RequireLineOfSight && !Collision.CanHit(npc.Center, 1, 1, pCenter, 1, 1))
                continue;
            float aggroAdjustedDist = manhattanDist - options.AggroRatio * p.aggro;
            // Implementation of "Finish Them Off": Add enormous amounts of virtual aggro to low health players
            if (options.FinishThemOff)
            {
                float missingHPRatio = MathHelper.Clamp(1f - p.statLife / (float)p.statLifeMax2, 0f, 1f);
                float bloodthirstAggro = MathHelper.Lerp(0f, NPCTargetingParameters.FinishThemOffMaxAggroBoost, missingHPRatio);
                aggroAdjustedDist -= bloodthirstAggro;
            }

            bool aggroDisabled = p.npcTypeNoAggro[npc.type];
            if (aggroDisabled && npc.direction != 0)
                aggroAdjustedDist += 1000f;
            bool cancelTargeting = false;
            // PreferSame targeting. If the same player from last time is a valid target, even if not the "best" target, pick it anyway.
            bool preferSameFound = options.TargetType == NPCTargetPreference.PreferSame && sameTargetAsLastTime;
            // Standard targeting. If the adjusted distance is lower, or this is the first valid target, actually choose the new target.
            bool standardTargetingRequirementsMet = !anyTargetAvailable || aggroAdjustedDist < distance;
            // If either targeting method succeeded, then this target is being engaged.
            bool engageThisTarget = preferSameFound || standardTargetingRequirementsMet;
            if (engageThisTarget)
            {
                anyTargetAvailable = true;
                tankMinionProjectileID = -1; // Reset any Stardust Guardian aggro because a real player was found.
                distance = aggroAdjustedDist;
                npc.target = p.whoAmI;
                // If PreferSame targeting is active, and the same player was found, cancel further iteration. They are being chosen above all others.
                if (preferSameFound)
                    cancelTargeting = true;
            }

            // "Tank pet" accomodation, AKA the 1.4+ Stardust Guardian
            // Basically, if the player would be targeted, give a chance to instead target their tank minion
            //
            // This behavior is not documented on the vanilla wiki.
            if (p.tankPet >= 0 && !aggroDisabled && !options.IgnoreTankMinions)
            {
                Projectile tankMinion = Main.projectile[p.tankPet];
                Vector2 tmCenter = tankMinion.Center;
                float manhattanDistToTankMinion = Math.Abs(targetCenter.X - tmCenter.X) + Math.Abs(targetCenter.Y - tmCenter.Y);
                // The Stardust Guardian is considered to have a 200 aggro bonus by default.
                // In Calamity this is scaled by the aggro ratio specified in options.
                manhattanDistToTankMinion -= options.AggroRatio * 200f;
                // The Stardust Guardian only attracts the attention of NPCs within a very short distance
                if (manhattanDistToTankMinion < distance && manhattanDistToTankMinion < 200f && Collision.CanHit(npc.Center, 1, 1, tmCenter, 1, 1))
                    tankMinionProjectileID = p.tankPet;
            }

            // If targeting has been short-circuited for any reason, cancel iteration over players.
            if (cancelTargeting)
                break;
        }

        // If the NPC has been aggroed by a Stardust Guardian instead of an actual player, account for that
        if (tankMinionProjectileID >= 0)
        {
            Projectile tankMinion = Main.projectile[tankMinionProjectileID];
            npc.targetRect = tankMinion.Hitbox;
            // Always set direction to a nonzero value. This NPC has been engaged in combat.
            npc.direction = 1;
            if (tankMinion.Center.X < npc.Center.X)
                npc.direction = -1;
            npc.directionY = 1;
            if (tankMinion.Center.Y < npc.Center.Y)
                npc.directionY = -1;
        }
        // Standard player aggro occurs here
        else
        {
            bool shouldFaceTarget = options.FaceTarget;
            // Sanitize targeted player index
            if (npc.target is < 0 or >= Main.maxPlayers)
                npc.target = 0;
            Player targetPlayer = Main.player[npc.target];
            npc.targetRect = targetPlayer.Hitbox;
            // Do not switch facing to look at dead players.
            if (targetPlayer.dead)
                shouldFaceTarget = false;
            // If already engaged in combat, do not switch facing to look at players that ignore your aggro.
            if (targetPlayer.npcTypeNoAggro[npc.type] && npc.direction != 0)
                shouldFaceTarget = false;
            if (shouldFaceTarget)
            {
                bool oldTargetWasValid = npc.oldTarget is >= 0 and < Main.maxPlayers;
                bool targetIsLowAggroNotUsingItem = targetPlayer.itemAnimation == 0 && targetPlayer.aggro < 0;
                bool willIgnoreStealthedPlayers = !npc.boss && options.IgnoreStealthedPlayers;
                // Regular NPCs (not bosses) will voluntarily ignore otherwise-valid player targets with less than zero aggro if they are not actively using an item.
                // This ONLY WORKS if they already have another valid target, aka multiplayer.
                // As such, having net less than zero aggro enables you to remain "stealthed" to regular enemies if you are not doing anything.
                // This is undocumented vanilla behavior.
                bool ignoreStealthedPlayer = willIgnoreStealthedPlayers && oldTargetWasValid && targetIsLowAggroNotUsingItem;
                if (!ignoreStealthedPlayer)
                {
                    // Always set direction to a nonzero value. This NPC has been engaged in combat.
                    npc.direction = 1;
                    if (targetPlayer.Center.X < npc.Center.X)
                        npc.direction = -1;
                    npc.directionY = 1;
                    if (targetPlayer.Center.Y < npc.Center.Y)
                        npc.directionY = -1;
                }
            }
        }

        // Confused enemies always run in the exact wrong direction, horizontally at least.
        if (npc.confused)
            npc.direction *= -1;
        // Apply net updates.
        bool directionChange = npc.direction != npc.oldDirection || npc.directionY != npc.oldDirectionY;
        bool targetChange = npc.target != npc.oldTarget;
        bool shouldNetUpdate = (directionChange || targetChange) && !npc.collideX && !npc.collideY;
        if (shouldNetUpdate || options.ForceNetUpdate)
            npc.netUpdate = true;
        return npc.target;
    }

    public static float PreciseDistanceToTileCollisionHit(Vector2 start, float rotation, float length, float step = 1)
    {
        Vector2 unitVect = rotation.ToRotationVector2();
        Vector2 end = unitVect * length;
        if (length < 1f)
        {
            Point endWorldPos = end.ToTileCoordinates();
            return IsSolid(Framing.GetTileSafely(endWorldPos)) ? 0 : length;
        }

        Vector2 currentPos = start;
        Point lastAirPos = new(-1, -1);
        for (float i = 0; i < length; i += step)
        {
            currentPos += unitVect * step;
            Point tilePos = currentPos.ToTileCoordinates();
            if (tilePos == lastAirPos)
                continue;
            if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
                continue;
            Tile tile = Main.tile[tilePos.X, tilePos.Y];
            if (!IsSolid(tile))
            {
                lastAirPos = tilePos;
                continue;
            }

            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                return (currentPos - start).Length();
            Vector2 tileWorldPos = new(tilePos.X * 16, tilePos.Y * 16);
            Vector2 currentPosInTile = currentPos - tileWorldPos;
            if (tile.IsHalfBlock)
            {
                if (currentPosInTile.Y >= 8f)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeDownLeft)
            {
                if (currentPosInTile.X <= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeDownRight)
            {
                if ((16 - currentPosInTile.X) <= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeUpLeft)
            {
                if (currentPosInTile.X <= (16 - currentPosInTile.Y))
                    return (currentPos - start).Length();
            }
            else if (tile.Slope == SlopeType.SlopeUpRight)
            {
                if (currentPosInTile.X >= currentPosInTile.Y)
                    return (currentPos - start).Length();
            }
        }

        return length;
    }

    private static bool IsSolid(Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
    public static int CountProjectiles(int type)
    {
        int count = 0;
        foreach (Projectile p in Projectile.ActiveProjectiles)
            if (p.type == type)
                count++;
        return count;
    }

    public static void SpawnBossUsingItem(Player player, int type)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.SpawnOnPlayer(player.whoAmI, type);
        else
            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
    }
}
