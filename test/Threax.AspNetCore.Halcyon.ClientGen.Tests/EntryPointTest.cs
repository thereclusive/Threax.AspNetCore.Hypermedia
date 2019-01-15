﻿using Halcyon.HAL.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NJsonSchema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Threax.AspNetCore.Halcyon.Ext;
using Threax.AspNetCore.Tests;
using Xunit;

namespace Threax.AspNetCore.Halcyon.ClientGen.Tests
{
    public class EntryPointTest : FileGenTests<Object, EntryPointTest.EntryPoint>
    {
        [HalModel]
        [HalEntryPoint]
        public partial class EntryPoint
        {
        }

        public class EntryPointController : Controller
        {
            public class Rels
            {
                public const String Get = "GetEntryPoint";
            }

            [HttpGet]
            [HalRel(Rels.Get)]
            public EntryPoint Get()
            {
                return new EntryPoint();
            }
        }
    }
}
