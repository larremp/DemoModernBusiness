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
    /*
        Proposed OrderBy Query Format:

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

            // TODO: Remove me when we have dynamic index support.
            var contentQuery = (IQuery<ContentItem, IIndex>)query.With<ContentItemIndex>();

            OrderBy(contentQuery, orderByArguments.AsJEnumerable(), typeof(ContentItemIndex));

            return contentQuery;
        }

        private void OrderBy(
            IQuery<ContentItem, IIndex> query, 
            IJEnumerable<JToken> orderByArguments, 
            Type currentIndexType,
            bool isFirst = true)
        {
            if (currentIndexType == null) return;

            foreach (var orderByArg in orderByArguments)
            {
                if (orderByArg.Type == JTokenType.Property)
                {
                    JProperty prop = (JProperty)orderByArg;
                    if (!prop.Value.HasValues)
                    {
                        // TODO: Need to be able to pass a type object. since type isn't known at compile time.
                        //       But there's only generic method.
                        //query = query.With(currentIndexType);

                        var fieldName = prop.Name.ToPascalCase();
                        var orderDirection = prop.Value<OrderByDirection?>();

                        query = OrderByField(fieldName, orderDirection, query, isFirst);
                        
                        // Keeps track of whether we should call OrderBy or ThenBy
                        isFirst = false;
                    }
                }

                if (orderByArg.HasValues)
                {
                    // TODO: Need a way to get index classes based on part names that are passed in.
                    //currentIndexType = GetIndexType(orderByArg.Name);
                    OrderBy(query, orderByArg.Children(), currentIndexType, isFirst);
                }
            }
        }

        private IQuery<ContentItem, IIndex> OrderByField(string fieldName, OrderByDirection? direction, IQuery<ContentItem, IIndex> query, bool isFirst)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            if (direction == null)
            {
                direction =  OrderByDirection.Ascending;
            }

            var expression = ToExpression(fieldName);

            if (direction == OrderByDirection.Ascending)
            {
                return isFirst ? query.OrderBy(expression) : query.ThenBy(expression);
            }
            else
            {
                return isFirst ? query.OrderByDescending(expression) : query.ThenByDescending(expression);
            }
        }

        private static Expression<Func<IIndex, object>> ToExpression(string property)
        {
            var parameter = Expression.Parameter(typeof(IIndex));
            var memberExpression = Expression.Property(parameter, property);     
            Expression convertExpr = Expression.Convert(memberExpression, typeof(object));
            var expression = Expression.Lambda(convertExpr, parameter);

            return (Expression<Func<IIndex, object>>)expression;
        }
    }
}