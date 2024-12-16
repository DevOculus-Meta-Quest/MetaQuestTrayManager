using System;
using System.Collections.Generic;
using System.Timers;

namespace MetaQuestTrayManager.Utils
{
    /// <summary>
    /// Manages multiple timers in a thread-safe manner for periodic or delayed task execution.
    /// </summary>
    public static class TimerManager
    {
        // Lock object to ensure thread safety for accessing the Timers dictionary.
        private static readonly object TimerLock = new object();

        // Dictionary to store timers, identified by unique timer IDs.
        private static Dictionary<string, System.Timers.Timer> Timers = new Dictionary<string, System.Timers.Timer>();

        /// <summary>
        /// Sets a new interval for an existing timer.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer.</param>
        /// <param name="interval">The new time interval for the timer.</param>
        /// <returns>True if the timer exists and the interval was updated; otherwise, false.</returns>
        public static bool SetNewInterval(string timerID, TimeSpan interval)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));

            lock (TimerLock)
            {
                if (Timers.ContainsKey(timerID))
                {
                    var timer = Timers[timerID];
                    timer.Interval = interval.TotalMilliseconds;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new timer with the specified settings.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer.</param>
        /// <param name="interval">The interval at which the timer should trigger events.</param>
        /// <param name="tickHandler">The event handler to execute when the timer elapses.</param>
        /// <param name="repeat">True if the timer should repeat; false for a single execution.</param>
        /// <returns>True if the timer was created successfully; otherwise, false.</returns>
        public static bool CreateTimer(string timerID, TimeSpan interval, ElapsedEventHandler tickHandler, bool repeat = true)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));
            if (tickHandler == null) throw new ArgumentNullException(nameof(tickHandler));

            lock (TimerLock)
            {
                if (!Timers.ContainsKey(timerID))
                {
                    var timer = new System.Timers.Timer
                    {
                        Interval = interval.TotalMilliseconds,
                        AutoReset = repeat,
                        Enabled = false
                    };

                    timer.Elapsed += tickHandler;
                    Timers.Add(timerID, timer);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts an existing timer.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer to start.</param>
        /// <returns>True if the timer was found and started; otherwise, false.</returns>
        public static bool StartTimer(string timerID)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));

            lock (TimerLock)
            {
                if (Timers.ContainsKey(timerID))
                {
                    var timer = Timers[timerID];
                    timer.Start();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Stops an existing timer.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer to stop.</param>
        /// <returns>True if the timer was found and stopped; otherwise, false.</returns>
        public static bool StopTimer(string timerID)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));

            lock (TimerLock)
            {
                if (Timers.ContainsKey(timerID))
                {
                    var timer = Timers[timerID];
                    timer.Stop();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a timer with the specified ID exists.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer to check.</param>
        /// <returns>True if the timer exists; otherwise, false.</returns>
        public static bool TimerExists(string timerID)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));

            lock (TimerLock)
                return Timers.ContainsKey(timerID);
        }

        /// <summary>
        /// Disposes of a specific timer and removes it from the manager.
        /// </summary>
        /// <param name="timerID">The unique identifier of the timer to dispose.</param>
        public static void DisposeTimer(string timerID)
        {
            if (string.IsNullOrEmpty(timerID)) throw new ArgumentNullException(nameof(timerID));

            lock (TimerLock)
            {
                if (Timers.ContainsKey(timerID))
                {
                    var timer = Timers[timerID];
                    Timers.Remove(timerID);

                    timer.Stop();
                    timer.Dispose();
                }
            }
        }

        /// <summary>
        /// Disposes of all timers managed by the TimerManager.
        /// </summary>
        public static void DisposeAllTimers()
        {
            lock (TimerLock)
            {
                foreach (var timer in Timers.Values)
                {
                    timer.Stop();
                    timer.Dispose();
                }

                Timers.Clear();
            }
        }
    }
}
