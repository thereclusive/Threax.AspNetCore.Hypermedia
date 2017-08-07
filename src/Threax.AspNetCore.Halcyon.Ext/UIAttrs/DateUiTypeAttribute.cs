﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Threax.AspNetCore.Halcyon.Ext.UIAttrs
{
    /// <summary>
    /// Use this to change the ui type of a property to a date. This will
    /// cause uis to allow just date input instead of date and time.
    /// </summary>
    public class DateUiTypeAttribute : UiTypeAttribute
    {
        public DateUiTypeAttribute() : base("date")
        {
        }
    }
}
