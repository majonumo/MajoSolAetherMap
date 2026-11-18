using Dalamud.Configuration;

namespace MajoSolAetherMap;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool DebugMode { get; set; }

    public bool OverlayEnabled { get; set; } = true;

    public bool ShowPendingCurrents { get; set; } = true;

    public bool ShowObtainedCurrents { get; set; } = true;

    public bool ShowQuestNpcs { get; set; } = true;

    public float MarkerSize { get; set; } = 18f;

    public float MarkerOpacity { get; set; } = 0.9f;

    public InterfaceLanguage Language { get; set; } = InterfaceLanguage.Spanish;

    // Zero follows the character's current territory.
    public uint SelectedTerritoryId { get; set; }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
