using FluentValidation;
using VibeVoice.Application.Dtos;
using VibeVoice.Application.Services;

namespace VibeVoice.Application.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).Password();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PreferredLanguage).Must(l => l is null or "en" or "ar").WithMessage("Language must be 'en' or 'ar'.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PreferredLanguage).Must(l => l is "en" or "ar").WithMessage("Language must be 'en' or 'ar'.");
        RuleFor(x => x.Theme).Must(t => t is "light" or "dark" or "system").WithMessage("Theme must be light, dark or system.");
        RuleFor(x => x.PreferredVoice).MaximumLength(100);
        RuleFor(x => x.Palette).Must(p => p is null or "sunset" or "oasis" or "ocean" or "neon")
            .WithMessage("Palette must be sunset, oasis, ocean or neon.");
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).Password().NotEqual(x => x.CurrentPassword).WithMessage("Choose a different password.");
    }
}

public sealed class SaveNoteRequestValidator : AbstractValidator<SaveNoteRequest>
{
    public SaveNoteRequestValidator()
    {
        RuleFor(x => x.Title).NotNull().MaximumLength(200);
        RuleFor(x => x.Body).MaximumLength(1_000_000);
        RuleFor(x => x.Color).MaximumLength(20);
        RuleFor(x => x.Tags).Must(t => t is null || t.Count <= TaggingService.MaxNames)
            .WithMessage($"At most {TaggingService.MaxNames} tags.");
    }
}

public sealed class SaveMemoryRequestValidator : AbstractValidator<SaveMemoryRequest>
{
    public SaveMemoryRequestValidator()
    {
        RuleFor(x => x.Title).NotNull().MaximumLength(200);
        RuleFor(x => x.Story).MaximumLength(1_000_000);
        RuleFor(x => x.MemoryDate).NotEqual(default(DateOnly)).WithMessage("The memory date is required.");
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Mood).IsInEnum();
        RuleFor(x => x.Tags).Must(t => t is null || t.Count <= TaggingService.MaxNames)
            .WithMessage($"At most {TaggingService.MaxNames} tags.");
        RuleFor(x => x.People).Must(t => t is null || t.Count <= TaggingService.MaxNames)
            .WithMessage($"At most {TaggingService.MaxNames} people.");
    }
}

public sealed class TtsRequestValidator : AbstractValidator<TtsRequest>
{
    public TtsRequestValidator()
    {
        RuleFor(x => x.Text).MaximumLength(AttachmentService.MaxTtsChars);
        RuleFor(x => x.Voice).MaximumLength(100);
        RuleFor(x => x.Language).MaximumLength(10);
    }
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).Password();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PreferredLanguage).Must(l => l is null or "en" or "ar").WithMessage("Language must be 'en' or 'ar'.");
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PreferredLanguage).Must(l => l is "en" or "ar").WithMessage("Language must be 'en' or 'ar'.");
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator() => RuleFor(x => x.NewPassword).Password();
}

public sealed class SaveRoleRequestValidator : AbstractValidator<SaveRoleRequest>
{
    public SaveRoleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

internal static class RuleExtensions
{
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(p => p.Any(char.IsLetter) && p.Any(char.IsDigit))
            .WithMessage("Password must contain at least one letter and one digit.");
}
