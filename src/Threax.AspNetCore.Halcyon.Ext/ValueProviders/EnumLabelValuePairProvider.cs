﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NJsonSchema;

namespace Threax.AspNetCore.Halcyon.Ext.ValueProviders
{
    public class EnumLabelValuePairProvider : IValueProvider
    {
        Type enumType;
        bool nullable;
        PropertyInfo propertyInfo;


        public EnumLabelValuePairProvider(Type enumType, bool nullable)
            :this(enumType, null, nullable)
        {

        }

        public EnumLabelValuePairProvider(Type enumType, PropertyInfo propertyInfo, bool nullable)
        {
            this.enumType = enumType;
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new InvalidOperationException($"Cannot get enum values for type that is not an enum {enumType.FullName}");
            }
            this.nullable = nullable;
            this.propertyInfo = propertyInfo;
        }

        public Task AddExtensions(JsonProperty schemaProp, ValueProviderArgs args)
        {
            var sources = GetSourcesSync();

            foreach(var source in sources)
            {
                schemaProp.EnumerationNames.Add(source.Label);
                schemaProp.Enumeration.Add(source.Value);
            }

            if (schemaProp.ExtensionData == null)
            {
                schemaProp.ExtensionData = new Dictionary<String, Object>();
            }

            return Task.FromResult(0);
        }

        protected virtual IEnumerable<LabelValuePair> GetSourcesSync()
        {
            if (nullable)
            {
                //Include the null enum label since we can take null values
                NullEnumLabelAttribute nullLabel = null;
                if (propertyInfo != null)
                {
                    nullLabel = propertyInfo.GetCustomAttribute<NullEnumLabelAttribute>();
                }

                if (nullLabel == null)
                {
                    nullLabel = enumType.GetTypeInfo().GetCustomAttribute<NullEnumLabelAttribute>();
                }

                if (nullLabel == null)
                {
                    nullLabel = new NullEnumLabelAttribute();
                }

                yield return new LabelValuePair()
                {
                    Label = nullLabel.Label,
                    Value = null
                };
            }

            foreach (var member in enumType.GetTypeInfo().DeclaredFields.Where(i => i.IsStatic)) //The static decalared fields are our enum values
            {
                var label = member.Name;
                var display = member.GetCustomAttribute<DisplayAttribute>();
                if(display != null)
                {
                    label = display.Name;
                }
                yield return new LabelValuePair()
                {
                    Label = label,
                    Value = member.Name
                };
            }
        }
    }
}
