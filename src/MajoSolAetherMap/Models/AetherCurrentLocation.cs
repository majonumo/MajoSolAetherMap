using System.Numerics;

namespace MajoSolAetherMap.Models;

public sealed record AetherCurrentLocation(
    uint TerritoryId,
    uint MapId,
    string TerritoryName,
    string MapName,
    Vector3 WorldPosition,
    Vector2 MapPosition);
