// Module_Vehicle - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Vehicle;
using static LastnFurious.VehicleStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Vehicle
    {
        // Fields

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region Vehicle (AGS struct from .ash converted to class)

    public class Vehicle : VehicleBase
    {
        // Fields
        public float bodyMass;
        public float bodyAerodynamics;
        public float hardImpactLossFactor;
        public float softImpactLossFactor;
        public float engineMaxPower;
        public float engineAccelerator;
        public float enginePowerGoal;
        public float enginePower;
        public float brakePower;
        public float driveWheelTorque;
        public float driveWheelGrip;
        public float driveWheelForce;
        public float distanceBetweenAxles;
        public float stillTurningVelocity;
        public float steeringWheelAngle;
        public VectorF turningAccel = new VectorF();
        public float driftVelocityFactor;
        public float weightForce;
        public float envGrip;
        public float envSlideFriction;
        public float envRollFriction;
        public float envResistance;
        public bool strictCollisions;
        public int[] collPtHit = new int[NUM_COLLISION_POINTS];
        public int[] collPtCarHit = new int[NUM_COLLISION_POINTS];
        public int numHits;
        public int numCarHits;
        public VectorF[] oldCollPt = new VectorF[NUM_COLLISION_POINTS];
        public int[] oldCollPtHit = new int[NUM_COLLISION_POINTS];
        public int[] oldCollPtCarHit = new int[NUM_COLLISION_POINTS];
        public float infoRollAntiforce;
        public float infoSlideAntiforce;
        public VectorF infoImpact = new VectorF();

        // Properties
        public float Accelerator
        {
            get
            {
                return this.engineAccelerator;
            }
            set
            {
                var power = value;
                this.engineAccelerator = Maths.ClampF(power, 0.0f, 1.0f);
            }
        }

        public float EnginePower
        {
            get
            {
                return this.enginePower;
            }
        }

        public float Brakes
        {
            get
            {
                return this.brakePower;
            }
            set
            {
                var power = value;
                this.brakePower = Maths.ClampF(power, 0.0f, 1.0f);
            }
        }

        // Methods
        public void UpdateEnviroment()
        {
            float[] slide_friction = new float[NUM_COLLISION_POINTS];
            float[] roll_friction = new float[NUM_COLLISION_POINTS];
            float[] env_res = new float[NUM_COLLISION_POINTS];
            float[] grip = new float[NUM_COLLISION_POINTS];
            float avg_slide_friction = 0.0f;
            float avg_roll_friction = 0.0f;
            float avg_env_res = 0.0f;
            float avg_grip = 0.0f;
            this.numHits = 0;
            int valid_terrain = 0;
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                VectorF colpt = this.collPoint[i];
                int room_x = FloatToInt(colpt.x, eRoundNearest);
                int room_y = FloatToInt(colpt.y, eRoundNearest);
                int screen_x = room_x - GetViewportX();
                int screen_y = room_y - GetViewportY();
                int area = GetWalkableAreaAt(screen_x, screen_y);
                if (Track.IsObstacle[area])
                    this.collPtHit[i] = area;
                else 
                    this.collPtHit[i] = -1;
                if (this.collPtHit[i] >= 0)
                {
                    this.numHits += 1;
                    continue;
                }
                valid_terrain += 1;
                slide_friction[i] = Track.TerraSlideFriction[area];
                avg_slide_friction += slide_friction[i];
                roll_friction[i] = Track.TerraRollFriction[area];
                avg_roll_friction += roll_friction[i];
                env_res[i] = Track.EnvResistance[area];
                avg_env_res += env_res[i];
                grip[i] = Track.TerraGrip[area];
                avg_grip += grip[i];
            }
            if (valid_terrain > 0)
            {
                float valid_terrain_f = IntToFloat(valid_terrain);
                avg_slide_friction /= valid_terrain_f;
                this.envSlideFriction = avg_slide_friction;
                avg_roll_friction /= valid_terrain_f;
                this.envRollFriction = avg_roll_friction;
                avg_env_res /= valid_terrain_f;
                this.envResistance = avg_env_res;
                avg_grip /= valid_terrain_f;
                this.envGrip = avg_grip;
            }
            else 
            {
                this.envSlideFriction = 0.0f;
                this.envRollFriction = 0.0f;
                this.envResistance = 0.0f;
                this.envGrip = 0.0f;
            }
            this.weightForce = this.bodyMass * Track.Gravity;
            this.driveWheelGrip = this.envGrip;
        }

        public void RunCollision(VectorF impactVelocity, float deltaTime)
        {
            if (this.numHits == 0)
                return;
            VectorF posImpact = VectorF.zero();
            VectorF negImpact = VectorF.zero();
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                if (this.collPtHit[i] < 0)
                    continue;
                if (this.oldCollPtHit[i] == this.collPtHit[i])
                    continue;
                VectorF impact = VectorF.subtract(this.collPoint[i], this.oldCollPt[i]);
                if (impact.isZero())
                    continue;
                impact.normalize();
                float velProjection = VectorF.projection(this.velocity, impact);
                if (velProjection > 0.0f)
                {
                    impact.scale(-velProjection * (2.0f - this.hardImpactLossFactor));
                    posImpact.max(impact);
                    negImpact.min(impact);
                }
            }
            impactVelocity.add(posImpact);
            impactVelocity.add(negImpact);
        }

        public void RunPhysics(float deltaTime)
        {
            int i = 0;
            if (this.strictCollisions && this.numHits > 0)
            {
                float x = 0f;
                float y = 0f;
                for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
                {
                    this.collPoint[i].set(this.oldCollPt[i]);
                    this.collPtHit[i] = this.oldCollPtHit[i];
                    x += this.collPoint[i].x;
                    y += this.collPoint[i].y;
                }
                this.position.x = x / IntToFloat(NUM_COLLISION_POINTS);
                this.position.y = y / IntToFloat(NUM_COLLISION_POINTS);
            }
            else 
            {
                for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
                {
                    this.oldCollPt[i].set(this.collPoint[i]);
                }
            }
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.oldCollPtHit[i] = this.collPtHit[i];
                this.oldCollPtCarHit[i] = this.collPtCarHit[i];
            }
            this.RunPhysicsBase(deltaTime);
            this.UpdateBody();
            this.UpdateEnviroment();
            this.enginePower = this.engineMaxPower * this.engineAccelerator;
            this.driveWheelTorque = this.enginePower;
            this.driveWheelTorque -= this.driveWheelTorque * this.brakePower;
            this.driveWheelForce = this.driveWheelTorque * this.driveWheelGrip;
            VectorF rollDirection = this.direction.clone();
            VectorF slideDirection = rollDirection.clone();
            slideDirection.rotate(Maths.Pi / 2.0f);
            float rollingVelocity = VectorF.projection(this.velocity, rollDirection);
            float slidingVelocity = VectorF.projection(this.velocity, slideDirection);
            this.turningAccel.makeZero();
            if (this.velocity.isZero())
            {
                this.angularVelocity = this.steeringWheelAngle * this.stillTurningVelocity;
            }
            else 
            {
                if (this.steeringWheelAngle != 0.0f)
                {
                    float steerAngle = this.steeringWheelAngle;
                    VectorF driveWheelPos = this.position.clone();
                    VectorF steerWheelPos = this.position.clone();
                    driveWheelPos.addScaled(this.direction, -this.distanceBetweenAxles / 2.0f);
                    steerWheelPos.addScaled(this.direction, this.distanceBetweenAxles / 2.0f);
                    VectorF driveWheelMovement = rollDirection.clone();
                    VectorF steerWheelMovement = rollDirection.clone();
                    steerWheelMovement.rotate(steerAngle);
                    driveWheelPos.addScaled(driveWheelMovement, rollingVelocity);
                    steerWheelPos.addScaled(steerWheelMovement, rollingVelocity);
                    VectorF newPosDir = VectorF.subtract(steerWheelPos, driveWheelPos);
                    newPosDir.normalize();
                    this.angularVelocity = VectorF.angleBetween(this.direction, newPosDir);
                    this.angularVelocity *= this.envGrip;
                    float dumb_drift_factor = Maths.ArcTan(Maths.AbsF(rollingVelocity / this.driftVelocityFactor)) / (Maths.Pi / 2.0f);
                    this.turningAccel = VectorF.subtract(newPosDir, rollDirection);
                    this.turningAccel.scale(rollingVelocity * (1.0f - dumb_drift_factor) * this.envGrip);
                }
                else 
                {
                    this.angularVelocity = 0.0f;
                }
            }
            VectorF rollResDir = rollDirection.clone();
            rollResDir.scale(-rollingVelocity);
            rollResDir.normalize();
            VectorF slideResDir = slideDirection.clone();
            slideResDir.scale(-slidingVelocity);
            slideResDir.normalize();
            float driveForce = (this.driveWheelForce * deltaTime) / this.bodyMass;
            rollingVelocity = Maths.AbsF(rollingVelocity);
            slidingVelocity = Maths.AbsF(slidingVelocity);
            float slide_friction = this.envSlideFriction * this.weightForce;
            float roll_friction = this.envRollFriction * this.weightForce;
            float airres_force = 0.5f * Track.AirResistance * this.bodyAerodynamics;
            float env_res_force = this.envResistance;
            float rollAF = ((slide_friction * this.brakePower + roll_friction * (1.0f - this.brakePower) +airres_force * rollingVelocity * rollingVelocity + env_res_force * rollingVelocity) * deltaTime) / this.bodyMass;
            float slideAF = ((slide_friction + airres_force * slidingVelocity * slidingVelocity + env_res_force * slidingVelocity) * deltaTime) / this.bodyMass;
            rollAF = Maths.ClampF(rollAF, 0.0f, rollingVelocity);
            slideAF = Maths.ClampF(slideAF, 0.0f, slidingVelocity);
            this.infoRollAntiforce = rollAF;
            this.infoSlideAntiforce = slideAF;
            this.velocity.addScaled(rollDirection, driveForce);
            float x1 = this.velocity.x;
            float y1 = this.velocity.y;
            this.velocity.addScaled(slideResDir, slideAF);
            this.velocity.addScaled(rollResDir, rollAF);
            float x2 = this.velocity.x;
            float y2 = this.velocity.y;
            if (x1 >= 0.0 && x2 < 0.0 || x1 <= 0.0 && x2 > 0.0f)
                this.velocity.x = 0.0f;
            if (y1 >= 0.0 && y2 < 0.0 || y1 <= 0.0 && y2 > 0.0f)
                this.velocity.y = 0.0f;
            this.velocity.addScaled(this.turningAccel, deltaTime);
            VectorF impactVelocity = VectorF.zero();
            this.RunCollision(impactVelocity, deltaTime);
            this.velocity.add(impactVelocity);
            this.infoImpact.set(impactVelocity);
        }

        public void Reset(VectorF pos, VectorF dir)
        {
            this.ResetBase(pos, dir);
            this.engineMaxPower = 200.0f;
            this.engineAccelerator = 0.0f;
            this.enginePowerGoal = 0.0f;
            this.enginePower = 0.0f;
            this.brakePower = 0.0f;
            this.driveWheelTorque = 0.0f;
            this.driveWheelForce = 0.0f;
            this.distanceBetweenAxles = this.bodyLength / 2.0f;
            this.stillTurningVelocity = 4.0f;
            this.driftVelocityFactor = 240.0f;
            this.steeringWheelAngle = 0.0f;
            this.turningAccel = new VectorF();
            this.bodyMass = 1.0f;
            this.bodyAerodynamics = 1.0f;
            this.hardImpactLossFactor = 0.5f;
            this.softImpactLossFactor = 0.8f;
            this.weightForce = 0.0f;
            this.envGrip = 0.0f;
            this.envSlideFriction = 0.0f;
            this.envRollFriction = 0.0f;
            this.envResistance = 0.0f;
            this.infoRollAntiforce = 0.0f;
            this.infoSlideAntiforce = 0.0f;
            this.infoImpact = new VectorF();
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.oldCollPt[i] = new VectorF();
                this.collPtHit[i] = -1;
                this.oldCollPtHit[i] = -1;
                this.collPtCarHit[i] = -1;
                this.oldCollPtCarHit[i] = -1;
            }
            this.numHits = 0;
            this.numCarHits = 0;
            this.UpdateBody();
            this.SyncCharacter();
        }

        public void Run(float deltaTime)
        {
            this.RunPhysics(deltaTime);
            this.SyncCharacter();
        }

        public void UnInit()
        {
            this.UnInitBase();
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.oldCollPt[i] = null;
            }
        }

        public VectorF DetectCollision(VectorF[] rect, VectorF otherVelocity, int otherIndex)
        {
            this.numCarHits = 0;
            VectorF p21 = VectorF.subtract(rect[1], rect[0]);
            VectorF p41 = VectorF.subtract(rect[3], rect[0]);
            float p21magnitude_squared = p21.x *p21.x + p21.y * p21.y;
            float p41magnitude_squared = p41.x *p41.x + p41.y * p41.y;
            int i = 0;
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                this.collPtCarHit[i] = -1;
                VectorF p = VectorF.subtract(this.collPoint[i], rect[0]);
                float pp21 = p.x * p21.x + p.y * p21.y;
                if (pp21 >= 0.0 && pp21 <= p21magnitude_squared)
                {
                    float pp41 = p.x * p41.x + p.y * p41.y;
                    if (pp41 >= 0.0 && pp41 <= p41magnitude_squared)
                    {
                        this.collPtCarHit[i] = otherIndex;
                        this.numCarHits += 1;
                    }
                }
            }
            if (this.numCarHits == 0)
                return null;
            VectorF impactVelocity = new VectorF();
            VectorF posImpact = VectorF.zero();
            VectorF negImpact = VectorF.zero();
            for (i = 0; i < NUM_COLLISION_POINTS; i += 1)
            {
                if (this.collPtCarHit[i] < 0)
                    continue;
                if (this.oldCollPtCarHit[i] == this.collPtCarHit[i])
                    continue;
                VectorF impact = VectorF.subtract(this.collPoint[i], this.oldCollPt[i]);
                if (impact.isZero())
                    continue;
                impact.normalize();
                float velProjection = VectorF.projection(this.velocity, impact);
                if (velProjection > 0.0f)
                {
                    impact.scale(-velProjection * (1.0f - this.softImpactLossFactor));
                    posImpact.max(impact);
                    negImpact.min(impact);
                    impact.negate();
                    impact.normalize();
                }
                float otherProjection = VectorF.projection(otherVelocity, impact);
                if (otherProjection < 0.0f)
                {
                    impact.scale(otherProjection * (1.0f - this.softImpactLossFactor));
                    posImpact.max(impact);
                    negImpact.min(impact);
                }
            }
            impactVelocity.add(posImpact);
            impactVelocity.add(negImpact);
            return impactVelocity;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class VehicleStaticRef
    {
    }

    #endregion
    
}
