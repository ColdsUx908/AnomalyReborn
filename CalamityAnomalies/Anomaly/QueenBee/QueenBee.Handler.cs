// Developed by ColdsUx

namespace CalamityAnomalies.Anomaly.QueenBee;

public sealed partial class QueenBee_Handler
{
    public static Vector2 GetOwnedCombCellCenter(NPC master) => master.Center + new Vector2(0f, -30f * master.scale);

    public static List<int> HoneyWetCombCells = [];

    public sealed class HoneyWetCombCellManager : ModSystem, IContentLoader
    {
        public override void PreUpdateProjectiles()
        {
            HoneyWetCombCells.Clear();
        }

        void IContentLoader.PostSetupContent()
        {
            On_Collision.WetCollision += On_Collision_WetCollision;
        }

        private static bool On_Collision_WetCollision(On_Collision.orig_WetCollision orig, Vector2 Position, int Width, int Height)
        {
            bool result = orig(Position, Width, Height);

            if (Collision.honey)
                return result;

            Rectangle hitbox = TOMathUtils.Collision.GetAdjustedHitboxForWetCollision(Position, Width, Height);

            foreach (int cellIndex in HoneyWetCombCells)
            {
                if (Projectile.TryGetProjectileFromIndex(cellIndex, out Projectile projectile) && projectile.active && projectile.TryGetModProjectile(out CombCell cell))
                {
                    if (cell.HitBox.Collides(hitbox))
                    {
                        Collision.honey = true;
                        return true;
                    }
                }
            }

            return result;
        }
    }
}
