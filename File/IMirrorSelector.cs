using System;
using System.Collections.Generic;
using System.Text;

namespace z.IO.File
{
    public interface IMirrorSelector
    {
        void Init(Downloader downloader);

        ResourceLocation GetNextResourceLocation(); 
    }
}
