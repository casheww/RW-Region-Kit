namespace RegionKit.Circuits.Abstract
{
    public class AbstractSwitch : AbstractBaseComponent
    {
        public AbstractSwitch(string pObjStr, string region, MObjSetup data)
                : base(pObjStr, region, data, CompType.Input, InputType.Switch) { }

    }
}
