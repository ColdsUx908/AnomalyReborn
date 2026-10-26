using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TransoceanicCodeAssist.Generators;

string source = """
    #nullable enable
    namespace Terraria {
        public class NPC { public int Value = 37; }
        public class Projectile { public int Value; }
        public class Item { }
    }
    namespace Terraria.ModLoader {
        public sealed class AutoloadAttribute(bool enabled) : System.Attribute { }
        public abstract class GlobalBase<T> {
            public abstract bool ConditionallyAppliesToEntities { get; }
            public virtual void SetDefaults(T entity) { }
            public virtual void Load() { }
        }
        public abstract class GlobalNPC : GlobalBase<Terraria.NPC> {
            public virtual int NewHook(Terraria.NPC npc, out int value) { value = npc.Value; return -1; }
            public virtual (int left, string right) Tuple(Terraria.NPC npc, string @event = "a,b)") => (npc.Value, @event);
            public virtual T Generic<T>(Terraria.NPC npc, ref T value) where T : class, new() => value;
            public virtual ref int Reference(Terraria.NPC npc) => ref npc.Value;
            public virtual ref readonly int ReadOnlyReference(Terraria.NPC npc) => ref npc.Value;
            public virtual int Input(Terraria.NPC npc, in int value) => value;
            public virtual int Params(Terraria.NPC npc, params int[] values) => values.Length;
            public virtual bool? Nullable(Terraria.NPC npc) => null;
            public virtual bool Existing(Terraria.NPC npc) => true;
            public virtual bool Overload(Terraria.NPC npc, int value) => true;
            public virtual bool Overload(Terraria.NPC npc, string value) => false;
            public virtual void CanNewHook(Terraria.NPC npc) { }
            [System.Obsolete("Renamed", true)] public virtual void OldHook(Terraria.NPC npc) { }
            public virtual void GlobalHook(int type) { }
            public virtual void WrongEntity(Terraria.Item item) { }
            public virtual void WrongPosition(int value, Terraria.NPC npc) { }
            public static void Static(Terraria.NPC npc) { }
            protected virtual void Protected(Terraria.NPC npc) { }
            public void NonVirtual(Terraria.NPC npc) { }
        }
        public abstract class GlobalProjectile : GlobalBase<Terraria.Projectile> {
            public virtual void AI(Terraria.Projectile projectile) { }
            public virtual bool Gate(Terraria.Projectile projectile) => true;
            public virtual bool Trigger(Terraria.Projectile projectile) => false;
            public virtual bool? Decision(Terraria.Projectile projectile) => null;
            public virtual float SpeedMultiplier(Terraria.Projectile projectile) => 1f;
            public virtual void Modify(Terraria.Projectile projectile, ref int value) { }
            public virtual void SendExtraAI(Terraria.Projectile projectile) { }
            public virtual void CanHitNPC(Terraria.Projectile projectile) { }
        }
        public abstract class GlobalItem : GlobalBase<Terraria.Item> {
            public virtual float Multiplier(Terraria.Item item) => 1f;
        }
    }
    namespace Transoceanic.Behaviors {
        public abstract class Parent {
            public Terraria.NPC _Entity = new();
            public virtual bool Existing() => false;
            public virtual void SetDefaults() { }
        }
        public abstract partial class SingleNPCBehavior : Parent {
            public virtual bool Overload(int value) => false;
            public virtual T Generic<T>(ref T value) where T : class, new() => value;
        }
        public abstract partial class SingleNPCBehavior { }
        public abstract partial class GlobalNPCBehavior {
            public virtual bool Existing(Terraria.NPC npc) => false;
        }
        public sealed class Concrete : SingleNPCBehavior { }
    }
    namespace Transoceanic.Behaviors {
        public abstract partial class SingleProjectileBehavior {
            public Terraria.Projectile _Entity = new();
            public virtual void SetDefaults() { }
        }
        public abstract partial class GlobalProjectileBehavior {
            public virtual void SetDefaults(Terraria.Projectile projectile) { }
        }
    }
    namespace Transoceanic.Behaviors {
        public abstract partial class SingleItemBehavior { public Terraria.Item _Entity = new(); }
        public abstract partial class GlobalItemBehavior { }
    }
    namespace Transoceanic.Behaviors.Dispatching {
        public class HookSet<T> {
            public System.Collections.Generic.List<T> Items = new();
            public string? LastHook;
            public System.Collections.Generic.IEnumerable<T> Enumerate([System.Runtime.CompilerServices.CallerMemberName] string? method = null) {
                LastHook = method; return Items;
            }
        }
        public sealed partial class GlobalProjectileBehaviorHandler : Terraria.ModLoader.GlobalProjectile {
            public override bool ConditionallyAppliesToEntities => false;
            public static readonly HookSet<Transoceanic.Behaviors.GlobalProjectileBehavior> BehaviorSet = new();
            public override void SetDefaults(Terraria.Projectile entity) { entity.Value = 99; }
        }
        public abstract partial class SingleProjectileBehaviorHandler<T> : Transoceanic.Behaviors.GlobalProjectileBehavior
            where T : Transoceanic.Behaviors.SingleProjectileBehavior {
            public T? Active;
            public string? LastHook;
            public bool TryGetBehavior(Terraria.Projectile entity, out T behavior,
                [System.Runtime.CompilerServices.CallerMemberName] string? method = null) {
                LastHook = method; behavior = Active!;
                if (Active is null) return false;
                Active._Entity = entity; return true;
            }
        }
        public sealed class TestGlobalBehavior : Transoceanic.Behaviors.GlobalProjectileBehavior {
            public bool GateValue;
            public bool TriggerValue;
            public bool? DecisionValue;
            public float Factor = 2;
            public int Calls;
            public override bool Gate(Terraria.Projectile p) { Calls++; return GateValue; }
            public override bool Trigger(Terraria.Projectile p) { Calls++; return TriggerValue; }
            public override bool? Decision(Terraria.Projectile p) { Calls++; return DecisionValue; }
            public override float SpeedMultiplier(Terraria.Projectile p) => Factor;
            public override void Modify(Terraria.Projectile p, ref int value) { value += p.Value; }
        }
        public sealed class TestSingleBehavior : Transoceanic.Behaviors.SingleProjectileBehavior {
            public override bool Gate() => false;
            public override bool Trigger() => true;
            public override bool? Decision() => false;
            public override void Modify(ref int value) { value += _Entity.Value; }
        }
        public sealed class TestSingleHandler : SingleProjectileBehaviorHandler<TestSingleBehavior> { }
        public static class DispatchTests {
            public static void Run() {
                void Check(bool condition) { if (!condition) throw new System.Exception("Dispatch invariant failed"); }
                var p = new Terraria.Projectile { Value = 7 };
                var handler = new GlobalProjectileBehaviorHandler();
                var set = GlobalProjectileBehaviorHandler.BehaviorSet;
                Check(handler.Gate(p) && !handler.Trigger(p) && handler.Decision(p) is null && handler.SpeedMultiplier(p) == 1);
                handler.SetDefaults(p); Check(p.Value == 99); p.Value = 7;
                var a = new TestGlobalBehavior { GateValue = false, TriggerValue = true };
                var b = new TestGlobalBehavior { GateValue = true, TriggerValue = false, DecisionValue = false, Factor = 3 };
                set.Items.Add(a); set.Items.Add(b);
                Check(!handler.Gate(p) && a.Calls == 1 && b.Calls == 1 && set.LastHook == "Gate");
                Check(handler.Trigger(p) && a.Calls == 2 && b.Calls == 2);
                Check(handler.SpeedMultiplier(p) == 6);
                Check(handler.Decision(p) == false && a.Calls == 3 && b.Calls == 3);
                a.DecisionValue = true; Check(handler.Decision(p) == true && a.Calls == 4 && b.Calls == 3);
                int value = 1; handler.Modify(p, ref value); Check(value == 15);
                var single = new TestSingleHandler();
                Check(single.Gate(p) && !single.Trigger(p) && single.Decision(p) is null);
                single.Active = new TestSingleBehavior();
                Check(!single.Gate(p) && single.LastHook == "Gate" && single.Trigger(p) && single.Decision(p) == false);
                value = 1; single.Modify(p, ref value); Check(value == 8 && single.Active._Entity == p);
            }
        }
    }
    """;
