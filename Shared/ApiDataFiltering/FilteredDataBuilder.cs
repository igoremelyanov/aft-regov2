using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Linq = System.Linq.Expressions;

namespace AFT.RegoV2.Shared.ApiDataFiltering
{
    public class FilteredDataBuilder<T>
    {
        private FilteredDataRequest FilteredDataRequest { get; }
        private IQueryable<T> _queryable;
        private readonly Dictionary<string, Func<string, Linq.Expression<Func<T, bool>>>> _filterActions;

        public FilteredDataBuilder(FilteredDataRequest filteredDataRequest, IQueryable<T> rawData)
        {
            FilteredDataRequest = filteredDataRequest;
            _queryable = rawData;
            
            _filterActions = new Dictionary<string, Func<string, Linq.Expression<Func<T, bool>>>>();

        }

        public FilteredDataResponse<T> GetPageData()
        {
            var recordsRequested = FilteredDataRequest.RowCount;
            var pageIndex = Math.Max(FilteredDataRequest.PageIndex - 1, 0);

            var recordsTotalCount = DoFilterData();

            if (FilteredDataRequest.TopRecords != 0) 
            {
                recordsTotalCount = Math.Min(FilteredDataRequest.TopRecords, recordsTotalCount);
            }

            var totalPages = 0;
            if (recordsRequested > 0)
            {
                totalPages = (int)Math.Ceiling(((float)recordsTotalCount) / recordsRequested);
            }

            DoOrder();


            if (FilteredDataRequest.TopRecords == 0)
            {
                _queryable = _queryable
                    .Skip(recordsRequested * pageIndex)
                    .Take(recordsRequested);
            }
            else
            {
                if (pageIndex == totalPages - 1)
                {
                    var numRecords = recordsTotalCount - recordsRequested * pageIndex;
                    _queryable = _queryable
                             .Skip(recordsRequested * pageIndex)
                             .Take(numRecords);
                }
                else
                {
                  _queryable = _queryable
                      .Skip(recordsRequested * pageIndex)
                      .Take(recordsRequested);
                }
            }

            return new FilteredDataResponse<T>
            {
                Page = pageIndex + 1,
                Total = totalPages,
                Records = recordsTotalCount,
                Rows = _queryable.ToList()
            };
        }

