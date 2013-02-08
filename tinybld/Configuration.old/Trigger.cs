namespace RobMensching.TinyBuild.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum TriggerInterval
    {
        Immediate,
        Daily,
        Weekly,
        Monthly,
    }

    public class Trigger
    {
        public Trigger()
        {
        }

        public Trigger(ParsedTriggerInterval interval, bool force)
        {
            this.Interval = (TriggerInterval)interval;
            this.Force = force;
        }

        public TriggerInterval Interval { get; set; }

        public bool Force { get; set; }
    }
}
