using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MajoSolAetherMap.Models;

namespace MajoSolAetherMap.Services;

/// <summary>
/// Read-only ImGui overlay for the native AreaMap addon. It never changes native
/// marker arrays, AgentMap, addon nodes, or game memory.
/// </summary>
public sealed class MapOverlayRenderer
{
    private const float MapCoordinateToTexturePixel = 50f;
    private const float NativeMapTextureSize = 2048f;

    private readonly IGameGui gameGui;
    private readonly AetherCurrentService aetherCurrentService;
    private readonly Configuration configuration;
    private readonly IPluginLog log;
    private string lastUnresolvedIdentity = string.Empty;
    private string lastIdentityKey = string.Empty;
    private uint lastResolvedMapId;
    private bool lastIdentityResolved;

    public MapOverlayRenderer(
        IGameGui gameGui,
        AetherCurrentService aetherCurrentService,
        Configuration configuration,
        IPluginLog log)
    {
        this.gameGui = gameGui;
        this.aetherCurrentService = aetherCurrentService;
        this.configuration = configuration;
        this.log = log;
    }

    public uint ViewedMapId { get; private set; }

    public string ViewedMapLabel { get; private set; } = string.Empty;

    public unsafe void Draw()
    {
        ViewedMapId = 0;
        ViewedMapLabel = string.Empty;

        if (!configuration.OverlayEnabled)
            return;

        var addon = gameGui.GetAddonByName<AddonAreaMap>("AreaMap");
        if (addon == null || !addon->IsReady || !addon->IsVisible)
            return;

        var component = addon->ComponentMap;
        if (component == null || component->AtkResNode == null || component->BaseMapImage == null)
            return;

        var areaText = ReadText(addon->AreaTextNode);
        var subAreaText = ReadText(addon->SubAreaTextNode);
        var titleText = ReadText(addon->TitleTextNode);
        ViewedMapLabel = string.Join(" · ", new[] { areaText, subAreaText, titleText }
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.Ordinal));

        var identityKey = string.Concat(areaText, "\u001F", subAreaText, "\u001F", titleText);
        uint mapId;
        if (!string.Equals(identityKey, lastIdentityKey, StringComparison.Ordinal))
        {
            lastIdentityKey = identityKey;
            lastIdentityResolved = aetherCurrentService.TryResolveOverlayMap(
                areaText, subAreaText, titleText, out lastResolvedMapId);
        }
        mapId = lastResolvedMapId;
        if (!lastIdentityResolved)
        {
            LogUnresolvedIdentity(ViewedMapLabel);
            return;
        }

        ViewedMapId = mapId;
        lastUnresolvedIdentity = string.Empty;
        var markers = aetherCurrentService.GetOverlayMarkers(mapId);
        if (markers.Count == 0)
            return;

        var clipBounds = GetScreenBounds(component->AtkResNode);
        if (clipBounds.Max.X <= clipBounds.Min.X || clipBounds.Max.Y <= clipBounds.Min.Y)
            return;

        var baseMapNode = &component->BaseMapImage->AtkResNode;
        var drawList = ImGui.GetForegroundDrawList();
        var markerSize = Math.Clamp(configuration.MarkerSize, 8f, 40f);
        var mousePosition = ImGui.GetMousePos();
        MapOverlayMarker? hoveredMarker = null;
        var hoveredDistance = float.MaxValue;

        drawList.PushClipRect(clipBounds.Min, clipBounds.Max, true);
        foreach (var marker in markers)
        {
            if (!ShouldShow(marker))
                continue;

            var point = MapToScreen(marker.Location.MapPosition, baseMapNode);
            var radius = markerSize * 0.5f;
            if (point.X + radius < clipBounds.Min.X || point.X - radius > clipBounds.Max.X ||
                point.Y + radius < clipBounds.Min.Y || point.Y - radius > clipBounds.Max.Y)
            {
                continue;
            }

            DrawMarker(drawList, marker, point, radius);

            var distance = Vector2.DistanceSquared(mousePosition, point);
            if (distance <= radius * radius && distance < hoveredDistance)
            {
                hoveredMarker = marker;
                hoveredDistance = distance;
            }
        }

