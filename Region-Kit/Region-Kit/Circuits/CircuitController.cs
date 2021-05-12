using ManagedPlacedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace RegionKit.Circuits
{
    class CircuitController : MonoBehaviour
    {
        public static CircuitController Instance { get; private set; }

        readonly Dictionary<string, Circuit> circuits = new Dictionary<string, Circuit>();


        public void Start()
        {
            Instance = this;

            if (!objectsSetUp) SetupObjects();
            if (!hooked) ApplyHooks();
        }

        public void Update()
        {
            List<string> idsForRemoval = new List<string>();

            foreach (string id in circuits.Keys)
            {
                Circuit c = circuits[id];
                if (c.IsEmpty)
                {
                    idsForRemoval.Add(id);
                }

                c.Update();
            }

            // remove empty circuits and hope that the garbage collector gets the objects?
            foreach (string id in idsForRemoval)
            {
                circuits.Remove(id);
            }

            UpdateComponentAllegiance();
        }

        /// <summary>
        /// Handles registration for fully managed managed objects in henpemaz's framework - <see cref="PlacedObjectsManager"/>
        /// </summary>
        private static void SetupObjects()
        {
            // basic components
            List<PlacedObjectsManager.ManagedField> fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Button), "Circuit_Button");
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Switch), "Circuit_Switch");


            // components with colours and other funky stuff
            fields.AddRange(new PlacedObjectsManager.ManagedField[]
            {
                new PlacedObjectsManager.FloatField(MKeys.flicker, 0, 1, 0.4f, 0.05f, displayName: "Flicker"),
                new PlacedObjectsManager.FloatField(MKeys.strength, 0, 1, 0.7f, 0.05f, displayName: "Strength"),
                new PlacedObjectsManager.IntegerField(MKeys.red, 0, 255, 80,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Red"),
                new PlacedObjectsManager.IntegerField(MKeys.green, 0, 255, 200,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Green"),
                new PlacedObjectsManager.IntegerField(MKeys.blue, 0, 255, 200,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.slider, "Blue")
            });
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(BasicLight), "Circuit_BasicLight");


            // logic gates with 2 inputs
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultA", "Input A"),
                new PlacedObjectsManager.StringField(MKeys.inputB, "defaultB", "Input B"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_TwoInputs.Op), LogicGate_TwoInputs.Op.AND,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(LogicGate_TwoInputs), "Circuit_LogicGate_2");


            // logic gates with a single input
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "default", "Input"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_OneInput.Op), LogicGate_OneInput.Op.NOT,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultOUT", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(LogicGate_OneInput), "Circuit_LogicGate_1");


            // clock pulse generator
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false),
                new PlacedObjectsManager.IntegerField(MKeys.clockOnMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "On-frames"),
                new PlacedObjectsManager.IntegerField(MKeys.clockOffMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "Off-frames")
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Clock), "Circuit_Clock");


            // flip flops
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "defaultD", "D"),
                new PlacedObjectsManager.StringField(MKeys.inputClock, "clock", "Clock"),
                new PlacedObjectsManager.StringField(MKeys.output, "defaultQ", "Q"),
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(FlipFlop), "Circuit_D_FlipFlop");

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultT", "T");
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(FlipFlop), "Circuit_T_FlipFlop");

            fields[0] = new PlacedObjectsManager.StringField(MKeys.inputA, "defaultJ", "J");
            fields.Insert(1, new PlacedObjectsManager.StringField(MKeys.inputB, "defaultK", "K"));
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(FlipFlop), "Circuit_JK_FlipFlop");

            objectsSetUp = true;
        }
        private static bool objectsSetUp = false;

        #region hooks

        private static void ApplyHooks()
        {
            On.Room.AddObject += Room_AddObject;
            On.DevInterface.ObjectsPage.RemoveObject += ObjectsPage_RemoveObject;
            hooked = true;
        }
        private static bool hooked = false;

        private static void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            orig(self, obj);

            if (obj is ICircuitComponent comp)
            {
                foreach (Circuit c in Instance.circuits.Values)
                {
                    foreach (ICircuitComponent otherComp in c.AllComponents)
                    {
                        // remove duplicated of the new object from previous loads of the room
                        if (comp.Data.owner.pos == otherComp.Data.owner.pos
                            && comp.GetType() == otherComp.GetType())
                        {
                            Instance.RemoveComponent(otherComp.CurrentCircuitID, otherComp);
                        }
                    }
                }

                Instance.AddComponent(comp.CurrentCircuitID, comp);
            }
        }

        private static void ObjectsPage_RemoveObject(On.DevInterface.ObjectsPage.orig_RemoveObject orig,
                DevInterface.ObjectsPage self, DevInterface.PlacedObjectRepresentation objRep)
        {
            foreach (Circuit c in Instance.circuits.Values)
            {
                foreach (ICircuitComponent comp in c.AllComponents)
                {
                    if (comp.Data.owner == objRep.pObj)
                    {
                        Instance.RemoveComponent(comp.LastCircuitID, comp);
                    }
                }
            }

            orig(self, objRep);
        }

        #endregion hooks

        public void AddComponent(string circuitID, ICircuitComponent component)
        {
            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.Type)
            {
                case CompType.Input:
                    circuits[circuitID].inputComponents.Add(component);
                    break;
                case CompType.Output:
                    circuits[circuitID].outputComponents.Add(component);
                    break;
            }

            component.LastCircuitID = circuitID;
            Debug.Log($"added component {component} to {circuitID}");
        }

        public bool RemoveComponent(string circuitID, ICircuitComponent component)
        {
            if (!circuits.ContainsKey(circuitID)) return false;

            bool success = false;
            switch (component.Type)
            {
                case CompType.Input:
                    success = circuits[circuitID].inputComponents.Remove(component);
                    break;
                case CompType.Output:
                    success = circuits[circuitID].outputComponents.Remove(component);
                    break;
            }

            if (success) Debug.Log($"removed component {component} from {circuitID}");
            return success;
        }

        /// <summary>
        /// Checks whether a component's current circuit ID has changed.
        /// If it has changed, then the component is migrated to the new circuit.
        /// </summary>
        void UpdateComponentAllegiance()
        {
            List<ICircuitComponent> componentsToMigrate = new List<ICircuitComponent>();

            foreach (Circuit c in circuits.Values)
            {
                foreach (ICircuitComponent comp in c.AllComponents)
                {
                    if (comp.LastCircuitID != comp.CurrentCircuitID)
                    {
                        componentsToMigrate.Add(comp);
                    }
                }
            }

            foreach (ICircuitComponent comp in componentsToMigrate)
            {
                Instance.MigrateComponent(comp);
                comp.LastCircuitID = comp.CurrentCircuitID;
            }
        }

        public void MigrateComponent(ICircuitComponent component)
        {
            if (component.LastCircuitID != null)
            {
                RemoveComponent(component.LastCircuitID, component);
            }

            if (component.Type == CompType.Output)
            {
                component.Activated = false;
            }
            else if (component is FlipFlop ff)
            {
                ff.Clear();
            }

            AddComponent(component.CurrentCircuitID, component);
        }

        /// <summary>
        /// Tries to get the Circuit object with the given circuit ID
        /// </summary>
        /// <returns>Whether the get was successful</returns>
        bool TryGetCircuit(string id, out Circuit circuit)
        {
            if (circuits.TryGetValue(id, out Circuit c))
            {
                circuit = c;
                return true;
            }
            circuit = null;
            return false;
        }


        /// <summary>
        /// A Managed bool field for henpemaz's framework that has no representation.
        /// Designed to store component activity for <see cref="ICircuitComponent.Activated"/>.
        /// </summary>
        private class ComponentActivityField : PlacedObjectsManager.BooleanField
        {
            public ComponentActivityField(string key, bool defaultValue) : base(key, defaultValue) { }

            public override bool NeedsControlPanel => false;    // no representation
        }

        class Circuit
        {
            public Circuit(string id)
            {
                this.id = id;
            }

            public readonly string id;
            public List<ICircuitComponent> inputComponents = new List<ICircuitComponent>();
            public List<ICircuitComponent> outputComponents = new List<ICircuitComponent>();

            public List<ICircuitComponent> AllComponents
            {
                get
                {
                    List<ICircuitComponent> components = new List<ICircuitComponent>(inputComponents);
                    components.AddRange(outputComponents);
                    return components;
                }
            }

            public bool IsEmpty =>
                inputComponents.Count == 0 &&
                outputComponents.Count == 0;

            bool hadPowerLastUpdate = false;
            public bool HasPower { get; private set; }

            public void Update()
            {
                HasPower = false;

                UpdateLogicGates();
                UpdateFlipFlops();

                foreach (ICircuitComponent comp in inputComponents)
                {
                    if ((comp is LogicGate gate && gate.Output) || (comp is FlipFlop ff && ff.Output))
                    {
                        HasPower = true;
                        break;
                    }
                    else if (comp.InType != InputType.LogicGate && comp.InType != InputType.FlipFlop && comp.Activated)
                    {
                        HasPower = true;
                        break;
                    }
                }

                // activate/deactivate output and logic components *only if necessary*
                bool powerChanged = HasPower != hadPowerLastUpdate;
                if (!powerChanged) return;

                Debug.Log($"{id} set to {HasPower}");

                if (HasPower)
                {
                    foreach (ICircuitComponent comp in outputComponents)
                    {
                        comp.Activated = true;
                    }
                    hadPowerLastUpdate = true;
                }
                else
                {
                    foreach (ICircuitComponent comp in outputComponents)
                    {
                        comp.Activated = false;
                    }
                    hadPowerLastUpdate = false;
                }
            }

            void UpdateLogicGates()
            {
                foreach (ICircuitComponent comp in inputComponents)
                {
                    if (comp is LogicGate gate)
                    {
                        // pass power status of input circuits to the logic gate
                        string[] inputIDs = gate.GetInputIDs();
                        bool[] inputs = new bool[inputIDs.Length];
                        for (int i = 0; i < inputIDs.Length; i++)
                        {
                            if (Instance.TryGetCircuit(inputIDs[i], out Circuit c))
                            {
                                inputs[i] = c.HasPower;
                            }
                            else inputs[i] = false;
                        }

                        // this will affect gate.Output, which will be read next Circuit.Update
                        gate.SetInputs(inputs);
                    }               
                }
            }

            void UpdateFlipFlops()
            {
                foreach (ICircuitComponent comp in inputComponents)
                {
                    if (comp is FlipFlop fflop)
                    {
                        Dictionary<string, string> inputIDs = fflop.GetInputIDs();
                        Dictionary<string, bool> inputs = new Dictionary<string, bool>();
                        foreach (KeyValuePair<string, string> pair in inputIDs)
                        {
                            // pair.Key is input label ("D") and pair.Value is circuit ID ("default_circuit")
                            if (Instance.TryGetCircuit(pair.Value, out Circuit c))
                            {
                                inputs[pair.Key] = c.HasPower;
                            }
                            else inputs[pair.Key] = false;
                        }

                        fflop.SetInputs(inputs);
                    }
                }
            }

            public void ClearPower()
            {
                foreach (ICircuitComponent comp in outputComponents)
                {
                    comp.Activated = false;
                }
                hadPowerLastUpdate = false;
                Debug.Log($"circuit {id} was cleared of power");
            }

        }

    }
}
