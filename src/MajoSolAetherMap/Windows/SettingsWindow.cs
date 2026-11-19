using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using MajoSolAetherMap.Services;

namespace MajoSolAetherMap.Windows;

public sealed class SettingsWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly MapOverlayRenderer mapOverlayRenderer;
    private IDisposable? themeScope;

    public SettingsWindow(
        Configuration configuration,
        MapOverlayRenderer mapOverlayRenderer)
        : base("Configuración · Aether Map##MajoSolAetherMapSettings")
    {
        this.configuration = configuration;
        this.mapOverlayRenderer = mapOverlayRenderer;
        Size = new Vector2(390, 430);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 360),
            MaximumSize = new Vector2(560, 700),
        };
    }

    public void Dispose()
    {
    }

    public override void PreDraw() => themeScope = AetherMapTheme.PushWindowStyle();

    public override void PostDraw()
    {
        themeScope?.Dispose();
        themeScope = null;
    }

    public override void Draw()
    {
        AetherMapTheme.IconInline(FontAwesomeIcon.Cog, AetherMapTheme.Turquoise);
        ImGui.TextColored(AetherMapTheme.Text, T("Configuración", "Settings"));
        ImGui.TextColored(AetherMapTheme.TextSecondary,
            T("Preferencias persistentes del mapa y la interfaz.", "Persistent map and interface preferences."));
        ImGui.Spacing();

        using (AetherMapTheme.BeginCard("LanguageSettings", 76))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T("IDIOMA", "LANGUAGE"));
            var languageLabel = configuration.Language == InterfaceLanguage.Spanish ? "Español" : "English";
            ImGui.SetNextItemWidth(MathF.Min(180f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.BeginCombo("##Language", languageLabel))
            {
                if (ImGui.Selectable("Español", configuration.Language == InterfaceLanguage.Spanish))
                {
                    configuration.Language = InterfaceLanguage.Spanish;
                    configuration.Save();
                }
                if (ImGui.Selectable("English", configuration.Language == InterfaceLanguage.English))
                {
                    configuration.Language = InterfaceLanguage.English;
                    configuration.Save();
                }
                ImGui.EndCombo();
            }
        }
        ImGui.Spacing();

        using (AetherMapTheme.BeginCard("OverlaySettings", 220))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T("OVERLAY DEL MAPA", "MAP OVERLAY"));
            ImGui.Spacing();
            DrawCheckbox(T("Activar overlay", "Enable overlay"), configuration.OverlayEnabled, value => configuration.OverlayEnabled = value);
            DrawCheckbox(T("Mostrar corrientes pendientes", "Show pending currents"), configuration.ShowPendingCurrents, value => configuration.ShowPendingCurrents = value);
            DrawCheckbox(T("Mostrar corrientes obtenidas", "Show obtained currents"), configuration.ShowObtainedCurrents, value => configuration.ShowObtainedCurrents = value);
            DrawCheckbox(T("Mostrar NPC de misiones", "Show quest NPCs"), configuration.ShowQuestNpcs, value => configuration.ShowQuestNpcs = value);

            var markerSize = configuration.MarkerSize;
            ImGui.SetNextItemWidth(MathF.Min(220f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.SliderFloat(T("Tamaño", "Size"), ref markerSize, 8f, 40f, "%.0f px"))
            {
                configuration.MarkerSize = markerSize;
                configuration.Save();
            }

            var opacity = configuration.MarkerOpacity * 100f;
            ImGui.SetNextItemWidth(MathF.Min(220f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.SliderFloat(T("Opacidad", "Opacity"), ref opacity, 20f, 100f, "%.0f%%"))
            {
                configuration.MarkerOpacity = opacity / 100f;
                configuration.Save();
            }

        }

        ImGui.Spacing();
        using (AetherMapTheme.BeginCard("AdvancedSettings", configuration.DebugMode ? 112 : 72))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T("AVANZADO", "ADVANCED"));
            DrawCheckbox(T("Modo Debug", "Debug mode"), configuration.DebugMode, value => configuration.DebugMode = value);
            if (configuration.DebugMode)
            {
                var status = mapOverlayRenderer.ViewedMapId == 0
                    ? $"{T("Mapa visible sin resolver", "Visible map unresolved")}: {mapOverlayRenderer.ViewedMapLabel}"
                    : $"{T("Mapa visible", "Visible map")}: {mapOverlayRenderer.ViewedMapId} · {mapOverlayRenderer.ViewedMapLabel}";
                AetherMapTheme.TruncatedText(status, ImGui.GetContentRegionAvail().X, AetherMapTheme.TextSecondary);
            }
        }

        ImGui.Spacing();
        ImGui.TextColored(AetherMapTheme.TextSecondary, "Lumina · Dalamud API 15 · Lectura no invasiva");
    }

    private string T(string spanish, string english) =>
        Localization.Text(configuration.Language, spanish, english);

    private void DrawCheckbox(string label, bool currentValue, Action<bool> setter)
    {
        var value = currentValue;
        if (!ImGui.Checkbox(label, ref value))
            return;

        setter(value);
        configuration.Save();
    }
}
