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
        }

        /// <summary>
        /// Handles registration for fully managed managed objects in henpemaz's framework
        /// </summary>
        private static void SetupObjects()
        {
            // basic components
            List<PlacedObjectsManager.ManagedField> fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default_circuit", "Circuit ID"),
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
                new PlacedObjectsManager.StringField(MKeys.inputA, "default_circuit", "Input A"),
                new PlacedObjectsManager.StringField(MKeys.inputB, "default_circuit", "Input B"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_TwoInputs.Op), LogicGate_TwoInputs.Op.AND,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "default_circuit", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(LogicGate_TwoInputs), "Circuit_LogicGate_2");

            // logic gates with a single input
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.inputA, "default_circuit", "Input"),
                new PlacedObjectsManager.EnumField(MKeys.logicOp, typeof(LogicGate_OneInput.Op), LogicGate_OneInput.Op.NOT,
                        control: PlacedObjectsManager.ManagedFieldWithPanel.ControlType.arrows, displayName: "Operator"),
                new PlacedObjectsManager.StringField(MKeys.output, "default_circuit", "Output"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(LogicGate_OneInput), "Circuit_LogicGate_1");

            // clock pulse generator
            fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default_circuit", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false),
                new PlacedObjectsManager.IntegerField(MKeys.clockOnMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "On-frames"),
                new PlacedObjectsManager.IntegerField(MKeys.clockOffMax, 10, 600, 60,
                        PlacedObjectsManager.ManagedFieldWithPanel.ControlType.text, "Off-frames")
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Clock), "Circuit_Clock");

            objectsSetUp = true;
        }
        private static bool objectsSetUp = false;

        public void AddComponent(string circuitID, ICircuitComponent component)
        {
            Debug.Log($"adding component {component} to {circuitID}");

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
        }

        public void RemoveComponent(string circuitID, ICircuitComponent component)
        {
            Debug.Log($"removing component {component} from {circuitID}");

            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.Type)
            {
                case CompType.Input:
                    circuits[circuitID].inputComponents.Remove(component);
                    break;
                case CompType.Output:
                    circuits[circuitID].outputComponents.Remove(component);
                    break;
            }
        }

        public void MigrateComponent(string oldID, string newID, ICircuitComponent component)
        {
            if (oldID != null)
            {
                RemoveComponent(oldID, component);
            }
            if (component.Type == CompType.Output)
            {
                component.Activated = false;
            }
            AddComponent(newID, component);
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
        /// Used for the `actiavted` field of circuit components.
        /// </summary>
        private class ComponentActivityField : PlacedObjectsManager.BooleanField
        {
            public ComponentActivityField(string key, bool defaultValue) : base(key, defaultValue)
            { }

            public override bool NeedsControlPanel => false;    // no representation
        }

        class Circuit
        {
            public Circuit(string ID)
            {
                this.ID = ID;
            }

            public string ID { get; private set; }
            public List<ICircuitComponent> inputComponents = new List<ICircuitComponent>();
            public List<ICircuitComponent> outputComponents = new List<ICircuitComponent>();

            public bool IsEmpty =>
                inputComponents.Count == 0 &&
                outputComponents.Count == 0;

            bool hadPowerLastUpdate = false;
            public bool HasPower { get; private set; }

            public void Update()
            {
                HasPower = false;

                UpdateLogicGates();

                foreach (ICircuitComponent comp in inputComponents)
                {
                    if (comp is LogicGate gate && gate.Output)
                    {
                        HasPower = true;
                        break;
                    }
                    else if (comp.InType != InputType.LogicGate && comp.Activated)
                    {
                        HasPower = true;
                        break;
                    }
                }

                // activate/deactivate output and logic components *only if necessary*
                bool powerChanged = HasPower != hadPowerLastUpdate;
                if (!powerChanged) return;

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
                    if (!(comp is LogicGate gate)) continue;

                    // pass power status of input circuits to the logic gate
                    string[] inputIDs = gate.GetInputIDs();
                    bool[] inputs = new bool[inputIDs.Length];
                    for (int i = 0; i < inputIDs.Length; i++)
                    {
                        if (Instance.TryGetCircuit(inputIDs[i], out Circuit c))
                        {
                            inputs[i] = c.HasPower;
                        }
                    }

                    // this will affect gate.Output, which will be read next Circuit.Update
                    gate.SetInputs(inputs);
                }
            }

            public void ForceReset()
            {
                // just in case...
                foreach (ICircuitComponent c in inputComponents)
                {
                    c.Activated = false;
                }
                foreach (BaseComponent c in outputComponents)
                {
                    c.Activated = false;
                }
                hadPowerLastUpdate = false;
            }

        }

    }
}
