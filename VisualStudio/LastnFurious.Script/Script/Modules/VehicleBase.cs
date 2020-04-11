// Module_VehicleBase - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_VehicleBase;
using static LastnFurious.VehicleBaseStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_VehicleBase
    {
        // Fields

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int NUM_COLLISION_POINTS = 4;


    }

    #endregion

    #region VehicleBase (AGS struct from .ash converted to class)

    public class VehicleBase
    {
        // Fields
        public Character c;
        public int carSprite;
        public int carSpriteAngle;
        public ViewFrame viewFrame;
        public DynamicSprite dSprite;
        public int dSpriteRotation;
        public VectorF[] collPointOff = new VectorF[NUM_COLLISION_POINTS];
        public float bodyLength;
        public float bodyWidth;
        public VectorF position = new VectorF();
        public VectorF direction = new VectorF();
        public VectorF velocity = new VectorF();
        public float angularVelocity;
        public VectorF[] collPoint = new VectorF[NUM_COLLISION_POINTS];

        // Properties
        public bool IsInit
        {
            get
            {
                return this.c != null;
            }
        }

        // Methods
        public void ResetBase(VectorF pos, VectorF dir)
        {
            if (pos == null)
                this.position = VectorF.create(Room.Width / 2, Room.Height / 2);
            else 
                this.position = pos.clone();
            if (dir == null)
                this.direction = VectorF.create(0, 1);
            else 
            {
                this.direction = dir.clone();
                this.direction.normalize();
            }
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.collPoint[i] = new VectorF();
            }
            this.velocity = VectorF.zero();
            this.angularVelocity = 0.0f;
        }

        public void RunPhysicsBase(float deltaTime)
        {
            this.position.addScaled(this.velocity, deltaTime);
            float rot_angle = this.angularVelocity * deltaTime;
            if (rot_angle != 0.0f)
                this.direction.rotate(rot_angle);
        }

        public void DetachCharacter()
        {
            this.carSprite = 0;
            this.carSpriteAngle = 0;
            if (this.viewFrame != null)
            {
                this.viewFrame.Graphic = 0;
            }
            this.viewFrame = null;
            if (this.dSprite != null)
            {
                this.dSprite.Delete();
            }
            this.dSprite = null;
            this.dSpriteRotation = 0;
            if (this.c != null)
            {
                this.c.UnlockView();
            }
            this.c = null;
        }

        public void UnInitBase()
        {
            this.DetachCharacter();
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.collPointOff[i] = null;
                this.collPoint[i] = null;
            }
            this.position = null;
            this.direction = null;
            this.velocity = null;
        }

        public void SyncCharacter()
        {
            if (this.c == null || this.position == null)
                return;
            this.c.x = FloatToInt(this.position.x, eRoundNearest);
            this.c.y = FloatToInt(this.position.y, eRoundNearest);
            if (this.c.Room != player.Room)
                this.c.ChangeRoom(player.Room);
            int angle = FloatToInt(Maths.RadiansToDegrees(this.direction.angle()), eRoundNearest);
            angle = angle - this.carSpriteAngle;
            angle = Maths.Angle360(angle);
            if (this.dSprite == null || angle != this.dSpriteRotation)
            {
                // Optimisation - Don't delete old sprite 
                //if (this.dSprite != null)
                //  this.dSprite.Delete();

                if (angle != 0)
                {
                    // NEW (keep drawing surface)
                    DynamicSprite spr = this.dSprite;
                    if (spr == null)
                        spr = DynamicSprite.CreateFromExistingSprite(this.carSprite, true);

                    // OLD
                    //DynamicSprite spr = DynamicSprite.CreateFromExistingSprite(this.carSprite, true);

                    spr.Rotate(angle);
                    this.dSprite = spr;
                    this.viewFrame.Graphic = this.dSprite.Graphic;

                    //DrawingSurface ds = this.dSprite.GetDrawingSurface();
                    //ds.DrawPixel(-1, -1);
                    //ds.Release();
                }
                else 
                {
                    this.dSprite = null;
                    this.viewFrame.Graphic = this.carSprite;
                }
                this.dSpriteRotation = angle;
            }
            int yoff = (Game.SpriteHeight[this.viewFrame.Graphic] - Game.SpriteHeight[this.carSprite]) / 2;
            yoff += Game.SpriteHeight[this.carSprite] / 2;
            this.c.LockViewOffset(this.viewFrame.View, 0, yoff);
            this.c.Loop = this.viewFrame.Loop;
            this.c.Frame = this.viewFrame.Frame;
        }

        public void UpdateBody()
        {
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                VectorF colpt = this.collPoint[i];
                colpt.set(this.collPointOff[i]);
                colpt.rotate(this.direction.angle());
                colpt.add(this.position);
            }
        }

        public void SetCharacter(Character c, int carSprite, CharacterDirection carSpriteDir, int view = 0, int loop = 0, int frame = 0)
        {
            this.DetachCharacter();
            int carl = 0;
            int  carw = 0;
            if (carSpriteDir == eDirectionDown || carSpriteDir == eDirectionUp)
            {
                carl = Game.SpriteHeight[carSprite];
                carw = Game.SpriteWidth[carSprite];
            }
            else if (carSpriteDir == eDirectionLeft || carSpriteDir == eDirectionRight)
            {
                carl = Game.SpriteWidth[carSprite];
                carw = Game.SpriteHeight[carSprite];
            }
            else 
            {
                AbortGame("Source car sprite direction cannot be diagonal, please provide sprite having one of the following directions: left, right, up or down.");
                return;
            }
            this.c = c;
            this.carSprite = carSprite;
            this.carSpriteAngle = RotatedView.AngleForLoop(carSpriteDir);
            this.viewFrame = Game.GetViewFrame(view, loop, frame);
            this.bodyLength = IntToFloat(carl);
            this.bodyWidth = IntToFloat(carw);
            this.collPointOff[0] = VectorF.create(carl / 2, -carw / 2);
            this.collPointOff[1] = VectorF.create(carl / 2, carw / 2);
            this.collPointOff[2] = VectorF.create(-carl / 2, carw / 2);
            this.collPointOff[3] = VectorF.create(-carl / 2, -carw / 2);
            this.SyncCharacter();
        }

        public void Reset(VectorF pos, VectorF dir)
        {
            this.ResetBase(pos, dir);
            this.UpdateBody();
            this.SyncCharacter();
        }

        public void Run(float deltaTime)
        {
            this.RunPhysicsBase(deltaTime);
            this.UpdateBody();
            this.SyncCharacter();
        }

        public void UnInit()
        {
            this.UnInitBase();
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class VehicleBaseStaticRef
    {
    }

    #endregion
    
}
