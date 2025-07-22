using System;
using System.Collections.Generic;

namespace Beatmapping
{
    /// <summary>
    ///     Represents a timing window for an interaction
    /// </summary>
    public class TimingWindow
    {
        public enum Score
        {
            Perfect,
            Pass,

            /// <summary>
            ///     Within failure window means instant failure
            /// </summary>
            Miss,

            /// <summary>
            ///     Timing is outside of all windows, so completely ignore it
            /// </summary>
            Invalid
        }

        public enum Direction
        {
            Early,
            Late
        }

        public double TargetTime { get; }

        /// <summary>
        ///     Timing window half widths, in seconds.
        ///     If a window's value is 0.1, then its total width is 0.2 (0.1 early, 0.1 late)
        ///     This should be sorted in ascending order when constructing.
        /// </summary>
        private readonly List<double> _windowHalfWidths;

        public TimingWindow(double targetTime, IEnumerable<double> windows)
        {
            TargetTime = targetTime;
            _windowHalfWidths = new List<double>(windows);
        }

        /// <summary>
        ///     Score a timing
        /// </summary>
        /// <param name="actualTime"></param>
        /// <returns></returns>
        public TimingResult CalculateTimingResult(double actualTime)
        {
            var result = new TimingResult();
            double delta = actualTime - TargetTime;

            result.TimeDelta = delta;
            result.Direction = delta < 0 ? Direction.Early : Direction.Late;

            double passWindowHalfWidth = _windowHalfWidths[^2];
            result.NormalizedTimeDelta = (float)(delta / passWindowHalfWidth);

            // Calculating which scoring window the timing falls into
            double distance = Math.Abs(delta);
            int windowIndex = -1;
            for (var i = 0; i < _windowHalfWidths.Count; i++)
            {
                if (distance <= _windowHalfWidths[i])
                {
                    windowIndex = i;
                    break;
                }
            }

            if (windowIndex == -1)
            {
                result.Score = Score.Invalid;
            }
            else
            {
                result.Score = (Score)windowIndex;
            }

            return result;
        }

        public struct TimingResult
        {
            public Score Score;

            /// <summary>
            ///     Time difference between target and actual. Negative if early, positive if late
            /// </summary>
            public double TimeDelta;

            public Direction Direction;
            public float NormalizedTimeDelta;
            
            public bool IsEarly() => Score == Score.Pass && Direction == Direction.Early;
            public bool IsLate() => Score == Score.Pass && Direction == Direction.Late;
            public bool IsPerfect() => Score == Score.Perfect;
        }
    }
}