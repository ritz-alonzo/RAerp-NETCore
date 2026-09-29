using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RAerp.Security.Idempontency.Services.Idempotency;

namespace RAerp.Security.Idempontency.Attributes
{
    /// <summary>
    /// Used filter for DI in the class, moved methods from Attribute to Filter
    /// </summary>
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private const string HeaderName = "Idempotency-Key";
        private readonly IIdempotencyService _idempotencyService;

        public IdempotencyFilter(IIdempotencyService idempotencyService)
        {
            _idempotencyService = idempotencyService;
        }

        #region Previous Implementation
        //public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        //{
        //    var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        //    var request = context.HttpContext.Request;

        //    // 1. Validate header presence
        //    if (!request.Headers.TryGetValue(HeaderName, out var extractedKey) || string.IsNullOrWhiteSpace(extractedKey))
        //    {
        //        context.Result = new BadRequestObjectResult($"The '{HeaderName}' header is missing.");
        //        return;
        //    }

        //    string cacheKey = $"idempotency:{extractedKey}";

        //    // 2. FIXED: Extract the [FromBody] model directly from ActionArguments
        //    // This avoids reading request.Body entirely, which is already empty/null.
        //    string currentPayloadHash = string.Empty;

        //    // ASP.NET Core populates ActionArguments with your deserialized models.
        //    // We find the first argument value that is a class/object.
        //    var boundModel = context.ActionArguments.Values.FirstOrDefault(v => v != null && v.GetType().IsClass && v.GetType() != typeof(string));
        //    if (boundModel == null)
        //        boundModel = context.ActionArguments.Values.FirstOrDefault(v => v != null && v.GetType() != typeof(string));

        //    if (boundModel != null)
        //    {
        //        // Deterministically serialize the model back to JSON for consistent hashing
        //        var serializedModel = JsonSerializer.Serialize(boundModel);
        //        currentPayloadHash = ComputeStorageHash(serializedModel);
        //    }

        //    // 3. Check cache for an existing execution state
        //    var cachedData = await cache.GetStringAsync(cacheKey);
        //    if (cachedData != null)
        //    {
        //        var meta = JsonSerializer.Deserialize<IdempotentMetadata>(cachedData);

        //        // Guard A: Payload Validation (Key reused, but data has changed)
        //        if (meta?.PayloadHash != currentPayloadHash)
        //        {
        //            context.Result = new ConflictObjectResult(new
        //            {
        //                error = "IdempotencyKeyConflict",
        //                message = "This idempotency key was previously used with a different request payload."
        //            });
        //            return;
        //        }

        //        // Guard B: In-flight State Exception (First request is still running)
        //        if (meta.Status == "InFlight")
        //        {
        //            context.Result = new StatusCodeResult(StatusCodes.Status409Conflict);
        //            // Alternatively: Return 425 Too Early, or a custom body asking the client to retry later
        //            context.HttpContext.Response.Headers.Append("Retry-After", "2");
        //            return;
        //        }

        //        // Scenario C: Completed Request (Replay cached response)
        //        context.Result = new ContentResult
        //        {
        //            StatusCode = meta.StatusCode!.Value,
        //            Content = meta.ResponseBody,
        //            ContentType = "application/json"
        //        };
        //        return;
        //    }

        //    // 4. No cache entry found: Mark state as In-Flight
        //    var inflightMeta = new IdempotentMetadata(extractedKey!, "InFlight", currentPayloadHash);
        //    var inflightOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) };
        //    await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(inflightMeta), inflightOptions);

        //    // 5. Execute core business logic
        //    var executedContext = await next();

        //    int? statusCode = executedContext.HttpContext.Response.StatusCode;

        //    // 6. Handle the result after execution
        //    if (executedContext.Exception == null &&
        //        statusCode is >= 200 and < 300)
        //    {
        //        // Success: Transition state to Completed and save response
        //        // Request.Body is a Stream that reads asynchronously
        //        string responseBody = string.Empty;
        //        // CRITICAL FIX: Extract object data directly from the Action Result model
        //        if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
        //        {
        //            responseBody = JsonSerializer.Serialize(objectResult.Value);
        //        }
        //        // Fallback for simple return string types
        //        else if (executedContext.Result is ContentResult contentResult)
        //        {
        //            responseBody = contentResult.Content ?? string.Empty;
        //        }

        //        var completedMeta = new IdempotentMetadata(
        //            extractedKey!,
        //            "Completed",
        //            currentPayloadHash,
        //            statusCode,
        //            responseBody,
        //            DateTime.UtcNow
        //        );

        //        var finalOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        //        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(completedMeta), finalOptions);
        //    }
        //    else
        //    {
        //        // Business logic failed or threw an unhandled exception: Evict key so client can cleanly retry
        //        await cache.RemoveAsync(cacheKey);
        //    }
        //}
        #endregion

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cancellationToken = context.HttpContext.RequestAborted;

