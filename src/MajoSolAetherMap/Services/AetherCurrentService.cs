using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using MajoSolAetherMap.Models;

namespace MajoSolAetherMap.Services;

#pragma warning disable Dalamud001 // IUnlockState is the supported API 15 source for Aether Current unlock state.

public sealed class AetherCurrentService : IDisposable
{
    private const uint AetherCurrentQuestRewardId = 2;

    // AetherCurrent.Quest contains five stale links in the current game data. The replacement
    // quests below are verified by Quest.OtherReward = 2 (QuestRewardOther "Aether Current").
    private static readonly IReadOnlyDictionary<uint, uint> QuestCorrections =
        new Dictionary<uint, uint>
        {
            [2818065] = 67326, // Natural Repellent -> Stolen Munitions
            [2818066] = 67333, // Chocobo's Last Stand -> The Hunter Becomes the Kweh
            [2818096] = 67364, // The Unceasing Gardener -> Hide Your Moogles
            [2818110] = 67410, // Search and Rescue -> Honoring the Past
            [2818328] = 69793, // Curing What Ails -> In Agama's Footsteps
        };

    private readonly IDataManager dataManager;
    private readonly IClientState clientState;
    private readonly IUnlockState unlockState;
    private readonly IGameGui gameGui;
    private readonly IPluginLog log;
    private readonly IReadOnlyDictionary<uint, Quest> questByCurrentId;
    private readonly IReadOnlyDictionary<uint, AetherCurrentLocation> locationByCurrentId;
    private readonly IReadOnlyDictionary<uint, IReadOnlyList<AetherCurrentLocation>> locationsByNpcId;
    private readonly IReadOnlyDictionary<uint, IReadOnlyList<MapOverlayMarker>> overlayTemplatesByMapId;
    private readonly IReadOnlyDictionary<uint, MapOverlayMarker> overlayTemplatesById;
    private readonly IReadOnlyList<MapIdentity> overlayMapIdentities;
    private readonly Dictionary<uint, OverlayCacheEntry> overlayStateCache = [];
    private readonly Dictionary<uint, ZoneSnapshotCacheEntry> zoneSnapshotCache = [];
    private readonly HashSet<uint> trackedManagedMarkerIds = [];
    private readonly HashSet<uint> suppressedManagedMarkerIds = [];
    private bool managedMarkerTrackingInitialized;
    private NativeFlagOwnership? nativeFlagOwnership;
    private int overlayRevision;

    public AetherCurrentService(
        IDataManager dataManager,
        IClientState clientState,
        IUnlockState unlockState,
        IGameGui gameGui,
        IPluginLog log)
    {
        this.dataManager = dataManager;
        this.clientState = clientState;
        this.unlockState = unlockState;
        this.gameGui = gameGui;
        this.log = log;
        questByCurrentId = BuildQuestAssociations();
        var locationIndexes = BuildLocationIndexes();
        locationByCurrentId = locationIndexes.ByCurrentId;
        locationsByNpcId = locationIndexes.ByNpcId;
        var overlayIndexes = BuildOverlayIndexes();
        overlayTemplatesByMapId = overlayIndexes.MarkersByMapId;
        overlayTemplatesById = overlayTemplatesByMapId.Values
            .SelectMany(markers => markers)
            .DistinctBy(marker => marker.Id)
            .ToDictionary(marker => marker.Id);
        overlayMapIdentities = overlayIndexes.MapIdentities;
        AvailableZones = BuildAvailableZones();
        LogQuestCoverage();

        clientState.Login += OnLogin;
        clientState.Logout += OnLogout;
        clientState.TerritoryChanged += OnTerritoryChanged;
        unlockState.Unlock += OnUnlock;

        Refresh();
    }

    public ZoneAetherCurrentSnapshot Current { get; private set; } =
        ZoneAetherCurrentSnapshot.LoggedOut;

    public IReadOnlyList<AetherZoneOption> AvailableZones { get; }

    public void Dispose()
    {
        clientState.Login -= OnLogin;
        clientState.Logout -= OnLogout;
        clientState.TerritoryChanged -= OnTerritoryChanged;
        unlockState.Unlock -= OnUnlock;
    }

    public bool PlaceMapFlag(AetherCurrentInfo current)
    {
        if (current.Location is not { } location)
            return false;

        var placed = gameGui.OpenMapWithMapLink(
            location.TerritoryId,
            location.MapId,
            location.WorldPosition);
        if (placed)
        {
            ActivateManagedMarker(current.RowId);
            CaptureNativeFlagOwnership(current.RowId, location);
        }
        return placed;
    }

