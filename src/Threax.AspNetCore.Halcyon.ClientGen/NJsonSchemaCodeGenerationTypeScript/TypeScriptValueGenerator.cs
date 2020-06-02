//-----------------------------------------------------------------------
// <copyright file="TypeScriptValueGenerator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

namespace NJsonSchema.CodeGeneration.TypeScript
{
    /// <summary>Converts the default value to a TypeScript identifier.</summary>
    public class TypeScriptValueGenerator : ValueGeneratorBase
    {
        /// <summary>Initializes a new instance of the <see cref="TypeScriptValueGenerator"/> class.</summary>
        /// <param name="settings">The settings.</param>
        public TypeScriptValueGenerator(TypeScriptGeneratorSettings settings)
            : base(settings)
        {
        }

        /// <summary>Gets the default value code.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="allowsNull">Specifies whether the default value assignment also allows null.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="typeNameHint">The type name hint to use when generating the type and the type name is missing.</param>
        /// <param name="useSchemaDefault">if set to <c>true</c> uses the default value from the schema if available.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <returns>The code.</returns>
        public override string GetDefaultValue(JsonSchema4 schema, bool allowsNull, string targetType, string typeNameHint, bool useSchemaDefault, TypeResolverBase typeResolver)
        {
            var value = base.GetDefaultValue(schema, allowsNull, targetType, typeNameHint, useSchemaDefault, typeResolver);
            if (value == null)
            {
                schema = schema.ActualSchema;
                if (schema != null && allowsNull == false)
                {
                    if (schema.IsArray)
                        return "[]";

                    if (schema.IsDictionary)
                        return "{}";

                    if (schema.Type.HasFlag(JsonObjectType.Object) && !schema.IsAbstract)
                        return "new " + targetType + "()";
                }
            }

            return value;
        }

        /// <summary>Converts the default value to a TypeScript number literal. </summary>
        /// <param name="type">The JSON type.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="format">Optional schema format</param>
        /// <returns>The TypeScript number literal.</returns>
        public override string GetNumericValue(JsonObjectType type, object value, string format)
        {
            return ConvertNumberToString(value);
        }
    }
}