using System.Text.RegularExpressions;
using FluentValidation;

namespace OnlineAuctionSystem.Application.Common
{
    public static class ValidationExtensions
    {
        private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", RegexOptions.Compiled);

        public static IRuleBuilderOptions<T, string> MustNotContainHtml<T>(this IRuleBuilder<T, string> ruleBuilder) =>
            ruleBuilder
                .Must(value => string.IsNullOrEmpty(value) || !HtmlTagPattern.IsMatch(value))
                .WithMessage("This field cannot contain HTML tags.");
    }
}