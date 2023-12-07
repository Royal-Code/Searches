
// only for .netstandard 2.1
#if NETSTANDARD2_1

namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    /// <summary>
    /// Reserved to be used by the compiler for tracking meta-data.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}

#endif