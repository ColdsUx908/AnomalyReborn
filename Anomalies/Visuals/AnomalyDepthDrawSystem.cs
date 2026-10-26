using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Anomalies.Visuals;

/// <summary>
/// 将 NPC 和弹幕的完整世界绘制投影到视觉深度平面。Z=0 时不切换 SpriteBatch。
/// <br/>使用当前 SpriteBatch 矩阵，兼容镜头缩放和重力翻转；不修改实体数据。
/// <br/>保持 Terraria 原有绘制层级，不执行跨实体的深度排序。
/// 仍沿用原版世界坐标视野裁剪；需要从屏幕外投影入画的 NPC 类型应设置 NPCID.Sets.MustAlwaysDraw。
/// 自定义绘制若自行重建 SpriteBatch 矩阵，需要自行组合本系统的投影矩阵。
/// </summary>
public sealed partial class AnomalyDepthDrawSystem : ModSystem
{
    /// <summary>焦距（世界像素）；Z 等于焦距时，视觉大小为原来的一半。</summary>
    public const float FocalLength = 1000f;

    /// <summary>观察者所在的近裁剪面；到达此平面或越过观察者的实体不再显示。</summary>
    public const float MinimumZ = -FocalLength;

    public static bool IsVisible(float z) => !float.IsFinite(z) || z > MinimumZ;

    public override void Load()
    {
        LoadNPCCollision();
        if (!Main.dedServ)
        {
            On_Main.DrawNPC += DrawNPC;
            On_Main.DrawProj += DrawProjectile;
            LoadNPCInterface();
        }
    }

    /// <summary>正深度缩小、负深度放大；最大倍率为10。可见性另由 IsVisible 裁剪，确保辅助坐标计算始终有限。</summary>
    public static float GetPerspectiveScale(float z) =>
        float.IsFinite(z) ? 1f / Math.Max(0.1f, 1f + z / FocalLength) : 1f;

    /// <summary>
    /// 在传入的二维坐标空间内，以 vanishingPoint 为消失点构造投影矩阵。
    /// 世界坐标可使用世界消失点；屏幕相对坐标则使用同空间下的消失点。
    /// </summary>
    public static Matrix CreatePerspectiveMatrix(float z, Vector2 vanishingPoint)
    {
        float scale = GetPerspectiveScale(z);
        return Matrix.CreateScale(scale, scale, 1f)
            * Matrix.CreateTranslation(new Vector3(vanishingPoint * (1f - scale), 0f));
    }

    /// <summary>投影单个位置，供独立绘制的粒子、预警等复用；不会自动缩放贴图。</summary>
    public static Vector2 ProjectPosition(Vector2 position, float z, Vector2 vanishingPoint) =>
        vanishingPoint + (position - vanishingPoint) * GetPerspectiveScale(z);

    /// <summary>将投影后的锚点贴近二维目标；返回投影后、镜头变换前的平移量。</summary>
    public static Vector2 GetAnchorTranslation(Vector2 anchor, Vector2 target, float z, Vector2 vanishingPoint, float weight) =>
        (target - ProjectPosition(anchor, z, vanishingPoint)) * MathHelper.Clamp(weight, 0f, 1f);

    private static Vector2 GetNPCAnchorTranslation(NPC npc, Vector2 vanishingPoint, bool includeNetOffset = true)
    {
        if (npc.Anomaly.Depth_DrawAnchor is not { } anchor)
            return Vector2.Zero;
        return GetAnchorTranslation(npc.Center + (includeNetOffset ? npc.netOffset : Vector2.Zero) + anchor.LocalOffset, anchor.Target,
            npc.Anomaly.Z, vanishingPoint, anchor.Weight);
    }

    /// <summary>与本体绘制共用的消失点，坐标空间为变换矩阵应用之前的屏幕相对坐标。</summary>
    public static Vector2 GetVanishingPoint(Matrix view, Vector2 viewportSize) =>
        Vector2.Transform(viewportSize * 0.5f, Matrix.Invert(view));

