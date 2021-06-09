using UnityEngine;

namespace RegionKit.Circuits.Real
{
    public class BasicLight : RealBaseComponent
    {
        public BasicLight(PlacedObject pObj, Room room) : base(pObj, room)
        {
            light = new LightSource(pObj.pos, false, Color.white, this);
            room.AddObject(light);
        }

        readonly LightSource light;

        public override void Update(bool eu)
        {
            light.color = CalculateColor();

            float s = Data.GetValue<float>(MKeys.sine);
            float f = Data.GetValue<float>(MKeys.flicker);
            light.setAlpha = Activated ? NoisifyAlpha(s, f) : 0;

            light.setRad = Data.GetValue<float>(MKeys.strength) * maxStrengthTileCount * 20;

            light.setPos = PObj.pos;
            base.Update(eu);
        }

        Color CalculateColor()
        {
            int r = Data.GetValue<int>(MKeys.red);
            int g = Data.GetValue<int>(MKeys.green);
            int b = Data.GetValue<int>(MKeys.blue);

            return new Color(r / 255f, g / 255f, b / 255f);
        }

        float NoisifyAlpha(float sineVal, float flickerVal)
        {
            float _alpha = baseAlpha;

            DoSineNoise(sineVal, ref _alpha);           // apply sine noise to alpha
            DoLargeFlickers(flickerVal, ref _alpha);    // larger brightness disruption

            return _alpha;
        }

        void DoSineNoise(float rate, ref float _alpha)
        {
            if (arcsin >= _2pi) arcsin = 0;                         // reset sine position
            else arcsin += _2pi * rate * sineRateMultiplier;        // progress along wavelength

            float sinMod = Mathf.Sin(arcsin) * 0.1f;

            _alpha = Mathf.Clamp01(_alpha + sinMod);
        }

        void DoLargeFlickers(float flicker, ref float _alpha)
        {
            if (flicker == 0) return;

            flickerCounter++;

            switch (state)
            {
                case LightState.TurningOff:
                case LightState.Off:
                    _alpha = 0;
                    break;

                case LightState.TurningOn:
                    _alpha *= Mathf.Sin(Mathf.PI / (2 * stateFrames) * (flickerCounter - 1));
                    break;
            }

            if (flickerCounter >= stateFrames)
            {
                // move to next state
                state++;
                if ((int)state > 3) state = 0;

                float rFlicker = Random.value * Random.value * flicker * flicker;

                // set new frame limit for the current state iteration
                switch (state)
                {
                    case LightState.On:
                        stateFrames = Mathf.RoundToInt(Mathf.Lerp(onMax, onMin, rFlicker));
                        break;

                    case LightState.TurningOff:
                        stateFrames = changeOff;
                        break;

                    case LightState.Off:
                        stateFrames = Mathf.RoundToInt(Mathf.Lerp(offMin, offMax, rFlicker));
                        break;

                    case LightState.TurningOn:
                        stateFrames = changeOn;
                        break;
                }

                flickerCounter = 0;
            }
        }

        const int maxStrengthTileCount = 32;
        const float baseAlpha = 0.9f;

        const float sineRateMultiplier = 0.05f;     // used to slow the rate of the sine noise cycle
        const float _2pi = 2 * Mathf.PI;

        const int onMin = 0;
        const int onMax = 256;
        const int offMin = 0;
        const int offMax = 128;
        const int changeOn = 8;
        const int changeOff = 2;

        int flickerCounter = 0;
        int stateFrames = -1;
        float arcsin = 0;           // for tracking the position along in the sine noise cycle


        LightState state = LightState.On;
        enum LightState
        {
            On,
            TurningOff,
            Off,
            TurningOn
        }

    }
}
