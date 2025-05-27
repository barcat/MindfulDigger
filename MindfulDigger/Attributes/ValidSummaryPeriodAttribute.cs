using System.ComponentModel.DataAnnotations;

namespace MindfulDigger.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidSummaryPeriodAttribute : ValidationAttribute
    {
        private static readonly List<string> AllowedPeriods = new List<string>
        {
            "last_7_days",
            "last_14_days",
            "last_30_days",
            "last_10_notes"
        };

        public ValidSummaryPeriodAttribute()
        {
            ErrorMessage = "Invalid period specified. Allowed values are: 'last_7_days', 'last_14_days', 'last_30_days', 'last_10_notes'.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string periodString)
            {
                if (!string.IsNullOrWhiteSpace(periodString) && AllowedPeriods.Contains(periodString.ToLowerInvariant()))
                {
                    return ValidationResult.Success;
                }
            }
            // Also handles null or empty string case due to the check above
            return new ValidationResult(ErrorMessage);
        }
    }
}
