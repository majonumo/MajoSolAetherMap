namespace MajoSolAetherMap.Models;

public sealed record AetherZoneOption(
    uint TerritoryId,
    uint MapId,
    string ZoneName,
    string ExpansionName);
