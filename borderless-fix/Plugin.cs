using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
namespace BorderlessFix;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Borderless fix";

    public IDalamudPluginInterface Dalamud { get; init; }
    public Config Config { get; init; }
    public WindowHooks Hooks { get; init; }

    public WindowSystem WindowSystem = new("BorderlessFix");
    private ConfigWindow _wndConfig;

    public Plugin(IDalamudPluginInterface dalamud, IGameInteropProvider interop, IPluginLog log)
    {
        Dalamud = dalamud;
        Config = Dalamud.GetPluginConfig() as Config ?? new Config();
        Hooks = new(Config, interop, log);

        _wndConfig = new ConfigWindow(this);
        WindowSystem.AddWindow(_wndConfig);

        Dalamud.UiBuilder.Draw += WindowSystem.Draw;
        Dalamud.UiBuilder.OpenConfigUi += () => _wndConfig.IsOpen = true;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        Hooks.Dispose();
    }
}