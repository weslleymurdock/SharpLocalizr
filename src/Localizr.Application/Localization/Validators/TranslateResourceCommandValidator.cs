using System.Globalization;
using FluentValidation;
using Localizr.Application.Localization.Commands;

namespace Localizr.Application.Localization.Validators;

/// <summary>Validates localization translation commands.</summary>
public sealed class TranslateResourceCommandValidator : AbstractValidator<TranslateResourceCommand>
{
    /// <summary>Initializes validation rules for translation commands.</summary>
    public TranslateResourceCommandValidator()
    {
        RuleFor(command => command.Resources)
            .NotNull()
            .Must(resources => resources.Count > 0)
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("The resource collection cannot be empty.");

        RuleFor(command => command.Culture)
            .NotEmpty()
            .WithErrorCode("BAD_REQUEST")
            .WithMessage("The target culture cannot be empty.")
            .Must(IsValidCulture)
            .WithErrorCode("UNPROCESSABLE_ENTITY")
            .WithMessage("The target culture must be a valid culture name.");
    }

    private static bool IsValidCulture(string culture)
    {
        try
        {
            CultureInfo.GetCultureInfo(culture);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
