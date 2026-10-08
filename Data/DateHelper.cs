using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyUP.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        string otherPropertyName;

        public DateGreaterThanAttribute(string otherPropertyName, string errorMessage)
            : base(errorMessage)
        {
            this.otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            try
            {
                if (value is not null)
                {
                    // Using reflection we can get a reference to the other date property, in this example the project start date
                    var otherPropertyInfo = validationContext.ObjectType.GetProperty(this.otherPropertyName);
                    // Let's check that otherProperty is of type DateTime as we expect it to be

                    DateTime toValidate = (DateTime)value;
                    DateTime referenceProperty = new DateTime();
                    try
                    {
                        referenceProperty = (DateTime)otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
                    }
                    catch { }
                   
                    // if the end date is lower than the start date, than the validationResult will be set to false and return
                    // a properly formatted error message
                    if (toValidate.CompareTo(referenceProperty) < 1)
                    {
                        validationResult = new ValidationResult(ErrorMessageString);
                    }
                }
             
            }
            catch (Exception)
            {
                // Do stuff, i.e. log the exception
                // Let it go through the upper levels, something bad happened
                throw;
            }

            return validationResult;
        }
    }
}
