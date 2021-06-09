namespace RegionKit.Circuits.Abstract
{
    public class AbstractClock : AbstractBaseComponent
    {
        public AbstractClock(string pObjStr, string region, MObjSetup data)
            : base(pObjStr, region, data, CompType.Input, InputType.Clock, "clock")
        {
            counterOnMax = Data.GetValue<int>(MKeys.clockOnMax);
            counterOffMax = Data.GetValue<int>(MKeys.clockOffMax);
        }

        public override void Update()
        {
            if (Realised)
            {
                counterOnMax = RealisedObj.Data.GetValue<int>(MKeys.clockOnMax);
                counterOffMax = RealisedObj.Data.GetValue<int>(MKeys.clockOffMax);
            }

            counter--;
            if (counter < -counterOffMax)
            {
                counter = counterOnMax;
                Activated = true;
            }
            else if (counter == 0)
            {
                Activated = false;
            }
        }

        int counterOnMax;   // abs of clock max
        int counterOffMax;  // abs of clock min
        int counter = 0;

        /*
        public Clock(PlacedObject pObj, Room room) : base(pObj, room, CompType.Input, InputType.Clock) { }

        int counterOnMax;   // abs of clock max
        int counterOffMax;  // abs of clock min
        int counter = 0;
        Color? onColour = null;

        public override void Update(bool eu)
        {
            base.Update(eu);

            counterOnMax = Data.GetValue<int>(MKeys.clockOnMax);
            counterOffMax = Data.GetValue<int>(MKeys.clockOffMax);

            counter--;
            if (counter < -counterOffMax)
            {
                counter = counterOnMax;
                Activated = true;
            }
            else if (counter == 0)
            {
                Activated = false;
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[]
            {
                new FSprite("Circle20", true)
            };

            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].scaleX = 2;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (onColour == null)
            {
                onColour = rCam.paletteTexture.GetPixel(30, 4);
            }

            sLeaser.sprites[0].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[0].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[0].color = rCam.currentPalette.blackColor;
            sLeaser.sprites[0].scaleX = 0.6f;
            sLeaser.sprites[0].scaleY = 0.6f;

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (sLeaser.sprites == null) return;

            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.color = palette.blackColor;
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }*/

    }
}
