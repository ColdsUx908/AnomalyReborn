using Anomalies.Bosses.QueenBee;

namespace Anomalies.GameContents.AnomalyItems.HoneyedWildbloom;

public sealed class FriendlyBeePet : AnomalyModProjectile
{
    public override LocalizedText DisplayName => ModContent.GetModProjectile<FriendlyBee>()?.DisplayName;
    public override string Texture => ModContent.GetModProjectile<FriendlyBee>()?.Texture;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;
        Main.projPet[Type] = true;

        ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Type], 5)
            .WithOffset(-10f, -26f).WhenNotSelected(0, 0);
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 1200;
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        Projectile.damage = 0;

        if (++Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 2)
                Projectile.frame = 0;
        }

        Player player = Owner;
        if (!Owner.Alive)
        {
            Projectile.timeLeft = 2;
            return;
        }

        Projectile.FloatingPetAI(false, 0.03f);
        Projectile.timeLeft = 300;

        Projectile.spriteDirection = (Projectile.velocity.X >= 0f).ToDirectionInt();
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
}

