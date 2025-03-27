using Forge.S4.Game;
using Forge.S4.Types.Native.Config;
using Forge.UX.Plugin;
using Forge.UX.Rendering.Text;
using Forge.UX.UI;
using Forge.UX.UI.Elements.Grouping;
using Forge.UX.UI.Elements.Grouping.Display;
using Forge.UX.UI.Elements.Grouping.Layout;
using Forge.UX.UI.Elements.Interaction;
using Forge.UX.UI.Elements.Static;
using Forge.UX.UI.Prefabs;
using Forge.UX.UI.Prefabs.Buttons;
using Forge.UX.UI.Prefabs.Groups;
using Forge.UX.UI.Prefabs.Groups.Layout;
using Forge.UX.UI.Prefabs.Text;

using System.Text;

namespace DebugPlugin {
    public class ConfigDebugMenu : IPluginScene {
        readonly IConfigApi configApi;

        public static bool Enabled = true;

        private UIText? configElement;

        public string TagName => "config-debug-menu";

        public IPrefab? Prefab { get; private set; }
        public bool AutoRegister => true;

        public bool Build(SceneBuilder builder) {
            if (!Enabled)
                return false;

            string test = """
                          <group Size="100%,100%">
                            <s4-window Id="window" Position="500,0" Size="500, 800">
                                <stack Id="layout" Size="100%,100%" MinimumDistance="80">
                                  <group Size="100%,60">
                                      <s4-button Position="0,0" Size="50%,100%" Id="close">Close</s4-button>
                                      <s4-button Position="50%,0" Size="50%,100%" Id="next">Next</s4-button>
                                  </group>
                                  <header Size="100%,20">Config</header>
                                  <scroll Size="100%,500">
                                      <s4-text Size="100%,100%" Id="config" FitText="true" TextHorizontalAlignment="Start" TextVerticalAlignment="Start">No config available</s4-text>
                                  </scroll>
                                </stack>
                            </s4-window>
                            
                            <group Size="241.5,45" Position="90%,50">
                                <s4-button Id="window-button" Size="100%,100%">Config Debug</s4-button>
                            </group>
                          </group>
                          """;

            bool succeeded = builder.CreateScene(test, out GroupPrefab? scene);
            if (!succeeded || scene == null)
                return false;

            Prefab = scene;

            Prefab.Instantiated += (element) => {
                UIGroup group = (UIGroup)element;

                group.OnTick += _ => {
                    TickMenu();
                };
                //window.OnProcess += (_, _) => {
                //    ProcessMenu();
                //};

                UIWindow window = group.Elements.GetById<UIWindow>("window")!;
                UIStack layout = group.Elements.GetById<UIStack>("layout")!;
                layout.Elements.GetById<UIButton>("close")!.OnInteract += (_) => {
                    window.Close();
                };

                layout.Elements.GetById<UIButton>("next")!.OnInteract += (_) => {
                    section++;
                    if (section >= configApi.GetSectionNames().Count)
                        section = 0;
                };

                configElement = group.Elements.GetById<UIText>("config")!;


                UIButton windowButton = group.Elements.GetById<UIButton>("window-button")!;
                windowButton.OnInteract += (_) => {
                    window.Open();
                };
                windowButton.OnProcess += (button, _) => {
                    if (window.Visible)
                        button.Hide();
                    else
                        button.Show();
                };
            };

            return true;
        }

        public ConfigDebugMenu(IConfigApi configApi) {
            this.configApi = configApi;
        }

        private int section = 0;
        private void TickMenu() {
            if (configElement == null)
                return;

            List<string> sections = configApi.GetSectionNames();
            string section = sections[this.section];

            StringBuilder sb = new StringBuilder();
            sb.Append(section);
            sb.Append(":\n");

            List<string> configs = configApi.GetConfigNamesForSection(section);
            foreach (string config in configs) {
                sb.Append("  ");
                sb.Append(config);
                sb.Append("=");

                switch (configApi.GetSettingType(section, config)) {
                    case CConfigVar.VarType.Unknown:
                    default:
                        sb.Append("Unknown");
                        break;
                    case CConfigVar.VarType.Int:
                        sb.Append(configApi.GetSettingInt(section, config, 0));
                        break;
                    case CConfigVar.VarType.Float:
                        sb.Append(configApi.GetSettingFloat(section, config, 0));
                        break;
                    case CConfigVar.VarType.String:
                        sb.Append("\"");
                        sb.Append(configApi.GetSettingString(section, config, ""));
                        sb.Append("\"");
                        break;
                    case CConfigVar.VarType.IntArray:
                        sb.Append("[");

                        int[] intArray = configApi.GetSettingIntArray(section, config, new int[0]);
                        sb.AppendJoin(",", intArray);

                        sb.Append("]");
                        break;
                    case CConfigVar.VarType.List:
                        sb.Append("List");
                        break;
                }

                sb.Append("\n");
            }

            configElement.Text = sb.ToString();
        }
    }
}