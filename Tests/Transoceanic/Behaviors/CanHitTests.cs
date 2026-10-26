using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class CanHitTests
{
    public static void Run(IEnumerable<MetadataReference> references, CSharpParseOptions options)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        const string relativePath = "Anomalies/Common/GlobalInstances/AnomalyGlobalInstances.CanHit.cs";
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, relativePath)))
            directory = directory.Parent;
        if (directory is null)
            throw new Exception("Cannot locate production CanHit source.");
        string fixture = """
            namespace Anomalies.Common;
            public struct Rectangle { }
            public class Player { }
            public class NPC {
                public AnomalyGlobalNPC Anomaly = new();
                public AnomalyNPCBehavior? Behavior;
                public string? LastHook;
                public bool Enabled = true;
                public bool TryGetBehavior(out AnomalyNPCBehavior behavior, [System.Runtime.CompilerServices.CallerMemberName] string? method = null) {
                    LastHook = method; behavior = Behavior!; return Enabled && Behavior is not null;
                }
            }
            public class Projectile {
                public AnomalyProjectileBehavior? Behavior;
                public string? LastHook;
                public bool TryGetBehavior(out AnomalyProjectileBehavior behavior, [System.Runtime.CompilerServices.CallerMemberName] string? method = null) {
                    LastHook = method; behavior = Behavior!; return Behavior is not null;
                }
            }
            public class Item {
                public AnomalyItemBehavior? Behavior;
                public string? LastHook;
                public bool TryGetBehavior(out AnomalyItemBehavior behavior, [System.Runtime.CompilerServices.CallerMemberName] string? method = null) {
                    LastHook = method; behavior = Behavior!; return Behavior is not null;
                }
            }
            public class AnomalyNPCBehavior {
                public int Calls;
                public virtual bool? CanBeHitByItem(Player p, Item i) { Calls++; return true; }
                public virtual bool? CanCollideWithPlayerMeleeAttack(Player p, Item i, Rectangle r) { Calls++; return false; }
                public virtual bool? CanBeHitByProjectile(Projectile p) { Calls++; return false; }
                public virtual bool CanBeHitByNPC(NPC n) { Calls++; return false; }
                public virtual bool CanHitPlayer(Player p, ref int slot) { Calls++; slot = 17; return false; }
                public virtual bool CanHitNPC(NPC n) { Calls++; return false; }
            }
            public class AnomalyProjectileBehavior {
                public int Calls;
                public bool? CanHitNPC(NPC n) { Calls++; return false; }
                public bool CanHitPvp(Player p) { Calls++; return false; }
                public bool CanHitPlayer(Player p) { Calls++; return false; }
            }
            public class AnomalyItemBehavior {
                public int Calls;
                public bool? CanHitNPC(Player p, NPC n) { Calls++; return false; }
                public bool? CanMeleeAttackCollideWithNPC(Rectangle r, Player p, NPC n) { Calls++; return false; }
                public bool CanHitPvp(Player p, Player t) { Calls++; return false; }
            }
            public class ModPlayer { public virtual bool CanHitNPC(NPC n) => true; }
            public class GlobalNPC {
                public virtual bool? CanBeHitByItem(NPC n, Player p, Item i) => null;
                public virtual bool? CanCollideWithPlayerMeleeAttack(NPC n, Player p, Item i, Rectangle r) => null;
                public virtual bool? CanBeHitByProjectile(NPC n, Projectile p) => null;
                public virtual bool CanBeHitByNPC(NPC n, NPC a) => true;
                public virtual bool CanHitPlayer(NPC n, Player p, ref int slot) => true;
                public virtual bool CanHitNPC(NPC n, NPC t) => true;
            }
            public class GlobalProjectile {
                public virtual bool? CanHitNPC(Projectile p, NPC n) => null;
                public virtual bool CanHitPvp(Projectile p, Player t) => true;
                public virtual bool CanHitPlayer(Projectile p, Player t) => true;
            }
            public class GlobalItem {
                public virtual bool? CanHitNPC(Item i, Player p, NPC n) => null;
                public virtual bool? CanMeleeAttackCollideWithNPC(Item i, Rectangle r, Player p, NPC n) => null;
                public virtual bool CanHitPvp(Item i, Player p, Player t) => true;
            }
            public sealed partial class AnomalyPlayer : ModPlayer { }
            public sealed partial class AnomalyGlobalNPC : GlobalNPC { public float Z; }
            public sealed partial class AnomalyGlobalProjectile : GlobalProjectile { }
            public sealed partial class AnomalyGlobalItem : GlobalItem { }
            public static class AnomalyDepthDrawSystem { public static bool IsVisible(float z) => z == 0; }
            public static class RoutingTests {
                public static void Run() {
                    void Check(bool condition) { if (!condition) throw new System.Exception("CanHit routing invariant failed"); }
                    var npc = new NPC(); var target = new NPC(); var player = new Player();
                    var projectile = new Projectile(); var item = new Item();
                    var gn = npc.Anomaly; var gp = new AnomalyGlobalProjectile(); var gi = new AnomalyGlobalItem();
                    int slot = 0;
                    Check(gn.CanBeHitByItem(npc, player, item) is null && gn.CanBeHitByProjectile(npc, projectile) is null);
                    Check(gn.CanBeHitByNPC(npc, target) && gn.CanHitNPC(npc, target) && gn.CanHitPlayer(npc, player, ref slot));
                    Check(gn.CanCollideWithPlayerMeleeAttack(npc, player, item, new()) is null);
                    Check(gp.CanHitNPC(projectile, target) is null && gp.CanHitPvp(projectile, player) && gp.CanHitPlayer(projectile, player));
                    Check(gi.CanHitNPC(item, player, target) is null && gi.CanMeleeAttackCollideWithNPC(item, new(), player, target) is null && gi.CanHitPvp(item, player, player));
                    var behavior = npc.Behavior = new AnomalyNPCBehavior();
                    Check(gn.CanBeHitByItem(npc, player, item) == true && npc.LastHook == "CanBeHitByItem");
                    Check(gn.CanBeHitByProjectile(npc, projectile) == false && npc.LastHook == "CanBeHitByProjectile");
                    Check(!gn.CanBeHitByNPC(npc, target) && npc.LastHook == "CanBeHitByNPC");
                    Check(!gn.CanHitPlayer(npc, player, ref slot) && slot == 17 && npc.LastHook == "CanHitPlayer");
                    Check(!gn.CanHitNPC(npc, target) && npc.LastHook == "CanHitNPC");
                    Check(gn.CanCollideWithPlayerMeleeAttack(npc, player, item, new()) == false && npc.LastHook == "CanCollideWithPlayerMeleeAttack");
                    Check(behavior.Calls == 6);
                    gn.Z = 1;
                    Check(gn.CanBeHitByItem(npc, player, item) == false && gn.CanBeHitByProjectile(npc, projectile) == false && !gn.CanBeHitByNPC(npc, target));
                    Check(behavior.Calls == 6 && !new AnomalyPlayer().CanHitNPC(npc));
                    gn.Z = 0; npc.Enabled = false;
                    Check(gn.CanHitPlayer(npc, player, ref slot) && behavior.Calls == 6);
                    projectile.Behavior = new AnomalyProjectileBehavior();
                    Check(gp.CanHitNPC(projectile, target) == false && projectile.LastHook == "CanHitNPC");
                    Check(!gp.CanHitPvp(projectile, player) && projectile.LastHook == "CanHitPvp");
                    Check(!gp.CanHitPlayer(projectile, player) && projectile.LastHook == "CanHitPlayer" && projectile.Behavior.Calls == 3);
                    item.Behavior = new AnomalyItemBehavior();
                    Check(gi.CanHitNPC(item, player, target) == false && item.LastHook == "CanHitNPC");
                    Check(gi.CanMeleeAttackCollideWithNPC(item, new(), player, target) == false && item.LastHook == "CanMeleeAttackCollideWithNPC");
                    Check(!gi.CanHitPvp(item, player, player) && item.LastHook == "CanHitPvp" && item.Behavior.Calls == 3);
                }
            }
            """;
        CSharpCompilation compilation = CSharpCompilation.Create("CanHitRoutingFixture",
            [CSharpSyntaxTree.ParseText(fixture, options), CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(directory.FullName, relativePath)), options)],
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        using MemoryStream stream = new();
        var emit = compilation.Emit(stream);
        if (!emit.Success)
            throw new Exception(string.Join("\n", emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        Assembly.Load(stream.ToArray()).GetType("Anomalies.Common.RoutingTests")!.GetMethod("Run")!.Invoke(null, null);
    }
}
