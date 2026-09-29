using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using RAerp.Security.Idempontency.Attributes;
using RAerp.Security.Idempontency.Records;
using RAerp.Security.Idempontency.Services.Idempotency;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IdempotentAttribute : TypeFilterAttribute
{
    public IdempotentAttribute() : base(typeof(IdempotencyFilter))
    {
    }
}