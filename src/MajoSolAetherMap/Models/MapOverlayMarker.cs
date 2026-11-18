namespace MajoSolAetherMap.Models;

public sealed record MapOverlayMarker(
    uint Id,
    string Name,
    MapOverlayMarkerKind Kind,
    bool IsCompleted,
    AetherCurrentLocation Location,
    string? NpcName);
