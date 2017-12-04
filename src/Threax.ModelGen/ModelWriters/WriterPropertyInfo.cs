﻿using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Text;
using Threax.AspNetCore.Models;

namespace Threax.ModelGen
{
    public interface IWriterPropertyInfo
    {
        bool IsValueType { get; }

        String ClrType { get; }

        bool IsRequiredInQuery { get; }

        bool ShowOnQueryUi { get; }

        String DisplayName { get; }

        bool HasRequiredAttribute { get; }

        int? MaxLength { get; }

        bool IsVirtual { get; }
    }

    public class TypeWriterPropertyInfo<T> : IWriterPropertyInfo
    {
        private Type type;

        public TypeWriterPropertyInfo()
        {
            this.type = typeof(T);
        }

        public bool IsValueType => type.IsValueType;

        public string ClrType => type.Name;

        public bool IsRequiredInQuery => false;

        public bool ShowOnQueryUi => false;

        public String DisplayName => null;

        public bool HasRequiredAttribute => false;

        public int? MaxLength => null;

        public bool IsVirtual => false;
    }

    public class SchemaWriterPropertyInfo : IWriterPropertyInfo
    {
        private JsonProperty prop;

        public SchemaWriterPropertyInfo(JsonProperty prop)
        {
            this.prop = prop;
        }

        public bool IsValueType => prop.IsClrValueType();

        public string ClrType => prop.GetClrType();

        public bool IsRequiredInQuery => QueryableAttribute.IsRequired(prop);

        public bool ShowOnQueryUi => QueryableAttribute.ShowOnUi(prop);

        public String DisplayName => prop.Title;

        public bool HasRequiredAttribute => prop.IsRequired;

        public int? MaxLength => prop.MaxLength;

        public bool IsVirtual => prop.IsVirtual();
    }
}
