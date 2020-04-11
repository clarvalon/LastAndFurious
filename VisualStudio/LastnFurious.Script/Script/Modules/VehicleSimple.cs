// Module_VehicleSimple - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_VehicleSimple;
using static LastnFurious.VehicleSimpleStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_VehicleSimple
    {
        // Fields

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region VehicleSimple (AGS struct from .ash converted to class)

    public class VehicleSimple
    {
        // Fields
        public Character c;
        public int carSprite;
        public int carSpriteAngle;
        public ViewFrame viewFrame;
        public DynamicSprite sprite;
        public int currentSpriteRotation;
        public VectorF[] collPointOff = new VectorF[NUM_COLLISION_POINTS];
        public VectorF[] collPoint = new VectorF[NUM_COLLISION_POINTS];
        public float engine;
        public float steering;
        public float brakes;
        public float friction;
        public VectorF position = new VectorF();
        public VectorF direction = new VectorF();
        public float directionAngle;
        public VectorF thrust = new VectorF();
        public VectorF brakeForce = new VectorF();
        public float driveVelocityValue;
        public VectorF driveVelocity = new VectorF();
        public VectorF impactVelocity = new VectorF();
        public VectorF velocity = new VectorF();
        public float thrustForceValue;
        public float brakeForceValue;
        public float[] terrafriction = new float[NUM_COLLISION_POINTS];
        public Point[] colpt = new Point[NUM_COLLISION_POINTS];

        // Properties
        public float Brakes
        {
            get
            {
                return this.brakes;
            }
            set
            {
                var power = value;
                this.brakes = power;
            }
        }

        public float Engine
        {
            get
            {
                return this.engine;
            }
            set
            {
                var power = value;
                this.engine = power;
            }
        }

        public float Steering
        {
            get
            {
                return this.steering;
            }
            set
            {
                var rads = value;
                this.steering = rads;
            }
        }

        public float Friction
        {
            get
            {
                return this.friction;
            }
        }

        // Methods
        public void syncCharacter()
        {
            if (this.c == null || this.position == null)
                return;
            this.c.x = FloatToInt(this.position.x, eRoundNearest);
            this.c.y = FloatToInt(this.position.y, eRoundNearest);
            int angle = FloatToInt(Maths.RadiansToDegrees(this.direction.angle()), eRoundNearest);
            angle = angle - this.carSpriteAngle;
            angle = Maths.Angle360(angle);
            if (this.sprite == null || angle != this.currentSpriteRotation)
            {
                if (angle != 0)
                {
                    DynamicSprite spr = DynamicSprite.CreateFromExistingSprite(this.carSprite, true);
                    spr.Rotate(angle);
                    this.sprite = spr;
                    this.viewFrame.Graphic = this.sprite.Graphic;
                }
                else 
                {
                    this.sprite = null;
                    this.viewFrame.Graphic = this.carSprite;
                }
                this.currentSpriteRotation = angle;
            }
            int yoff = (Game.SpriteHeight[this.viewFrame.Graphic] - Game.SpriteHeight[this.carSprite]) / 2;
            yoff += Game.SpriteHeight[this.carSprite] / 2;
            this.c.LockViewOffset(this.viewFrame.View, 0, yoff);
            this.c.Loop = this.viewFrame.Loop;
            this.c.Frame = this.viewFrame.Frame;
        }

        public void setCharacter(Character c, int carSprite, CharacterDirection carSpriteDir, int view = 0, int loop = 0, int frame = 0)
        {
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
            this.collPointOff[0] = VectorF.create(carl / 2, -carw / 2);
            this.collPointOff[1] = VectorF.create(carl / 2, carw / 2);
            this.collPointOff[2] = VectorF.create(-carl / 2, carw / 2);
            this.collPointOff[3] = VectorF.create(-carl / 2, -carw / 2);
            this.colpt[0] = new Point();
            this.colpt[1] = new Point();
            this.colpt[2] = new Point();
            this.colpt[3] = new Point();
            this.syncCharacter();
        }

        public void reset(VectorF pos, VectorF dir)
        {
            this.engine = 0.0f;
            this.steering = 0.0f;
            this.brakes = 0.0f;
            this.friction = 0.0f;
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
            this.directionAngle = this.direction.angle();
            this.thrust = new VectorF();
            this.brakeForce = new VectorF();
            this.driveVelocityValue = 0.0f;
            this.driveVelocity = new VectorF();
            this.impactVelocity = new VectorF();
            this.velocity = new VectorF();
            this.thrustForceValue = 0.0f;
            this.brakeForceValue = 0.0f;
            this.syncCharacter();
        }

        public void processInteraction(float deltaTime)
        {
            int i = 0;
            float[] friction = new float[NUM_COLLISION_POINTS];
            float avg_friction = 0.0f;
            bool[] hit_area = new bool[NUM_COLLISION_POINTS];
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                VectorF colpoint = this.collPointOff[i].clone();
                colpoint.rotate(this.direction.angle());
                colpoint.add(this.position);
                this.collPoint[i] = colpoint;
                int room_x = FloatToInt(colpoint.x, eRoundNearest);
                int room_y = FloatToInt(colpoint.y, eRoundNearest);
                int screen_x = room_x - GetViewportX();
                int screen_y = room_y - GetViewportY();
                int area = GetWalkableAreaAt(screen_x, screen_y);
                if (area == 0)
                {
                    hit_area[i] = true;
                }
                else 
                {
                    friction[i] = Track.TerraSlideFriction[area];
                    avg_friction += friction[i];
                    this.terrafriction[i] = friction[i];
                }
                this.colpt[i].x = FloatToInt(colpoint.x, eRoundNearest);
                this.colpt[i].y = FloatToInt(colpoint.y, eRoundNearest);
            }
            avg_friction /= IntToFloat(NUM_COLLISION_POINTS);
            this.friction = avg_friction;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                if (!hit_area[i])
                    continue;
                VectorF impact = VectorF.subtract(this.position, this.collPoint[i]);
                impact.normalize();
                impact.scale(Maths.AbsF(this.driveVelocityValue));
                this.impactVelocity.add(impact);
                float projection = VectorF.projection(impact, this.driveVelocity);
                if (this.driveVelocityValue < 0.0f)
                    projection = -projection;
                this.driveVelocityValue += projection;
            }
        }

        public void run(float deltaTime)
        {
            this.position.addScaled(this.velocity, deltaTime);
            float rot_angle = 0f;
            if (this.driveVelocityValue >= 0.0f)
                rot_angle = this.steering * deltaTime;
            else 
                rot_angle = -this.steering * deltaTime;
            this.directionAngle = Maths.Angle2Pi(this.directionAngle + rot_angle);
            this.direction.rotate(rot_angle);
            this.processInteraction(deltaTime);
            float absVelocity = Maths.AbsF(this.driveVelocityValue);
            float thrustResistance = this.friction * absVelocity + this.brakes;
            float thrustForce = this.engine * deltaTime;
            float brakeForce = Maths.MinF(thrustResistance * deltaTime, absVelocity);
            if (this.driveVelocityValue < 0.0f)
                brakeForce = -brakeForce;
            this.driveVelocityValue += thrustForce - brakeForce;
            this.driveVelocity.set(this.direction);
            this.driveVelocity.scale(this.driveVelocityValue);
            this.thrustForceValue = thrustForce;
            this.brakeForceValue = brakeForce;
            float impactValue = this.impactVelocity.length();
            if (impactValue > 0.0f)
            {
                impactValue = Maths.MaxF(0.0f, impactValue - (this.friction * impactValue * 10.0f + this.brakes) * deltaTime);
                this.impactVelocity.truncate(impactValue);
            }
            this.velocity.set(this.driveVelocity);
            this.velocity.add(this.impactVelocity);
            this.syncCharacter();
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class VehicleSimpleStaticRef
    {
    }

    #endregion
    
}
