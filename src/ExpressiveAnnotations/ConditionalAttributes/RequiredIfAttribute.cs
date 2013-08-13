﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ExpressiveAnnotations.ConditionalAttributes
{
    /// <summary>
    /// Provides conditional attribute to calculate validation result based on related property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

        private const string _defaultErrorMessage = "The {0} field is required because depends on {1} field.";

        /// <summary>
        /// Field from which runtime value is extracted.
        /// </summary>
        public string DependentProperty { get; set; }
        /// <summary>
        /// Expected value for dependent field. Instead of hardcoding there is also possibility for dynamic extraction of 
        /// target value from other field, by providing its name inside square parentheses. Star character stands for any value.
        /// </summary>
        public object TargetValue { get; set; }

        public RequiredIfAttribute()
            : base(_defaultErrorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {            
            // get a reference to the property this validation depends upon
            var field = Helper.ExtractProperty(validationContext.ObjectInstance, DependentProperty);
            var attribs = field.GetCustomAttributes(typeof (DisplayAttribute), false);
            var attrib = attribs.Length > 0 ? (DisplayAttribute) attribs[0] : null;
            var dependentPropertyName = attrib != null ? attrib.GetName() : DependentProperty;

            // get the value of the dependent property
            var dependentValue = Helper.ExtractValue(validationContext.ObjectInstance, DependentProperty);
            // fetch target value
            var targetValue = Helper.FetchTargetValue(TargetValue, validationContext);

            // compare the value against the target value
            if (Helper.Compare(dependentValue, targetValue))
            {
                // match => means we should try validating this field                                
                if (!_innerAttribute.IsValid(value) || (value is bool && !(bool) value))
                    // validation failed - return an error
                    return new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                                                              validationContext.DisplayName, dependentPropertyName));
            }

            return ValidationResult.Success;
        }
    }
}