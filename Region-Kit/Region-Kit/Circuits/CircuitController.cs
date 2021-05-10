﻿using ManagedPlacedObjects;
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

        private static void SetupObjects()
        {
            // set up representations and data for basic components (only circuit ID) using henpemaz' framework
            List<PlacedObjectsManager.ManagedField> fields = new List<PlacedObjectsManager.ManagedField>()
            {
                new PlacedObjectsManager.StringField(MKeys.circuitID, "default_circuit", "Circuit ID"),
                new ComponentActivityField(MKeys.activated, false)
            };
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Button), "Circuit_Button");
            PlacedObjectsManager.RegisterFullyManagedObjectType(fields.ToArray(), typeof(Switch), "Circuit_Switch");

            // set up reps for components with colours and other funky stuff
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

            objectsSetUp = true;
        }
        private static bool objectsSetUp = false;

        public void AddComponent(string circuitID, BaseComponent component)
        {
            Debug.Log($"adding component {component} to {circuitID}");

            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.type)
            {
                case BaseComponent.Type.Input:
                    circuits[circuitID].inputComponents.Add(component);
                    break;
                case BaseComponent.Type.Output:
                    circuits[circuitID].outputComponents.Add(component);
                    break;
                case BaseComponent.Type.LogicGate:
                    circuits[circuitID].logicComponents.Add(component);
                    break;
            }
        }

        public void RemoveComponent(string circuitID, BaseComponent component)
        {
            Debug.Log($"removing component {component} from {circuitID}");

            if (!circuits.ContainsKey(circuitID))
            {
                circuits[circuitID] = new Circuit(circuitID);
            }

            switch (component.type)
            {
                case BaseComponent.Type.Input:
                    circuits[circuitID].inputComponents.Remove(component);
                    break;
                case BaseComponent.Type.Output:
                    circuits[circuitID].outputComponents.Remove(component);
                    break;
                case BaseComponent.Type.LogicGate:
                    circuits[circuitID].logicComponents.Remove(component);
                    break;
            }
        }

        public void MigrateComponent(string oldID, string newID, BaseComponent component)
        {
            if (oldID != null)
            {
                RemoveComponent(oldID, component);
            }
            if (component.type == BaseComponent.Type.Output)
            {
                component.Deactivate();
            }
            AddComponent(newID, component);
        }

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
            public List<BaseComponent> inputComponents = new List<BaseComponent>();
            public List<BaseComponent> outputComponents = new List<BaseComponent>();
            public List<BaseComponent> logicComponents = new List<BaseComponent>();

            public bool IsEmpty =>
                inputComponents.Count == 0 &&
                outputComponents.Count == 0 &&
                logicComponents.Count == 0;

            bool hadPowerLastUpdate = false;

            public void Update()
            {
                bool hasPower = false;

                foreach (BaseComponent c in inputComponents)
                {
                    PlacedObjectsManager.ManagedData data = (c.pObj.data as PlacedObjectsManager.ManagedData);
                    if (data != null && data.GetValue<bool>(MKeys.activated))
                    {
                        hasPower = true;
                        break;
                    }
                }

                // activate/deactivate output and logic components *only if necessary*
                bool powerChanged = hasPower != hadPowerLastUpdate;
                if (!powerChanged) return;

                if (hasPower)
                {
                    foreach (BaseComponent c in outputComponents)
                    {
                        c.Activate();
                    }
                    foreach (BaseComponent c in logicComponents)
                    {
                        c.Activate();
                    }
                    hadPowerLastUpdate = true;
                }
                else
                {
                    foreach (BaseComponent c in outputComponents)
                    {
                        c.Deactivate();
                    }
                    foreach (BaseComponent c in logicComponents)
                    {
                        c.Deactivate();
                    }
                    hadPowerLastUpdate = false;
                }
            }

            public void ForceReset()
            {
                // just in case...
                foreach (BaseComponent c in inputComponents)
                {
                    (c.pObj.data as PlacedObjectsManager.ManagedData).SetValue(MKeys.activated, false);
                }
                foreach (BaseComponent c in outputComponents)
                {
                    c.Deactivate();
                }
                foreach (BaseComponent c in logicComponents)
                {
                    c.Deactivate();
                }
                hadPowerLastUpdate = false;
            }

        }

    }
}
