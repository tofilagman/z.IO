using System;
using System.Collections.Generic;
using System.Text;
using z.IO.File;

namespace z.IO.File.Extensions
{
    public interface IExtension
    {
        string Name { get; }

        IUIExtension UIExtension { get; }
    }
}
