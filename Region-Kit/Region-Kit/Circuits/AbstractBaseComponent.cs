
namespace RegionKit.Circuits
{
    public abstract class AbstractBaseComponent
    {
        public AbstractBaseComponent(string pObjTypeStr, string region, MObjSetup data,
                CompType compType, InputType inType = InputType.NotAnInput, string circuitID = "default")
        {
            this.pObjTypeStr = pObjTypeStr;
            this.region = region;
            this.data = data;

            this.compType = compType;
            this.inType = compType == CompType.Output ? InputType.NotAnInput : inType;

            Activated = false;

            currentCircuitID = circuitID;
            LastCircuitID = circuitID;

            Realised = false;
        }

        public string PObjTypeStr => pObjTypeStr;
        readonly string pObjTypeStr;

        public string Region => region;
        readonly string region;

        public MObjSetup Data => data;
        readonly MObjSetup data;

        public CompType CompType => compType;
        readonly CompType compType;
        public InputType InType => inType;
        readonly InputType inType;

        public bool Activated
        {
            get => Data.GetValue<bool>(MKeys.activated);
            set => Data.SetValue(MKeys.activated, value);
        }

        public string CurrentCircuitID => currentCircuitID;
        readonly string currentCircuitID;
        public string LastCircuitID { get; set; }

        public bool Realised { get; protected set; }
        public RealBaseComponent RealisedObj { get; protected set; }

        public virtual void Update() { }

        /*
        public BaseComponent(PlacedObject pObj, Room room, CompType type, InputType inType = InputType.NotAnInput)
        {
            this.pObj = pObj;
            this.room = room;
            Type = type;

            if (type == CompType.Input && inType == InputType.NotAnInput)
                throw new System.ArgumentException("Input component can't have input type NotAnInput");
            _inType = inType;

            _data = pObj.data as PlacedObjectsManager.ManagedData;

            // some components should always start deactivated
            if (type == CompType.Output)
            {
                _data.SetValue(MKeys.activated, false);
            }
        }

        public readonly PlacedObject pObj;
        //protected readonly PlacedObjectsManager.ManagedData _data;
        //public PlacedObjectsManager.ManagedData Data => _data;

        private readonly InputType _inType;
        public CompType Type { get; set; }
        public InputType InType => Type == CompType.Input ? _inType : InputType.NotAnInput;

        public bool Activated
        {
            get => Data.GetValue<bool>(MKeys.activated);
            set => Data.SetValue(MKeys.activated, value);
        }

        public string CurrentCircuitID
        {
            get
            {
                return this is LogicGate || this is FlipFlop ?
                    Data.GetValue<string>(MKeys.output) : Data.GetValue<string>(MKeys.circuitID);
            }
        }
        public string LastCircuitID { get; set; }*/

    }
}
