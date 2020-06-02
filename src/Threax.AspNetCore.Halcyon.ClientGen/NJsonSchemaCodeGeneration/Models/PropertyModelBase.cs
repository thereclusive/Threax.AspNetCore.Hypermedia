//-----------------------------------------------------------------------
// <copyright file="PropertyModelBase.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;

namespace NJsonSchema.CodeGeneration.Models
{
    /// <summary>The property template model base class.</summary>
    public abstract class PropertyModelBase
    {
        private readonly ClassTemplateModelBase _classTemplateModel;
        private readonly JsonProperty _property;
        private readonly TypeResolverBase _typeResolver;
        private readonly CodeGeneratorSettingsBase _settings;

        /// <summary>Initializes a new instance of the <see cref="PropertyModelBase"/> class.</summary>
        /// <param name="property">The property.</param>
        /// <param name="classTemplateModel">The class template model.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="settings">The settings.</param>
        protected PropertyModelBase(
            JsonProperty property,
            ClassTemplateModelBase classTemplateModel,
            TypeResolverBase typeResolver,
            CodeGeneratorSettingsBase settings)
        {
            _classTemplateModel = classTemplateModel;
            _property = property;
            _settings = settings;
            _typeResolver = typeResolver;

            PropertyName = _settings.PropertyNameGenerator.Generate(_property);
        }

        /// <summary>Gets the type of the property.</summary>
        public abstract string Type { get; }

        /// <summary>Gets the default value generator.</summary>
        public ValueGeneratorBase ValueGenerator => _settings.ValueGenerator;

        /// <summary>Gets a value indicating whether the property has default value.</summary>
        public bool HasDefaultValue => !string.IsNullOrEmpty(DefaultValue);

        /// <summary>Gets the default value as string.</summary>
        public string DefaultValue => ValueGenerator.GetDefaultValue(_property,
            _property.IsNullable(_settings.SchemaType), Type, _property.Name, _settings.GenerateDefaultValues, _typeResolver);

        /// <summary>Gets the name of the property.</summary>
        public string PropertyName { get; set; }

        /// <summary>Gets a value indicating whether the property is a string enum array.</summary>
        public bool IsStringEnumArray =>
            _property.ActualTypeSchema.IsArray &&
            _property.ActualTypeSchema.Item != null &&
            _property.ActualTypeSchema.Item.ActualSchema.IsEnumeration &&
            _property.ActualTypeSchema.Item.ActualSchema.Type.HasFlag(JsonObjectType.String);

        /// <summary>Gets the type name hint for the property.</summary>
        protected string GetTypeNameHint()
        {
            var propertyName = PropertyName;
            if (_property.IsEnumeration == false)
                return propertyName;

            var className = _classTemplateModel.ClassName;
            if (className.Contains("Anonymous"))
                return propertyName;

            if (propertyName.StartsWith(className, StringComparison.OrdinalIgnoreCase))
                return propertyName;

            return className + ConversionUtilities.ConvertToUpperCamelCase(PropertyName, false);
        }
    }
}