using Halcyon.HAL.Attributes;
using Test.Controllers.Api;
using Test.InputModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threax.AspNetCore.Halcyon.Ext;

namespace Test.ViewModels
{
    public partial class LeftCollection : PagedCollectionViewWithQuery<Left, LeftQuery>
    {
        
    }
}