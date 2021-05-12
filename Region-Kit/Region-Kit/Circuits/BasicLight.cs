using UnityEngine;

namespace RegionKit.Circuits
{
    class BasicLight : BaseComponent
    {
        public BasicLight(PlacedObject pObj, Room room) : base(pObj, room, CompType.Output)
        {
            light = new LightSource(pObj.pos, false, Color.white, this);
            room.AddObject(light);
        }

        readonly LightSource light;

        public override void Update(bool eu)
        {
            light.color = CalculateColor();

            light.setAlpha = Activated ? NoisifyAlpha(Data.GetValue<float>(MKeys.flicker)) : 0;

            light.setRad = Data.GetValue<float>(MKeys.strength) * maxStrengthTileCount * 20;

            light.setPos = pObj.pos;
            base.Update(eu);
        }

        Color CalculateColor()
        {
            int r = Data.GetValue<int>(MKeys.red);
            int g = Data.GetValue<int>(MKeys.green);
            int b = Data.GetValue<int>(MKeys.blue);

            return new Color(r / 255f, g / 255f, b / 255f);
        }

        float NoisifyAlpha(float flicker)
        {
            float _alpha = baseAlpha;

            flickerWait--;
            if (flickerWait < -8)
            {
                flickerWait = (int)Mathf.Lerp(10, 120, Random.value);
            }

            float clampedFlicker = Mathf.Clamp(flicker, 0.05f, 1f);
            if (xForSin > 2 * Mathf.PI) xForSin = 0;
            else xForSin += Mathf.PI / (clampedFlicker * baseSinWavelength / 2) + (Random.Range(-10, 10) / 10);

            if (flickerWait <= 0)
            {
                _alpha *= Mathf.Pow(flickerRatio, 10 + flickerWait);
            }

            float sinMod = Mathf.Sin(xForSin) * 0.05f;
            return Mathf.Clamp01(_alpha + sinMod);
        }

        const int maxStrengthTileCount = 30;
        int flickerWait = 100;
        const float baseAlpha = 0.9f;
        const float flickerRatio = 0.8f;
        const int baseSinWavelength = 20;
        float xForSin = 2 * Mathf.PI * Random.value;

    }
}
