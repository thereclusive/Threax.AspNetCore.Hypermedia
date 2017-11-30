﻿using NJsonSchema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Threax.AspNetCore.Models;

namespace Threax.ModelGen
{
    class GeneratorSettings
    {
        public void Configure()
        {
            if (Path.GetExtension(Source) != ".json")
            {
                if (!Directory.Exists(AppOutDir))
                {
                    throw new DirectoryNotFoundException($"Cannot find app out directory {AppOutDir}");
                }

                var assembly = ProjectAssemblyLoader.LoadProjectAssembly(AppOutDir);
                var type = assembly.GetType(Source);
                var schemaTask = JsonSchema4.FromTypeAsync(type, new NJsonSchema.Generation.JsonSchemaGeneratorSettings()
                {
                    DefaultEnumHandling = EnumHandling.String,
                    FlattenInheritanceHierarchy = true,
                });
                schemaTask.Wait();
                Schema = schemaTask.Result;

                //Modify the schema to make each none or object property aware of its original type
                foreach (var schemaProp in Schema.Properties.Values)
                {
                    var prop = type.GetProperty(schemaProp.Name);
                    if (prop != null && (schemaProp.IsType(JsonObjectType.None) || schemaProp.IsType(JsonObjectType.Object)))
                    {
                        var propType = prop.PropertyType;
                        schemaProp.Type = JsonObjectType.Object;
                        schemaProp.Format = propType.Name;

                        //If we allow collections check the collection type
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) && propType.GenericTypeArguments.Length > 0)
                        {
                            var enumerableType = propType.GenericTypeArguments.First();
                            schemaProp.Type = JsonObjectType.Array;
                            schemaProp.Item = new JsonProperty()
                            {
                                Type = JsonObjectType.Object,
                                Format = enumerableType.Name
                            };
                        }
                    }
                }
            }
            else
            {
                if (!File.Exists(Source))
                {
                    throw new MessageException($"Cannot find schema file {Source}.");
                }

                var schemaTask = JsonSchema4.FromFileAsync(Source);
                schemaTask.Wait();
                Schema = schemaTask.Result;
            }

            if (Schema.ExtensionData == null) //Make sure this exists
            {
                Schema.ExtensionData = new Dictionary<String, Object>();
            }

            ModelName = Schema.Title;
            Object pluralTitleObj;
            if (Schema.ExtensionData.TryGetValue(PluralNameAttribute.Name, out pluralTitleObj))
            {
                PluralModelName = pluralTitleObj.ToString();
            }
            else
            {
                PluralModelName = ModelName + "s";
            }

            //Make sure directories exist before trying to write files
            WriteApp = WriteApp && Directory.Exists(AppOutDir);
            WriteTests = WriteTests && Directory.Exists(TestOutDir);
        }

        public String AppNamespace { get; set; }

        public String Source { get; set; }

        public String AppOutDir { get; set; }

        public String TestOutDir { get; set; }

        public bool WriteApp { get; set; } = true;

        public bool WriteTests { get; set; } = true;

        public string UiController { get; set; } = "Home";

        public String ModelName { get; set; }

        public String PluralModelName { get; set; }

        public JsonSchema4 Schema { get; set; }

        public bool HasCreated { get; set; } = true;

        public bool HasModified { get; set; } = true;
    }
}
