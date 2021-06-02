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

        }

    }
}
