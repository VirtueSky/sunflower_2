﻿using System;
using System.Diagnostics;

namespace VirtueSky.Inspector
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class PropertyTooltipAttribute : Attribute
    {
        public string Tooltip { get; }

        public PropertyTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}