//-----------------------------------------------------------------------
// <copyright file="JsonSchemaValidator.cs" company="NJsonSchema">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/rsuter/NJsonSchema/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace NJsonSchema.Validation
{
    /// <summary>Class to validate a JSON schema against a given <see cref="JToken"/>. </summary>
    public class JsonSchemaValidator
    {
        /// <summary>Validates the given JSON data.</summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>The list of validation errors.</returns>
        public ICollection<ValidationError> Validate(string jsonData, JsonSchema4 schema)
        {
            var jsonObject = JToken.Parse(jsonData);
            return Validate(jsonObject, schema);
        }

        /// <summary>Validates the given JSON token.</summary>
        /// <param name="token">The token.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>The list of validation errors.</returns>
        public ICollection<ValidationError> Validate(JToken token, JsonSchema4 schema)
        {
            return Validate(token, schema.ActualSchema, null, token.Path);
        }

        /// <summary>Validates the given JSON token.</summary>
        /// <param name="token">The token.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="propertyName">The current property name.</param>
        /// <param name="propertyPath">The current property path.</param>
        /// <returns>The list of validation errors.</returns>
        protected virtual ICollection<ValidationError> Validate(JToken token, JsonSchema4 schema, string propertyName, string propertyPath)
        {
            var errors = new List<ValidationError>();

            ValidateAnyOf(token, schema, propertyName, propertyPath, errors);
            ValidateAllOf(token, schema, propertyName, propertyPath, errors);
            ValidateOneOf(token, schema, propertyName, propertyPath, errors);
            ValidateNot(token, schema, propertyName, propertyPath, errors);
            ValidateType(token, schema, propertyName, propertyPath, errors);
            ValidateEnum(token, schema, propertyName, propertyPath, errors);
            ValidateProperties(token, schema, propertyName, propertyPath, errors);

            return errors;
        }

        private void ValidateType(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            var types = GetTypes(schema).ToDictionary(t => t, t => (ICollection<ValidationError>)new List<ValidationError>());
            if (types.Count > 1)
            {
                foreach (var type in types)
                {
                    ValidateArray(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateString(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateNumber(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateInteger(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateBoolean(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateNull(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                    ValidateObject(token, schema, type.Key, propertyName, propertyPath, (List<ValidationError>)type.Value);
                }

                // just one has to validate when multiple types are defined
                if (types.All(t => t.Value.Count > 0))
                {
                    errors.Add(new MultiTypeValidationError(
                        ValidationErrorKind.NoTypeValidates, propertyName, propertyPath, types, token, schema));
                }
            }
            else
            {
                ValidateArray(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateString(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateNumber(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateInteger(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateBoolean(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateNull(token, schema, schema.Type, propertyName, propertyPath, errors);
                ValidateObject(token, schema, schema.Type, propertyName, propertyPath, errors);
            }
        }

        private static readonly IEnumerable<JsonObjectType> JsonObjectTypes = Enum
            .GetValues(typeof(JsonObjectType))
            .Cast<JsonObjectType>()
            .Where(t => t != JsonObjectType.None)
            .ToList();

        private IEnumerable<JsonObjectType> GetTypes(JsonSchema4 schema)
        {
            return JsonObjectTypes.Where(t => schema.Type.HasFlag(t));
        }

        private void ValidateAnyOf(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.AnyOf.Count > 0)
            {
                var propertyErrors = schema.AnyOf.ToDictionary(s => s, s => Validate(token, s));
                if (propertyErrors.All(s => s.Value.Count != 0))
                    errors.Add(new ChildSchemaValidationError(ValidationErrorKind.NotAnyOf, propertyName, propertyPath, propertyErrors, token, schema));
            }
        }

        private void ValidateAllOf(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.AllOf.Count > 0)
            {
                var propertyErrors = schema.AllOf.ToDictionary(s => s, s => Validate(token, s));
                if (propertyErrors.Any(s => s.Value.Count != 0))
                    errors.Add(new ChildSchemaValidationError(ValidationErrorKind.NotAllOf, propertyName, propertyPath, propertyErrors, token, schema));
            }
        }

        private void ValidateOneOf(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.OneOf.Count > 0)
            {
                var propertyErrors = schema.OneOf.ToDictionary(s => s, s => Validate(token, s));
                if (propertyErrors.Count(s => s.Value.Count == 0) != 1)
                    errors.Add(new ChildSchemaValidationError(ValidationErrorKind.NotOneOf, propertyName, propertyPath, propertyErrors, token, schema));
            }
        }

        private void ValidateNot(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.Not != null)
            {
                if (Validate(token, schema.Not).Count == 0)
                    errors.Add(new ValidationError(ValidationErrorKind.ExcludedSchemaValidates, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateNull(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (type.HasFlag(JsonObjectType.Null))
            {
                if (token != null && token.Type != JTokenType.Null)
                    errors.Add(new ValidationError(ValidationErrorKind.NullExpected, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateEnum(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.Enumeration.Contains(null) && token?.Type == JTokenType.Null)
                return;

            if (schema.Enumeration.Count > 0 && schema.Enumeration.All(v => v?.ToString() != token?.ToString()))
                errors.Add(new ValidationError(ValidationErrorKind.NotInEnumeration, propertyName, propertyPath, token, schema));
        }

        private void ValidateString(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            var isString = token.Type == JTokenType.String || token.Type == JTokenType.Date ||
                           token.Type == JTokenType.Guid || token.Type == JTokenType.TimeSpan ||
                           token.Type == JTokenType.Uri;

            if (isString)
            {
                var value = token.Type == JTokenType.Date ? (token as JValue).ToString("yyyy-MM-ddTHH:mm:ssK") : token.Value<string>();
                if (value != null)
                {
                    if (!string.IsNullOrEmpty(schema.Pattern))
                    {
                        if (!Regex.IsMatch(value, schema.Pattern))
                            errors.Add(new ValidationError(ValidationErrorKind.PatternMismatch, propertyName, propertyPath, token, schema));
                    }
                    if (schema.MinLength.HasValue && value.Length < schema.MinLength)
                        errors.Add(new ValidationError(ValidationErrorKind.StringTooShort, propertyName, propertyPath, token, schema));

                    if (schema.MaxLength.HasValue && value.Length > schema.MaxLength)
                        errors.Add(new ValidationError(ValidationErrorKind.StringTooLong, propertyName, propertyPath, token, schema));

                    if (!string.IsNullOrEmpty(schema.Format))
                    {
                        if (schema.Format == JsonFormatStrings.DateTime)
                        {
                            var acceptableFormats = new string[] {
                                "yyyy-MM-dd'T'HH:mm:ss.FFFK",
                                "yyyy-MM-dd' 'HH:mm:ss.FFFK",
                                "yyyy-MM-dd'T'HH:mm:ssK",
                                "yyyy-MM-dd' 'HH:mm:ssK",
                                "yyyy-MM-dd'T'HH:mm:ss",
                                "yyyy-MM-dd' 'HH:mm:ss",
                                "yyyy-MM-dd'T'HH:mm",
                                "yyyy-MM-dd' 'HH:mm",
                                "yyyy-MM-dd'T'HH",
                                "yyyy-MM-dd' 'HH",
                                "yyyy-MM-dd",
                                "yyyy-MM-dd",
                                "yyyyMMdd",
                                "yyyy-MM",
                                "yyyy" };
                            DateTimeOffset dateTimeResult;
                            if (token.Type != JTokenType.Date && DateTimeOffset.TryParseExact(value, acceptableFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeResult) == false)
                                errors.Add(new ValidationError(ValidationErrorKind.DateTimeExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Date)
                        {
                            DateTime dateTimeResult;
                            if (token.Type != JTokenType.Date && (DateTime.TryParseExact(value, "yyyy-MM-dd", null, DateTimeStyles.None, out dateTimeResult) == false || dateTimeResult.Date != dateTimeResult))
                                errors.Add(new ValidationError(ValidationErrorKind.DateExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Time)
                        {
                            DateTime dateTimeResult;
                            if (token.Type != JTokenType.Date && DateTime.TryParseExact(value, "HH:mm:ss.FFFFFFFK", null, DateTimeStyles.None, out dateTimeResult) == false)
                                errors.Add(new ValidationError(ValidationErrorKind.TimeExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.TimeSpan)
                        {
                            TimeSpan timeSpanResult;
                            if (token.Type != JTokenType.TimeSpan && TimeSpan.TryParse(value, out timeSpanResult) == false)
                                errors.Add(new ValidationError(ValidationErrorKind.TimeSpanExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Uri)
                        {
                            Uri uriResult;
                            if (token.Type != JTokenType.Uri && Uri.TryCreate(value, UriKind.Absolute, out uriResult) == false)
                                errors.Add(new ValidationError(ValidationErrorKind.UriExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Email)
                        {
                            var isEmail = Regex.IsMatch(value,
                                @"^\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*" +
                                @"@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z$", RegexOptions.IgnoreCase);

                            if (!isEmail)
                                errors.Add(new ValidationError(ValidationErrorKind.EmailExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.IpV4)
                        {
                            var isIpV4 = Regex.IsMatch(value,
                                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.IgnoreCase);

                            if (!isIpV4)
                                errors.Add(new ValidationError(ValidationErrorKind.IpV4Expected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.IpV6)
                        {
                            var isIpV6 = Uri.CheckHostName(value) == UriHostNameType.IPv6;

                            if (!isIpV6)
                                errors.Add(new ValidationError(ValidationErrorKind.IpV6Expected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Guid)
                        {
                            Guid guid;
                            if (Guid.TryParse(value, out guid) == false)
                                errors.Add(new ValidationError(ValidationErrorKind.GuidExpected, propertyName, propertyPath, token, schema));
                        }

                        if (schema.Format == JsonFormatStrings.Hostname)
                        {
                            var isHostname = Regex.IsMatch(value, "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*" +
                                                                  "([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$", RegexOptions.IgnoreCase);
                            if (!isHostname)
                                errors.Add(new ValidationError(ValidationErrorKind.HostnameExpected, propertyName, propertyPath, token, schema));
                        }

#pragma warning disable 618 //Base64 check is used for backward compatibility
                        if (schema.Format == JsonFormatStrings.Byte || schema.Format == JsonFormatStrings.Base64)
#pragma warning restore 618
                        {
                            var isBase64 = (value.Length % 4 == 0) && Regex.IsMatch(value, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

                            if (!isBase64)
                                errors.Add(new ValidationError(ValidationErrorKind.Base64Expected, propertyName, propertyPath, token, schema));
                        }
                    }
                }
            }
            else
            {
                if (type.HasFlag(JsonObjectType.String))
                    errors.Add(new ValidationError(ValidationErrorKind.StringExpected, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateNumber(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (type.HasFlag(JsonObjectType.Number))
            {
                if (token.Type != JTokenType.Float && token.Type != JTokenType.Integer)
                    errors.Add(new ValidationError(ValidationErrorKind.NumberExpected, propertyName, propertyPath, token, schema));
            }

            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                try
                {
                    var value = token.Value<decimal>();

                    if (schema.Minimum.HasValue && (schema.IsExclusiveMinimum ? value <= schema.Minimum : value < schema.Minimum))
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooSmall, propertyName, propertyPath, token, schema));

                    if (schema.Maximum.HasValue && (schema.IsExclusiveMaximum ? value >= schema.Maximum : value > schema.Maximum))
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooBig, propertyName, propertyPath, token, schema));

                    if (schema.ExclusiveMinimum.HasValue && value <= schema.ExclusiveMinimum)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooSmall, propertyName, propertyPath, token, schema));

                    if (schema.ExclusiveMaximum.HasValue && value >= schema.ExclusiveMaximum)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooBig, propertyName, propertyPath, token, schema));

                    if (schema.MultipleOf.HasValue && value % schema.MultipleOf != 0)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberNotMultipleOf, propertyName, propertyPath, token, schema));
                }
                catch (OverflowException)
                {
                    var value = token.Value<double>();

                    if (schema.Minimum.HasValue && (schema.IsExclusiveMinimum ? value <= (double)schema.Minimum : value < (double)schema.Minimum))
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooSmall, propertyName, propertyPath, token, schema));

                    if (schema.Maximum.HasValue && (schema.IsExclusiveMaximum ? value >= (double)schema.Maximum : value > (double)schema.Maximum))
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooBig, propertyName, propertyPath, token, schema));

                    if (schema.ExclusiveMinimum.HasValue && value <= (double)schema.ExclusiveMinimum)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooSmall, propertyName, propertyPath, token, schema));

                    if (schema.ExclusiveMaximum.HasValue && value >= (double)schema.ExclusiveMaximum)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberTooBig, propertyName, propertyPath, token, schema));

                    if (schema.MultipleOf.HasValue && value % (double)schema.MultipleOf != 0)
                        errors.Add(new ValidationError(ValidationErrorKind.NumberNotMultipleOf, propertyName, propertyPath, token, schema));
                }
            }
        }

        private void ValidateInteger(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (type.HasFlag(JsonObjectType.Integer))
            {
                if (token.Type != JTokenType.Integer)
                    errors.Add(new ValidationError(ValidationErrorKind.IntegerExpected, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateBoolean(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (type.HasFlag(JsonObjectType.Boolean))
            {
                if (token.Type != JTokenType.Boolean)
                    errors.Add(new ValidationError(ValidationErrorKind.BooleanExpected, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateObject(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (type.HasFlag(JsonObjectType.Object))
            {
                var obj = token as JObject;
                if (obj == null)
                    errors.Add(new ValidationError(ValidationErrorKind.ObjectExpected, propertyName, propertyPath, token, schema));
            }
        }

        private void ValidateProperties(JToken token, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            var obj = token as JObject;
            if (obj == null && schema.Type.HasFlag(JsonObjectType.Null))
                return;

            foreach (var propertyInfo in schema.Properties)
            {
                var newPropertyPath = !string.IsNullOrEmpty(propertyPath) ? propertyPath + "." + propertyInfo.Key : propertyInfo.Key;

                var property = obj?.Property(propertyInfo.Key);
                if (property != null)
                {
                    var propertyErrors = Validate(property.Value, propertyInfo.Value.ActualSchema, propertyInfo.Key, newPropertyPath);
                    errors.AddRange(propertyErrors);
                }
                else if (propertyInfo.Value.IsRequired)
                    errors.Add(new ValidationError(ValidationErrorKind.PropertyRequired, propertyInfo.Key, newPropertyPath, token, schema));
            }

            if (obj != null)
            {
                var properties = obj.Properties().ToList();

                ValidateMaxProperties(token, properties, schema, propertyName, propertyPath, errors);
                ValidateMinProperties(token, properties, schema, propertyName, propertyPath, errors);

                var additionalProperties = properties.Where(p => !schema.Properties.ContainsKey(p.Name)).ToList();

                ValidatePatternProperties(additionalProperties, schema, errors);
                ValidateAdditionalProperties(token, additionalProperties, schema, propertyName, propertyPath, errors);
            }
        }

        private void ValidateMaxProperties(JToken token, IList<JProperty> properties, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.MaxProperties > 0 && properties.Count() > schema.MaxProperties)
                errors.Add(new ValidationError(ValidationErrorKind.TooManyProperties, propertyName, propertyPath, token, schema));
        }

        private void ValidateMinProperties(JToken token, IList<JProperty> properties, JsonSchema4 schema, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.MinProperties > 0 && properties.Count() < schema.MinProperties)
                errors.Add(new ValidationError(ValidationErrorKind.TooFewProperties, propertyName, propertyPath, token, schema));
        }

        private void ValidatePatternProperties(List<JProperty> additionalProperties, JsonSchema4 schema, List<ValidationError> errors)
        {
            foreach (var property in additionalProperties.ToArray())
            {
                var patternPropertySchema = schema.PatternProperties.FirstOrDefault(p => Regex.IsMatch(property.Name, p.Key));
                if (patternPropertySchema.Value != null)
                {
                    var error = TryCreateChildSchemaError(property.Value,
                        patternPropertySchema.Value, ValidationErrorKind.AdditionalPropertiesNotValid, property.Name, property.Path);

                    if (error != null)
                        errors.Add(error);

                    additionalProperties.Remove(property);
                }
            }
        }

        private void ValidateAdditionalProperties(JToken token, List<JProperty> additionalProperties, JsonSchema4 schema,
            string propertyName, string propertyPath, List<ValidationError> errors)
        {
            if (schema.AdditionalPropertiesSchema != null)
            {
                foreach (var property in additionalProperties)
                {
                    var error = TryCreateChildSchemaError(property.Value,
                        schema.AdditionalPropertiesSchema, ValidationErrorKind.AdditionalPropertiesNotValid, property.Name, property.Path);
                    if (error != null)
                        errors.Add(error);
                }
            }
            else
            {
                if (!schema.AllowAdditionalProperties && additionalProperties.Any())
                {
                    foreach (var property in additionalProperties)
                    {
                        var newPropertyPath = !string.IsNullOrEmpty(propertyPath) ? propertyPath + "." + property.Name : property.Name;
                        errors.Add(new ValidationError(ValidationErrorKind.NoAdditionalPropertiesAllowed, property.Name, newPropertyPath, property, schema));
                    }
                }
            }
        }

        private void ValidateArray(JToken token, JsonSchema4 schema, JsonObjectType type, string propertyName, string propertyPath, List<ValidationError> errors)
        {
            var array = token as JArray;
            if (array != null)
            {
                if (schema.MinItems > 0 && array.Count < schema.MinItems)
                    errors.Add(new ValidationError(ValidationErrorKind.TooFewItems, propertyName, propertyPath, token, schema));

                if (schema.MaxItems > 0 && array.Count > schema.MaxItems)
                    errors.Add(new ValidationError(ValidationErrorKind.TooManyItems, propertyName, propertyPath, token, schema));

                if (schema.UniqueItems && array.Count != array.Select(a => a.ToString()).Distinct().Count())
                    errors.Add(new ValidationError(ValidationErrorKind.ItemsNotUnique, propertyName, propertyPath, token, schema));

                for (var index = 0; index < array.Count; index++)
                {
                    var item = array[index];

                    var propertyIndex = string.Format("[{0}]", index);
                    var itemPath = !string.IsNullOrEmpty(propertyPath) ? propertyPath + propertyIndex : propertyIndex;

                    if (schema.Item != null)
                    {
                        var error = TryCreateChildSchemaError(item, schema.Item, ValidationErrorKind.ArrayItemNotValid, propertyIndex, itemPath);
                        if (error != null)
                            errors.Add(error);
                    }

                    ValidateAdditionalItems(item, schema, index, propertyPath, errors);
                }
            }
            else if (type.HasFlag(JsonObjectType.Array))
                errors.Add(new ValidationError(ValidationErrorKind.ArrayExpected, propertyName, propertyPath, token, schema));
        }

        private void ValidateAdditionalItems(JToken item, JsonSchema4 schema, int index, string propertyPath, List<ValidationError> errors)
        {
            if (schema.Items.Count > 0)
            {
                var propertyIndex = string.Format("[{0}]", index);
                if (schema.Items.Count > index)
                {
                    var error = TryCreateChildSchemaError(item,
                        schema.Items.ElementAt(index), ValidationErrorKind.ArrayItemNotValid, propertyIndex, propertyPath + propertyIndex);
                    if (error != null)
                        errors.Add(error);
                }
                else
                {
                    if (schema.AdditionalItemsSchema != null)
                    {
                        var error = TryCreateChildSchemaError(item,
                            schema.AdditionalItemsSchema, ValidationErrorKind.AdditionalItemNotValid, propertyIndex, propertyPath + propertyIndex);
                        if (error != null)
                            errors.Add(error);
                    }
                    else
                    {
                        if (!schema.AllowAdditionalItems)
                        {
                            errors.Add(new ValidationError(ValidationErrorKind.TooManyItemsInTuple,
                                propertyIndex, propertyPath + propertyIndex, item, schema));
                        }
                    }
                }
            }
        }

        private ChildSchemaValidationError TryCreateChildSchemaError(JToken token, JsonSchema4 schema, ValidationErrorKind errorKind, string property, string path)
        {
            var errors = Validate(token, schema.ActualSchema, null, path);
            if (errors.Count == 0)
                return null;

            var errorDictionary = new Dictionary<JsonSchema4, ICollection<ValidationError>>();
            errorDictionary.Add(schema, errors);

            return new ChildSchemaValidationError(errorKind, property, path, errorDictionary, token, schema);
        }
    }
}
