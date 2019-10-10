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
    public class ClaimPatternAuthorizationHandler : AuthorizationHandler<ClaimPatternAuthorizationHandler>, IAuthorizationRequirement {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimPatternAuthorizationHandler"/>.
        /// </summary>
        /// <param name="claimType">The claim type that must be absent if no values are provided.</param>
        /// <param name="AllowedValues">The optional list of claim values, which, if present, 
        /// the claim must NOT match.</param>
        public ClaimPatternAuthorizationHandler(
                string requirementScope, ScopePatternOptions options) {

            RequirementScope = requirementScope.ToLower();

            if (options != null) {

                IsOidc = options.IsOidc;

                if (IsOidc)
                    UserScopePrefix = options.UserScopePrefix?.ToLower();

                ExclusionPrefix = options.ExclusionPrefix;
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

        public string UserScopePrefix { get; } = "user_";
        public bool IsOidc { get; }

        /// <summary>
        /// NOTE: Exclusions are evaluated after all included scopes.
        /// NOTE: When only exclusions are present, application-level scope
        ///       is used as the base from which exclusions are applied.
        /// </summary>
        public string ExclusionPrefix { get; } = "-";



        public List<string> PositiveMatches { get; } = new List<string> { };
        public List<string> NegativeMatches { get; } = new List<string> { };


        /// <summary>
        /// Makes a decision if authorization is allowed based on the claims requirements specified.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ClaimPatternAuthorizationHandler requirement) {

            MatchType matchType = MatchType.Unmatched;

            var scopeClaimType = $"{(IsOidc ? UserScopePrefix : "")}scope".ToLower();

            if (context.User.Claims != null && context.User.Claims.Count() > 0) {

                var scopeClaims = context.User.Claims
                        .Where(c => c.Type.ToLower() == scopeClaimType)
                        .Select(c=> c.Value.ToLower());

                //if the cache of positive matches contains one of the provided 
                //scope patterns, the requirement will be met and no further processing will occur
                matchType = scopeClaims.Any(c => PositiveMatches.Contains(c)) ? MatchType.Positive : MatchType.Unmatched;


                //if unmatched and the cache of negative matches contains one of the 
                //provided scope patterns, flag as a negative match
                if (matchType == MatchType.Unmatched)
                    matchType = scopeClaims.Any(c => NegativeMatches.Contains(c)) ? MatchType.Negative : MatchType.Unmatched;

                //if still unmatched, evaluate the scopeClaims with pattern-matching algorithm
                if (matchType == MatchType.Unmatched)
                    matchType = EvaluatePattern(requirement.RequirementScope, scopeClaims);

            }
            if (matchType == MatchType.Positive) {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }

        public MatchType EvaluatePattern(string requirementPattern, IEnumerable<string> scopeClaims) {

            MatchType matchType = MatchType.Unmatched;

            //if the client (or user) has more than one scope (or user_scope) claim,
            //only one of these scope values has to match (effectively, an OR condition)
            foreach (var scopeClaim in scopeClaims) {
                var scope = scopeClaim;

                //prepend a universally matching pattern to scopes that start with an exclusion 
                if (scopeClaim.StartsWith(ExclusionPrefix))
                    scope = "*," + scope; 

                //logically, we will treat the last matching pattern in the array of patterns 
                //as the pattern that determines the nature of match -- positive or negative
                foreach (var pattern in scope.Split().Reverse()) {
                    if (pattern.StartsWith(ExclusionPrefix)) {
                        var match = requirementPattern.Matches(pattern.Substring(1));
                        if (match) {
                            matchType = MatchType.Negative;
                            NegativeMatches.Add(scope); //register this pattern in cache as negative match
                            break; //continue with outer loop if a negative match
                        }
                    } else {
                        var match = requirementPattern.Matches(pattern);
                        if (match) {
                            matchType = MatchType.Positive; 
                            PositiveMatches.Add(scope); //register this pattern in cache as positive match
                            return matchType; //short-circuit if a positive match
                        }
                    }
                }
            }
            return matchType;

        }

    }

    public enum MatchType { Unmatched, Positive, Negative }

    /// <summary>
    /// From https://www.geeksforgeeks.org/wildcard-pattern-matching/
    /// This supports ? (single char wildcard) and * (multi-character wildcard)
    /// </summary>
    public static class StringExtensions {
        // Function that matches input str with 
        // given wildcard pattern 
        internal static bool Matches(this string str, string pattern) {

            int n = str.Length;
            int m = str.Length;

            // empty pattern can only match with 
            // empty string 
            if (m == 0)
                return (n == 0);

            // lookup table for storing results of 
            // subproblems 
            bool[,] lookup = new bool[n + 1, m + 1];

            // initialize lookup table to false 
            for (int i = 0; i < n + 1; i++)
                for (int j = 0; j < m + 1; j++)
                    lookup[i, j] = false;

            // empty pattern can match with  
            // empty string 
            lookup[0, 0] = true;

            // Only '*' can match with empty string 
            for (int j = 1; j <= m; j++)
                if (pattern[j - 1] == '*')
                    lookup[0, j] = lookup[0, j - 1];

            // fill the table in bottom-up fashion 
            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    // Two cases if we see a '*' 
                    // a) We ignore '*'' character and move 
                    // to next character in the pattern, 
                    //     i.e., '*' indicates an empty sequence. 
                    // b) '*' character matches with ith 
                    //     character in input 
                    if (pattern[j - 1] == '*')
                        lookup[i, j] = lookup[i, j - 1] ||
                                       lookup[i - 1, j];

                    // Current characters are considered as 
                    // matching in two cases 
                    // (a) current character of pattern is '?' 
                    // (b) characters actually match 
                    else if (pattern[j - 1] == '?' ||
                                 str[i - 1] == pattern[j - 1])
                        lookup[i, j] = lookup[i - 1, j - 1];

                    // If characters don't match 
                    else lookup[i, j] = false;
                }
            }
            return lookup[n, m];
        }
    }


}