var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
    .Select(path => MetadataReference.CreateFromFile(path));
var compilation = CSharpCompilation.Create("BehaviorGeneratorFixture", [CSharpSyntaxTree.ParseText(source, parseOptions)],
    references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
GeneratorDriver driver = CSharpGeneratorDriver.Create([new BehaviorGenerator().AsSourceGenerator()], parseOptions: parseOptions);
driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
Check(!diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), string.Join("\n", diagnostics));
var result = driver.GetRunResult();
Check(result.GeneratedTrees.Length == 8, "Expected six behavior files and two dispatch files.");
string generated = string.Join("\n", result.GeneratedTrees.Select(t => t.ToString()));
foreach (string excluded in new[] { "CanNewHook", "CanHitNPC", "OldHook", "GlobalHook", "WrongEntity", "WrongPosition", " Static(", " Protected(", " NonVirtual(", " Load(", " Existing(" })
    Check(!generated.Contains(excluded), "Unexpected hook: " + excluded);
string single = result.GeneratedTrees.Single(t => t.FilePath.EndsWith("/SingleNPCBehavior.Hooks.g.cs") ||
    t.FilePath.EndsWith("\\SingleNPCBehavior.Hooks.g.cs")).ToString();
Check(!single.Contains(" Generic<"), "Generic handwritten implementation must win.");
Check(single.Contains("Overload(string value)"), "A handwritten overload must not suppress other overloads.");
Check(!single.Contains(" SetDefaults("), "Inherited behavior hooks must not be regenerated.");
string dispatch = string.Join("\n", result.GeneratedTrees.Where(t => t.FilePath.EndsWith(".Dispatch.g.cs")));
Check(!dispatch.Contains("SendExtraAI") && !dispatch.Contains("CanHitNPC"), "Excluded network/CanHit callbacks must not be registered by generated handlers.");
Check(!output.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
    string.Join("\n", output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error)) + "\n" + generated);
using var stream = new MemoryStream();
var emit = output.Emit(stream);
Check(emit.Success, string.Join("\n", emit.Diagnostics));
var assembly = Assembly.Load(stream.ToArray());
var concrete = assembly.GetType("Transoceanic.Behaviors.Concrete")!;
object instance = Activator.CreateInstance(concrete)!;
object?[] arguments = [null];
Check((int)concrete.GetMethod("NewHook")!.Invoke(instance, arguments)! == -1 && (int)arguments[0]! == 37,
    "Original defaults and out parameters must receive the bound entity.");
Check(concrete.GetMethod("Nullable")!.Invoke(instance, null) is null, "Nullable default must be preserved.");
assembly.GetType("Transoceanic.Behaviors.Dispatching.DispatchTests")!.GetMethod("Run")!.Invoke(null, null);
CanHitTests.Run(references, parseOptions);
Console.WriteLine("PASS: behavior signatures/defaults; dispatch AND/OR, multipliers, priority, defaults, ref parameters, binding, caller names and manual overrides; unified CanHit routing.");

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