    /// <summary>使用投影后的视觉区域检测鼠标；bounds 和 vanishingPoint 使用相同的世界坐标空间。</summary>
    public static bool HoverIntersects(Rectangle mouse, Rectangle bounds, float z, Vector2 vanishingPoint)
    {
        if (!IsVisible(z))
            return false;
        if (z == 0f)
            return mouse.Intersects(bounds);
        Vector2 position = ProjectPosition(bounds.TopLeft(), z, vanishingPoint);
        Vector2 size = bounds.Size() * GetPerspectiveScale(z);
        return new FloatRectangle(position, size).Collides(mouse);
    }

    private static Vector2 ViewportSize => Main.instance.GraphicsDevice.Viewport.Bounds.Size();

    private static void DrawNPC(On_Main.orig_DrawNPC orig, Main self, int index, bool behindTiles)
    {
        float z = Main.npc[index].GetGlobalNPC<AnomalyGlobalNPC>().Z;
        if (!IsVisible(z))
            return;
        if (GetPerspectiveScale(z) == 1f && Main.npc[index].Anomaly.Depth_DrawAnchor is null)
        {
            orig(self, index, behindTiles);
            return;
        }

        using SpriteBatchScope scope = BeginPerspective(z, Main.npc[index]);
        orig(self, index, behindTiles);
    }

    private static void DrawProjectile(On_Main.orig_DrawProj orig, Main self, int index)
    {
        float z = Main.projectile[index].GetGlobalProjectile<AnomalyGlobalProjectile>().Z;
        if (!IsVisible(z))
            return;
        if (GetPerspectiveScale(z) == 1f)
        {
            orig(self, index);
            return;
        }

        using SpriteBatchScope scope = BeginPerspective(z);
        orig(self, index);
    }

