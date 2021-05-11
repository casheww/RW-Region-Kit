﻿using ManagedPlacedObjects;
using RWCustom;
using UnityEngine;

namespace RegionKit.Circuits
{
    class Button : BaseComponent, IDrawable
    {
        public Button(PlacedObject pObj, Room room) : base(pObj, room, CompType.Input, InputType.Button)
        {
            counter = counterMax;
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
                    Activated = false;
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
                    Activated = true;
                    Debug.Log($"button started powering {data.GetValue<string>(MKeys.circuitID)}");
                }
            }
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

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
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

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
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
        }
    }

}
