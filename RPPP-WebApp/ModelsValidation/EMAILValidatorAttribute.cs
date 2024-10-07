using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class EMAILValidatorAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null || !(value is string))
        {
            return false;
        }

        string email = ((string)value).Trim();

        if (string.IsNullOrEmpty(email))
        {
            return true; 
        }

        string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

        return Regex.IsMatch(email, pattern);
    }
}
