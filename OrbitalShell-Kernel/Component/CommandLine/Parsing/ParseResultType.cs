﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public enum ParseResultType
    {
        Valid,
        NotValid,
        NotIdentified,
        Empty,
        Ambiguous,
        SyntaxError
    }
}
