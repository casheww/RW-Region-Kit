
namespace RegionKit.Circuits
{
    class AbstractGenericComponent : AbstractBaseComponent
    {
        public AbstractGenericComponent(string pObjStr, string region, MObjSetup data)
            : base(pObjStr, region, data, CompType.Output) { }

        public override void Update() { }

    }
}
