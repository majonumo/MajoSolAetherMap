namespace MajoSolAetherMap.Models;

public sealed record MapOverlayMarker(
    uint Id,
    uint AetherCurrentId,
    string Name,
    MapOverlayMarkerKind Kind,
    bool IsCompleted,
    AetherCurrentLocation Location,
    string? NpcName);
