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
    public partial class ValueEntity : IValue, IValueId, ICreatedModified
    {
        [Key]
        public Guid ValueId { get; set; }

        public bool Checkbox { get; set; }

        public DateTime DateOnly { get; set; }

        public String Hidden { get; set; }

        public String Password { get; set; }

        public String Select { get; set; }

        public String TextArea { get; set; }

        public String CustomType { get; set; }

        public bool CheckboxOverride { get; set; }

        public DateTime DateOnlyOverride { get; set; }

        public String HiddenOverride { get; set; }

        public String PasswordOverride { get; set; }

        public String SelectOverride { get; set; }

        public String TextAreaOverride { get; set; }

        public String CustomTypeOverride { get; set; }

        public bool CheckboxOverrideSelectAll { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

    }
}
