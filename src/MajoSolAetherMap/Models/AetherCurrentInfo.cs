namespace MajoSolAetherMap.Models;

public sealed record AetherCurrentInfo(
    uint RowId,
    string Name,
    AetherCurrentKind Kind,
    bool IsUnlocked,
    AetherCurrentLocation? Location,
    AetherCurrentQuestInfo? Quest);
