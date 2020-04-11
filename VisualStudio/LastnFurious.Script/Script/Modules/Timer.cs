// Module_Timer - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Timer;
using static LastnFurious.TimerStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Timer
    {
        // Fields
        public Timer[] Timers = new Timer[MAX_TIMERS];
        public float GameTickTime;

        // Methods
        public int FindFreeSlot()
        {
            int i = 0;
            for (i = 0; i < MAX_TIMERS; i += 1)
            {
                if (Timers[i] == null)
                    return i;
            }
            return -1;
        }

        public Timer StartTimer(bool realtime, float timeout, RepeatStyle repeat)
        {
            int id = FindFreeSlot();
            if (id == -1)
            {
                Display("Timer.asc: timers limit reached, cannot start another timer before any of the active ones has stopped.");
                return null;
            }
            Timer timer = new Timer();
            timer.Init(id, realtime, timeout, repeat);
            Timers[id] = timer;
            return timer;
        }

        public void repeatedly_execute_always()
        {
            if (IsGamePaused())
                return;
            GameTickTime = 1.0f / IntToFloat(GetGameSpeed());
            int i = 0;
            for (i = 0; i < MAX_TIMERS; i += 1)
            {
                Timer timer = Timers[i];
                if (timer != null)
                {
                    if (!timer.Countdown())
                    {
                        timer.RemoveRef();
                    }
                }
            }
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int MAX_TIMERS = 20;


    }

    #endregion

    #region Timer (AGS managed struct from .ash converted to class)

    public class Timer
    {
        // Fields
        public int _id;
        public bool _realtime;
        public float _timeout;
        public bool _repeat;
        public float _remains;
        public bool _evt;

        // Properties
        public bool IsActive
        {
            get
            {
                return this._id >= 0;
            }
        }

        public bool EvtExpired
        {
            get
            {
                return this._evt;
            }
        }

        // Methods
        public void RemoveRef()
        {
            if (this._id >= 0)
            {
                Timers[this._id] = null;
                this._id = -1;
            }
        }

        public void Stop()
        {
            this.RemoveRef();
            this._evt = false;
        }

        public void Init(int id, bool realtime, float timeout, RepeatStyle repeat)
        {
            this._id = id;
            this._realtime = realtime;
            this._timeout = timeout;
            this._repeat = repeat == RepeatStyle.eRepeat ? true : false;
            this._remains = timeout;
            this._evt = false;
        }

        public static Timer Start(int timeout, RepeatStyle repeat = eOnce)
        {
            return StartTimer(false, IntToFloat(timeout), repeat);
        }

        public static Timer StartRT(float timeout_s, RepeatStyle repeat = eOnce)
        {
            return StartTimer(true, timeout_s, repeat);
        }

        public static bool HasExpired(Timer t)
        {
            return t != null && t.EvtExpired;
        }

        public static void StopIt(Timer t)
        {
            if (t != null)
                t.Stop();
        }

        public bool Countdown()
        {
            if (this._realtime)
                this._remains -= GameTickTime;
            else 
                this._remains -= 1.0f;
            if (this._remains <= TINY_FLOAT)
            {
                this._evt = true;
                if (this._repeat)
                    this._remains = this._timeout;
                return this._repeat;
            }
            this._evt = false;
            return true;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class TimerStaticRef
    {
        // Static Fields
        public static Timer[] Timers { get { return GlobalBase.TimerInstance.Timers; } set { GlobalBase.TimerInstance.Timers = value; } }
        public static float GameTickTime { get { return GlobalBase.TimerInstance.GameTickTime; } set { GlobalBase.TimerInstance.GameTickTime = value; } }

        // Static Methods
        public static int FindFreeSlot()
        {
            return GlobalBase.TimerInstance.FindFreeSlot();
        }

        public static Timer StartTimer(bool realtime, float timeout, RepeatStyle repeat)
        {
            return GlobalBase.TimerInstance.StartTimer(realtime, timeout, repeat);
        }

    }

    #endregion
    
}
