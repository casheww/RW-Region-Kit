using ManagedPlacedObjects;
using RWCustom;
using UnityEngine;

namespace RegionKit.Circuits
{
    class Button : BaseComponent
    {
        public Button(PlacedObject pObj, Room room) : base(pObj, room)
        {
            counter = counterMax;
            type = Type.Input;
            _inType = InputType.Button;
        }

        const int counterMax = 150;
        const int activationRadius = 3;
        int counter;
        Color? onColour = null;

        public override void Update(bool eu)
        {
            base.Update(eu);

            PlacedObjectsManager.ManagedData data = pObj.data as PlacedObjectsManager.ManagedData;

            if (data.GetValue<bool>(MKeys.activated))
            {
                counter--;
                if (counter < 0)
                {
                    data.SetValue("activated", false);
                    Debug.Log($"button stopped powering {data.GetValue<string>(MKeys.circuitID)}");
                    counter = counterMax;
                }
                return;
            }

            foreach (AbstractCreature aCreature in room.game.Players)
            {
                IntVector2 coordInRoom = new IntVector2((int)pObj.pos.x / 20, (int)pObj.pos.y / 20);
                float dist = Custom.WorldCoordFloatDist(
                    Custom.MakeWorldCoordinate(coordInRoom, room.abstractRoom.index),
                    aCreature.pos);

                if (Input.GetKey(KeyCode.D) && dist < activationRadius)
                {
                    data.SetValue(MKeys.activated, true);
                    Debug.Log($"button started powering {data.GetValue<string>(MKeys.circuitID)}");
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (onColour == null)
            {
                onColour = rCam.paletteTexture.GetPixel(30, 4);
            }

            sLeaser.sprites[0].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[0].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[0].color = rCam.currentPalette.blackColor;
            sLeaser.sprites[0].scaleX = 20;
            sLeaser.sprites[0].scaleY = 15;

            sLeaser.sprites[1].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[1].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[1].color = onColour != null ? (Color)onColour : rCam.currentPalette.blackColor;
            sLeaser.sprites[1].scaleX = 10;
            sLeaser.sprites[1].scaleY = 8;

            sLeaser.sprites[1].isVisible = (pObj.data as PlacedObjectsManager.ManagedData).GetValue<bool>(MKeys.activated);

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[]
            {
                new FSprite("pixel", true),
                new FSprite("pixel", true)
            };

            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].scaleX = 4;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            sLeaser.sprites[1].anchorY = 0;
            sLeaser.sprites[1].scaleX = 4;
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            AddToContainer(sLeaser, rCam, null);
        }
    }

}