    private static SpriteBatchScope BeginPerspective(float z, NPC npc = null)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Matrix view = new SpriteBatchSnapshot(spriteBatch).TransformMatrix;
        // 投影在镜头变换前进行；逆变换确保缩放/翻转后消失点仍位于屏幕中心。
        // UI 阶段 screenWidth/Height 会被 UIScale 改写，使用真实视口避免消失点偏移。
        Vector2 vanishingPoint = GetVanishingPoint(view, ViewportSize);
        Vector2 translation = npc is null ? Vector2.Zero : GetNPCAnchorTranslation(npc, vanishingPoint + Main.screenPosition);
        return new SpriteBatchScope(spriteBatch,
            transformMatrix: CreatePerspectiveMatrix(z, vanishingPoint)
                * Matrix.CreateTranslation(new Vector3(translation, 0f)) * view);
    }

    #region NPCInterface
    [ThreadStatic]
    private static SpriteBatchScope _NPCInterfaceScope;

    private static void LoadNPCInterface()
    {
        On_Main.DrawInterface_14_EntityHealthBars += DrawNPCInterface;
        IL_Main.HoverOverNPCs += ProjectNPCHover;
        IL_Main.DrawInterface_14_EntityHealthBars += ProjectNPCInterface;
    }

    private static bool CheckNPCHover(ref Rectangle mouse, Rectangle bounds, NPC npc)
    {
        float z = npc.Anomaly.Z;
        if (z == 0f && npc.Anomaly.Depth_DrawAnchor is null)
            return mouse.Intersects(bounds);
        Vector2 vanishingPoint = GetVanishingPoint(Main.GameViewMatrix.TransformationMatrix, ViewportSize) + Main.screenPosition;
        // HoverOverNPCs 已临时将 netOffset 加入 NPC.position。
        Vector2 translation = GetNPCAnchorTranslation(npc, vanishingPoint, includeNetOffset: false);
        // 将鼠标移回平移前的投影空间，与本体和血条共用同一个锚点偏移。
        Rectangle translatedMouse = new((int)MathF.Round(mouse.X - translation.X), (int)MathF.Round(mouse.Y - translation.Y), mouse.Width, mouse.Height);
        return HoverIntersects(translatedMouse, bounds, z, vanishingPoint);
    }

    private static void ProjectNPCHover(ILContext il)
    {
        ILCursor cursor = new(il);
        // 使用原版/其他模组已修改完成的 hover bounds，只替换最后的鼠标相交测试。
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall(typeof(NPCLoader), nameof(NPCLoader.ModifyHoverBoundingBox)))
            || !cursor.Prev.Previous.MatchLdloc(out int npcLocal))
        {
            AnomalyUtils.ILFailure(nameof(ProjectNPCHover), "Could not locate NPC hover bounds.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall<Rectangle>(nameof(Rectangle.Intersects))))
        {
            AnomalyUtils.ILFailure(nameof(ProjectNPCHover), "Could not locate the NPC hover intersection test.");
            return;
        }
        cursor.Remove();
        cursor.Emit(OpCodes.Ldloc, npcLocal);
        cursor.EmitDelegate(CheckNPCHover);
        // 文本继续交给原版 MouseTextHackZoom / MouseTextInner，在 UI 平面按正常字号显示。
        // Z 仅决定鼠标是否碰到投影后的 NPC，不缩放光标旁的文本。
    }

    private static void DrawNPCInterface(On_Main.orig_DrawInterface_14_EntityHealthBars orig, Main self)
    {
        SpriteBatchScope previous = _NPCInterfaceScope;
        _NPCInterfaceScope = null;
        try
        {
            orig(self);
        }
        finally
        {
            EndNPCInterface();
            _NPCInterfaceScope = previous;
        }
    }

    private static void BeginNPCInterface(NPC npc)
    {
        EndNPCInterface();
        if (npc.active && npc.type > NPCID.None && npc.shimmerTransparency == 0f
            && (npc.Anomaly.Z != 0f || npc.Anomaly.Depth_DrawAnchor is not null) && IsVisible(npc.Anomaly.Z))
            _NPCInterfaceScope = BeginPerspective(npc.Anomaly.Z, npc);
    }

    private static void EndNPCInterface()
    {
        _NPCInterfaceScope?.Dispose();
        _NPCInterfaceScope = null;
    }

    private static void ProjectNPCInterface(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdfld<Entity>(nameof(Entity.active)))
            || !cursor.Prev.Previous.MatchLdloc(out int npcIndexLocal))
        {
            AnomalyUtils.ILFailure(nameof(ProjectNPCInterface), "Could not locate the NPC health bar loop.");
            return;
        }
        // 所有原版血条分支、模组血条钩子及手柄名称都位于这个 NPC 循环内。
        // 先验证两个插入点，避免版本不匹配时只应用了开始作用域却没有正确结束。
        ILCursor end = new(il) { Index = cursor.Index };
        if (!end.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(npcIndexLocal),
                i => i.MatchLdcI4(0),
                i => i.OpCode.Code is Code.Bge or Code.Bge_S))
        {
            AnomalyUtils.ILFailure(nameof(ProjectNPCInterface), "Could not locate the end of the NPC health bar loop.");
            return;
        }
        Instruction afterLoop = end.Next;
        cursor.Emit(OpCodes.Dup);
        cursor.EmitDelegate(BeginNPCInterface);
        // 保留原版 active 判断，同时让越过观察者的 NPC 跳过整个血条/名称分支。
        cursor.Remove();
        cursor.EmitDelegate((NPC npc) => npc.active && IsVisible(npc.Anomaly.Z));
        // 必须在玩家血条循环之前恢复，不能将最后一个 NPC 的深度带到玩家血条上。
        end.Goto(afterLoop, MoveType.Before);
        end.EmitDelegate(EndNPCInterface);
    }
    #endregion NPCInterface

    #region NPCCollision
    private static void LoadNPCCollision()
    {
        IL_Projectile.Damage += ProjectProjectileNPCCollision;
        IL_Player.ProcessHitAgainstNPC += ProjectMeleeNPCCollision;
    }

    /// <summary>投影后的世界碰撞箱；使用向外取整保留边缘不足一像素的区域。</summary>
    public static Rectangle ProjectHitbox(Rectangle bounds, float z, Vector2 vanishingPoint, Vector2 translation)
    {
        if (!IsVisible(z))
            return Rectangle.Empty;
        if (z == 0f && translation == Vector2.Zero)
            return bounds;
        Vector2 topLeft = ProjectPosition(bounds.TopLeft(), z, vanishingPoint) + translation;
        Vector2 bottomRight = ProjectPosition(bounds.BottomRight(), z, vanishingPoint) + translation;
        int left = (int)MathF.Floor(topLeft.X), top = (int)MathF.Floor(topLeft.Y);
        return new Rectangle(left, top, (int)MathF.Ceiling(bottomRight.X) - left, (int)MathF.Ceiling(bottomRight.Y) - top);
    }

    private static Rectangle GetNPCDamageHitbox(Rectangle bounds, NPC npc)
    {
        float z = npc.Anomaly.Z;
        if (!IsVisible(z))
            return Rectangle.Empty;
        if (z == 0f && npc.Anomaly.Depth_DrawAnchor is null)
            return bounds;
        // 原版近战/弹幕在命中者客户端检查碰撞，再同步伤害；专服没有观察者镜头。
        if (Main.dedServ)
            return bounds;
        Vector2 pivot = GetVanishingPoint(Main.GameViewMatrix.TransformationMatrix, ViewportSize) + Main.screenPosition;
        // 两条伤害流程均已临时将 netOffset 加入 NPC.position，不得再次叠加。
        Vector2 translation = GetNPCAnchorTranslation(npc, pivot, includeNetOffset: false);
        return ProjectHitbox(bounds, z, pivot, translation);
    }

    private static bool ProjectileHitsNPC(Projectile projectile, Rectangle projectileBounds, Rectangle npcBounds, NPC npc) =>
        IsVisible(npc.Anomaly.Z) && projectile.Colliding(projectileBounds, GetNPCDamageHitbox(npcBounds, npc));

    private static void ProjectProjectileNPCCollision(ILContext il)
    {
        ILCursor cursor = new(il);
        // 替换传给 Colliding 的目标区域，保留激光、鞭子及其他模组的自定义弹幕形状。
        // 两个分支分别为普通 NPC 和额外扩张碰撞箱的 NPC 414。
        if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld<Main>(nameof(Main.npc)),
                i => i.MatchLdloc(out _),
                i => i.OpCode == OpCodes.Ldelem_Ref,
                i => i.MatchCallvirt<NPC>(nameof(NPC.getRect)),
                i => i.MatchCall<Projectile>(nameof(Projectile.Colliding))))
        {
            AnomalyUtils.ILFailure(nameof(ProjectProjectileNPCCollision), "Could not locate the projectile/NPC collision rectangle.");
            return;
        }
        cursor.Next.Next.MatchLdloc(out int npcIndex);
        Instruction normalCollision = cursor.Next.Next.Next.Next.Next;
        Instruction expandedCollision = il.Body.Instructions.Take(cursor.Index).LastOrDefault(i => i.MatchCall<Projectile>(nameof(Projectile.Colliding)));
        if (expandedCollision is null)
        {
            AnomalyUtils.ILFailure(nameof(ProjectProjectileNPCCollision), "Could not locate the expanded NPC collision branch.");
            return;
        }
        foreach (Instruction collision in new[] { expandedCollision, normalCollision })
        {
            cursor.Goto(collision, MoveType.Before);
            cursor.Remove();
            cursor.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npc)));
            cursor.Emit(OpCodes.Ldloc, npcIndex);
            cursor.Emit(OpCodes.Ldelem_Ref);
            cursor.EmitDelegate(ProjectileHitsNPC);
        }
    }

    private static void ProjectMeleeNPCCollision(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.npc)),
                i => i.MatchLdarg(out _),
                i => i.OpCode == OpCodes.Ldelem_Ref,
                i => i.MatchStloc(out _)))
        {
            AnomalyUtils.ILFailure(nameof(ProjectMeleeNPCCollision), "Could not locate the melee target NPC.");
            return;
        }
        cursor.Prev.MatchStloc(out int npcLocal);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Rectangle>(".ctor")))
        {
            AnomalyUtils.ILFailure(nameof(ProjectMeleeNPCCollision), "Could not locate the melee target rectangle.");
            return;
        }
        Instruction address = il.Body.Instructions.Take(cursor.Index).LastOrDefault(i => i.MatchLdloca(out _));
        if (address is null || !address.MatchLdloca(out int rectangleLocal))
        {
            AnomalyUtils.ILFailure(nameof(ProjectMeleeNPCCollision), "Could not locate the melee rectangle local.");
            return;
        }
        // 在原版相交、火山剑线段检测和其他模组的近战碰撞钩子之前投影。
        cursor.Emit(OpCodes.Ldloc, rectangleLocal);
        cursor.Emit(OpCodes.Ldloc, npcLocal);
        cursor.EmitDelegate(GetNPCDamageHitbox);
        cursor.Emit(OpCodes.Stloc, rectangleLocal);
    }
    #endregion NPCCollision
}
