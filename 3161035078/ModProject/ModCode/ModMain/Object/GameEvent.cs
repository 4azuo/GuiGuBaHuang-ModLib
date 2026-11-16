using ModLib.Helper;

namespace MOD_nE7UL2.Object
{
    /// <summary>
    /// Periodic game event window (start month, duration, period).
    /// </summary>
    public class GameEvent
    {
        public string Name { get; set; }
        public int Start { get; set; }
        public int Dur { get; set; }
        public int Period { get; set; }

        /// <summary>
        /// True when the current month is inside a window.
        /// </summary>
        public bool IsRunningEvent()
        {
            var curMonth = g.world.run.roundMonth + 1;
            for (int i = 0; true; i++)
            {
                var from = i * Period + Start;
                var to = from + (Dur - 1);
                if (ValueHelper.IsBetween(curMonth, from, to))
                    return true;
                if (from > curMonth)
                    return false;
            }
        }
    }
}
