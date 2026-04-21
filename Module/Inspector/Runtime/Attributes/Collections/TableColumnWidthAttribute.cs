using System;
using System.Diagnostics;

namespace VirtueSky.Inspector
{
    /// <summary>
    /// Đặt độ rộng cột khi field được hiển thị trong [TableList].
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class TableColumnWidthAttribute : Attribute
    {
        public float Width    { get; }
        public float MinWidth { get; }
        public float MaxWidth { get; }
        public bool  AutoResize { get; }

        /// <param name="width">Độ rộng ban đầu của cột.</param>
        /// <param name="minWidth">Độ rộng tối thiểu (mặc định = width).</param>
        /// <param name="maxWidth">Độ rộng tối đa (mặc định = không giới hạn).</param>
        /// <param name="autoResize">Cho phép auto-resize khi ResizeToFit (mặc định false).</param>
        public TableColumnWidthAttribute(float width, float minWidth = -1f, float maxWidth = float.MaxValue, bool autoResize = false)
        {
            Width      = width;
            MinWidth   = minWidth < 0 ? width : minWidth;
            MaxWidth   = maxWidth;
            AutoResize = autoResize;
        }
    }
}