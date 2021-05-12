﻿
namespace RegionKit.Circuits
{
    abstract class LogicGate : BaseComponent
    {
        public LogicGate(PlacedObject pObj, Room room) : base(pObj, room, CompType.Input, InputType.LogicGate)
        {
            // logic gates should always start off
            _data.SetValue(MKeys.activated, false);
        }

        public abstract void SetInputs(bool[] inputs);
        public abstract bool Output { get; }
        public abstract string[] GetInputIDs();
    }

    class LogicGate_TwoInputs : LogicGate
    {
        public LogicGate_TwoInputs(PlacedObject pObj, Room room) : base(pObj, room) { }

        bool a = false; bool b = false;     // inputs

        public override void SetInputs(bool[] inputs)
        {
            if (inputs.Length != 2)
                throw new System.ArgumentException("Logic gates with two inputs require exactly two input values");

            a = inputs[0];
            b = inputs[1];
        }

        public override bool Output
        {
            get
            {
                switch (Data.GetValue<Op>(MKeys.logicOp))
                {
                    default:
                    case Op.AND:
                        return a && b;
                    case Op.NAND:
                        return !(a && b);
                    case Op.OR:
                        return a || b;
                    case Op.NOR:
                        return !(a || b);
                    case Op.XOR:
                        return a ^ b;
                }
            }
        }

        public override string[] GetInputIDs()
        {
            return new string[]
            {
                Data.GetValue<string>(MKeys.inputA),
                Data.GetValue<string>(MKeys.inputB)
            };
        }

        public enum Op
        {
            AND,
            NAND,
            OR,
            NOR,
            XOR
        }
    }

    class LogicGate_OneInput : LogicGate
    {
        public LogicGate_OneInput(PlacedObject pObj, Room room) : base(pObj, room) { }

        bool a = false;     // input

        public override void SetInputs(bool[] inputs)
        {
            if (inputs.Length != 1)
                throw new System.ArgumentException("Logic gates with one input require exactly one input value");

            a = inputs[0];
        }

        public override bool Output
        {
            get
            {
                switch (Data.GetValue<Op>(MKeys.logicOp))
                {
                    default:
                    case Op.Buffer:
                        return a;
                    case Op.NOT:
                        return !a;
                }
            }
        }

        public override string[] GetInputIDs()
        {
            return new string[]
            {
                Data.GetValue<string>(MKeys.inputA)
            };
        }

        public enum Op
        {
            Buffer,
            NOT
        }
    }

}
