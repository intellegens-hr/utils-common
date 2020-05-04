using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Intellegens.Commons.Validation
{
    public static class DataAnnotationsValidationUtil //If name ends with Validator, Essperta CQRS won't work ...
    {
        private static List<PropertyInfo> GetPropertiesToValidate<T>()
        {
            var type = typeof(T);
            return typeof(T)
                .GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                //.Where(x => x.CustomAttributes.Where(x => x.AttributeType == typeof(ValidationPropertyAttribute)).Any())
                .ToList();
        }

        private static readonly ConcurrentDictionary<Type, Type?> metadataTypeCache = new ConcurrentDictionary<Type, Type?>();
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> propertiesPerTypeCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        private static Type GetMetadataType(Type type)
        {
            if (!metadataTypeCache.ContainsKey(type))
            {
                var attribute = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                metadataTypeCache[type] = attribute?.MetadataClassType;
            }

            return metadataTypeCache[type];
        }

        private static IEnumerable<ValidationResult> ValidateProperties(object validationObject, ValidationContext validationContext)
        {
            //get validation object type and metadata type used to validate it's properties (if any)
            var validationObjectType = validationObject.GetType();
            var metadataType = GetMetadataType(validationObjectType);

            //no point in continuing if metadata is not specified
            if (metadataType == null)
                yield break;

            //caching
            if (!propertiesPerTypeCache.ContainsKey(validationObjectType))
                propertiesPerTypeCache[validationObjectType] = validationObject.GetType().GetProperties().ToList();

            var properties = propertiesPerTypeCache[validationObjectType];

            //iterate all properties and validate each one
            foreach (var p in properties)
            {
                PropertyInfo property = metadataType.GetProperty(p.Name);

                //get property value from object
                object value = validationObjectType.GetProperty(p.Name).GetValue(validationObject, null);

                //get all invalid ValidationAttribute-s
                var invalidValidationAttributes = property.GetCustomAttributes(true)
                    .Where(x => x is ValidationAttribute)
                    .Select(x => x as ValidationAttribute)
                    .Where(x => !x.IsValid(value));

                foreach (var invalidAttribute in invalidValidationAttributes)
                {
                    validationContext.DisplayName = p.Name; //Used to format validation message with member name
                    yield return invalidAttribute.GetValidationResult(value, validationContext);
                }
            };
        }

        //TODO: This probably doesn't work for deep structures.
        public static List<ValidationResult> ValidateDataAnnotations<T>(T instanceToValidate)
        {
            var results = new List<ValidationResult>();
            var instanceContext = new ValidationContext(instanceToValidate);
            Validator.TryValidateObject(instanceToValidate, instanceContext, results, true);

            if (!results.Any())
                GetPropertiesToValidate<T>()
                    .ForEach(x =>
                    {
                        var validationObject = instanceToValidate.GetType().GetProperty(x.Name).GetValue(instanceToValidate);

                        if (validationObject == null)
                            return;

                        var context = new ValidationContext(validationObject);

                        //First try to validate entire object
                        List<ValidationResult> newResults = new List<ValidationResult>();
                        Validator.TryValidateObject(validationObject, context, newResults, true);

                        //if nothing is found - validate each member
                        if (!newResults.Any())
                        {
                            var propertyValidationResults = ValidateProperties(validationObject, context);
                            results.AddRange(propertyValidationResults);
                        }

                        results.AddRange(newResults);
                    });

            return results;
        }
    }
}