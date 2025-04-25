using DryIoc;

using Forge.Config;
using Forge.Engine;
using Forge.UX.Plugin;

namespace DebugModule {
    public class DebugModule : IModule {
        public string Name => "Debug Module";

        public void Initialize(Container injector) {
            injector.Register<IPluginScene, EntityDebugMenu>();
            injector.Register<IPluginScene, ConfigDebugMenu>();
        }
    }
}
