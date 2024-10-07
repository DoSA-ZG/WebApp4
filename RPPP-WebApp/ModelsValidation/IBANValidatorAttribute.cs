using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RPPP_WebApp.ModelsValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class IBANValidatorAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null || !(value is string))
            {
                return false;
            }

            string iban = ((string)value).Replace(" ", "").ToUpper();

            string pattern = "^[A-Za-z]{2}\\d{18,32}$";

            return Regex.IsMatch(iban, pattern);
        }
    }
}
