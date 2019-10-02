﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EDennis.AspNetCore.Base.Security {
    /// <summary>
    /// Implements an <see cref="IAuthorizationHandler"/> and <see cref="IAuthorizationRequirement"/>
    /// which succeeds if no Allowed claim type values are present and if the claim type itself 
    /// is not Allowed.
    /// 
    /// NOTE: This is adapted from ... https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authorization/Infrastructure/ClaimsAuthorizationRequirement.cs
    /// 
    /// </summary>
    public class ClaimPatternAuthorizationRequirement : AuthorizationHandler<ClaimPatternAuthorizationRequirement>, IAuthorizationRequirement {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimPatternAuthorizationRequirement"/>.
        /// </summary>
        /// <param name="claimType">The claim type that must be absent if no values are provided.</param>
        /// <param name="AllowedValues">The optional list of claim values, which, if present, 
        /// the claim must NOT match.</param>
        public ClaimPatternAuthorizationRequirement(string claimType,
                string requirementScope, IOptions<SecurityOptions> options) {

            ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));

            RequirementScope = requirementScope.ToLower();

            if (options != null) {
                ScopeClaimType = options.Value.ScopeClaimType.ToLower();
                PatternClaimType = options.Value.PatternClaimType.ToLower();
                //NamedPatterns = options.Value.NamedPatterns;

                if (options.Value.NamedPatterns != null && options.Value.NamedPatterns.Count() > 0)
                    MatchingNamedPatterns = options.Value.NamedPatterns
                        .Where(p => IsPatternMatch(requirementScope, p.Value)).Select(p => p.Key.ToLower());

                GloballyIgnoredScopes = options.Value.GloballyIgnoredScopes;
                ExclusionPrefix = options.Value.ExclusionPrefix;
            }
        }

        /// <summary>
        /// Gets the claim types, one or more of which must be present.
        /// </summary>
        public string ClaimType { get; }

        /// <summary>
        /// Gets the optional list of claim values, which, if present, 
        /// the claim must match.
        /// </summary>
        public string RequirementScope { get; }

        public string ScopeClaimType { get; } = "Scope";
        public string PatternClaimType { get; } = "Role";

        /// <summary>
        /// NOTE: Exclusions are evaluated after all included scopes.
        /// NOTE: When only exclusions are present, application-level scope
        ///       is used as the base from which exclusions are applied.
        /// </summary>
        public string ExclusionPrefix { get; } = "-";

        public string[] GloballyIgnoredScopes { get; set; } = new string[] { };


        /// <summary>
        /// NOTE: This can be used to configure roles for users.
        /// </summary>
        //public Dictionary<string, string[]> NamedPatterns { get; }

        public IEnumerable<string> MatchingNamedPatterns { get; } = new string[] { };


        /// <summary>
        /// Makes a decision if authorization is allowed based on the claims requirements specified.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ClaimPatternAuthorizationRequirement requirement) {

            var found = false;

            if (context.User.Claims != null && context.User.Claims.Count() > 0) {

                found = context.User.Claims
                        .Any(c => c.Type.ToLower() == PatternClaimType 
                            && MatchingNamedPatterns
                                .Contains(c.Value.ToLower()));

                if (!found) {

                    var scopePatterns = context.User?.Claims?
                        .Where(c => c.Type.ToLower() == ScopeClaimType.ToLower())
                        .Select(c => c.Value)
                        .Where(s => !GloballyIgnoredScopes.Contains(s));

                    if (scopePatterns != null && scopePatterns.Count() > 0)
                        found = IsMatch(requirement.RequirementScope, scopePatterns);
                }

            }
            if (found) {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }

        private bool IsMatch(string requirementScope, IEnumerable<string> testPatterns) {

            foreach (var pattern in testPatterns.Select(p => p.ToLower())) {
                if (pattern == requirementScope) {
                    return true;
                } else if (requirementScope.StartsWith(pattern + ".")) {
                    return true;
                }
            }
            return false;

        }



        private bool IsPatternMatch(string requirementPattern, IEnumerable<string> testPatterns) {

            var found = false;
            var hasPositiveScopes = false;

            foreach (var pattern in testPatterns
                    .Where(p => !p.StartsWith(ExclusionPrefix))) {

                hasPositiveScopes = true;

                if (pattern.ToLower() == requirementPattern.ToLower())
                    found = true;
                else if (requirementPattern.ToLower().StartsWith(pattern.ToLower() + "."))
                    found = true;
                else if (Regex.IsMatch(requirementPattern, pattern.Replace(".", "\\.").Replace("*", ".*"), RegexOptions.IgnoreCase))
                    found = true;
            }

            foreach (var pattern in testPatterns
                    .Where(p => p.StartsWith(ExclusionPrefix))
                    .Select(p => p.Substring(ExclusionPrefix.Length))) {

                if (!hasPositiveScopes)
                    found = true;

                if (pattern.ToLower() == requirementPattern.ToLower())
                    found = false;
                else if (requirementPattern.ToLower().StartsWith(pattern.ToLower() + "."))
                    found = false;
                else if (Regex.IsMatch(requirementPattern, pattern.Replace(".", "\\.").Replace("*", ".*"), RegexOptions.IgnoreCase))
                    found = false;
            }

            return found;

        }


    }
}



