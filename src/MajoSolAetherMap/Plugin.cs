using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MajoSolAetherMap.Windows;
using MajoSolAetherMap.Services;

namespace MajoSolAetherMap;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/aethermap";

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

#pragma warning disable Dalamud001
    [PluginService]
    internal static IUnlockState UnlockState { get; private set; } = null!;
#pragma warning restore Dalamud001

    private readonly WindowSystem windowSystem = new("MajoSolAetherMap");
    private readonly AetherCurrentService aetherCurrentService;
    private readonly MapOverlayRenderer mapOverlayRenderer;
    private readonly MainWindow mainWindow;
    private readonly SettingsWindow settingsWindow;
    private readonly AboutWindow aboutWindow;

    internal Configuration Configuration { get; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        if (Configuration.Upgrade())
            Configuration.Save();
        aetherCurrentService = new AetherCurrentService(
            DataManager,
            ClientState,
            UnlockState,
            Framework,
            GameGui,
            Log);
        mapOverlayRenderer = new MapOverlayRenderer(
            GameGui,
            aetherCurrentService,
            Configuration,
            Log);
        settingsWindow = new SettingsWindow(Configuration, mapOverlayRenderer);
        aboutWindow = new AboutWindow(Configuration, TextureProvider);
        mainWindow = new MainWindow(
            aetherCurrentService,
            TextureProvider,
            Configuration,
            ToggleSettingsWindow,
            ToggleAboutWindow);
        windowSystem.AddWindow(mainWindow);
        windowSystem.AddWindow(settingsWindow);
        windowSystem.AddWindow(aboutWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Abre o cierra Majo & Sol Aether Map.",
            ShowInHelp = true,
        });

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.Draw += mapOverlayRenderer.Draw;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleSettingsWindow;

        Log.Information("Majo & Sol Aether Map cargado correctamente.");
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.Draw -= mapOverlayRenderer.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainWindow;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleSettingsWindow;
        CommandManager.RemoveHandler(CommandName);

        windowSystem.RemoveAllWindows();
        mainWindow.Dispose();
        settingsWindow.Dispose();
        aboutWindow.Dispose();
        aetherCurrentService.Dispose();
    }

    private void OnCommand(string command, string arguments) => ToggleMainWindow();

    private void ToggleMainWindow() => mainWindow.Toggle();

    private void ToggleSettingsWindow() => settingsWindow.Toggle();

    private void ToggleAboutWindow() => aboutWindow.Toggle();
}