        drawList.PopClipRect();
        if (hoveredMarker is not null && IsInside(mousePosition, clipBounds))
            DrawTooltip(hoveredMarker);
    }

    private bool ShouldShow(MapOverlayMarker marker) => marker.Kind switch
    {
        MapOverlayMarkerKind.ExplorationCurrent when marker.IsCompleted => configuration.ShowObtainedCurrents,
        MapOverlayMarkerKind.ExplorationCurrent => configuration.ShowPendingCurrents,
        _ => configuration.ShowQuestNpcs,
    };

    private void DrawMarker(ImDrawListPtr drawList, MapOverlayMarker marker, Vector2 center, float radius)
    {
        var alpha = Math.Clamp(configuration.MarkerOpacity, 0.2f, 1f);
        var outline = Color(0.04f, 0.08f, 0.12f, alpha);
        switch (marker.Kind)
        {
            case MapOverlayMarkerKind.ExplorationCurrent when !marker.IsCompleted:
                drawList.AddQuadFilled(
                    center + new Vector2(0, -radius),
                    center + new Vector2(radius * 0.72f, 0),
                    center + new Vector2(0, radius),
                    center + new Vector2(-radius * 0.72f, 0),
                    Color(0.24f, 0.92f, 0.86f, alpha));
                drawList.AddQuad(
                    center + new Vector2(0, -radius),
                    center + new Vector2(radius * 0.72f, 0),
                    center + new Vector2(0, radius),
                    center + new Vector2(-radius * 0.72f, 0),
                    outline,
                    1.5f);
                drawList.AddLine(center + new Vector2(0, -radius * 0.58f), center, Color(0.85f, 1f, 1f, alpha), 1.3f);
                break;

            case MapOverlayMarkerKind.ExplorationCurrent:
                drawList.AddCircleFilled(center, radius * 0.72f, Color(0.32f, 0.78f, 0.48f, alpha), 16);
                drawList.AddCircle(center, radius * 0.72f, outline, 16, 1.5f);
                drawList.AddLine(center + new Vector2(-radius * 0.32f, 0), center + new Vector2(-radius * 0.08f, radius * 0.25f), Color(1f, 1f, 1f, alpha), 1.4f);
                drawList.AddLine(center + new Vector2(-radius * 0.08f, radius * 0.25f), center + new Vector2(radius * 0.38f, -radius * 0.3f), Color(1f, 1f, 1f, alpha), 1.4f);
                break;

            case MapOverlayMarkerKind.MainScenarioNpc:
                drawList.AddQuadFilled(
                    center + new Vector2(0, -radius),
                    center + new Vector2(radius, 0),
                    center + new Vector2(0, radius),
                    center + new Vector2(-radius, 0),
                    Color(0.95f, 0.74f, 0.20f, alpha));
                drawList.AddCircleFilled(center, radius * 0.42f, Color(0.14f, 0.32f, 0.62f, alpha), 14);
                drawList.AddQuad(center + new Vector2(0, -radius), center + new Vector2(radius, 0), center + new Vector2(0, radius), center + new Vector2(-radius, 0), outline, 1.5f);
                break;

            default:
                drawList.AddCircleFilled(center, radius * 0.82f, Color(0.18f, 0.48f, 0.86f, alpha), 18);
                drawList.AddCircle(center, radius * 0.82f, Color(0.95f, 0.74f, 0.20f, alpha), 18, 2f);
                drawList.AddCircleFilled(center, radius * 0.25f, Color(1f, 0.92f, 0.58f, alpha), 12);
                break;
        }
    }

    private void DrawTooltip(MapOverlayMarker marker)
    {
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(marker.Kind == MapOverlayMarkerKind.ExplorationCurrent
            ? marker.Name
            : marker.NpcName ?? T("NPC no disponible", "NPC unavailable"));
        if (marker.Kind != MapOverlayMarkerKind.ExplorationCurrent)
        {
            ImGui.Separator();
            ImGui.TextWrapped(marker.Name);
            ImGui.TextDisabled(marker.Kind == MapOverlayMarkerKind.MainScenarioNpc
                ? T("Historia principal", "Main scenario")
                : T("Misión secundaria", "Side quest"));
        }

        ImGui.TextDisabled(marker.IsCompleted
            ? T("Completada / obtenida", "Completed / obtained")
            : T("Pendiente", "Pending"));
        ImGui.TextDisabled($"X {marker.Location.MapPosition.X:0.0} · Y {marker.Location.MapPosition.Y:0.0}");
        ImGui.EndTooltip();
    }

    private string T(string spanish, string english) =>
        Localization.Text(configuration.Language, spanish, english);

    private static unsafe string ReadText(AtkTextNode* node) =>
        node == null ? string.Empty : node->NodeText.ToString();

    private static unsafe Vector2 MapToScreen(Vector2 mapPosition, AtkResNode* baseMapNode)
    {
        var texturePoint = (mapPosition - Vector2.One) * MapCoordinateToTexturePixel;
        var localPoint = new Vector2(
            texturePoint.X / NativeMapTextureSize * baseMapNode->Width,
            texturePoint.Y / NativeMapTextureSize * baseMapNode->Height);
        return TransformPoint(baseMapNode, localPoint);
    }

    private static unsafe ScreenBounds GetScreenBounds(AtkResNode* node)
    {
        var points = new[]
        {
            TransformPoint(node, Vector2.Zero),
            TransformPoint(node, new Vector2(node->Width, 0)),
            TransformPoint(node, new Vector2(0, node->Height)),
            TransformPoint(node, new Vector2(node->Width, node->Height)),
        };
        return new ScreenBounds(
            new Vector2(points.Min(point => point.X), points.Min(point => point.Y)),
            new Vector2(points.Max(point => point.X), points.Max(point => point.Y)));
    }

    private static unsafe Vector2 TransformPoint(AtkResNode* node, Vector2 localPoint) =>
        new(
            node->ScreenX + localPoint.X * node->Transform.M11 + localPoint.Y * node->Transform.M21,
            node->ScreenY + localPoint.X * node->Transform.M12 + localPoint.Y * node->Transform.M22);

    private static bool IsInside(Vector2 point, ScreenBounds bounds) =>
        point.X >= bounds.Min.X && point.X <= bounds.Max.X &&
        point.Y >= bounds.Min.Y && point.Y <= bounds.Max.Y;

    private static uint Color(float red, float green, float blue, float alpha) =>
        ImGui.ColorConvertFloat4ToU32(new Vector4(red, green, blue, alpha));

    private void LogUnresolvedIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity) || identity == lastUnresolvedIdentity)
            return;

        lastUnresolvedIdentity = identity;
        log.Debug("Overlay omitido: el mapa visible no se pudo resolver de forma única ({Identity}).", identity);
    }

    private readonly record struct ScreenBounds(Vector2 Min, Vector2 Max);
}
