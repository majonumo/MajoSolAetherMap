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

    public SettingsWindow(Configuration configuration, MapOverlayRenderer mapOverlayRenderer)
        : base("Settings · Aether Map##MajoSolAetherMapSettings")
    {
        this.configuration = configuration;
        this.mapOverlayRenderer = mapOverlayRenderer;
        Size = new Vector2(430, 650);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(360, 500),
            MaximumSize = new Vector2(640, 900),
        };
    }

    public void Dispose()
    {
    }

    public override void PreDraw()
    {
        WindowName = $"{T(UiText.Settings)} · Aether Map##MajoSolAetherMapSettings";
        themeScope = AetherMapTheme.PushWindowStyle(configuration.WindowBackgroundOpacity);
    }

    public override void PostDraw()
    {
        themeScope?.Dispose();
        themeScope = null;
    }

    public override void Draw()
    {
        AetherMapTheme.IconInline(FontAwesomeIcon.Cog, AetherMapTheme.Turquoise);
        ImGui.TextColored(AetherMapTheme.Text, T(UiText.Settings));
        AetherMapTheme.WrappedText(T(UiText.InterfaceDescription), AetherMapTheme.TextSecondary);
        ImGui.Spacing();

        using (Card("LanguageSettings", 76))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T(UiText.Language));
            var effective = Localization.EffectiveLanguage(configuration);
            var languageLabel = configuration.FollowClientLanguage
                ? $"{T(UiText.Automatic)} · {Localization.LanguageName(effective)}"
                : Localization.LanguageName(configuration.Language);
            ImGui.SetNextItemWidth(MathF.Min(280f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.BeginCombo("##Language", languageLabel))
            {
                if (ImGui.Selectable(T(UiText.Automatic), configuration.FollowClientLanguage))
                {
                    configuration.FollowClientLanguage = true;
                    configuration.Save();
                }

                foreach (var language in Localization.SupportedLanguages)
                {
                    if (!ImGui.Selectable(Localization.LanguageName(language),
                            !configuration.FollowClientLanguage && configuration.Language == language))
                        continue;

                    configuration.Language = language;
                    configuration.FollowClientLanguage = false;
                    configuration.Save();
                }
                ImGui.EndCombo();
            }
        }
        ImGui.Spacing();

        using (Card("AppearanceSettings", 154))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T(UiText.Appearance));
            ImGui.Spacing();
            DrawOpacitySlider(UiText.WindowBackgroundOpacity, configuration.WindowBackgroundOpacity,
                value => configuration.WindowBackgroundOpacity = value);
            DrawOpacitySlider(UiText.CardBackgroundOpacity, configuration.CardBackgroundOpacity,
                value => configuration.CardBackgroundOpacity = value);
            if (ImGui.Button(T(UiText.ResetAppearance)))
            {
                configuration.WindowBackgroundOpacity = 1f;
                configuration.CardBackgroundOpacity = 1f;
                configuration.Save();
            }
        }
        ImGui.Spacing();

        using (Card("OverlaySettings", 220))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T(UiText.MapOverlay));
            ImGui.Spacing();
            DrawCheckbox(T(UiText.EnableOverlay), configuration.OverlayEnabled, value => configuration.OverlayEnabled = value);
            DrawCheckbox(T(UiText.ShowPendingCurrents), configuration.ShowPendingCurrents, value => configuration.ShowPendingCurrents = value);
            DrawCheckbox(T(UiText.ShowObtainedCurrents), configuration.ShowObtainedCurrents, value => configuration.ShowObtainedCurrents = value);
            DrawCheckbox(T(UiText.ShowQuestNpcs), configuration.ShowQuestNpcs, value => configuration.ShowQuestNpcs = value);

            var markerSize = configuration.MarkerSize;
            ImGui.SetNextItemWidth(MathF.Min(220f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.SliderFloat(T(UiText.MarkerSize), ref markerSize, 8f, 40f, "%.0f px"))
            {
                configuration.MarkerSize = markerSize;
                configuration.Save();
            }

            var opacity = configuration.MarkerOpacity * 100f;
            ImGui.SetNextItemWidth(MathF.Min(220f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
            if (ImGui.SliderFloat(T(UiText.MarkerOpacity), ref opacity, 20f, 100f, "%.0f%%"))
            {
                configuration.MarkerOpacity = opacity / 100f;
                configuration.Save();
            }
        }

        ImGui.Spacing();
        using (Card("AdvancedSettings", configuration.DebugMode ? 112 : 72))
        {
            ImGui.TextColored(AetherMapTheme.Gold, T(UiText.Advanced));
            DrawCheckbox(T(UiText.DebugMode), configuration.DebugMode, value => configuration.DebugMode = value);
            if (configuration.DebugMode)
            {
                var status = mapOverlayRenderer.ViewedMapId == 0
                    ? $"{T(UiText.VisibleMapUnresolved)}: {mapOverlayRenderer.ViewedMapLabel}"
                    : $"{T(UiText.VisibleMap)}: {mapOverlayRenderer.ViewedMapId} · {mapOverlayRenderer.ViewedMapLabel}";
                AetherMapTheme.TruncatedText(status, ImGui.GetContentRegionAvail().X, AetherMapTheme.TextSecondary);
            }
        }

        ImGui.Spacing();
        ImGui.TextColored(AetherMapTheme.TextSecondary, T(UiText.TechnicalFooter));
    }

    private IDisposable Card(string id, float height) => AetherMapTheme.BeginCard(
        id,
        height,
        backgroundOpacity: configuration.CardBackgroundOpacity);

    private string T(UiText key) => Localization.Text(configuration, key);

    private void DrawOpacitySlider(UiText label, float setting, Action<float> setter)
    {
        var percentage = setting * 100f;
        ImGui.TextColored(AetherMapTheme.TextSecondary, T(label));
        ImGui.SetNextItemWidth(MathF.Min(250f * AetherMapTheme.Scale, ImGui.GetContentRegionAvail().X));
        if (!ImGui.SliderFloat($"##{label}", ref percentage, 30f, 100f, "%.0f%%"))
            return;

        setter(percentage / 100f);
        configuration.Save();
    }

    private void DrawCheckbox(string label, bool currentValue, Action<bool> setter)
    {
        var value = currentValue;
        if (!ImGui.Checkbox(label, ref value))
            return;

        setter(value);
        configuration.Save();
    }
}
