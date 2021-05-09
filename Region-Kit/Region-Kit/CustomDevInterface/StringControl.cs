using System.Reflection;
using DevInterface;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace RegionKit.CustomDevInterface
{
    class StringControl : PositionedDevUINode, IDevUISignals
    {
        // ============ hooks

        public static void Setup()
        {
            SetupInputDetours();
            On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
        }
        public static void Disable()
        {
            UndoInputDetours();
            On.RainWorldGame.RawUpdate -= RainWorldGame_RawUpdate;
        }

        static void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (self.devUI == null)
            {
                activeStringControl = null;
            }
        }

        // ============ end hooks


        // ============ setup detours for input capture

        public static void SetupInputDetours()
        {
            MethodBase getKeyMethod = typeof(Input).GetMethod("GetKey", new System.Type[] { typeof(string) });
            MethodBase captureInputMethod = typeof(StringControl).GetMethod("CaptureInput", new System.Type[] { typeof(string) });
            inputDetour_string = new NativeDetour(getKeyMethod, captureInputMethod);

            getKeyMethod = typeof(Input).GetMethod("GetKey", new System.Type[] { typeof(KeyCode) });
            captureInputMethod = typeof(StringControl).GetMethod("CaptureInput", new System.Type[] { typeof(KeyCode) });
            inputDetour_code = new NativeDetour(getKeyMethod, captureInputMethod);
        }
        static NativeDetour inputDetour_string;
        static NativeDetour inputDetour_code;

        public static void UndoInputDetours()
        {
            inputDetour_string.Undo();
            inputDetour_code.Undo();
        }

        public static bool CaptureInput(string key)
        {
            KeyCode code = (KeyCode)System.Enum.Parse(typeof(KeyCode), key.ToUpper());
            return CaptureInput(code);
        }

        public static bool CaptureInput(KeyCode code)
        {
            bool res;

            if (activeStringControl == null)
            {
                res = orig_GetKey(code);
            }
            else
            {
                if (code == KeyCode.Escape)
                {
                    res = orig_GetKey(code);
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }

        static bool orig_GetKey(KeyCode code)
        {
            bool res;
            inputDetour_code.Undo();
            res = Input.GetKey(code);
            inputDetour_code.Apply();
            return res;
        }

        // ============ end of detours stuff


        public StringControl(DevUI owner, string IDstring, DevUINode parentNode,
                Vector2 pos, string title, string startingText)
                : base(owner, IDstring, parentNode, pos)
        {

            subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0, 0), 56, title));
            subNodes.Add(new DevUILabel(owner, "Text", this, new Vector2(60, 0), 136, startingText));
            subNodes.Add(new ArrowButton(owner, "Enter", this, new Vector2(200, 0), 90));
        }

        public string Text
        {
            get
            {
                return subNodes[1].fLabels[0].text;
            }
            set
            {
                subNodes[1].fLabels[0].text = value;
            }
        }

        public override void Update()
        {
            if (owner.mouseClick && !clickedLastUpdate)
            {
                if ((subNodes[1] as RectangularDevUINode).MouseOver && activeStringControl != this)
                {
                    // replace whatever instance/null that was focused
                    activeStringControl = this;
                    subNodes[1].fLabels[0].color = new Color(0.1f, 0.4f, 0.2f);
                }
                else if (activeStringControl == this)
                {
                    activeStringControl = null;
                    subNodes[1].fLabels[0].color = Color.black;
                }

                clickedLastUpdate = true;
            }
            else if (!owner.mouseClick)
            {
                clickedLastUpdate = false;
            }

            if (activeStringControl == this)
            {
                foreach (char c in Input.inputString)
                {
                    if (c == '\b')
                    {
                        if (Text.Length != 0)
                            Text = Text.Substring(0, Text.Length - 1);
                    }
                    else if (c == '\n' || c == '\r')
                    {
                        SetValue(Text);
                    }
                    else
                    {
                        Text += c;
                    }
                }
            }
        }
        bool clickedLastUpdate = false;
        static StringControl activeStringControl = null;

        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            Debug.Log("signal");
            SetValue(Text);
        }

        public virtual void SetValue(string newValue) { }

    }
}
