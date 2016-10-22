using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Linq.AutoProject
{
    public class AutoProjectActivator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {           
            if (IsMethodCallExprForProjectIntoMethod(node) == false)
            {
                return base.VisitMethodCall(node);
            }

            Expression substituteExpr = CreateSubstituteMemberInitExprFromProjectMethodCall(node);

            return substituteExpr;
            
        }

        private MethodInfo GetMethodInfoForProjectInto()
        {
            return typeof(IQueryableExtensions).GetMethod(nameof(IQueryableExtensions.AutoProjectInto),
                                BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
        }

        private bool IsMethodCallExprForProjectIntoMethod(MethodCallExpression node)
        {
            var projectIntoMethodInfo = GetMethodInfoForProjectInto();

            return node.Method.IsGenericMethod
                && node.Method.GetGenericMethodDefinition() == projectIntoMethodInfo;
        }

        private class PropMatch
        {
            public PropertyInfo PropInSourceType { get; set; }
            public PropertyInfo PropInTargetType { get; set; }
        }

        private Expression CreateSubstituteMemberInitExprFromProjectMethodCall(MethodCallExpression node)
        {
            Expression sourceExpr = node.Arguments[0];
            Type sourceType = (node.Arguments[0].Type);
            MemberInitExpression targetInitExpr = ExtractMemberInitExpressionFromProjectArgument(node.Arguments[1]);

            PropMatch[] projectablePropertiesNotBoundByTargetInit
                = DeterminePropertiesThatCanBeProjectedFromSourceToTargetAndAreNotBoundByTargetInit(sourceType, targetInitExpr);

            MemberAssignment[] additonalBindings 
                = GetMemberAssignmentExpressionsProjectingSourceIntoTarget(sourceExpr, projectablePropertiesNotBoundByTargetInit);

            var combinedBindings = targetInitExpr.Bindings.Union(additonalBindings);

            var newInitExpr = Expression.MemberInit(targetInitExpr.NewExpression, combinedBindings);

            return newInitExpr;
        }

        private MemberInitExpression ExtractMemberInitExpressionFromProjectArgument(Expression expression)
        {
            LambdaExpression lambdaExpr = expression.As<UnaryExpression>()?.Operand.As<LambdaExpression>()
                                            ?? expression.As<LambdaExpression>();

            MemberInitExpression extractedExpression = lambdaExpr?.Body.As<MemberInitExpression>();

            if(extractedExpression == null)
            {
                var newExpr = lambdaExpr?.Body.As<NewExpression>();
                if(newExpr != null)
                {
                    extractedExpression = Expression.MemberInit(newExpr);
                }
            }

            if(extractedExpression == null)
            {
                var ProjectInto = nameof(IQueryableExtensions.AutoProjectInto);

                throw new NotSupportedException($"{ProjectInto} has an invalid argument." +
                    "Argument must be a lambda consisting solely of a constructor for the object to project into. " + 
                    "I.E,:\r\n" + 
                    $"queryableOfTypeSource.Select(source => source.{ProjectInto}(() => new Target(){{\r\n" + 
                    "   anotherVar = 5 \r\n" + 
                    "}})");
            }

            return extractedExpression;
        }

        private PropMatch[] DeterminePropertiesThatCanBeProjectedFromSourceToTargetAndAreNotBoundByTargetInit(Type sourceType, MemberInitExpression targetInitExpr)
        {
            string[] propsInTargetTypeBoundByInitExpr = targetInitExpr.Bindings.Select(x => x.Member).Select(x => x.Name).ToArray();
            PropertyInfo[] propsInTargetTypeNotBoundByInitExpr = targetInitExpr
                                    .Type
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(x =>
                                    {
                                        return x.GetSetMethod(false) != null;
                                    })
                                    .Where(x => propsInTargetTypeBoundByInitExpr.Contains(x.Name) == false)
                                    .ToArray();

            PropMatch[] remainingSuitableProperties 
                = sourceType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x =>
                        {
                            return x.GetGetMethod(false) != null;
                        })
                        .Select(sourceProp =>
                        {
                            var matchingTargetProp = GetPropertyInfoMatchingNameAndType(sourceProp, propsInTargetTypeNotBoundByInitExpr);
                            return new PropMatch
                            {
                                PropInSourceType = sourceProp,
                                PropInTargetType = matchingTargetProp
                            };
                        })
                        .Where(x => x.PropInTargetType != null)
                        .ToArray();

            return remainingSuitableProperties;

        }

        private PropertyInfo GetPropertyInfoMatchingNameAndType(PropertyInfo toSearch, PropertyInfo[] seachIn)
        {
            return seachIn.FirstOrDefault(x => x.Name == toSearch.Name && x.PropertyType == toSearch.PropertyType);
        }

        private MemberAssignment[] GetMemberAssignmentExpressionsProjectingSourceIntoTarget(Expression sourceExpr, PropMatch[] propertiesToProject)
        {
            return propertiesToProject.Select(x =>
            {
                var initValueExpr = Expression.MakeMemberAccess(sourceExpr, x.PropInSourceType);
                return Expression.Bind(x.PropInTargetType, initValueExpr);
            })
            .ToArray();
        }

    }
}
