using Dalamud.Configuration;

namespace MajoSolAetherMap;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    public bool DebugMode { get; set; }

    public bool OverlayEnabled { get; set; } = true;

    public bool ShowPendingCurrents { get; set; } = true;

    public bool ShowObtainedCurrents { get; set; } = true;

    public bool ShowQuestNpcs { get; set; } = true;

    public float MarkerSize { get; set; } = 18f;

    public float MarkerOpacity { get; set; } = 0.9f;

    public InterfaceLanguage Language { get; set; } = InterfaceLanguage.English;

    public bool FollowClientLanguage { get; set; } = true;

    public float WindowBackgroundOpacity { get; set; } = 1f;

    public float CardBackgroundOpacity { get; set; } = 1f;

    // Zero follows the character's current territory.
    public uint SelectedTerritoryId { get; set; }

    public bool Upgrade()
    {
        var changed = false;
        if (Version < 2)
        {
            // v1 only offered explicit Spanish/English selection; preserve that choice.
            FollowClientLanguage = false;
            Version = 2;
            changed = true;
        }

        if (!Localization.IsSupported(Language))
        {
            Language = InterfaceLanguage.English;
            changed = true;
        }

        var windowOpacity = System.Math.Clamp(WindowBackgroundOpacity, 0.3f, 1f);
        var cardOpacity = System.Math.Clamp(CardBackgroundOpacity, 0.3f, 1f);
        if (windowOpacity != WindowBackgroundOpacity || cardOpacity != CardBackgroundOpacity)
        {
            WindowBackgroundOpacity = windowOpacity;
            CardBackgroundOpacity = cardOpacity;
            changed = true;
        }

        return changed;
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
