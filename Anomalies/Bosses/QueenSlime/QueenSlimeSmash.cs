// Developed by ColdsUx

namespace Anomalies.Bosses.QueenSlime;

public sealed class QueenSlimeSmash : AnomalyProjectileBehavior<QueenSlimeSmash>
{
    public override int ApplyingType => ProjectileID.QueenSlimeSmash;

    public override bool AllowCalamityLogic(CalamityLogicType_ProjectileBehavior type) => type switch
    {
        CalamityLogicType_ProjectileBehavior.PreAI => false,
        _ => true,
    };

    public override bool PreAI()
    {
        float maxHitboxSize = 20f;

        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] <= 0f)
        {
            if (Projectile.ai[0] == 0f)
                SoundEngine.PlaySound(SoundID.Item167, Projectile.Center);

            return false;
        }
        if (Projectile.ai[0] > 9f)
        {
            Projectile.Kill();
            return false;
        }

        Projectile.velocity = Vector2.Zero;
        Projectile.position = Projectile.Center;
        Projectile.Size = new Vector2(16f, 8f) * MathHelper.Lerp(5f, maxHitboxSize, Utils.GetLerpValue(0f, 9f, Projectile.ai[0]));
        Projectile.Center = Projectile.position;
        Point point = Projectile.TopLeft.ToTileCoordinates();
        Point point2 = Projectile.BottomRight.ToTileCoordinates();
        int num2 = point.X / 2 + point2.X / 2;
        int num3 = Projectile.width / 2;
        if ((int)Projectile.ai[0] % 3 != 0)
            return false;

        int num4 = (int)Projectile.ai[0] / 3;
        for (int i = point.X; i <= point2.X; i++)
        {
            for (int j = point.Y; j <= point2.Y; j++)
            {
                if (Vector2.Distance(Projectile.Center, new Vector2(i * 16, j * 16)) > num3)
                    continue;

                Tile tileSafely = Framing.GetTileSafely(i, j);
                bool isPlatform = tileSafely.HasTile && (TileID.Sets.Platforms[tileSafely.TileType] || tileSafely.TileType == TileID.PlanterBox);
                if (!isPlatform)
                {
                    if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType])
                        continue;

                    Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                    if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType])
                        continue;
                }

                int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
                for (int k = 0; k < num5; k++)
                {
                    Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                    obj.velocity.Y -= 3f + num4 * 1.5f;
                    obj.velocity.Y *= Main.rand.NextFloat();
                    obj.velocity.Y *= 0.75f;
                    obj.scale += num4 * 0.03f;
                }

                if (num4 >= 2)
                {
                    if (Projectile.type == ProjectileID.QueenSlimeSmash)
                    {
                        Color newColor = NPC.AI_121_QueenSlime_GetDustColor();
                        newColor.A = 150;
                        for (int l = 0; l < num5 - 1; l++)
                        {
                            int num6 = Dust.NewDust(Projectile.position, 12, 12, DustID.TintableDust, 0f, 0f, 50, newColor, 1.5f);
                            Main.dust[num6].velocity.Y -= 0.1f + num4 * 0.5f;
                            Main.dust[num6].velocity.Y *= Main.rand.NextFloat();
                            Main.dust[num6].velocity.X *= Main.rand.NextFloatDirection() * 3f;
                            Main.dust[num6].position = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
                            if (!Main.rand.NextBool(3))
                            {
                                Main.dust[num6].velocity *= 0.5f;
                                Main.dust[num6].noGravity = true;
                            }
                        }
                    }
                    else
                    {
                        for (int m = 0; m < num5 - 1; m++)
                        {
                            Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                            obj2.velocity.Y -= 1f + num4;
                            obj2.velocity.Y *= Main.rand.NextFloat();
                            obj2.velocity.Y *= 0.75f;
                        }
                    }
                }

                if (num5 <= 0 || Main.rand.NextBool(3))
                    continue;

                float num7 = Math.Abs(num2 - i) / (maxHitboxSize / 2f);
                if (Projectile.type == ProjectileID.QueenSlimeSmash)
                {
                    Color newColor2 = NPC.AI_121_QueenSlime_GetDustColor();
                    newColor2.A = 150;
                    for (int n = 0; n < 3; n++)
                    {
                        int num8 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 50, newColor2, 2f - num4 * 0.15f + num7 * 0.5f);
                        Main.dust[num8].velocity.Y -= 0.1f + num4 * 0.5f + num7 * num4 * 1f;
                        Main.dust[num8].velocity.Y *= Main.rand.NextFloat();
                        Main.dust[num8].velocity.X *= Main.rand.NextFloatDirection() * 3f;
                        Main.dust[num8].position = new Vector2(i * 16 + 20, j * 16 + 20);
                        if (!Main.rand.NextBool(3))
                        {
                            Main.dust[num8].velocity *= 0.5f;
                            Main.dust[num8].noGravity = true;
                        }
                    }
                }
                else
                {
                    Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromAI(), Projectile.position, Vector2.Zero, 61 + Main.rand.Next(3), 1f - num4 * 0.15f + num7 * 0.5f);
                    gore.velocity.Y -= 0.1f + num4 * 0.5f + num7 * num4 * 1f;
                    gore.velocity.Y *= Main.rand.NextFloat();
                    gore.position = new Vector2(i * 16 + 20, j * 16 + 20);
                }
            }
        }

        return false;
    }

    public override Color? GetAlpha(Color lightColor) => lightColor;
}
