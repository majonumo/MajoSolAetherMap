namespace MajoSolAetherMap.Models;

public sealed record AetherCurrentQuestInfo(
    uint QuestId,
    string Name,
    AetherCurrentQuestType Type,
    bool IsCompleted,
    uint NpcId,
    string? NpcName,
    QuestLocationStatus LocationStatus,
    AetherCurrentLocation? Location,
    int CandidateLocationCount);
