// Module_Camera - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Camera;
using static LastnFurious.CameraStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Camera
    {
        // Fields
        public CameraImpl CameraData = new CameraImpl();
        public ZoomPlaceholder ZoomDebug = new ZoomPlaceholder();
        public FreeLookImpl FreeLookData = new FreeLookImpl();
        public CameraActionImpl CameraActionData = new CameraActionImpl();

        // Methods
        public void game_start()
        {
            CameraData.ResetAll();
            CameraData.DefaultTarget = true;
            FreeLookData.Reset();
            CameraActionData.Reset();
        }

        public void late_repeatedly_execute_always()
        {
            if (IsGamePaused())
                return;
            if (!CameraData.DefaultTarget)
                CameraData.Update();
            if (FreeLookData.On)
                FreeLookData.Update();
            if (CameraActionData.Do)
                CameraActionData.Update();
            if (CameraData.Zoom != 100.0f)
                ZoomDebug.Update();
        }

        public void on_event(EventType eventVar, int data)
        {
            if (eventVar == eEventEnterRoomBeforeFadein)
            {
                CameraData.OnRoomInit();
                Camera2.TargetCharacter = player;
                Camera2.Snap();
            }
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region CameraImpl (AGS struct from .asc converted to class)

    public class CameraImpl
    {
        // Fields
        public float TargettingAcceleration;
        public bool DefaultTarget;
        public Character TargetCharacter;
        public Object TargetObject;
        public Point TargetLocation = new Point();
        public bool StaticTarget;
        public float Zoom;
        public float Width2;
        public float Height2;
        public float X1;
        public float X2;
        public float Y1;
        public float Y2;
        public float CameraX;
        public float CameraY;
        public bool LockedToTarget;
        public float TargetX;
        public float TargetY;
        public float OldTargetX;
        public float OldTargetY;
        public float PanToX;
        public float PanToY;
        public float SpeedX;
        public float SpeedY;

        // Methods
        public void UpdateViewport()
        {
            SetViewport(FloatToInt(this.CameraX - this.Width2, eRoundNearest), FloatToInt(this.CameraY - this.Height2, eRoundNearest));
        }

        public bool ImposeBounds()
        {
            float limx = this.CameraX;
            float limy = this.CameraY;
            if (this.CameraX < this.X1)
                limx = this.X1;
            else if (this.CameraX > this.X2)
                limx = this.X2;
            if (this.CameraY < this.Y1)
                limy = this.Y1;
            else if (this.CameraY > this.Y2)
                limy = this.Y2;
            bool was_adj = false;
            if (limx != this.CameraX || limy != this.CameraY)
            {
                this.CameraX = limx;
                this.CameraY = limy;
                was_adj = true;
            }
            this.UpdateViewport();
            return was_adj;
        }

        public void SetFromViewport()
        {
            this.CameraX = IntToFloat(GetViewportX()) + this.Width2;
            this.CameraY = IntToFloat(GetViewportY()) + this.Height2;
            this.ImposeBounds();
        }

        public void TakeControl()
        {
            CameraData.SetFromViewport();
            SetViewport(GetViewportX(), GetViewportY());
        }

        public void ReleaseControl()
        {
            ReleaseViewport();
        }

        public void ResetTarget()
        {
            this.DefaultTarget = false;
            this.TargetCharacter = null;
            this.TargetObject = null;
            this.TargetLocation = null;
            this.StaticTarget = false;
            this.LockedToTarget = false;
            this.TargetX = -1.0f;
            this.TargetY = -1.0f;
            this.OldTargetX = -1.0f;
            this.OldTargetY = -1.0f;
        }

        public void ResetMovement()
        {
            this.OldTargetX = -1.0f;
            this.OldTargetY = -1.0f;
            this.SpeedX = 0.0f;
            this.SpeedY = 0.0f;
        }

        public void ResetAll()
        {
            this.Width2 = 0.0f;
            this.Height2 = 0.0f;
            this.X1 = 0.0f;
            this.X2 = 0.0f;
            this.Y1 = 0.0f;
            this.Y2 = 0.0f;
            this.Zoom = 100.0f;
            this.ResetTarget();
            this.ResetMovement();
            this.TargettingAcceleration = 0.0f;
            this.CameraX = -1.0f;
            this.CameraY = -1.0f;
        }

        public void OnRoomInit()
        {
            this.Width2 = IntToFloat(system.ViewportWidth) / 2.0f;
            this.Height2 = IntToFloat(system.ViewportHeight) / 2.0f;
            this.X1 = 0.0f;
            this.X2 = IntToFloat(Room.Width);
            this.Y1 = 0.0f;
            this.Y2 = IntToFloat(Room.Height);
            this.ResetMovement();
        }

        public void SetTo(int x, int y)
        {
            this.CameraX = IntToFloat(x);
            this.CameraY = IntToFloat(y);
            this.ImposeBounds();
        }

        public void SetToF(float x, float y)
        {
            this.CameraX = x;
            this.CameraY = y;
            this.ImposeBounds();
        }

        public void SetViewportTo(int x, int y)
        {
            SetViewport(x, y);
            this.SetFromViewport();
        }

        public bool DoMove(float x, float y, float accel)
        {
            float dist = Maths.Sqrt(Maths.RaiseToPower(x - this.CameraX, 2.0f) + Maths.RaiseToPower(y - this.CameraY, 2.0f));
            if (dist <= TINY_FLOAT)
            {
                this.CameraX = x;
                this.CameraY = y;
            }
            else 
            {
                float dirx = 1.0f;
                float diry = 1.0f;
                if (this.TargetX < this.CameraX)
                    dirx = -1.0f;
                if (this.TargetY < this.CameraY)
                    diry = -1.0f;
                if (this.SpeedX < 0.0 && dirx >= 0.0 || this.SpeedX > 0.0 && dirx < 0.0f)
                    this.SpeedX = 0.0f;
                if (this.SpeedY < 0.0 && diry >= 0.0 || this.SpeedY > 0.0 && diry < 0.0f)
                    this.SpeedY = 0.0f;
                if (accel > 0.0f)
                {
                    this.SpeedX += accel * dirx;
                    this.SpeedY += accel * diry;
                }
                if (dirx >= 0.0 && this.CameraX + this.SpeedX > x || dirx <= 0.0 && this.CameraX + this.SpeedX < x)
                    this.CameraX = x;
                else 
                    this.CameraX += this.SpeedX;
                if (diry >= 0.0 && this.CameraY + this.SpeedY > y || diry <= 0.0 && this.CameraY + this.SpeedY < y)
                    this.CameraY = y;
                else 
                    this.CameraY += this.SpeedY;
            }
            return this.ImposeBounds();
        }

        public bool UpdateTarget()
        {
            if (this.TargetCharacter != null)
            {
                this.TargetX = IntToFloat(this.TargetCharacter.x);
                this.TargetY = IntToFloat(this.TargetCharacter.y);
            }
            else if (this.TargetObject != null)
            {
                this.TargetX = IntToFloat(this.TargetObject.X);
                this.TargetY = IntToFloat(this.TargetObject.Y);
            }
            else if (this.TargetLocation != null)
            {
                this.TargetX = IntToFloat(this.TargetLocation.x);
                this.TargetY = IntToFloat(this.TargetLocation.y);
            }
            else if (!this.StaticTarget)
                return false;
            return true;
        }

        public void Update()
        {
            if (!this.UpdateTarget())
                return;
            if (!this.LockedToTarget)
            {
                this.OldTargetX = this.TargetX;
                this.OldTargetY = this.TargetY;
                return;
            }
            bool snapped = this.CameraX == this.OldTargetX && this.CameraY == this.OldTargetY;
            if (snapped)
            {
                this.SetToF(this.TargetX, this.TargetY);
            }
            else if (this.TargettingAcceleration <= TINY_FLOAT)
            {
                this.SetToF(this.TargetX, this.TargetY);
            }
            else 
            {
                this.DoMove(this.TargetX, this.TargetY, this.TargettingAcceleration);
            }
            this.OldTargetX = this.TargetX;
            this.OldTargetY = this.TargetY;
        }

    }

    #endregion

    #region ZoomPlaceholder (AGS struct from .asc converted to class)

    public class ZoomPlaceholder
    {
        // Fields
        public DynamicSprite Rect;
        public Overlay O;
        public float Width2;
        public float Height2;

        // Methods
        public void Remove()
        {
            if (this.O != null && this.O.Valid)
                this.O.Remove();
            this.O = null;
            if (this.Rect != null)
                this.Rect.Delete();
            this.Rect = null;
        }

        public void Create(float perc)
        {
            this.Remove();
            float sys_w = IntToFloat(system.ViewportWidth);
            float sys_h = IntToFloat(system.ViewportHeight);
            this.Width2 = (sys_w * perc / 100.0f) / 2.0f;
            float w = sys_w * perc / 100.0f;
            float h = sys_h * perc / 100.0f;
            this.Width2 = w / 2.0f;
            this.Height2 = h / 2.0f;
            float x = CameraData.CameraX - this.Width2;
            float y = CameraData.CameraY - this.Height2;
            this.Rect = DynamicSprite.Create(FloatToInt(w), FloatToInt(h), false);
            DrawingSurface ds = this.Rect.GetDrawingSurface();
            ds.Clear();
            ds.DrawingColor = Game.GetColorFromRGB(0, 50, 200);
            ds.DrawFrame(0, 0, ds.Width - 1, ds.Height - 1);
            ds.DrawString(2, 2, eFontNotalot35Regular12, StringFormatAGS("Zoom: %0.1f%%", perc));
            ds.Release();
            this.O = Overlay.CreateGraphical(FloatToInt(x, eRoundNearest) - GetViewportX(),FloatToInt(y, eRoundNearest) - GetViewportY(),this.Rect.Graphic, true);
        }

        public void Update()
        {
            if (this.O == null || !this.O.Valid)
                return;
            float x = CameraData.CameraX - this.Width2;
            float y = CameraData.CameraY - this.Height2;
            this.O.X = FloatToInt(x, eRoundNearest) - GetViewportX();
            this.O.Y = FloatToInt(y, eRoundNearest) - GetViewportY();
        }

    }

    #endregion

    #region FreeLookImpl (AGS struct from .asc converted to class)

    public class FreeLookImpl
    {
        // Fields
        public bool On;

        // Methods
        public void Reset()
        {
            this.On = false;
        }

        public void Update()
        {
            int x = GetViewportX(), y = GetViewportY();
            int w = system.ViewportWidth;
            int h = system.ViewportHeight;
            int x1 = 100;
            int x2 = system.ViewportWidth - 100;
            int y1 = 75;
            int y2 = system.ViewportHeight - 75;
            if (mouse.x > x2)
            {
                x+=((mouse.x-x2)/4);
                if ((x+w)>Room.Width)
                    x = Room.Width-w;
            }
            else if (mouse.x < x1)
            {
                x-=((x1-mouse.x)/4);
                if (x<0)
                    x = 0;
            }
            if (mouse.y > y2)
            {
                y+=((mouse.y-y2)/3);
                if ((y+h)>Room.Height)
                    y = Room.Height-h;
            }
            else if (mouse.y < y1)
            {
                y-=((y1-mouse.y)/3);
                if (y<0)
                    y = 0;
            }
            SetViewport(x, y);
        }

    }

    #endregion

    #region CameraActionImpl (AGS struct from .asc converted to class)

    public class CameraActionImpl
    {
        // Fields
        public bool Do;
        public float PanToX;
        public float PanToY;
        public float Accel;

        // Methods
        public void Reset()
        {
            this.Do = false;
            this.PanToX = -1.0f;
            this.PanToY = -1.0f;
            this.Accel = 0.0f;
        }

        public void Update()
        {
            bool canmove = CameraData.DoMove(this.PanToX, this.PanToY, this.Accel);
            if (!canmove || CameraData.CameraX == this.PanToX && CameraData.CameraY == this.PanToY)
                this.Do = false;
        }

    }

    #endregion

    #region Camera (AGS struct from .ash converted to class)

    public class Camera2
    {
        // Properties
        public static float TargettingAcceleration
        {
            get
            {
                return CameraData.TargettingAcceleration;
            }
            set
            {
                var f = value;
                CameraData.TargettingAcceleration = f;
            }
        }

        public static bool DefaultTarget
        {
            get
            {
                return CameraData.DefaultTarget;
            }
            set
            {
                var on = value;
                if (on)
                {
                    CameraData.ResetTarget();
                    CameraData.LockedToTarget = true;
                    CameraData.ReleaseControl();
                }
                else if (CameraData.DefaultTarget)
                {
                    CameraData.TakeControl();
                }
                CameraData.DefaultTarget = on;
            }
        }

        public static Character TargetCharacter
        {
            get
            {
                return CameraData.TargetCharacter;
            }
            set
            {
                var c = value;
                if (c == null && CameraData.DefaultTarget)
                    return;
                CameraData.TakeControl();
                CameraData.ResetTarget();
                CameraData.LockedToTarget = true;
                CameraData.TargetCharacter = c;
            }
        }

        public static Object TargetObject
        {
            get
            {
                return CameraData.TargetObject;
            }
            set
            {
                var o = value;
                if (o == null && CameraData.DefaultTarget)
                    return;
                CameraData.TakeControl();
                CameraData.ResetTarget();
                CameraData.LockedToTarget = true;
                CameraData.TargetObject = o;
            }
        }

        public static Point TargetLocation
        {
            get
            {
                return CameraData.TargetLocation;
            }
            set
            {
                var p = value;
                if (p == null && CameraData.DefaultTarget)
                    return;
                CameraData.TakeControl();
                CameraData.ResetTarget();
                CameraData.LockedToTarget = true;
                CameraData.TargetLocation = p;
            }
        }

        public static bool StaticTarget
        {
            get
            {
                return CameraData.LockedToTarget;
            }
        }

        public static float Zoom
        {
            get
            {
                return CameraData.Zoom;
            }
            set
            {
                var f = value;
                CameraData.Zoom = f;
                if (CameraData.Zoom == 100.0f)
                    ZoomDebug.Remove();
                else 
                    ZoomDebug.Create(f);
            }
        }

        public static int CameraX
        {
            get
            {
                return FloatToInt(CameraData.CameraX, eRoundNearest);
            }
        }

        public static int CameraY
        {
            get
            {
                return FloatToInt(CameraData.CameraY, eRoundNearest);
            }
        }

        public static int CameraWidth
        {
            get
            {
                return FloatToInt(CameraData.Width2, eRoundNearest) * 2;
            }
        }

        public static int CameraHeight
        {
            get
            {
                return FloatToInt(CameraData.Height2, eRoundNearest) * 2;
            }
        }

        public static bool LockedToTarget
        {
            get
            {
                return CameraData.LockedToTarget;
            }
        }

        public static int TargetX
        {
            get
            {
                return FloatToInt(CameraData.TargetX, eRoundNearest);
            }
        }

        public static int TargetY
        {
            get
            {
                return FloatToInt(CameraData.TargetY, eRoundNearest);
            }
        }

        // Methods
        public static void SetStaticTarget(int x, int y)
        {
            CameraData.ResetTarget();
            CameraData.LockedToTarget = true;
            CameraData.StaticTarget = true;
            CameraData.TargetX = IntToFloat(x);
            CameraData.TargetY = IntToFloat(y);
        }

        public static void SetBounds(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                int a = x1;
                x1 = x2;
                x2 = a;
            }
            if (y1 > y2)
            {
                int a = y1;
                y1 = y2;
                y2 = a;
            }
            if (x1 < 0)
                x1 = 0;
            else if (x1 > Room.Width)
                x1 = Room.Width;
            if (x2 < 0)
                x2 = 0;
            else if (x2 > Room.Width)
                x2 = Room.Width;
            if (y1 < 0)
                y1 = 0;
            else if (y1 > Room.Height)
                y1 = Room.Height;
            if (y2 < 0)
                y2 = 0;
            else if (y2 > Room.Height)
                y2 = Room.Height;
            CameraData.X1 = IntToFloat(x1);
            CameraData.X2 = IntToFloat(x2);
            CameraData.Y1 = IntToFloat(y1);
            CameraData.Y2 = IntToFloat(y2);
            CameraData.ImposeBounds();
        }

        public static void ResetBounds()
        {
            CameraData.X1 = 0.0f;
            CameraData.X2 = IntToFloat(Room.Width);
            CameraData.Y1 = 0.0f;
            CameraData.Y2 = IntToFloat(Room.Height);
            CameraData.ImposeBounds();
        }

        public static void Lock()
        {
            CameraData.LockedToTarget = true;
            if (CameraData.DefaultTarget)
                CameraData.ReleaseControl();
        }

        public static void Release()
        {
            CameraData.LockedToTarget = false;
            if (CameraData.DefaultTarget)
                CameraData.TakeControl();
        }

        public static void Snap()
        {
            if (CameraData.LockedToTarget)
            {
                CameraData.UpdateTarget();
                CameraData.SetToF(CameraData.TargetX, CameraData.TargetY);
                CameraData.OldTargetX = CameraData.TargetX;
                CameraData.OldTargetY = CameraData.TargetY;
            }
        }

        public static void CenterAt(int x, int y)
        {
            CameraData.SetTo(x, y);
            if (!CameraData.LockedToTarget && CameraData.DefaultTarget)
                CameraData.ReleaseControl();
        }

        public static void ViewportAt(int x, int y)
        {
            CameraData.SetViewportTo(x, y);
            if (!CameraData.LockedToTarget && CameraData.DefaultTarget)
                CameraData.ReleaseControl();
        }

    }

    #endregion

    #region FreeLook (AGS struct from .ash converted to class)

    public class FreeLook
    {
        // Properties
        public static bool Enabled
        {
            get
            {
                return FreeLookData.On;
            }
            set
            {
                var on = value;
                if (on == FreeLookData.On)
                    return;
                if (on)
                    Camera2.Release();
                else 
                    Camera2.Lock();
                FreeLookData.On = on;
            }
        }

    }

    #endregion

    #region CameraAction (AGS struct from .ash converted to class)

    public class CameraAction
    {
        // Methods
        public static void Pos(int x, int y)
        {
            Camera2.Release();
            CameraData.SetTo(x, y);
        }

        public static void Pan(int x, int y, float speed, float accel, BlockingStyle block)
        {
            Camera2.Release();
            CameraActionData.Do = true;
            CameraActionData.PanToX = IntToFloat(x);
            CameraActionData.PanToY = IntToFloat(y);
            CameraActionData.Accel = accel;
            if (CameraActionData.PanToX >= CameraData.CameraX)
                CameraData.SpeedX = speed;
            else 
                CameraData.SpeedX = -speed;
            if (CameraActionData.PanToY >= CameraData.CameraY)
                CameraData.SpeedY = speed;
            else 
                CameraData.SpeedY = -speed;
            if (block == eBlock)
            {
                while (CameraActionData.Do)
                {
                    Wait(1);
                }
            }
        }

        public static void LinearZoomOnto(int x, int y, float next_zoom, float reach_at_zoom)
        {
            Camera2.Release();
            float at_xf = CameraData.CameraX;
            float at_yf = CameraData.CameraY;
            float end_xf = IntToFloat(x);
            float end_yf = IntToFloat(y);
            float dist_x = end_xf - at_xf;
            float dist_y = end_yf - at_yf;
            float dist_z = reach_at_zoom - CameraData.Zoom;
            float step_z = next_zoom - CameraData.Zoom;
            float xf = at_xf + dist_x * step_z / dist_z;
            float yf = at_yf + dist_y * step_z / dist_z;
            CameraData.SetToF(xf, yf);
            Camera2.Zoom = next_zoom;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class CameraStaticRef
    {
        // Static Fields
        public static CameraImpl CameraData { get { return GlobalBase.CameraInstance.CameraData; } set { GlobalBase.CameraInstance.CameraData = value; } }
        public static ZoomPlaceholder ZoomDebug { get { return GlobalBase.CameraInstance.ZoomDebug; } set { GlobalBase.CameraInstance.ZoomDebug = value; } }
        public static FreeLookImpl FreeLookData { get { return GlobalBase.CameraInstance.FreeLookData; } set { GlobalBase.CameraInstance.FreeLookData = value; } }
        public static CameraActionImpl CameraActionData { get { return GlobalBase.CameraInstance.CameraActionData; } set { GlobalBase.CameraInstance.CameraActionData = value; } }

        // Static Methods
        public static void late_repeatedly_execute_always()
        {
            GlobalBase.CameraInstance.late_repeatedly_execute_always();
        }

    }

    #endregion
    
}
