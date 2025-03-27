using DryIoc;

using Forge.Config;
using Forge.Engine;
using Forge.UX.Plugin;

namespace DebugPlugin {
    public class Plugin : IPlugin {
        public int Priority { get; }

        public Plugin() {

        }

        public void Initialize() {
            DI.Dependencies.Register<IPluginScene, EntityDebugMenu>();
            DI.Dependencies.Register<IPluginScene, ConfigDebugMenu>();
        }
    }
}
