using System.Collections.Generic;
using UnityEngine;

namespace RegionKit.Circuits
{
    class CircuitController : MonoBehaviour
    {
        public static CircuitController Instance { get; private set; }

        public const int minCircuit = 0;
        public const int maxCircuit = 31;
        Dictionary<int, Circuit> circuits = new Dictionary<int, Circuit>();

        public void Start()
        {
            Instance = this;
            
            for (int i = minCircuit; i <= maxCircuit; i++)
            {
                circuits[i] = new Circuit(i);
            }
        }

        public void Update()
        {
            for (int i = minCircuit; i <= maxCircuit; i++)
            {
                circuits[i].Update();
            }
        }

        public void AddComponent(int circuitNum, BaseComponent component)
        {
            Debug.Log($"adding component {component} to {circuitNum}");
            switch (component.type)
            {
                case BaseComponent.Type.Input:
                    circuits[circuitNum].inputComponents.Add(component);
                    break;
                case BaseComponent.Type.Output:
                    circuits[circuitNum].outputComponents.Add(component);
                    break;
            }
        }

        public void RemoveComponent(int circuitNum, BaseComponent component)
        {
            Debug.Log($"removing component {component} from {circuitNum}");
            switch (component.type)
            {
                case BaseComponent.Type.Input:
                    circuits[circuitNum].inputComponents.Remove(component);
                    break;
                case BaseComponent.Type.Output:
                    circuits[circuitNum].outputComponents.Remove(component);
                    break;
            }
        }


        class Circuit
        {
            public Circuit(int number)
            {
                Number = number;
            }

            public int Number { get; private set; }
            public List<BaseComponent> inputComponents = new List<BaseComponent>();
            public List<BaseComponent> outputComponents = new List<BaseComponent>();

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
                    hadPowerLastUpdate = true;
                }
                else
                {
                    foreach (BaseComponent c in outputComponents)
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
                hadPowerLastUpdate = false;
            }

        }

    }
}
