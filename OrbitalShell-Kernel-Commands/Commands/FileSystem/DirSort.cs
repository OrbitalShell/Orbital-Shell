using System;

namespace OrbitalShell.Commands.FileSystem
{
    [Flags]
    public enum DirSort
    {
        /// <summary>
        /// sort not specified
        /// </summary>
        not_specified = 0,

        /// <summary>
        /// sort by name or fullname
        /// </summary>
        name = 1,

        /// <summary>
        /// sort by size
        /// </summary>
        size = 2,

        /// <summary>
        /// sort by extension
        /// </summary>
        ext = 4,

        /// <summary>
        /// files on top
        /// </summary>
        file = 8,

        /// <summary>
        /// dirs on top
        /// </summary>
        dir = 16,

        /// <summary>
        /// reverse sort
        /// </summary>
        rev = 32
    }

}
