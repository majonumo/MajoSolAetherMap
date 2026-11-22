using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MajoSolAetherMap.Models;
using MajoSolAetherMap.Services;

namespace MajoSolAetherMap.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private static readonly string DisplayVersion =
        typeof(Plugin).Assembly.GetName().Version?.ToString(3) ?? "0.6.0";
    private readonly AetherCurrentService aetherCurrentService;
    private readonly ITextureProvider textureProvider;
    private readonly Configuration configuration;
    private readonly Action openSettings;
    private readonly Action openAbout;
    private IDisposable? themeScope;
    private string? cachedLoadingImagePath;
    private ISharedImmediateTexture? cachedLoadingImage;

    public MainWindow(
        AetherCurrentService aetherCurrentService,
        ITextureProvider textureProvider,
        Configuration configuration,
        Action openSettings,
        Action openAbout)
        : base("Majo & Sol Aether Map##MajoSolAetherMapMainWindow",
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.aetherCurrentService = aetherCurrentService;
        this.textureProvider = textureProvider;
        this.configuration = configuration;
        this.openSettings = openSettings;
        this.openAbout = openAbout;
        Size = new Vector2(420, 620);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 480),
            MaximumSize = new Vector2(760, 1000),
        };
    }

    public void Dispose()
    {
        cachedLoadingImage = null;
        cachedLoadingImagePath = null;
    }

    public override void PreDraw() =>
        themeScope = AetherMapTheme.PushWindowStyle(configuration.WindowBackgroundOpacity);

    public override void PostDraw()
    {
        themeScope?.Dispose();
        themeScope = null;
    }

    public override void Draw()
    {
        DrawHeader();
        ImGui.Spacing();
        var snapshot = aetherCurrentService.Current;
        if (!snapshot.IsLoggedIn)
        {
            using (AetherMapTheme.BeginCard("LoggedOut", 112,
                       backgroundOpacity: configuration.CardBackgroundOpacity))
            {
                AetherMapTheme.IconInline(FontAwesomeIcon.Compass, AetherMapTheme.Turquoise);
                ImGui.TextColored(AetherMapTheme.Text, T(UiText.WaitingForCharacter));
                ImGui.Spacing();
                AetherMapTheme.WrappedText(T(UiText.WaitingForCharacterHelp), AetherMapTheme.TextSecondary);
            }
            return;
        }

        DrawZoneSelector();
        snapshot = configuration.SelectedTerritoryId == 0
            ? snapshot
            : aetherCurrentService.GetZoneSnapshot(configuration.SelectedTerritoryId);
        DrawZoneSummary(snapshot);
        ImGui.Spacing();
        DrawProgressCards(snapshot);
        ImGui.Spacing();
        DrawObjectives(snapshot);
    }

    private void DrawHeader()
    {
        if (!ImGui.BeginTable("Header", 3, ImGuiTableFlags.SizingFixedFit))
            return;
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 34f * AetherMapTheme.Scale);
        ImGui.TableSetupColumn("Brand", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 102f * AetherMapTheme.Scale);
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        var iconCenter = ImGui.GetCursorScreenPos() + new Vector2(14f, 17f) * AetherMapTheme.Scale;
        ImGui.GetWindowDrawList().AddCircleFilled(iconCenter, 15f * AetherMapTheme.Scale,
            AetherMapTheme.ToU32(new Vector4(AetherMapTheme.Turquoise.X, AetherMapTheme.Turquoise.Y,
                AetherMapTheme.Turquoise.Z, 0.15f)));
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 6f * AetherMapTheme.Scale);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 8f * AetherMapTheme.Scale);
        AetherMapTheme.Icon(FontAwesomeIcon.Compass, AetherMapTheme.Turquoise);

        ImGui.TableSetColumnIndex(1);
        ImGui.TextColored(AetherMapTheme.Text, "MAJO & SOL");
        ImGui.TextColored(AetherMapTheme.Gold, $"AETHER MAP  ·  v{DisplayVersion}");

        ImGui.TableSetColumnIndex(2);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5f * AetherMapTheme.Scale);
        if (AetherMapTheme.IconButton("About", FontAwesomeIcon.InfoCircle, T(UiText.About)))
            openAbout();
        ImGui.SameLine(0, 4f * AetherMapTheme.Scale);
        if (AetherMapTheme.IconButton("Settings", FontAwesomeIcon.Cog, T(UiText.OpenSettings)))
            openSettings();
        ImGui.SameLine(0, 4f * AetherMapTheme.Scale);
        if (AetherMapTheme.IconButton("Close", FontAwesomeIcon.Times, T(UiText.Close)))
            IsOpen = false;
        ImGui.EndTable();
        ImGui.Separator();
    }

    private void DrawZoneSelector()
    {
        var currentLabel = T(UiText.CurrentZone);
        if (configuration.SelectedTerritoryId != 0 &&
            aetherCurrentService.AvailableZones.All(zone => zone.TerritoryId != configuration.SelectedTerritoryId))
        {
            configuration.SelectedTerritoryId = 0;
            configuration.Save();
        }
        var preview = configuration.SelectedTerritoryId == 0
            ? currentLabel
            : aetherCurrentService.AvailableZones
                .FirstOrDefault(zone => zone.TerritoryId == configuration.SelectedTerritoryId)?.ZoneName ?? currentLabel;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (!ImGui.BeginCombo("##ZoneSelector", preview))
            return;

        if (ImGui.Selectable(currentLabel, configuration.SelectedTerritoryId == 0))
        {
            configuration.SelectedTerritoryId = 0;
            configuration.Save();
        }
        foreach (var zone in aetherCurrentService.AvailableZones)
        {
            var label = $"{zone.ExpansionName} · {zone.ZoneName}";
            if (ImGui.Selectable(label, configuration.SelectedTerritoryId == zone.TerritoryId))
            {
                configuration.SelectedTerritoryId = zone.TerritoryId;
                configuration.Save();
            }
        }
        ImGui.EndCombo();
        ImGui.Spacing();
    }

    private void DrawZoneSummary(ZoneAetherCurrentSnapshot snapshot)
    {
        using (AetherMapTheme.BeginCard("ZoneSummary", 132,
                   backgroundOpacity: configuration.CardBackgroundOpacity))
        {
            DrawZoneImage(snapshot.LoadingImagePath);
            var childPosition = ImGui.GetWindowPos();
            var childSize = ImGui.GetWindowSize();
            var circleRadius = 38f * AetherMapTheme.Scale;
            var circleCenter = new Vector2(childPosition.X + childSize.X - 54f * AetherMapTheme.Scale,
                childPosition.Y + childSize.Y * 0.5f);
            var progress = snapshot.TotalCount == 0 ? 0f : (float)snapshot.UnlockedCount / snapshot.TotalCount;
            AetherMapTheme.CircularProgress(circleCenter, circleRadius, progress);

            var percentText = snapshot.TotalCount == 0 ? "—" : $"{MathF.Round(progress * 100f):0}%";
            var percentSize = ImGui.CalcTextSize(percentText);
            ImGui.GetWindowDrawList().AddText(
                circleCenter - new Vector2(percentSize.X * 0.5f, percentSize.Y * 0.75f),
                AetherMapTheme.ToU32(AetherMapTheme.Text), percentText);
            var countText = snapshot.TotalCount == 0 ? T(UiText.NoCurrents) : $"{snapshot.UnlockedCount}/{snapshot.TotalCount}";
            var countSize = ImGui.CalcTextSize(countText);
            ImGui.GetWindowDrawList().AddText(
                circleCenter + new Vector2(-countSize.X * 0.5f, 7f * AetherMapTheme.Scale),
                AetherMapTheme.ToU32(AetherMapTheme.TextSecondary), countText);

            var textWidth = MathF.Max(80f, childSize.X - 124f * AetherMapTheme.Scale);
            AetherMapTheme.TruncatedText(snapshot.ZoneName, textWidth, AetherMapTheme.Text);
            AetherMapTheme.TruncatedText(snapshot.ExpansionName, textWidth, AetherMapTheme.Gold);
            ImGui.Spacing();
            if (snapshot.TotalCount == 0)
                ImGui.TextColored(AetherMapTheme.TextSecondary,
                    T(UiText.NoCurrents));
            else if (snapshot.AreAllCurrentsUnlocked)
            {
                AetherMapTheme.IconInline(FontAwesomeIcon.CheckCircle, AetherMapTheme.Completed);
                ImGui.TextColored(AetherMapTheme.Completed, T(UiText.CurrentsCompleted));
            }
            else
            {
                ImGui.TextColored(AetherMapTheme.TextSecondary, T(UiText.PendingObjectives));
                ImGui.SameLine();
                ImGui.TextColored(AetherMapTheme.Gold, snapshot.PendingCount.ToString());
            }
            if (snapshot.IsFlightUnlocked == true)
                ImGui.TextColored(AetherMapTheme.Completed, T(UiText.FlightUnlocked));
        }
    }

    private void DrawZoneImage(string? path)
    {
        var texture = GetZoneTexture(path);
        if (texture is null)
            return;
        var wrap = texture.GetWrapOrEmpty();
        if (wrap.Width <= 1 || wrap.Height <= 1)
            return;
        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddImage(wrap.Handle, min, max, Vector2.Zero, Vector2.One,
            AetherMapTheme.ToU32(new Vector4(1f, 1f, 1f, 0.28f * configuration.CardBackgroundOpacity)));
        drawList.AddRectFilled(min, max,
            AetherMapTheme.ToU32(new Vector4(AetherMapTheme.Background.X, AetherMapTheme.Background.Y,
                AetherMapTheme.Background.Z, 0.56f * configuration.CardBackgroundOpacity)),
            9f * AetherMapTheme.Scale);
    }

    private ISharedImmediateTexture? GetZoneTexture(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        if (string.Equals(path, cachedLoadingImagePath, StringComparison.Ordinal))
            return cachedLoadingImage;
        cachedLoadingImagePath = path;
        cachedLoadingImage = textureProvider.GetFromGame(path);
        return cachedLoadingImage;
    }

    private void DrawProgressCards(ZoneAetherCurrentSnapshot snapshot)
    {
        var categories = new List<(string Id, FontAwesomeIcon Icon, string Label, int Unlocked, int Total)>(2);
        if (snapshot.ExplorationCount > 0)
            categories.Add(("ExplorationProgress", FontAwesomeIcon.Gem, T(UiText.Exploration),
                snapshot.ExplorationUnlockedCount, snapshot.ExplorationCount));
        if (snapshot.QuestCount > 0)
            categories.Add(("QuestProgress", FontAwesomeIcon.Scroll, T(UiText.Quests),
                snapshot.QuestUnlockedCount, snapshot.QuestCount));
        if (categories.Count == 0)
            return;

        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var width = categories.Count == 1 ? ImGui.GetContentRegionAvail().X
            : (ImGui.GetContentRegionAvail().X - spacing) * 0.5f;
        for (var index = 0; index < categories.Count; index++)
        {
            if (index > 0)
                ImGui.SameLine();
            var category = categories[index];
            using (AetherMapTheme.BeginCard(category.Id, 70, width: width,
                       backgroundOpacity: configuration.CardBackgroundOpacity))
            {
                AetherMapTheme.IconInline(category.Icon, AetherMapTheme.Turquoise);
                ImGui.TextColored(AetherMapTheme.TextSecondary, category.Label);
                ImGui.TextColored(AetherMapTheme.Text, $"{category.Unlocked} / {category.Total}");
                AetherMapTheme.CompactProgress((float)category.Unlocked / category.Total);
            }
        }
    }

    private void DrawObjectives(ZoneAetherCurrentSnapshot snapshot)
    {
        ImGui.TextColored(AetherMapTheme.TextSecondary, T(UiText.Objectives));
        if (snapshot.TotalCount > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(AetherMapTheme.Gold, snapshot.PendingCount.ToString());
        }
        ImGui.PushStyleColor(ImGuiCol.ChildBg,
            AetherMapTheme.WithOpacity(AetherMapTheme.Background, configuration.WindowBackgroundOpacity));
        ImGui.BeginChild("ObjectiveScroll", Vector2.Zero, false, ImGuiWindowFlags.None);
        ImGui.PopStyleColor();

        if (snapshot.TotalCount == 0)
        {
            ImGui.Spacing();
            ImGui.TextColored(AetherMapTheme.TextSecondary,
                T(UiText.NoObjectives));
            ImGui.EndChild();
            return;
        }

        var pendingCurrents = snapshot.Currents
            .Where(current => current.Kind == AetherCurrentKind.Exploration && !current.IsUnlocked).ToArray();
        var completedCurrents = snapshot.Currents
            .Where(current => current.Kind == AetherCurrentKind.Exploration && current.IsUnlocked).ToArray();
        var quests = snapshot.Currents.Where(current => current.Quest is not null)
            .Select(current => current.Quest!).ToArray();
        var pendingQuests = quests.Where(quest => !quest.IsCompleted).ToArray();
        var completedQuests = quests.Where(quest => quest.IsCompleted).ToArray();

        if (pendingCurrents.Length + pendingQuests.Length > 0)
        {
            ImGui.TextColored(AetherMapTheme.Turquoise, T(UiText.Pending));
            foreach (var current in pendingCurrents)
                DrawCurrentRow(current);
            foreach (var quest in pendingQuests)
                DrawQuestRow(quest);
        }
        else
        {
            AetherMapTheme.IconInline(FontAwesomeIcon.CheckCircle, AetherMapTheme.Completed);
            ImGui.TextColored(AetherMapTheme.Completed,
                T(UiText.AllObjectivesComplete));
        }

        var completedCount = completedCurrents.Length + completedQuests.Length;
        if (completedCount > 0)
        {
            ImGui.Spacing();
            if (ImGui.CollapsingHeader($"{T(UiText.Completed)} ({completedCount})##CompletedObjectives"))
            {
                foreach (var current in completedCurrents)
                    DrawCurrentRow(current);
                foreach (var quest in completedQuests)
                    DrawQuestRow(quest);
            }
        }
        ImGui.EndChild();
    }

    private void DrawCurrentRow(AetherCurrentInfo current)
    {
        var height = 58f + (configuration.DebugMode ? 17f : 0f);
        using (AetherMapTheme.BeginCard($"Current{current.RowId}", height,
                   backgroundOpacity: configuration.CardBackgroundOpacity))
        {
            var rowTop = ImGui.GetCursorScreenPos();
            var right = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X - 40f * AetherMapTheme.Scale;
            var hasRemovableFlag = aetherCurrentService.HasRemovableMapFlag(current.RowId);
            var removeLabel = T(UiText.RemoveMarker);
            var removeWidth = ImGui.CalcTextSize(removeLabel).X + 14f * AetherMapTheme.Scale;
            var actionsLeft = hasRemovableFlag
                ? right - removeWidth - 5f * AetherMapTheme.Scale
                : right;
            AetherMapTheme.IconInline(FontAwesomeIcon.Gem,
                current.IsUnlocked ? AetherMapTheme.Completed : AetherMapTheme.Turquoise);
            var titleWidth = MathF.Max(60f, actionsLeft - ImGui.GetCursorScreenPos().X - 6f * AetherMapTheme.Scale);
            AetherMapTheme.TruncatedText(LocalizeCurrentName(current.Name), titleWidth,
                current.IsUnlocked ? AetherMapTheme.TextSecondary : AetherMapTheme.Text);
            if (current.Location is { } location)
            {
                ImGui.TextColored(AetherMapTheme.TextSecondary,
                    $"X {location.MapPosition.X:0.0}  ·  Y {location.MapPosition.Y:0.0}");
                if (configuration.DebugMode)
                    ImGui.TextColored(AetherMapTheme.TextSecondary, $"AetherCurrent {current.RowId}");
                if (hasRemovableFlag)
                {
                    ImGui.SetCursorScreenPos(new Vector2(actionsLeft, rowTop.Y + 7f * AetherMapTheme.Scale));
                    if (ImGui.Button($"{removeLabel}##RemoveFlag{current.RowId}",
                            new Vector2(removeWidth, 26f * AetherMapTheme.Scale)))
                        aetherCurrentService.RemoveMapFlag(current.RowId);
                }
                ImGui.SetCursorScreenPos(new Vector2(right, rowTop.Y + 7f * AetherMapTheme.Scale));
                if (AetherMapTheme.IconButton($"Flag{current.RowId}", FontAwesomeIcon.Flag,
                        T(UiText.PlaceFlag)))
                    aetherCurrentService.PlaceMapFlag(current);
            }
            else if (configuration.DebugMode)
                ImGui.TextColored(AetherMapTheme.TextSecondary, $"AetherCurrent {current.RowId}");
        }
        ImGui.Spacing();
    }

    private void DrawQuestRow(AetherCurrentQuestInfo quest)
    {
        var height = 76f + (configuration.DebugMode ? 17f : 0f);
        using (AetherMapTheme.BeginCard($"Quest{quest.QuestId}", height,
                   backgroundOpacity: configuration.CardBackgroundOpacity))
        {
            var rowTop = ImGui.GetCursorScreenPos();
            var right = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X - 40f * AetherMapTheme.Scale;
            var hasRemovableFlag = aetherCurrentService.HasRemovableMapFlag(quest.QuestId);
            var removeLabel = T(UiText.RemoveMarker);
            var removeWidth = ImGui.CalcTextSize(removeLabel).X + 14f * AetherMapTheme.Scale;
            var actionsLeft = hasRemovableFlag
                ? right - removeWidth - 5f * AetherMapTheme.Scale
                : right;
            var isMainScenario = quest.Type == AetherCurrentQuestType.MainScenario;
            AetherMapTheme.IconInline(isMainScenario ? FontAwesomeIcon.ExclamationCircle : FontAwesomeIcon.Scroll,
                quest.IsCompleted ? AetherMapTheme.Completed : isMainScenario ? AetherMapTheme.Gold : AetherMapTheme.Blue);
            var titleWidth = MathF.Max(60f, actionsLeft - ImGui.GetCursorScreenPos().X - 6f * AetherMapTheme.Scale);
            AetherMapTheme.TruncatedText(quest.Name, titleWidth,
                quest.IsCompleted ? AetherMapTheme.TextSecondary : AetherMapTheme.Text);
            AetherMapTheme.TruncatedText($"NPC · {quest.NpcName ?? T(UiText.Unavailable)}", titleWidth,
                AetherMapTheme.TextSecondary);
            if (quest.LocationStatus == QuestLocationStatus.Resolved && quest.Location is { } location)
            {
                ImGui.TextColored(AetherMapTheme.TextSecondary,
                    $"X {location.MapPosition.X:0.0}  ·  Y {location.MapPosition.Y:0.0}");
                if (configuration.DebugMode)
                    ImGui.TextColored(AetherMapTheme.TextSecondary,
                        $"Quest {quest.QuestId} · ENpc {quest.NpcId} · {quest.LocationStatus}");
                if (hasRemovableFlag)
                {
                    ImGui.SetCursorScreenPos(new Vector2(actionsLeft, rowTop.Y + 16f * AetherMapTheme.Scale));
                    if (ImGui.Button($"{removeLabel}##RemoveNpcFlag{quest.QuestId}",
                            new Vector2(removeWidth, 26f * AetherMapTheme.Scale)))
                        aetherCurrentService.RemoveMapFlag(quest.QuestId);
                }
                ImGui.SetCursorScreenPos(new Vector2(right, rowTop.Y + 16f * AetherMapTheme.Scale));
                if (AetherMapTheme.IconButton($"Npc{quest.QuestId}", FontAwesomeIcon.MapMarkerAlt,
                        T(UiText.LocateNpc)))
                    aetherCurrentService.PlaceNpcFlag(quest);
            }
            else
            {
                ImGui.TextColored(AetherMapTheme.Gold, quest.LocationStatus == QuestLocationStatus.Ambiguous
                    ? T(UiText.AmbiguousLocation)
                    : T(UiText.LocationUnavailable));
                if (configuration.DebugMode)
                    ImGui.TextColored(AetherMapTheme.TextSecondary,
                        $"Quest {quest.QuestId} · ENpc {quest.NpcId} · {quest.LocationStatus}");
            }
        }
        ImGui.Spacing();
    }

    private string T(UiText key) => Localization.Text(configuration, key);

    // Exploration labels are generated by the plugin; Lumina-provided names stay untouched.
    private string LocalizeCurrentName(string name) => name.StartsWith("Corriente de exploración", StringComparison.Ordinal)
        ? $"{T(UiText.Exploration)} {name["Corriente de exploración".Length..].Trim()}"
        : name;
}
