using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
namespace BorderlessFix;

public class ConfigWindow : Window, IDisposable
{
    private Plugin _plugin;

    public ConfigWindow(Plugin plugin) : base("Borderless fix config")
    {
        Size = new Vector2(232, 75);
        SizeCondition = ImGuiCond.Once;
        _plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        if (ImGui.Checkbox("Do not overlap taskbar", ref _plugin.Config.UseWorkArea))
        {
            _plugin.Hooks.Reinit();
            _plugin.Dalamud.SavePluginConfig(_plugin.Config);
        }

        if (ImGui.CollapsingHeader("Debug info"))
        {
            _plugin.Hooks.DrawDebug();
        }
    }
}
