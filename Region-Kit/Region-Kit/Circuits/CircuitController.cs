using System.Collections.Generic;
using UnityEngine;

namespace RegionKit.Circuits
{
    class CircuitController : MonoBehaviour
    {
        public static CircuitController Instance { get; private set; }

        Dictionary<string, Circuit> circuits = new Dictionary<string, Circuit>();

        public void Start()
        {
            Instance = this;
        }

        public void Update()
        {
            foreach (Circuit c in circuits.Values)
            {
                c.Update();
            }
        }

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

        public void UpdateComponentRegistration(string oldID, string newID, BaseComponent component)
        {
            if (oldID != null)
            {
                RemoveComponent(oldID, component);
            }
            AddComponent(newID, component);
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

            bool hadPowerLastUpdate = false;

            public void Update()
            {
                bool hasPower = false;

                foreach (BaseComponent c in inputComponents)
                {
                    InputComponentData data = (c.pObj.data as InputComponentData);
                    if (data != null && data.activated)
                    {
                        hasPower = true;
                        break;
                    }
                }

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
                foreach (BaseComponent c in inputComponents)
                {
                    (c.pObj.data as InputComponentData).activated = false;
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
