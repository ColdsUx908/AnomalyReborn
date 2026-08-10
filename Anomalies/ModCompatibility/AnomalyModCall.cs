// Developed by ColdsUx

namespace Anomalies.ModCompatibility;

public static class AnomalyModCall
{
    public static object Call(params object[] args)
    {
        if (args.Length == 0 || args[0] is not string option)
            return null;

        switch (option.ToLower())
        {
            case "world":
                if (args.Length == 1 || args[1] is not string variable)
                    return null;

                switch (variable.ToLower())
                {
                    case "anomaly":
                        if (args.Length == 2)
                            return AnomalySharedData.Anomaly;
                        else if (args[2] is bool newValue)
                            return AnomalySharedData.Anomaly = newValue;
                        else
                            return null;
                    case "anomalyultra":
                        if (args.Length == 2)
                            return AnomalySharedData.AnomalyUltramundane;
                        else if (args[2] is bool newValue)
                            return AnomalySharedData.AnomalyUltramundane = newValue;
                        else
                            return null;
                }

                return null;
        }

        return null;
    }
}
