using Dalamud.Configuration;
using System;
namespace BorderlessFix;

[Serializable]
public class Config : IPluginConfiguration
{
    public bool UseWorkArea;
    public int Version { get; set; } = 0;
}
