using Transoceanic.Runtime.Reflection;

[assembly: SupportedByTransoceanic]

namespace Anomalies;

public sealed class AnomalyMain : Mod
{
    internal static AnomalyMain Instance { get; private set; }

    internal static bool Loading { get; private set; }

    internal static bool Loaded { get; private set; }

    internal static bool Unloading { get; private set; }

    internal static bool Unloaded { get; private set; }

    public override void Load()
    {
        Loading = true;
        try
        {
            Instance = this;

            foreach (IAnomalyLoader loader in
                from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IAnomalyLoader>(AnomalySharedData.Assembly)
                orderby pair.Type.GetMethod(nameof(IAnomalyLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                select pair.Instance)
            {
                loader.Load();
            }
        }
        finally
        {
            Loaded = true;
            Loading = false;
        }
    }

    public override void Unload()
    {
        Unloading = true;
        try
        {
            if (Loaded)
            {
                foreach (IAnomalyLoader loader in (
                    from pair in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<IAnomalyLoader>(AnomalySharedData.Assembly)
                    orderby pair.Type.GetMethod(nameof(IAnomalyLoader.Load), TOReflectionUtils.UniversalBindingFlags)?.Attribute<LoadPriorityAttribute>()?.Priority ?? 0 descending
                    select pair.Instance).Reverse())
                {
                    loader.Unload();
                }
                Instance = null;
            }
        }
        finally
        {
            Unloaded = true;
            Unloading = false;
        }
    }

    public override object Call(params object[] args) => AnomalyModCall.Call(args);

    public override void HandlePacket(BinaryReader reader, int whoAmI) => AnomalySynchronization.HandlePacket(this, reader, whoAmI);
}