        private object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException(nameof(conversionType));
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            if (conversionType == typeof(double))
                value = ((string)value).Replace(".", ",");
            if (conversionType.IsEnum)
                return Enum.Parse(conversionType, (string)value, true);
            if (conversionType == typeof(Guid))
                return Guid.Parse((string)value);
            if (conversionType == typeof (DateTimeOffset))
                return DateTimeOffset.Parse((string)value);
            return Convert.ChangeType(value, conversionType);
        }
        private int DoFilterData()
        {
            if (FilteredDataRequest.Filters == null)
                return _queryable.Count();

            var filters = FilteredDataRequest.Filters.Where(filter => !string.IsNullOrEmpty(filter.Data)).ToList();

            foreach (var filterData in filters)
            {
                var dataKey = filterData.Field;
                var dataOp = filterData.Comparison;
                var dataValue = filterData.Data;

                Linq.Expression<Func<T, bool>> wherePredicate;

                if (_filterActions.ContainsKey(dataKey.ToLower()))
                {
                    wherePredicate = _filterActions[dataKey.ToLower()].Invoke(dataValue);
                }
                else if ( dataKey.Contains("-date-range-"))
                {
                   var param = Linq.Expression.Parameter(typeof(T), "x");
                   var propName = dataKey.Substring(0, dataKey.IndexOf("-date-range-", 0));
                   var property = BuildPropertyExpression(param, propName);

                   var filter = Linq.Expression.Constant
                             (
                                 ChangeType(dataValue, property.Type), property.Type
                             );
                   Linq.BinaryExpression condition; 
                   if(dataKey.Contains("-date-range-start"))
                   {
                       condition = Linq.Expression.GreaterThanOrEqual(property, filter);
                   }
                   else
                   {
                       if (filter.Type == typeof(DateTimeOffset))
                        filter = Linq.Expression.Constant(((DateTimeOffset) filter.Value).AddDays(1), property.Type);
                       else
                        filter = Linq.Expression.Constant(((DateTime)filter.Value).AddDays(1), property.Type);

                       condition = Linq.Expression.LessThan(property, filter);
                   }
                   wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                else if (dataOp == ComparisonOperator.In)
                {
                    var param = Linq.Expression.Parameter(typeof(T), "x");
                    var property = BuildPropertyExpression(param, dataKey);
                    var collectionType = typeof(ICollection<>).MakeGenericType(property.Type);
                    var collectionData = JsonConvert.DeserializeObject(dataValue, collectionType);
                    var method = collectionType.GetMethod("Contains", new[] { property.Type });
                    var value = Linq.Expression.Constant(collectionData, collectionType);
                    var condition = Linq.Expression.Call(value, method, property);

                    wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                else
                {
                    var param = Linq.Expression.Parameter(typeof (T), "x");
                    var property = BuildPropertyExpression(param, dataKey);
                    var value = Linq.Expression.Constant(ChangeType(dataValue, property.Type), property.Type);
                    if (dataOp == ComparisonOperator.Cn && property.Type != typeof (string))
                    {
                        dataOp = ComparisonOperator.Eq;
                    }

                    var contains = typeof(string).GetMethod("Contains", new[] { typeof (string) });
                    var startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof (string) });
                    var endsWith = typeof(string).GetMethod("EndsWith", new[] { typeof (string) });
                    
                    Linq.Expression condition;
                    switch (dataOp)
                    {
                        case ComparisonOperator.Eq:
                            condition = Linq.Expression.Equal(property, value);
                            break;
                        case ComparisonOperator.Ne:
                            condition = Linq.Expression.NotEqual(property, value);
                            break;
                        case ComparisonOperator.Lt:
                            condition = Linq.Expression.LessThan(property, value);
                            break;
                        case ComparisonOperator.Le:
                            condition = Linq.Expression.LessThanOrEqual(property, value);
                            break;
                        case ComparisonOperator.Gt:
                            condition = Linq.Expression.GreaterThan(property, value);
                            break;
                        case ComparisonOperator.Ge:
                            condition = Linq.Expression.GreaterThanOrEqual(property, value);
                            break;
                        case ComparisonOperator.Bw:
                            condition = Linq.Expression.Call(property, startsWith, value);
                            break;
                        case ComparisonOperator.Bn:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, startsWith, value));
                            break;
                        case ComparisonOperator.Ni:
                            condition = Linq.Expression.Not(Linq.Expression.Call(value, contains, property));
                            break;
                        case ComparisonOperator.Ew:
                            condition = Linq.Expression.Call(property, endsWith, value);
                            break;
                        case ComparisonOperator.En:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, endsWith, value));
                            break;
                        case ComparisonOperator.Cn:
                            condition = Linq.Expression.Call(property, contains, value);
                            break;
                        case ComparisonOperator.Nc:
                            condition = Linq.Expression.Not(Linq.Expression.Call(property, contains, value));
                            break;
                        default:
                            condition = Linq.Expression.Equal(property, value);
                            break;
                    }
                    wherePredicate = Linq.Expression.Lambda<Func<T, bool>>(condition, param);
                }
                _queryable = _queryable.Where(wherePredicate);
            }
            return _queryable.Count();
        }
        private void DoOrder()
        {
            if (string.IsNullOrWhiteSpace(FilteredDataRequest.SortColumn))
                return;

            var isAscending = FilteredDataRequest.SortSord == null ||
                  FilteredDataRequest.SortSord.ToLowerInvariant().Contains("asc");

            var paramExpr = Linq.Expression.Parameter(_queryable.ElementType, "p");
            var expr = BuildPropertyExpression(paramExpr, FilteredDataRequest.SortColumn);
            var orderByExpr = Linq.Expression.Lambda(expr, paramExpr);
            var orderByCallExpr = Linq.Expression.Call(typeof(Queryable), isAscending ? "OrderBy" : "OrderByDescending",
                new[] { _queryable.ElementType, expr.Type }, _queryable.Expression, orderByExpr);

            _queryable = _queryable.Provider.CreateQuery<T>(orderByCallExpr);
        }

        // Building the expression x => x.[prop].[prop]...
        private static Linq.Expression BuildPropertyExpression(Linq.Expression paramExpr, string propertyPath)
        {
            var propTokens = propertyPath.Split('.');
            return propTokens.Aggregate(paramExpr, Linq.Expression.Property);
        }
    }
}
