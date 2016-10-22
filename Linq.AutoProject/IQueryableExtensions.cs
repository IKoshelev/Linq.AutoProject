using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Linq.AutoProject
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Projects all public properties of the source that have a 
        /// public property with matching name and type to the target.
        /// Only projects properties which were not bound by target creation lambda.
        /// You will need to call <reference name="ActivateAutoProjects"> on your IQueryable
        /// to reqrite its expression activating the projection. 
        /// </summary>
        /// <typeparam name="TResult">Type of the source of projection.</typeparam>
        /// <typeparam name="TSource">Type of the target of projection.</typeparam>
        /// <param name="source">Object from which to project.</param>
        /// <param name="targetInitLambda">Lamda that inits a new instance of target. 
        /// It will be modified by adding projections of all suitable properties to it,
        /// and then expression tree of the query will be modified by replacing the call to 
        /// <reference name="AutoProjectInto"> with its body.</reference> </param>
        /// <returns>Does not actually return, will be modified</returns>
        public static TResult AutoProjectInto<TResult, TSource>(
            this TSource source,
            Expression<Func<TResult>> targetInitLambda)
        {
            throw new NotImplementedException(
                $"In order to activate {nameof(IQueryableExtensions)}.{nameof(AutoProjectInto)}" + 
                $" you must call {nameof(IQueryableExtensions)}.{nameof(ActivateAutoProjects)}" + 
                $" extension method on your {nameof(IQueryable)} before evaluating the final query.");
        }

        /// <summary>
        /// Activates all instances of <reference name="AutoProjectInto"> inside
        /// a given query by replacing their invocation with the invocation of
        /// init lambda inside and adding into that init all properties that
        /// can be projected into it from the source of original invocation.
        /// </summary>
        /// <typeparam name="T">Type of IQueryable, does not change</typeparam>
        /// <param name="source">IQueryable<T> in which to activate <reference name="AutoProjectInto"> calls</param>
        /// <returns>If no calls were found - same IQueryable<T>, else - new IQueryable<T>
        /// where the cals have now been rewritten and activated.</returns>
        public static IQueryable<T> ActivateAutoProjects<T>(this IQueryable<T> source)
        {
            var expr = source.Expression;
            var newExpr = new AutoProjectActivator().Visit(expr);
            if(newExpr == expr)
            {
                return source;
            }
            var newQuery = source.Provider.CreateQuery<T>(newExpr);
            return newQuery;
        }
    }
}
