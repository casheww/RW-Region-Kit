namespace RegionKit.Circuits.Abstract
{
    public class AbstractButton : AbstractBaseComponent
    {
        public AbstractButton(string pObjStr, string region, MObjSetup data)
                : base(pObjStr, region, data, CompType.Input, InputType.Button) { }

        public override void Update() { }

    }
}
