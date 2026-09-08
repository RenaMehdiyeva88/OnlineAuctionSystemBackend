using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineAuctionSystem.Presentation.Filters
{

    // Model-binding validation (missing/malformed fields, data annotations) runs
    // before a request ever reaches MediatR. This filter short-circuits those
    // requests with a 400 early, complementing FluentValidation's ValidationBehaviour
    // in Application, which validates business rules inside the pipeline.
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
                {
                    status = 400,
                    message = "Validation failed.",
                    errors
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No-op — nothing to do after the action executes.
        }
    }
}
