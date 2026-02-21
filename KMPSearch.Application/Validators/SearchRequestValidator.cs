using FluentValidation;
using KMPSearch.Application.DTOs;

namespace KMPSearch.Application.Validators;

public class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    private static readonly string[] ValidSortFields = { "relevance", "date", "title" };
    private static readonly string[] ValidSortOrders = { "asc", "desc" };

    public SearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");

        When(x => x.Sort != null, () =>
        {
            RuleFor(x => x.Sort!.Field)
                .Must(field => ValidSortFields.Contains(field.ToLower()))
                .WithMessage($"Sort field must be one of: {string.Join(", ", ValidSortFields)}");

            RuleFor(x => x.Sort!.Order)
                .Must(order => ValidSortOrders.Contains(order.ToLower()))
                .WithMessage($"Sort order must be one of: {string.Join(", ", ValidSortOrders)}");
        });

        When(x => x.Filters?.DateRange != null, () =>
        {
            RuleFor(x => x.Filters!.DateRange!)
                .Must(dr => dr.From == null || dr.To == null || dr.From <= dr.To)
                .WithMessage("DateRange 'From' must be less than or equal to 'To'");
        });
    }
}
