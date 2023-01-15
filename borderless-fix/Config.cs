using Dalamud.Configuration;
using System;

namespace BorderlessFix
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool UseWorkArea;
    }
}
