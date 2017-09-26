using System;
using System.Collections.Generic;
using System.Text;

namespace z.IO.File
{
    public enum SegmentState
    {
        Idle,
        Connecting,
        Downloading,
        Paused,
        Finished,
        Error,
    }
}
