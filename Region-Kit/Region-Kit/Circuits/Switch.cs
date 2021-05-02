using RWCustom;
using UnityEngine;

namespace RegionKit.Circuits
{
    class Switch : BaseComponent
    {
        public Switch(PlacedObject pObj, Room room) : base(pObj, room)
        {
            type = Type.Input;
            _inType = InputType.Switch;
        }

        const int activationRadius = 3;
        Color? onColour = null;

        public override void Update(bool eu)
        {
            base.Update(eu);
            
            InputComponentData data = pObj.data as InputComponentData;

            foreach (AbstractCreature aCreature in room.game.Players)
            {
                IntVector2 coordInRoom = new IntVector2((int)pObj.pos.x / 20, (int)pObj.pos.y / 20);
                float dist = Custom.WorldCoordFloatDist(
                    Custom.MakeWorldCoordinate(coordInRoom, room.abstractRoom.index),
                    aCreature.pos);

                if (Input.GetKeyDown(KeyCode.D) && dist < activationRadius)
                {
                    data.activated = !data.activated;
                    Debug.Log($"switched {(data.activated ? "on" : "off")} circuit {data.circuitNumber}");
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (onColour == null)
            {
                onColour = rCam.paletteTexture.GetPixel(30, 4);
            }

            // base
            sLeaser.sprites[0].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[0].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[0].color = rCam.currentPalette.blackColor;
            sLeaser.sprites[0].scaleX = 20;
            sLeaser.sprites[0].scaleY = 15;

            // status light
            sLeaser.sprites[1].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[1].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[1].color = onColour != null ? (Color)onColour : rCam.currentPalette.blackColor;
            sLeaser.sprites[1].scaleX = 10;
            sLeaser.sprites[1].scaleY = 8;

            // switch lever
            sLeaser.sprites[2].x = pObj.pos.x - camPos.x;
            sLeaser.sprites[2].y = pObj.pos.y - camPos.y;
            sLeaser.sprites[2].color = rCam.currentPalette.blackColor + new Color(0.3f, 0.1f, 0.15f);
            sLeaser.sprites[2].scaleX = 10;
            sLeaser.sprites[2].scaleY = 8;

            sLeaser.sprites[1].isVisible = (pObj.data as InputComponentData).activated;

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
                new FSprite("pixel", true),
                new FSprite("pixel", true)
            };

            sLeaser.sprites[0].anchorY = 0;
            sLeaser.sprites[0].scaleX = 4;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            sLeaser.sprites[1].anchorY = 0;
            sLeaser.sprites[1].scaleX = 4;
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            sLeaser.sprites[2].anchorY = 2;
            sLeaser.sprites[2].scaleX = 6;
            sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["CustomDepth"];

            AddToContainer(sLeaser, rCam, null);
        }

    }
}
