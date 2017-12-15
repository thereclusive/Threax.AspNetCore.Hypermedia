using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Halcyon.HAL.Attributes;
using Threax.AspNetCore.Halcyon.Ext;
using Threax.AspNetCore.Models;
using Threax.AspNetCore.Tracking;
using Test.Models;

namespace Test.Database 
{
    public partial class ValueEntity : IValue, IValueId, IValue_Info, ICreatedModified
    {
        [Key]
        public Guid ValueId { get; set; }

        public String Info { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

    }
}
