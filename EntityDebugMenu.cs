using Forge.S4.Game;
using Forge.S4.Types;
using Forge.S4.Types.Native.Entities;
using Forge.UX.Input;
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

namespace DebugPlugin {
    public class EntityDebugMenu : IPluginScene {
        readonly IInputManager inputManager;
        readonly IEventApi eventManager;
        readonly IEntityApi entityManager;

        public static bool Enabled = true;

        public uint entityId = 0;

        private UIText? entityElement;

        public string TagName => "entity-debug-menu";

        public IPrefab? Prefab { get; private set; }
        public bool AutoRegister => true;

        public bool Build(SceneBuilder builder) {
            if (!Enabled)
                return false;

            string test = """
                          <menu><!-- AttachedScreen="Ingame">-->
                            <s4-window Id="window" Position="500,0" Size="395,790">
                                <stack Id="layout" Size="100%,100%" MinimumDistance="80">
                                  <group Size="100%,60">
                                      <s4-button Position="0,0" Size="50%,100%" Id="close">Close</s4-button>
                                      <s4-button Position="50%,0" Size="50%,100%" Id="next">Next</s4-button>
                                  </group>
                                  <group Size="100%,60">
                                      <s4-button Position="0,0" Size="50%,100%" Id="hurt">Tribe</s4-button>
                                      <s4-button Position="50%,0" Size="50%,100%" Id="team">Team</s4-button>
                                  </group>
                                  <header Id="header" Size="100%,10">Element</header>
                                  <scroll Size="100%,500">
                                    <s4-text Id="entity" Size="700,800" FitText="true" TextHorizontalAlignment="Start" TextVerticalAlignment="Start">No Entity Active</s4-text>
                                  </scroll>
                                </stack>
                            </s4-window>
                            
                            <s4-button Id="window-button" Position="90%,0.0" Size="241.5,45">Forge Debug</s4-button>
                          </menu>
                          
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

                entityElement = group.Elements.GetById<UIText>("entity")!;
                UIButton nextButton = group.Elements.GetById<UIButton>("next")!;
                nextButton.OnInteract += (_) => {
                    entityId++;
                    uint entities = entityManager.EntityPoolSize;
                    if (entityId > entities)
                        entityId = 0;
                };

                UIButton hurtButton = group.Elements.GetById<UIButton>("hurt")!;
                hurtButton.OnInteract += (_) => {
                    unsafe {
                        IEntity* entity = entityManager.GetEntity(entityId);
                        if (entity != null) {
                            entity->Heal(5);
                        }
                    }
                };
                UIButton teamButton = group.Elements.GetById<UIButton>("team")!;
                teamButton.OnInteract += (_) => {
                    unsafe {
                        IEntity* entity = entityManager.GetEntity(entityId);
                        if (entity != null) {
                            entity->Damage(5);
                        }
                    }
                };

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

        public EntityDebugMenu(IInputManager inputManager, IEventApi eventManager, IEntityApi entityManager) {
            this.inputManager = inputManager;
            this.eventManager = eventManager;
            this.entityManager = entityManager;
        }


        private void TickMenu() {
            entityElement!.Text = "No world active";
            unsafe {
                IEntity** allEntities = entityManager.BackingEntityPool;
                var selection = entityManager.Selection;
                if (selection != null && selection->Count > 0) {
                    entityElement!.Text = "Selection: " + selection->Count;
                    entityId = selection->entityIds[0];
                }

                IEntity* entity = entityManager.GetEntity(entityId);
                if (entity != null) {
                    EntityClass entityClass = entityManager.ClassOf(entity);
                    uint id = entity->id;

                    entityElement!.Text = $@"EntityPool[{entityId}]: {id}/{entity->id2}
Class: {entityClass}
Base Type: {entity->baseType}
Entity Type: {entity->entityType}
ObjectId: {entity->objectId}
Tribe: {(Tribe)entity->tribe}
Player: {entity->player}
Health: {entity->health}
Selection: {entity->selectionFlags:b8}
unk_16: {entity->unk_16}
unk_17: {entity->unk_17}
X: {entity->x}, Y: {entity->y}
unk_1c: {entity->unk_1c}
unk_1d: {entity->unk_1d}
unk_1e: {entity->unk_1e}
";

                    if (entityClass is EntityClass.Settler or EntityClass.Building) {
                        IAnimatedEntity* animatedEntity = (IAnimatedEntity*)entity;
                        entityElement.Text += $@"Animation Frame: {animatedEntity->animationFrameIndex}
unk7: {animatedEntity->unk_25}
JIL: {animatedEntity->unk_26}
nextEntity: {animatedEntity->nextEntity}
prevEntity: {animatedEntity->prevEntity}
globalTick: {animatedEntity->globalTick}
unk_30: {animatedEntity->unk_30}
unk_34: {animatedEntity->unk_34:X}
unk_38: {animatedEntity->unk_38:X}

";
                    }

                    if (entityClass == EntityClass.Settler) {
                        CSettler* settler = (CSettler*)entity;
                        entityElement.Text += $@"Role: {(uint)settler->role:X}";
                    }
                }
            }
        }
    }
}