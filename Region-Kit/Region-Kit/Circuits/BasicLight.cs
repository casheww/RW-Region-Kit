using UnityEngine;

namespace RegionKit.Circuits
{
    class BasicLight : BaseComponent
    {
        public BasicLight(PlacedObject pObj, Room room) : base(pObj, room)
        {
            type = Type.Output;
            activated = false;
            light = new LightSource(pObj.pos, false, Color.white, this);
            room.AddObject(light);
        }

        LightSource light;
        bool activated;

        public override void Activate()
        {
            activated = true;
        }

        public override void Deactivate()
        {
            activated = false;
        }

        public override void Update(bool eu)
        {
            ColorComponentData data = pObj.data as ColorComponentData;

            light.color = data.color;

            flickerWait--;
            if (flickerWait < -8)
            {
                flickerWait = (int)Mathf.Lerp(10, 120, Random.value);
            }

            float clampedFlicker = Mathf.Clamp(data.flicker, 0.05f, 1f);
            if (xForSin > 2 * Mathf.PI) xForSin = 0;
            else xForSin += Mathf.PI / (clampedFlicker * baseSinWavelength / 2) + (Random.Range(-10, 10) / 10);

            light.setAlpha = activated ? AlphaWithNoise : 0;

            light.setRad = data.strength * maxStrengthTileCount * 20;

            light.setPos = pObj.pos;
            base.Update(eu);
        }

        float AlphaWithNoise
        {
            get
            {
                float _alpha = baseAlpha;
                
                if (flickerWait <= 0)
                {
                    _alpha *= Mathf.Pow(flickerRatio, 10 + flickerWait);
                }

                float sinMod = Mathf.Sin(xForSin) * 0.05f;
                return Mathf.Clamp01(_alpha + sinMod);
            }
        }

        const int maxStrengthTileCount = 30;
        int flickerWait = 100;
        const float baseAlpha = 0.9f;
        const float flickerRatio = 0.8f;
        const int baseSinWavelength = 20;
        float xForSin = 2 * Mathf.PI * Random.value;

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        { }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[0];
        }

    }
}