            //----------------------------------------------------
            // Validate Header
            //----------------------------------------------------

            if (!context.HttpContext.Request.Headers.TryGetValue(
                    HeaderName,
                    out var values))
            {
                context.Result = new BadRequestObjectResult(
                    new ProblemDetails
                    {
                        Title = "Missing Idempotency-Key",
                        Detail = $"'{HeaderName}' header is required.",
                        Status = StatusCodes.Status400BadRequest
                    });

                return;
            }

            var key = values.ToString();

            //----------------------------------------------------
            // Redis
            //----------------------------------------------------

            var cached =
                await _idempotencyService.GetCachedResponseAsync(
                    key,
                    cancellationToken);

            if (cached != null)
            {
                context.Result = new ContentResult
                {
                    StatusCode = cached.StatusCode,
                    ContentType = "application/json",
                    Content = cached.ResponseBody
                };

                return;
            }

            //----------------------------------------------------
            // Request Body
            //----------------------------------------------------

            var request = context.ActionArguments.Values.FirstOrDefault();

            if (request == null)
            {
                context.Result = new BadRequestResult();
                return;
            }

            //----------------------------------------------------
            // SQL Transaction
            //----------------------------------------------------

            await _idempotencyService.BeginTransactionAsync(
                cancellationToken);

            try
            {
                //------------------------------------------------
                // SQL Lock
                //------------------------------------------------

                await _idempotencyService.AcquireLockAsync(
                    key,
                    cancellationToken);

                //------------------------------------------------
                // Database
                //------------------------------------------------

                var record =
                    await _idempotencyService.GetRecordAsync(
                        key,
                        cancellationToken);

                if (record != null)
                {
                    //------------------------------------------------
                    // Validate Hash
                    //------------------------------------------------

                    await _idempotencyService.ValidateRequestHashAsync(
                        record,
                        request,
                        cancellationToken);

                    //------------------------------------------------
                    // Completed
                    //------------------------------------------------

                    if (record.Status == "Completed")
                    {
                        await _idempotencyService.CacheResponseAsync(
                            record,
                            cancellationToken);

                        await _idempotencyService.CommitAsync(
                            cancellationToken);

                        context.Result = new ContentResult
                        {
                            StatusCode = record.StatusCode,
                            ContentType = "application/json",
                            Content = record.Response
                        };

                        return;
                    }

                    //------------------------------------------------
                    // Processing
                    //------------------------------------------------

                    if (record.Status == "Processing")
                    {
                        await _idempotencyService.RollbackAsync(
                            cancellationToken);

                        context.Result =
                            new ConflictObjectResult(
                                new ProblemDetails
                                {
                                    Title = "Duplicate Request",
                                    Detail = "The request is already being processed.",
                                    Status = StatusCodes.Status409Conflict
                                });

                        return;
                    }

                    //------------------------------------------------
                    // Failed
                    //------------------------------------------------

                    if (record.Status == "Failed")
                    {
                        context.Result =
                            new ConflictObjectResult(
                                new ProblemDetails
                                {
                                    Title = "Previous Request Failed",
                                    Detail = "Retry using a new Idempotency-Key.",
                                    Status = StatusCodes.Status409Conflict
                                });

                        return;
                    }
                }

                //------------------------------------------------
                // Create Processing Record
                //------------------------------------------------

                await _idempotencyService
                    .CreateProcessingRecordAsync(
                        key,
                        context.HttpContext.Request.Path,
                        request,
                        cancellationToken);

                //------------------------------------------------
                // Execute Controller
                //------------------------------------------------

                var executed = await next();

                //------------------------------------------------
                // Exception?
                //------------------------------------------------

                if (executed.Exception != null &&
                    !executed.ExceptionHandled)
                {
                    await _idempotencyService.MarkFailedAsync(
                        key,
                        executed.Exception,
                        cancellationToken);

                    await _idempotencyService.RollbackAsync(
                        cancellationToken);

                    return;
                }

                //------------------------------------------------
                // Save Response
                //------------------------------------------------

                if (executed.Result is ObjectResult objectResult)
                {
                    await _idempotencyService.CompleteAsync(
                        key,
                        objectResult,
                        cancellationToken);
                }
                else
                {
                    await _idempotencyService.CompleteAsync(
                        key,
                        executed.Result!,
                        cancellationToken);
                }

                //------------------------------------------------
                // Commit
                //------------------------------------------------

                await _idempotencyService.CommitAsync(
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await _idempotencyService.RollbackAsync(
                    cancellationToken);

                throw new Exception(ex.Message);
            }
        }
    }
}
