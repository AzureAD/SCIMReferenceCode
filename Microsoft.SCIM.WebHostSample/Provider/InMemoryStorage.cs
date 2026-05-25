// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.SCIM;

    public class InMemoryStorage
    {
        internal readonly IDictionary<string, Core2Group> Groups;
        internal readonly IDictionary<string, Core2EnterpriseUser> Users;

        private InMemoryStorage()
        {
            this.Groups = new Dictionary<string, Core2Group>();
            this.Users = new Dictionary<string, Core2EnterpriseUser>();
        }

        private static readonly Lazy<InMemoryStorage> InstanceValue =
                                new Lazy<InMemoryStorage>(
                                        () =>
                                            new InMemoryStorage());

        public static InMemoryStorage Instance => InMemoryStorage.InstanceValue.Value;
    }

    /// <summary>
    /// Enables the efficient, dynamic composition of query predicates.
    /// Source: C# 9.0 in a Nutshell http://www.albahari.com/nutshell/predicatebuilder.aspx
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Creates a predicate that evaluates to true.
        /// </summary>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        /// <summary>
        /// Creates a predicate that evaluates to false.
        /// </summary>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

}
