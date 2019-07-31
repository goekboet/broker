using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace http.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Scope { get; }

        public ScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }

    public class HasScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ScopeRequirement requirement)
    {
        var scopes = from c in context.User.Claims
                     where c.Type == "scope" && 
                           c.Value == requirement.Scope &&
                           c.Issuer == requirement.Issuer
                     select c;

        if (scopes.Any())
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
}