using RegionKit.Circuits.Abstract;
using System.Collections.Generic;

namespace RegionKit.Circuits
{
    partial class CircuitController
    {
        public class Circuit
        {
            public Circuit(string id)
            {
                this.id = id;
            }

            public readonly string id;
            public List<AbstractBaseComponent> inputComponents = new List<AbstractBaseComponent>();
            public List<AbstractBaseComponent> outputComponents = new List<AbstractBaseComponent>();

            public List<AbstractBaseComponent> AllComponents
            {
                get
                {
                    List<AbstractBaseComponent> components = new List<AbstractBaseComponent>(inputComponents);
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

                foreach (AbstractBaseComponent comp in inputComponents)
                {
                    if ((comp is AbstractLogicGate gate && gate.Output) || (comp is AbstractFlipFlop ff && ff.Output))
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

                foreach (AbstractBaseComponent comp in outputComponents)
                {
                    Setup.Log(comp);
                    comp.Activated = HasPower;
                }
                hadPowerLastUpdate = HasPower;
            }

            void UpdateLogicGates()
            {
                foreach (AbstractBaseComponent comp in inputComponents)
                {
                    if (comp is AbstractLogicGate gate)
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
                foreach (AbstractBaseComponent comp in inputComponents)
                {
                    if (comp is AbstractFlipFlop fflop)
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

        }

    }
}
