using System;
using System.Collections.Generic;
using System.Text;

namespace z.IO.File
{
    public interface ISegmentCalculator
    {
        CalculatedSegment[] GetSegments(int segmentCount, RemoteFileInfo fileSize);
    }
}