    public bool PlaceNpcFlag(AetherCurrentQuestInfo quest)
    {
        if (quest.LocationStatus != QuestLocationStatus.Resolved ||
            quest.Location is not { } location)
        {
            return false;
        }

        var placed = gameGui.OpenMapWithMapLink(
            location.TerritoryId,
            location.MapId,
            location.WorldPosition);
        if (placed)
        {
            ActivateManagedMarker(quest.QuestId);
            CaptureNativeFlagOwnership(quest.QuestId, location);
        }
        return placed;
    }

    public bool HasRemovableMapFlag(uint markerId)
    {
        return nativeFlagOwnership is { MarkerId: var ownedMarkerId } &&
               ownedMarkerId == markerId &&
               IsOwnedNativeFlagStillActive();
    }

    public bool RemoveMapFlag(uint markerId)
    {
        if (!HasRemovableMapFlag(markerId))
            return false;

        unsafe
        {
            var agentMap = AgentMap.Instance();
            if (agentMap is null)
                return false;

            agentMap->FlagMarkerCount = 0;
        }

        nativeFlagOwnership = null;
        log.Information("Bandera nativa gestionada {MarkerId} eliminada.", markerId);
        return true;
    }

    public ZoneAetherCurrentSnapshot GetZoneSnapshot(uint territoryId)
    {
        if (territoryId == 0 || territoryId == Current.TerritoryId)
            return Current;
        if (!clientState.IsLoggedIn)
            return ZoneAetherCurrentSnapshot.LoggedOut;
        if (zoneSnapshotCache.TryGetValue(territoryId, out var cached) && cached.Revision == overlayRevision)
            return cached.Snapshot;

        var snapshot = BuildZoneSnapshot(territoryId);
        zoneSnapshotCache[territoryId] = new ZoneSnapshotCacheEntry(overlayRevision, snapshot);
        return snapshot;
    }

    public IReadOnlyList<MapOverlayMarker> GetOverlayMarkers(uint mapId)
    {
        if (!overlayTemplatesByMapId.TryGetValue(mapId, out var templates))
            return [];

        if (overlayStateCache.TryGetValue(mapId, out var cached) && cached.Revision == overlayRevision)
            return cached.Markers;

        var currentSheet = dataManager.GetExcelSheet<AetherCurrent>();
        var markers = templates
            .Where(template => !suppressedManagedMarkerIds.Contains(template.Id))
            .Select(template => template with
            {
                IsCompleted = IsMarkerCompleted(template, currentSheet),
            })
            .ToArray();

        overlayStateCache[mapId] = new OverlayCacheEntry(overlayRevision, markers);
        return markers;
    }

