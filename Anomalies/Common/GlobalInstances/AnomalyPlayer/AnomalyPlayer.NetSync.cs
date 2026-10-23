using Anomalies.SpecialContents.Developer.EventideReunion;

namespace Anomalies.Common;

public sealed partial class AnomalyPlayer
{
    partial void SyncExtraData(int toWho, int fromWho, bool newPlayer) =>
        EventideReunionResonancePlayer.Sync(Player, toWho, fromWho);
}
