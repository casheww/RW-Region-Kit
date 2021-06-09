namespace RegionKit.Circuits.Abstract
{
    public class AbstractGenericOutput : AbstractBaseComponent
    {
        public AbstractGenericOutput(string pObjStr, string region, MObjSetup data)
            : base(pObjStr, region, data, CompType.Output) { }

        public override void Update() { }

    }
}
