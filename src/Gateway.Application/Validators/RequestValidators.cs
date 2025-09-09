using FluentValidation;
using Gateway.Application.DTOs;

namespace Gateway.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50);
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(1);
    }
}

public sealed class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.Messages)
            .NotEmpty()
            .Must(messages => messages.Any())
            .WithMessage("At least one message is required");
            
        RuleForEach(x => x.Messages)
            .ChildRules(message =>
            {
                message.RuleFor(m => m.Role)
                    .NotEmpty()
                    .Must(role => role is "system" or "user" or "assistant")
                    .WithMessage("Role must be 'system', 'user', or 'assistant'");
                    
                message.RuleFor(m => m.Content)
                    .NotEmpty()
                    .MaximumLength(100000);
            });
            
        RuleFor(x => x.Temperature)
            .InclusiveBetween(0.0, 2.0)
            .When(x => x.Temperature.HasValue);
            
        RuleFor(x => x.MaxTokens)
            .GreaterThan(0)
            .LessThanOrEqualTo(100000)
            .When(x => x.MaxTokens.HasValue);
            
        RuleFor(x => x.TopP)
            .InclusiveBetween(0.0, 1.0)
            .When(x => x.TopP.HasValue);
    }
}

public sealed class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
{
    public CreateConversationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}
