using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Filters
{
    public class OrderByFilter : GraphQLFilter<ContentItem>
    {
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context)
        {
            if (!context.HasPopulatedArgument("orderBy"))
            {
                return query;
            }

            var orderByArguments = JObject.FromObject(context.Arguments["orderBy"]);

            if (orderByArguments == null)
            {
                return query;
            }

            // TODO: Need a way to get index classes based on part names that are passed in.
            var contentQuery = query.With<ContentItemIndex>();

            bool orderApplied = false;
            foreach (var orderByArg in orderByArguments)
            {
                var orderByField = orderByArg.Key.ToPascalCase();

                // TODO: Determine if argument is not Enum type and therefore is nested object
                var defaultOrder = orderByArg.Value.ToObject<OrderByDirection>();

                if (!string.IsNullOrWhiteSpace(orderByField))
                {
                    var memberExpression = ToExpression(orderByField);

                    if (defaultOrder == OrderByDirection.Ascending)
                    {
                        contentQuery = orderApplied ? contentQuery.ThenBy(memberExpression) :
                            contentQuery.OrderBy(memberExpression);
                    }
                    else
                    {
                        contentQuery = orderApplied ? contentQuery.ThenByDescending(memberExpression) :
                            contentQuery.OrderByDescending(memberExpression);
                    }

                    orderApplied = true;
                }
            }

            return contentQuery;
        }

        public static Expression<Func<ContentItemIndex, object>> ToExpression(string property)
        {
            var parameter = Expression.Parameter(typeof(ContentItemIndex));
            var memberExpression = Expression.Property(parameter, property);     
            Expression convertExpr = Expression.Convert(memberExpression, typeof(object));
            var expression = Expression.Lambda(convertExpr, parameter);

            return (Expression<Func<ContentItemIndex, object>>)expression;
        }
    }

    /*
        query OrderedArticles {
          article(
            orderBy: {
              publishedUtc: DESC
              subtitle: ASC
              autoroutePart: {
                path: ASC 
              }
            }
          ) {
            htmlBodyPart {
              html
            }
          }
        }
     */
}