    public bool TryResolveOverlayMap(
        string areaText,
        string subAreaText,
        string titleText,
        out uint mapId)
    {
        var labels = new[] { areaText, subAreaText, titleText }
            .Select(NormalizeMapLabel)
            .Where(label => label.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var candidates = overlayMapIdentities
            .Select(identity => (identity.MapId, Score: identity.Score(labels)))
            .Where(candidate => candidate.Score > 0)
            .OrderByDescending(candidate => candidate.Score)
            .ToArray();

        if (candidates.Length > 0 &&
            (candidates.Length == 1 || candidates[0].Score > candidates[1].Score))
        {
            mapId = candidates[0].MapId;
            return true;
        }

        mapId = 0;
        return false;
    }

    private void ActivateManagedMarker(uint markerId)
    {
        if (!overlayTemplatesById.TryGetValue(markerId, out var marker))
            return;

        var currentSheet = dataManager.GetExcelSheet<AetherCurrent>();
        if (IsMarkerCompleted(marker, currentSheet))
        {
            trackedManagedMarkerIds.Remove(markerId);
            if (suppressedManagedMarkerIds.Add(markerId))
                InvalidateCaches();
            return;
        }

        trackedManagedMarkerIds.Add(markerId);
        if (!suppressedManagedMarkerIds.Remove(markerId))
            return;

        InvalidateCaches();
    }

    private void InitializeManagedMarkerTracking()
    {
        var currentSheet = dataManager.GetExcelSheet<AetherCurrent>();
        foreach (var marker in overlayTemplatesById.Values)
        {
            if (!IsMarkerCompleted(marker, currentSheet))
                trackedManagedMarkerIds.Add(marker.Id);
        }

        managedMarkerTrackingInitialized = true;
    }

    private void SuppressCompletedTrackedMarkers()
    {
        if (trackedManagedMarkerIds.Count == 0)
            return;

        var currentSheet = dataManager.GetExcelSheet<AetherCurrent>();
        foreach (var markerId in trackedManagedMarkerIds.ToArray())
        {
            if (!overlayTemplatesById.TryGetValue(markerId, out var marker) ||
                !IsMarkerCompleted(marker, currentSheet))
            {
                continue;
            }

            trackedManagedMarkerIds.Remove(markerId);
            suppressedManagedMarkerIds.Add(markerId);
            RemoveMapFlag(markerId);
            log.Information(
                "Marcador gestionado {MarkerId} ocultado al completarse su objetivo.",
                markerId);
        }
    }

    private bool IsMarkerCompleted(
        MapOverlayMarker marker,
        ExcelSheet<AetherCurrent> currentSheet)
    {
        return currentSheet.TryGetRow(marker.AetherCurrentId, out var current) &&
               unlockState.IsAetherCurrentUnlocked(current);
    }

    private void InvalidateCaches()
    {
        overlayRevision++;
        overlayStateCache.Clear();
        zoneSnapshotCache.Clear();
    }

    private unsafe void CaptureNativeFlagOwnership(
        uint markerId,
        AetherCurrentLocation expectedLocation)
    {
        var agentMap = AgentMap.Instance();
        if (agentMap is null || agentMap->FlagMarkerCount != 1)
        {
            nativeFlagOwnership = null;
            log.Warning(
                "No se pudo asumir propiedad de la bandera nativa para el marcador {MarkerId}.",
                markerId);
            return;
        }

        ref var flag = ref agentMap->FlagMapMarkers[0];
        // IGameGui.OpenMapWithMapLink serializes each world coordinate as
        // `(int)coordinate * 1000`, so the native flag stores whole world units.
        var expectedX = (int)expectedLocation.WorldPosition.X;
        var expectedY = (int)expectedLocation.WorldPosition.Z;
        var matchesRequestedFlag = flag.TerritoryId == expectedLocation.TerritoryId &&
                                   flag.MapId == expectedLocation.MapId &&
                                   MathF.Abs(flag.XFloat - expectedX) <= 0.001f &&
                                   MathF.Abs(flag.YFloat - expectedY) <= 0.001f;
        if (!matchesRequestedFlag)
        {
            nativeFlagOwnership = null;
            log.Warning(
                "La bandera nativa activa no coincide con el marcador solicitado {MarkerId}; " +
                "no se asumirá propiedad.",
                markerId);
            return;
        }

        nativeFlagOwnership = new NativeFlagOwnership(
            markerId,
            flag.TerritoryId,
            flag.MapId,
            flag.XFloat,
            flag.YFloat,
            flag.MapMarker.IconId);
    }

    private unsafe bool IsOwnedNativeFlagStillActive()
    {
        if (nativeFlagOwnership is not { } ownership)
            return false;

        var agentMap = AgentMap.Instance();
        if (agentMap is null || agentMap->FlagMarkerCount != 1)
        {
            nativeFlagOwnership = null;
            return false;
        }

        ref var flag = ref agentMap->FlagMapMarkers[0];
        var stillOwned = flag.TerritoryId == ownership.TerritoryId &&
                         flag.MapId == ownership.MapId &&
                         flag.MapMarker.IconId == ownership.IconId &&
                         MathF.Abs(flag.XFloat - ownership.X) <= 0.001f &&
                         MathF.Abs(flag.YFloat - ownership.Y) <= 0.001f;
        if (!stillOwned)
            nativeFlagOwnership = null;
        return stillOwned;
    }

    private void Refresh()
    {
        InvalidateCaches();

        if (!clientState.IsLoggedIn)
        {
            Current = ZoneAetherCurrentSnapshot.LoggedOut;
            return;
        }

        if (managedMarkerTrackingInitialized)
            SuppressCompletedTrackedMarkers();
        else
            InitializeManagedMarkerTracking();

        try
        {
            var territoryId = clientState.TerritoryType;
            var territorySheet = dataManager.GetExcelSheet<TerritoryType>();

            if (!territorySheet.TryGetRow(territoryId, out var territory))
            {
                Current = new ZoneAetherCurrentSnapshot(
                    true,
                    territoryId,
                    clientState.MapId,
                    $"Territorio {territoryId}",
                    "Expansión no disponible",
                    null,
                    false,
                    false,
                    null,
                    []);
                return;
            }

            var zoneName = GetZoneName(territory);
            var expansionName = GetExpansionName(territory);
            var loadingImagePath = GetLoadingImagePath(territory);
            var isUrban = territory.TerritoryIntendedUse.RowId == 0;
            var isARealmReborn = territory.ExVersion.RowId == 0;
            var mapId = clientState.MapId != 0
                ? clientState.MapId
                : territory.Map.IsValid
                    ? territory.Map.RowId
                    : 0;

            if (!territory.AetherCurrentCompFlgSet.IsValid)
            {
                Current = new ZoneAetherCurrentSnapshot(
                    true,
                    territoryId,
                    mapId,
                    zoneName,
                    expansionName,
                    loadingImagePath,
                    isUrban,
                    isARealmReborn,
                    null,
                    []);
                return;
            }

            var currentSet = territory.AetherCurrentCompFlgSet.Value;
            var isFlightUnlocked = unlockState.IsAetherCurrentCompFlgSetUnlocked(currentSet);
            var currents = new List<AetherCurrentInfo>();
            var explorationNumber = 0;

            // ARR outdoor territories share a completion flag set for flight, but do not use
            // the field/quest Aether Current system introduced in Heavensward.
            if (!isARealmReborn)
            {
                foreach (var currentRef in currentSet.AetherCurrents)
                {
                    // The sheet contains a fixed-size collection. Older zones have empty slots.
                    if (!currentRef.IsValid)
                        continue;

                    var current = currentRef.Value;
                    var isQuest = current.Quest.IsValid;
                    Quest quest = default;
                    if (isQuest && !questByCurrentId.TryGetValue(current.RowId, out quest))
                        continue;
                    var kind = isQuest
                        ? AetherCurrentKind.Quest
                        : AetherCurrentKind.Exploration;
                    var questInfo = isQuest ? CreateQuestInfo(quest) : null;
                    var name = questInfo?.Name ?? $"Corriente de exploración {++explorationNumber}";

                    locationByCurrentId.TryGetValue(current.RowId, out var location);
                    currents.Add(new AetherCurrentInfo(
                        current.RowId,
                        name,
                        kind,
                        unlockState.IsAetherCurrentUnlocked(current),
                        isQuest ? null : location,
                        questInfo));
                }
            }

            Current = new ZoneAetherCurrentSnapshot(
                true,
                territoryId,
                mapId,
                zoneName,
                expansionName,
                loadingImagePath,
                isUrban,
                isARealmReborn,
                isFlightUnlocked,
                currents);
        }
        catch (Exception exception)
        {
            log.Error(exception, "No se pudieron actualizar las Aether Currents del territorio actual.");
        }
    }

    private ZoneAetherCurrentSnapshot BuildZoneSnapshot(uint territoryId)
    {
        try
        {
            var territorySheet = dataManager.GetExcelSheet<TerritoryType>();
            if (!territorySheet.TryGetRow(territoryId, out var territory))
                return ZoneAetherCurrentSnapshot.LoggedOut;

            var zoneName = GetZoneName(territory);
            var expansionName = GetExpansionName(territory);
            var loadingImagePath = GetLoadingImagePath(territory);
            var mapId = territory.Map.IsValid ? territory.Map.RowId : 0;
            var isUrban = territory.TerritoryIntendedUse.RowId == 0;
            var isARealmReborn = territory.ExVersion.RowId == 0;
            if (!territory.AetherCurrentCompFlgSet.IsValid)
            {
                return new ZoneAetherCurrentSnapshot(
                    true, territoryId, mapId, zoneName, expansionName, loadingImagePath,
                    isUrban, isARealmReborn, null, []);
            }

            var currentSet = territory.AetherCurrentCompFlgSet.Value;
            var currents = new List<AetherCurrentInfo>();
            var explorationNumber = 0;
            if (!isARealmReborn)
            {
                foreach (var currentRef in currentSet.AetherCurrents)
                {
                    if (!currentRef.IsValid)
                        continue;
                    var current = currentRef.Value;
                    var isQuest = current.Quest.IsValid;
                    Quest quest = default;
                    if (isQuest && !questByCurrentId.TryGetValue(current.RowId, out quest))
                        continue;
                    var questInfo = isQuest ? CreateQuestInfo(quest) : null;
                    locationByCurrentId.TryGetValue(current.RowId, out var location);
                    currents.Add(new AetherCurrentInfo(
                        current.RowId,
                        questInfo?.Name ?? $"Corriente de exploración {++explorationNumber}",
                        isQuest ? AetherCurrentKind.Quest : AetherCurrentKind.Exploration,
                        unlockState.IsAetherCurrentUnlocked(current),
                        isQuest ? null : location,
                        questInfo));
                }
            }

            return new ZoneAetherCurrentSnapshot(
                true, territoryId, mapId, zoneName, expansionName, loadingImagePath,
                isUrban, isARealmReborn,
                unlockState.IsAetherCurrentCompFlgSetUnlocked(currentSet), currents);
        }
        catch (Exception exception)
        {
            log.Error(exception, "No se pudo construir el progreso del territorio {TerritoryId}.", territoryId);
            return ZoneAetherCurrentSnapshot.LoggedOut;
        }
    }

    private IReadOnlyList<AetherZoneOption> BuildAvailableZones()
    {
        return dataManager.GetExcelSheet<TerritoryType>()
            .Where(territory => territory.IsInUse && territory.ExVersion.RowId != 0 &&
                                territory.AetherCurrentCompFlgSet.IsValid &&
                                territory.AetherCurrentCompFlgSet.Value.AetherCurrents.Any(current => current.IsValid))
            .DistinctBy(territory => territory.AetherCurrentCompFlgSet.RowId)
            .Select(territory => new AetherZoneOption(
                territory.RowId,
                territory.Map.IsValid ? territory.Map.RowId : 0,
                GetZoneName(territory),
                GetExpansionName(territory)))
            .OrderBy(zone => zone.ExpansionName, StringComparer.CurrentCulture)
            .ThenBy(zone => zone.ZoneName, StringComparer.CurrentCulture)
            .ToArray();
    }

    private OverlayIndexes BuildOverlayIndexes()
    {
        try
        {
            var activeCurrents = dataManager.GetExcelSheet<TerritoryType>()
                .Where(territory => territory.ExVersion.RowId != 0 && territory.AetherCurrentCompFlgSet.IsValid)
                .SelectMany(territory => territory.AetherCurrentCompFlgSet.Value.AetherCurrents)
                .Where(current => current.IsValid)
                .Select(current => current.Value)
                .DistinctBy(current => current.RowId)
                .ToArray();

            var markers = new List<MapOverlayMarker>();
            foreach (var current in activeCurrents)
            {
                if (!current.Quest.IsValid)
                {
                    if (locationByCurrentId.TryGetValue(current.RowId, out var location))
                    {
                        markers.Add(new MapOverlayMarker(
                            current.RowId,
                            current.RowId,
                            "Corriente de exploración",
                            MapOverlayMarkerKind.ExplorationCurrent,
                            false,
                            location,
                            null));
                    }

                    continue;
                }

                if (!questByCurrentId.TryGetValue(current.RowId, out var associatedQuest))
                    continue;

                var quest = CreateQuestInfo(associatedQuest, false);
                if (quest.LocationStatus != QuestLocationStatus.Resolved || quest.Location is not { } questLocation)
                    continue;

                var kind = quest.Type switch
                {
                    AetherCurrentQuestType.MainScenario => MapOverlayMarkerKind.MainScenarioNpc,
                    AetherCurrentQuestType.SideQuest => MapOverlayMarkerKind.SideQuestNpc,
                    _ => MapOverlayMarkerKind.UnknownQuestNpc,
                };
                markers.Add(new MapOverlayMarker(
                    quest.QuestId,
                    current.RowId,
                    quest.Name,
                    kind,
                    false,
                    questLocation,
                    quest.NpcName));
            }

            var markersByMapId = markers
                .GroupBy(marker => marker.Location.MapId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<MapOverlayMarker>)group.ToArray());

            var mapSheet = dataManager.GetExcelSheet<Map>();
            var identities = new List<MapIdentity>();
            foreach (var mapId in markersByMapId.Keys)
            {
                if (!mapSheet.TryGetRow(mapId, out var map))
                    continue;

                var placeName = map.PlaceName.IsValid ? map.PlaceName.Value.Name.ToString() : string.Empty;
                var subName = map.PlaceNameSub.IsValid ? map.PlaceNameSub.Value.Name.ToString() : string.Empty;
                var regionName = map.PlaceNameRegion.IsValid ? map.PlaceNameRegion.Value.Name.ToString() : string.Empty;
                identities.Add(new MapIdentity(
                    mapId,
                    NormalizeMapLabel(placeName),
                    NormalizeMapLabel(subName),
                    NormalizeMapLabel(regionName),
                    NormalizeMapLabel(GetMapName(map, placeName))));
            }

            log.Information(
                "Overlay preparado para {MapCount} mapas con {MarkerCount} marcadores estáticos.",
                markersByMapId.Count,
                markers.Count);
            return new OverlayIndexes(markersByMapId, identities);
        }
        catch (Exception exception)
        {
            log.Error(exception, "No se pudieron construir los índices del overlay.");
            return new OverlayIndexes(
                new Dictionary<uint, IReadOnlyList<MapOverlayMarker>>(),
                []);
        }
    }

