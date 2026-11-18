using System.Collections.Generic;
using System.Linq;

namespace MajoSolAetherMap.Models;

public sealed record ZoneAetherCurrentSnapshot(
    bool IsLoggedIn,
    uint TerritoryId,
    uint MapId,
    string ZoneName,
    string ExpansionName,
    string? LoadingImagePath,
    bool IsUrban,
    bool IsARealmReborn,
    bool? IsFlightUnlocked,
    IReadOnlyList<AetherCurrentInfo> Currents)
{
    public static ZoneAetherCurrentSnapshot LoggedOut { get; } =
        new(false, 0, 0, string.Empty, string.Empty, null, false, false, null, []);

    public int TotalCount => Currents.Count;

    public int UnlockedCount => Currents.Count(current => current.IsUnlocked);

    public int PendingCount => TotalCount - UnlockedCount;

    public int ExplorationCount => Currents.Count(current => current.Kind == AetherCurrentKind.Exploration);

    public int ExplorationUnlockedCount => Currents.Count(
        current => current.Kind == AetherCurrentKind.Exploration && current.IsUnlocked);

    public int QuestCount => Currents.Count(current => current.Kind == AetherCurrentKind.Quest);

    public int QuestUnlockedCount => Currents.Count(
        current => current.Kind == AetherCurrentKind.Quest && current.IsUnlocked);

    public bool HasExplorationCurrents => ExplorationCount > 0;

    public bool AreAllCurrentsUnlocked => TotalCount > 0 && UnlockedCount == TotalCount;
}
