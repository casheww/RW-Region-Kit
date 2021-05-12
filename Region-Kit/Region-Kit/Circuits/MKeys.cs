using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegionKit.Circuits
{
    /// <summary>
    /// ManagedData keys for circuit object data fields.
    /// Useful for quick refactoring and typo avoidance.
    /// </summary>
    struct MKeys
    {
        // basic
        public const string circuitID = "circuit_id";
        public const string activated = "activated";

        // lights and colour
        public const string flicker = "flicker";
        public const string strength = "strength";
        public const string red = "red";
        public const string green = "green";
        public const string blue = "blue";

        // logic gates & flip flops
        public const string logicOp = "logic";
        public const string inputA = "l_inputA";
        public const string inputB = "l_inputB";
        public const string inputClock = "l_inputC";
        public const string output = "l_output";
        public const string flipFlop = "ff_type";

        // clock
        public const string clockOnMax = "clock_on_max";
        public const string clockOffMax = "clock_off_max";
    }
}
