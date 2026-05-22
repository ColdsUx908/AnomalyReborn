// Developed by ColdsUx

using CalamityMod.Projectiles.BaseProjectiles;

namespace CalamityAnomalies.GameContents.Developer;

public sealed class ColdheartIcicleHoldout : BaseShortswordProjectile, ICAModProjectile, ILocalizedModType
{
    public override string Texture => ColdheartIcicle.TexturePath;

    public override LocalizedText DisplayName => ModContent.GetModItem<ColdheartIcicle>().DisplayName;

    public bool IsRightClick;
    public int Phase = 1;
    public int SubPhase = 1;

    public override float FadeInDuration => base.FadeInDuration;
    public override float FadeOutDuration => base.FadeOutDuration;
    public override float TotalDuration => base.TotalDuration;

    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 12;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.DamageType = TrueMeleeNoSpeedDamageClass_Publicizer.Instance;
        Projectile.timeLeft = 360;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ArmorPenetration = 350258;
    }

    public override void SetVisualOffsets()
    {
        const int HalfSpriteWidth = ColdheartIcicle.SpriteWidth / 2;
        DrawOriginOffsetX = 0f;
        DrawOffsetX = Projectile.width / 2 - HalfSpriteWidth;
        DrawOriginOffsetY = Projectile.height / 2 - HalfSpriteWidth;
    }

    public override void AI()
    {
        Behavior();
        if (Projectile.IsOnOwnerClient)
        {
            if (IsRightClick)
            {
            }
            else
            {
            }
        }
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        float extraDamage = target.statLifeMax2 / 50f;
        modifiers.FinalDamage.Flat += extraDamage;
        CombatText.NewText(target.getRect(), Color.Cyan, extraDamage.ToString(), true, false);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.ForceCrit();
        if (target.type != NPCID.TargetDummy)
        {
            float extraDamage = target.lifeMax / 50;
            modifiers.FinalDamage.Flat += extraDamage;
        }
    }

    public void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        baseDRModifier *= 0f;
        timedDRModifier *= 0f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        TOCombatTextUtils.ChangeHitNPCText(t => t.color = Color.Cyan);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        new RotatedRectangle(FloatRectangle.FromInnerPoint(Projectile.Center, 9f, 17.5f, 2.25f, 2.25f), Projectile.rotation).Collides(targetHitbox);

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(IsRightClick);
        writer.Write(Phase);
        writer.Write(SubPhase);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        IsRightClick = reader.ReadBoolean();
        Phase = reader.ReadInt32();
        SubPhase = reader.ReadInt32();
    }
}