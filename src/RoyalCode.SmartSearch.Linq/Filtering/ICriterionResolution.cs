using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal interface ICriterionResolution
{
    Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam);

    bool IsLacking([NotNullWhen(true)] out Lack? lack);
}
