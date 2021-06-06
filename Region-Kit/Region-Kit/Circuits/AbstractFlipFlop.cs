using System.Collections.Generic;

namespace RegionKit.Circuits
{
    class AbstractFlipFlop : AbstractBaseComponent
    {
        public AbstractFlipFlop(string pObjStr, string region, MObjSetup data)
                : base(pObjStr, region, data, CompType.Input, InputType.LogicGate)
        {
            switch (data.Name)
            {
                default:
                case "Circuit_D_FlipFlop":
                    type = FlipFlopType.D;
                    break;
                case "Circuit_T_FlipFlop":
                    type = FlipFlopType.T;
                    break;
                case "Circuit_JK_FlipFlop":
                    type = FlipFlopType.JK;
                    break;
            }

            // flipflops should always start off
            data.SetValue(MKeys.activated, false);

        }

        readonly FlipFlopType type;

        public override void Update()
        {
            base.Update();

            if (frameDelay == 0)
            {
                qPrev = qNext;
            }
            frameDelay--;

            if (!inputs.ContainsKey("Clock")) return;

            if (lastClock == false && inputs["Clock"] == true)      // rising edge check
            {
                switch (type)
                {
                    default:
                    case FlipFlopType.D:
                        qNext = inputs["D"];
                        break;

                    case FlipFlopType.T:
                        qNext = inputs["T"] ? !qPrev : qPrev;
                        break;

                    case FlipFlopType.JK:
                        qNext = inputs["J"] && !qPrev || !inputs["K"] && qPrev;
                        break;

                }

                frameDelay = frameDelayMax;     // simulate flipflop delay
            }

            lastClock = inputs["Clock"];
        }

        public bool Output => qPrev;

        bool qPrev = false;
        bool qNext = false;
        const int frameDelayMax = 1;
        int frameDelay = frameDelayMax;
        bool lastClock = false;

        public Dictionary<string, string> GetInputIDs()
        {
            switch (type)
            {
                default:
                case FlipFlopType.D:
                    return new Dictionary<string, string>()
                    {
                        { "D", Data.GetValue<string>(MKeys.inputA) },
                        { "Clock", Data.GetValue<string>(MKeys.inputClock) }
                    };

                case FlipFlopType.T:
                    return new Dictionary<string, string>()
                    {
                        { "T", Data.GetValue<string>(MKeys.inputA) },
                        { "Clock", Data.GetValue<string>(MKeys.inputClock) }
                    };

                case FlipFlopType.JK:
                    return new Dictionary<string, string>()
                    {
                        { "J", Data.GetValue<string>(MKeys.inputA) },
                        { "K", Data.GetValue<string>(MKeys.inputB) },
                        { "Clock", Data.GetValue<string>(MKeys.inputClock) }
                    };
            }
        }

        public void SetInputs(Dictionary<string, bool> inputs)
        {
            bool valid = false;

            switch (type)
            {
                case FlipFlopType.D:
                    if (inputs.ContainsKey("D") && inputs.ContainsKey("Clock"))
                        valid = true;
                    break;

                case FlipFlopType.T:
                    if (inputs.ContainsKey("T") && inputs.ContainsKey("Clock"))
                        valid = true;
                    break;

                case FlipFlopType.JK:
                    if (inputs.ContainsKey("J") && inputs.ContainsKey("K") && inputs.ContainsKey("Clock"))
                        valid = true;
                    break;
            }

            if (!valid)
                throw new System.ArgumentException("Invalid input dictionary for this type of flipflop. " +
                    "Use FlipFlop.GetInputIDs to see the keys to use.");

            this.inputs = inputs;
        }

        public void Clear()
        {
            qNext = false;
        }

        Dictionary<string, bool> inputs = new Dictionary<string, bool>();

        public enum FlipFlopType
        {
            D,
            T,
            JK
        }

    }
}
