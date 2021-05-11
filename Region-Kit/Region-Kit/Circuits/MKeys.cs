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

        // logic gate enum
        public const string logicOp = "logic";
        public const string inputA = "l_inputA";
        public const string inputB = "l_inputB";
        public const string output = "l_output";
    }
}
