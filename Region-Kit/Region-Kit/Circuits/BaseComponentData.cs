using System.Text.RegularExpressions;

namespace RegionKit.Circuits
{
    class BaseComponentData : PlacedObject.Data
    {
        public BaseComponentData(PlacedObject owner) : base(owner)
        {
            circuitNumber = 0;
        }

        public override void FromString(string s)
        {
            base.FromString(s);
            string[] array = Regex.Split(s, "~");
            if (array.Length >= 1)
            {
                circuitNumber = int.Parse(array[0]);
            }
        }

        public override string ToString()
        {
            return circuitNumber.ToString();
        }

        public int circuitNumber;
    }

}
