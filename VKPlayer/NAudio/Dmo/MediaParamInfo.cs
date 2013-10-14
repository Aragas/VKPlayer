using System;
using System.Runtime.InteropServices;

namespace NAudio.Dmo
{
    /// <summary>
    ///     MP_PARAMINFO
    /// </summary>
    internal struct MediaParamInfo
    {
        public MediaParamCurveType mopCaps;
        public MediaParamType mpType;
        public float mpdMaxValue;
        public float mpdMinValue; // MP_DATA is a float
        public float mpdNeutralValue;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szLabel;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szUnitText;
    }

    /// <summary>
    ///     MP_TYPE
    /// </summary>
    internal enum MediaParamType
    {
        /// <summary>
        ///     MPT_INT
        /// </summary>
        Int,

        /// <summary>
        ///     MPT_FLOAT
        /// </summary>
        Float,

        /// <summary>
        ///     MPT_BOOL
        /// </summary>
        Bool,

        /// <summary>
        ///     MPT_ENUM
        /// </summary>
        Enum,

        /// <summary>
        ///     MPT_MAX
        /// </summary>
        Max,
    }

    /// <summary>
    ///     MP_CURVE_TYPE
    /// </summary>
    [Flags]
    internal enum MediaParamCurveType
    {
        MP_CURVE_JUMP = 0x1,
        MP_CURVE_LINEAR = 0x2,
        MP_CURVE_SQUARE = 0x4,
        MP_CURVE_INVSQUARE = 0x8,
        MP_CURVE_SINE = 0x10
    }
}