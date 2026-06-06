using System;
using FluentValidation;

namespace MangaPublishingSystem.Application.Validations
{
    public static class ValidationRules
    {
        public const int MaxTextLength = 255;
        public const int MaxDescriptionLength = 1000;
        public const decimal MaxMoneyValue = 9999999999999999.99m; // decimal(18,2) limit

        // General Error Messages
        public const string RequiredField = "{PropertyName} is required.";
        public const string InvalidId = "{PropertyName} must be greater than 0.";
        public const string LengthExceeded = "{PropertyName} must not exceed {MaxLength} characters.";
        
        // Financial Messages
        public const string MoneyNegativeInvalid = "{PropertyName} cannot be negative.";
        public const string MoneyScaleInvalid = "{PropertyName} supports up to 2 decimal places.";
        public const string MoneyPrecisionInvalid = "{PropertyName} exceeds maximum database precision.";

        // Percentage Messages
        public const string PercentageOutOfRange = "{PropertyName} must be between 0 and 100.";
        public const string PercentageScaleInvalid = "{PropertyName} supports up to 2 decimal places.";

        // Tasks / Performance Messages
        public const string RatingOutOfRange = "Rating must be between 1 and 5.";
        public const string ZIndexNegative = "Z-Index order cannot be negative.";
        public const string OrderDeadlineInvalid = "Deadline must be a date in the future.";

        // Extension Methods for common validation rules
        
        // Validation rule for optional Decimal? money properties (e.g. custom escrow splits)
        public static IRuleBuilderOptions<T, decimal?> MoneyRule<T>(this IRuleBuilder<T, decimal?> rule)
        {
            return rule
                .Must(value => !value.HasValue || value.Value >= 0)
                .WithMessage(MoneyNegativeInvalid)
                .Must(value => !value.HasValue || decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero) == value.Value)
                .WithMessage(MoneyScaleInvalid)
                .Must(value => !value.HasValue || Math.Abs(value.Value) <= MaxMoneyValue)
                .WithMessage(MoneyPrecisionInvalid);
        }

        // Validation rule for required Decimal money properties (e.g. paymentAmount, budget, unitPrice)
        public static IRuleBuilderOptions<T, decimal> MoneyRule<T>(this IRuleBuilder<T, decimal> rule)
        {
            return rule
                .Must(value => value >= 0)
                .WithMessage(MoneyNegativeInvalid)
                .Must(value => decimal.Round(value, 2, MidpointRounding.AwayFromZero) == value)
                .WithMessage(MoneyScaleInvalid)
                .Must(value => Math.Abs(value) <= MaxMoneyValue)
                .WithMessage(MoneyPrecisionInvalid);
        }

        // Validation rule for decimal rate or percentages (e.g. OnTimeRate, DisputeRate, AssistantPercentage, MangakaPercentage)
        public static IRuleBuilderOptions<T, decimal> PercentageRule<T>(this IRuleBuilder<T, decimal> rule)
        {
            return rule
                .Must(value => value >= 0 && value <= 100)
                .WithMessage(PercentageOutOfRange)
                .Must(value => decimal.Round(value, 2, MidpointRounding.AwayFromZero) == value)
                .WithMessage(PercentageScaleInvalid);
        }

        public static IRuleBuilderOptions<T, decimal?> PercentageRule<T>(this IRuleBuilder<T, decimal?> rule)
        {
            return rule
                .Must(value => !value.HasValue || (value.Value >= 0 && value.Value <= 100))
                .WithMessage(PercentageOutOfRange)
                .Must(value => !value.HasValue || decimal.Round(value.Value, 2, MidpointRounding.AwayFromZero) == value.Value)
                .WithMessage(PercentageScaleInvalid);
        }

        // Validation rule for ratings (1 to 5 stars)
        public static IRuleBuilderOptions<T, int?> RatingRule<T>(this IRuleBuilder<T, int?> rule)
        {
            return rule
                .Must(value => !value.HasValue || (value.Value >= 1 && value.Value <= 5))
                .WithMessage(RatingOutOfRange);
        }

        public static IRuleBuilderOptions<T, int> RatingRule<T>(this IRuleBuilder<T, int> rule)
        {
            return rule
                .Must(value => value >= 1 && value <= 5)
                .WithMessage(RatingOutOfRange);
        }

        // Validation rule for non-negative ZIndex order
        public static IRuleBuilderOptions<T, int> ZIndexRule<T>(this IRuleBuilder<T, int> rule)
        {
            return rule
                .Must(value => value >= 0)
                .WithMessage(ZIndexNegative);
        }
    }
}