    private LocationIndexes BuildLocationIndexes()
    {
        try
        {
            var activeCurrents = GetActiveCurrents();
            var explorationIds = activeCurrents
                .Where(current => !current.Quest.IsValid)
                .Select(current => current.RowId)
                .ToHashSet();
            var questNpcIds = questByCurrentId.Values
                .Select(quest => quest.IssuerStart.RowId)
                .Where(npcId => npcId != 0)
                .ToHashSet();

            var eObjToCurrent = new Dictionary<uint, uint>();
            foreach (var eObj in dataManager.GetExcelSheet<EObj>())
            {
                if (explorationIds.Contains(eObj.Data.RowId))
                    eObjToCurrent.TryAdd(eObj.RowId, eObj.Data.RowId);
            }

            var byCurrentId = new Dictionary<uint, AetherCurrentLocation>();
            var mutableByNpcId = new Dictionary<uint, List<AetherCurrentLocation>>();
            foreach (var level in dataManager.GetExcelSheet<Level>())
            {
                var isCurrentLocation = eObjToCurrent.TryGetValue(level.Object.RowId, out var currentId);
                var isNpcLocation = questNpcIds.Contains(level.Object.RowId);
                if ((!isCurrentLocation && !isNpcLocation) || CreateLocation(level) is not { } location)
                    continue;

                if (isCurrentLocation)
                    byCurrentId.TryAdd(currentId, location);

                if (isNpcLocation)
                {
                    if (!mutableByNpcId.TryGetValue(level.Object.RowId, out var locations))
                        mutableByNpcId[level.Object.RowId] = locations = [];
                    locations.Add(location);
                }
            }

            var byNpcId = mutableByNpcId.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<AetherCurrentLocation>)pair.Value);
            log.Information(
                "Se indexaron {CurrentLocationCount} corrientes de exploración y {NpcCount} NPC de misiones.",
                byCurrentId.Count,
                byNpcId.Count);
            return new LocationIndexes(byCurrentId, byNpcId);
        }
        catch (Exception exception)
        {
            log.Error(exception, "No se pudieron construir los índices de ubicaciones.");
            return new LocationIndexes(
                new Dictionary<uint, AetherCurrentLocation>(),
                new Dictionary<uint, IReadOnlyList<AetherCurrentLocation>>());
        }
    }

    private AetherCurrentQuestInfo CreateQuestInfo(Quest quest, bool readCompletion = true)
    {
        var name = quest.Name.ToString();
        if (string.IsNullOrWhiteSpace(name))
            name = "Misión de Aether Current";

        var npcId = quest.IssuerStart.RowId;
        string? npcName = null;
        var hasResident = dataManager.GetExcelSheet<ENpcResident>().TryGetRow(npcId, out var resident);
        var hasBase = dataManager.GetExcelSheet<ENpcBase>().TryGetRow(npcId, out _);
        if (hasResident && hasBase)
        {
            var singular = resident.Singular.ToString();
            if (!string.IsNullOrWhiteSpace(singular))
                npcName = singular;
        }

        var (status, location, candidateCount) = npcName is null
            ? (QuestLocationStatus.Unavailable, null, 0)
            : ResolveQuestLocation(quest, npcId);

        return new AetherCurrentQuestInfo(
            quest.RowId,
            name,
            GetQuestType(quest),
            readCompletion && unlockState.IsQuestCompleted(quest),
            npcId,
            npcName,
            status,
            location,
            candidateCount);
    }

    private (QuestLocationStatus Status, AetherCurrentLocation? Location, int CandidateCount)
        ResolveQuestLocation(Quest quest, uint npcId)
    {
        locationsByNpcId.TryGetValue(npcId, out var candidates);
        var candidateCount = candidates?.Count ?? 0;

        if (quest.IssuerLocation.IsValid)
        {
            var issuerLevel = quest.IssuerLocation.Value;
            if (issuerLevel.Object.RowId == npcId && CreateLocation(issuerLevel) is { } directLocation)
                return (QuestLocationStatus.Resolved, directLocation, candidateCount);
        }

        return candidateCount switch
        {
            1 => (QuestLocationStatus.Resolved, candidates![0], candidateCount),
            > 1 => (QuestLocationStatus.Ambiguous, null, candidateCount),
            _ => (QuestLocationStatus.Unavailable, null, candidateCount),
        };
    }

    private static AetherCurrentQuestType GetQuestType(Quest quest)
    {
        if (!quest.JournalGenre.IsValid || !quest.JournalGenre.Value.JournalCategory.IsValid)
            return AetherCurrentQuestType.Unknown;

        return quest.JournalGenre.Value.JournalCategory.Value.SeparateType switch
        {
            1 => AetherCurrentQuestType.MainScenario,
            3 => AetherCurrentQuestType.SideQuest,
            _ => AetherCurrentQuestType.Unknown,
        };
    }

    private static AetherCurrentLocation? CreateLocation(Level level)
    {
        if (!level.Territory.IsValid || !level.Map.IsValid)
            return null;

        var territory = level.Territory.Value;
        var map = level.Map.Value;
        var worldPosition = new Vector3(level.X, level.Y, level.Z);
        var mapPosition = MapUtil.WorldToMap(new Vector2(level.X, level.Z), map);
        var territoryName = GetZoneName(territory);

        return new AetherCurrentLocation(
            territory.RowId,
            map.RowId,
            territoryName,
            GetMapName(map, territoryName),
            worldPosition,
            mapPosition);
    }

    private static string GetMapName(Map map, string fallback)
    {
        var placeName = map.PlaceName.IsValid ? map.PlaceName.Value.Name.ToString() : string.Empty;
        var subName = map.PlaceNameSub.IsValid ? map.PlaceNameSub.Value.Name.ToString() : string.Empty;

        if (!string.IsNullOrWhiteSpace(subName) && !string.Equals(placeName, subName, StringComparison.Ordinal))
            return string.IsNullOrWhiteSpace(placeName) ? subName : $"{placeName} — {subName}";
        return string.IsNullOrWhiteSpace(placeName) ? fallback : placeName;
    }

    private void LogQuestCoverage()
    {
        var questCurrents = questByCurrentId.Values
            .Select(quest => CreateQuestInfo(quest, false))
            .ToArray();

        log.Information(
            "Cobertura de misiones: {Resolved} resueltas, {Ambiguous} ambiguas y {Unavailable} sin ubicación ({Total} total).",
            questCurrents.Count(quest => quest.LocationStatus == QuestLocationStatus.Resolved),
            questCurrents.Count(quest => quest.LocationStatus == QuestLocationStatus.Ambiguous),
            questCurrents.Count(quest => quest.LocationStatus == QuestLocationStatus.Unavailable),
            questCurrents.Length);
    }

    private AetherCurrent[] GetActiveCurrents() =>
        dataManager.GetExcelSheet<TerritoryType>()
            .Where(territory => territory.ExVersion.RowId != 0 && territory.AetherCurrentCompFlgSet.IsValid)
            .SelectMany(territory => territory.AetherCurrentCompFlgSet.Value.AetherCurrents)
            .Where(current => current.IsValid)
            .Select(current => current.Value)
            .DistinctBy(current => current.RowId)
            .ToArray();

    private IReadOnlyDictionary<uint, Quest> BuildQuestAssociations()
    {
        var questSheet = dataManager.GetExcelSheet<Quest>();
        var activeQuestCurrents = GetActiveCurrents()
            .Where(current => current.Quest.IsValid)
            .ToArray();
        var candidates = new List<(uint CurrentId, Quest Quest, bool Corrected)>();
        var invalidCount = 0;

        foreach (var current in activeQuestCurrents)
        {
            var directQuest = current.Quest.Value;
            if (directQuest.OtherReward.RowId == AetherCurrentQuestRewardId)
            {
                candidates.Add((current.RowId, directQuest, false));
                continue;
            }

            if (!QuestCorrections.TryGetValue(current.RowId, out var correctedQuestId) ||
                !questSheet.TryGetRow(correctedQuestId, out var correctedQuest) ||
                correctedQuest.OtherReward.RowId != AetherCurrentQuestRewardId)
            {
                invalidCount++;
                log.Error(
                    "AsociaciÃ³n de Aether Current invÃ¡lida: Current {CurrentId} referencia Quest {QuestId} ({QuestName}), que no concede una corriente.",
                    current.RowId,
                    directQuest.RowId,
                    directQuest.Name.ToString());
                continue;
            }

            candidates.Add((current.RowId, correctedQuest, true));
            log.Warning(
                "AsociaciÃ³n de Aether Current corregida: Current {CurrentId}, Quest {OldQuestId} ({OldQuestName}) -> Quest {QuestId} ({QuestName}).",
                current.RowId,
                directQuest.RowId,
                directQuest.Name.ToString(),
                correctedQuest.RowId,
                correctedQuest.Name.ToString());
        }

        var ambiguousQuestIds = candidates
            .GroupBy(candidate => candidate.Quest.RowId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();
        foreach (var ambiguousQuestId in ambiguousQuestIds)
        {
            log.Error(
                "AsociaciÃ³n de Aether Current ambigua: Quest {QuestId} estÃ¡ enlazada a mÃ¡s de una corriente.",
                ambiguousQuestId);
        }

        var associations = candidates
            .Where(candidate => !ambiguousQuestIds.Contains(candidate.Quest.RowId))
            .ToDictionary(candidate => candidate.CurrentId, candidate => candidate.Quest);
        var associatedQuestIds = associations.Values.Select(quest => quest.RowId).ToHashSet();
        var unassociatedRewardQuests = questSheet
            .Where(quest => quest.OtherReward.RowId == AetherCurrentQuestRewardId &&
                            !associatedQuestIds.Contains(quest.RowId))
            .ToArray();
        foreach (var quest in unassociatedRewardQuests)
        {
            log.Error(
                "MisiÃ³n con recompensa Aether Current sin asociaciÃ³n: Quest {QuestId} ({QuestName}).",
                quest.RowId,
                quest.Name.ToString());
        }

        log.Information(
            "ValidaciÃ³n de asociaciones: {Valid} vÃ¡lidas, {Corrected} corregidas, {Invalid} invÃ¡lidas, {Ambiguous} ambiguas y {Unassociated} recompensas sin asociar.",
            associations.Count,
            candidates.Count(candidate => candidate.Corrected),
            invalidCount,
            ambiguousQuestIds.Count,
            unassociatedRewardQuests.Length);
        return associations;
    }

    private sealed record LocationIndexes(
        IReadOnlyDictionary<uint, AetherCurrentLocation> ByCurrentId,
        IReadOnlyDictionary<uint, IReadOnlyList<AetherCurrentLocation>> ByNpcId);

    private sealed record OverlayIndexes(
        IReadOnlyDictionary<uint, IReadOnlyList<MapOverlayMarker>> MarkersByMapId,
        IReadOnlyList<MapIdentity> MapIdentities);

    private sealed record OverlayCacheEntry(
        int Revision,
        IReadOnlyList<MapOverlayMarker> Markers);

    private sealed record ZoneSnapshotCacheEntry(
        int Revision,
        ZoneAetherCurrentSnapshot Snapshot);

    private sealed record NativeFlagOwnership(
        uint MarkerId,
        uint TerritoryId,
        uint MapId,
        float X,
        float Y,
        uint IconId);

    private sealed record MapIdentity(
        uint MapId,
        string PlaceName,
        string SubName,
        string RegionName,
        string CombinedName)
    {
        public int Score(IReadOnlyCollection<string> labels)
        {
            var score = 0;
            if (CombinedName.Length > 0 && labels.Contains(CombinedName))
                score += 120;
            if (PlaceName.Length > 0 && labels.Contains(PlaceName))
                score += 100;
            if (SubName.Length > 0 && labels.Contains(SubName))
                score += 20;
            if (RegionName.Length > 0 && labels.Contains(RegionName))
                score += 5;
            return score;
        }
    }

    private static string NormalizeMapLabel(string value) =>
        string.Join(' ', value.Trim().ToUpperInvariant().Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));

    private static string GetZoneName(TerritoryType territory)
    {
        if (territory.PlaceName.IsValid)
        {
            var placeName = territory.PlaceName.Value.Name.ToString();
            if (!string.IsNullOrWhiteSpace(placeName))
                return placeName;
        }

        var territoryName = territory.Name.ToString();
        return string.IsNullOrWhiteSpace(territoryName)
            ? $"Territorio {territory.RowId}"
            : territoryName;
    }

    private static string GetExpansionName(TerritoryType territory)
    {
        if (territory.ExVersion.IsValid)
        {
            var name = territory.ExVersion.Value.Name.ToString();
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return territory.ExVersion.RowId == 0 ? "A Realm Reborn" : "Expansión no disponible";
    }

    private string? GetLoadingImagePath(TerritoryType territory)
    {
        if (!territory.LoadingImage.IsValid)
            return null;

        var fileName = territory.LoadingImage.Value.FileName.ToString();
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var highResolutionPath = $"ui/loadingimage/{fileName}_hr1.tex";
        if (dataManager.FileExists(highResolutionPath))
            return highResolutionPath;

        var standardPath = $"ui/loadingimage/{fileName}.tex";
        return dataManager.FileExists(standardPath) ? standardPath : null;
    }

    private void OnLogin() => Refresh();

    private void OnLogout(int type, int code)
    {
        trackedManagedMarkerIds.Clear();
        suppressedManagedMarkerIds.Clear();
        nativeFlagOwnership = null;
        managedMarkerTrackingInitialized = false;
        InvalidateCaches();
        Current = ZoneAetherCurrentSnapshot.LoggedOut;
    }

    private void OnTerritoryChanged(uint territoryId) => Refresh();

    private void OnUnlock(RowRef rowRef) => Refresh();
}

#pragma warning restore Dalamud001
