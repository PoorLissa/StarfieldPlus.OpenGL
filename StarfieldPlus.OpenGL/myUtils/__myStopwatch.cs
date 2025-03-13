// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace StarfieldPlus.OpenGL.myUtils
{
    using System.Diagnostics;
    using System.Threading;

    public class myStopwatch
    {
        private Stopwatch _stopwatch = null;
        private int _sleepTime;
        private long _targetFrameTime;

        public myStopwatch(bool doStart = false, long frameRate = 13)
        {
            _targetFrameTime = frameRate;
            _stopwatch = new Stopwatch();

            if (doStart)
            {
                Start();
            }
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void MakeFaster()
        {
            if (_targetFrameTime > 0)
                _targetFrameTime--;
        }

        public void MakeSlower()
        {
            _targetFrameTime++;
        }

        public int GetRate()
        {
            return (int)_targetFrameTime;
        }

        public void WaitAndRestart()
        {
            // Calculate the time to sleep to maintain a consistent frame rate
            _sleepTime = (int)(_targetFrameTime - _stopwatch.ElapsedMilliseconds);

            if (_sleepTime > -1)
                Thread.Sleep(_sleepTime);

            _stopwatch.Restart();
        }
    }
}
