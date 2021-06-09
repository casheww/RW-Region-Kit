using RegionKit.Circuits.Real;
using System;

namespace RegionKit.Circuits.Abstract
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

            data.SetValue(MKeys.circuitID, circuitID);
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
            get
            {
                if (Realised)
                {
                    return RealisedObj.Activated;
                }
                return Data.GetValue<bool>(MKeys.activated);
            }
            set
            {
                if (Realised)
                {
                    RealisedObj.Activated = value;
                }
                Data.SetValue(MKeys.activated, value);
            }
        }

        public string CurrentCircuitID
        {
            get
            {
                if (Realised)
                {
                    return RealisedObj.CurrentCircuitID;
                }
                return Data.GetValue<string>(MKeys.circuitID);
            }
        }

        public string LastCircuitID { get; set; }

        public bool Realised { get; set; }
        public RealBaseComponent RealisedObj { get; set; }

        /// <summary>
        /// If overriding, be sure to call base.Update!
        /// </summary>
        public virtual void Update()
        {
            if (RealisedObj.slatedForDeletetion)
            {
                Realised = false;
                RealisedObj = null;
            }
            else
            {
                LastCircuitID = CurrentCircuitID;
                Data.SetValue(MKeys.circuitID, RealisedObj.CurrentCircuitID);
            }
        }

    }
}
