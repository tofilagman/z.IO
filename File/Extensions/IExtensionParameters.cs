using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace z.IO.File.Extensions
{
    public interface IExtensionParameters
    {
        event PropertyChangedEventHandler ParameterChanged;
    }
}
