using FluentValidation;
using MediatR;

namespace dWebShop.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        // Async so validators may use MustAsync (e.g. uniqueness checks). Sequential,
        // not Task.WhenAll: validators can share the request's scoped DbContext.
        var context = new ValidationContext<TRequest>(request);
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var v in validators)
            failures.AddRange((await v.ValidateAsync(context, ct)).Errors.Where(f => f is not null));

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
