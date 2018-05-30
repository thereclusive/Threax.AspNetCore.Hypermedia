﻿using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Text;
using Threax.AspNetCore.Models;

namespace Threax.ModelGen.TestGenerators
{
    public class ModelTestWrapperGenerated
    {
        public static String GetFileName(JsonSchema4 schema)
        {
            return $"{schema.Title}/{schema.Title}Tests.Generated.cs";
        }

        public static String Get(JsonSchema4 schema, String ns)
        {
            String Model, model;
            NameGenerator.CreatePascalAndCamel(schema.Title, out Model, out model);
            String Models, models;
            NameGenerator.CreatePascalAndCamel(schema.GetPluralName(), out Models, out models);
            String createArgs = "";

            var equalAssertFunc = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new ModelEqualityAssert(), schema, ns, ns);

            var createInputFunc = "";
            if (schema.CreateInputModel())
            {
                createArgs = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new ModelCreateArgs(), schema, ns, ns, p => p.CreateInputModel());
                createInputFunc = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new CreateInputModel(createArgs), schema, ns, ns, p => p.CreateInputModel());
            }

            var createEntityFunc = "";
            if (schema.CreateEntity())
            {
                createArgs = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new ModelCreateArgs(), schema, ns, ns, p => p.CreateEntity());
                createEntityFunc = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new CreateEntity(schema, createArgs), schema, ns, ns, p => p.CreateEntity());
            }

            var createViewFunc = "";
            if (schema.CreateViewModel())
            {
                createArgs = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new ModelCreateArgs(), schema, ns, ns, p => p.CreateViewModel());
                createViewFunc = ModelTypeGenerator.Create(schema, schema.GetPluralName(), new CreateViewModel(schema, createArgs), schema, ns, ns, p => p.CreateViewModel());
            }

            return Create(ns, Model, model, Models, models, equalAssertFunc, createInputFunc, createEntityFunc, createViewFunc, schema.GetExtraNamespaces(StrConstants.FileNewline));
        }

        private static String Create(String ns, String Model, String model, String Models, String models, String equalAssertFunc, String createInputFunc, String createEntityFunc, String createViewFunc, String additionalNs)
        {
            return
$@"using AutoMapper;
using {ns}.Database;
using {ns}.InputModels;
using {ns}.Repository;
using {ns}.Models;
using {ns}.ViewModels;
using System;
using Threax.AspNetCore.Tests;
using Xunit;
using System.Collections.Generic;{additionalNs}

namespace {ns}.Tests
{{
    public static partial class {Model}Tests
    {{
{createInputFunc}

{createEntityFunc}

{createViewFunc}

{equalAssertFunc}
    }}
}}";
        }
    }
}
