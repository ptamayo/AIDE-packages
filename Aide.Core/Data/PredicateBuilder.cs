using System;
using System.Linq;
using System.Linq.Expressions;

namespace Aide.Core.Data
{
    // Amazing resource that will help to avoid the AsEnumerable in linq queries
    // For further details see the links below:
    // http://www.albahari.com/nutshell/predicatebuilder.aspx
    // http://www.albahari.com/nutshell/linqkit.aspx
    // https://stackoverflow.com/questions/782339/how-to-dynamically-add-or-operator-to-where-clause-in-linq
    // Example 1:
    // var searchPredicate = PredicateBuilder.False<Songs>();
    // foreach(string str in strArray)
    // {
    //    searchPredicate = searchPredicate.Or(SongsVar => SongsVar.Tags.Contains(...your conditions...));
    // }
    // var allSongMatches = db.Songs.Where(searchPredicate)
    // Example 2:
    // var searchPredicate = PredicateBuilder.False<order_item>();
    // foreach(var item in orderItems)
    // {
    // 	searchPredicate = searchPredicate.Or(x => x.order_item_id == item.order_item_id && x.authorization_folio == item.authorization_folio);
    // }
    // var query = order_item.Where(searchPredicate);
    // var entities = query.Count();
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
