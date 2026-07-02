using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// Default implementation of <see cref="IOrderByGenerator"/>, using <see cref="PropertySelection"/> for
/// lookup of properties.
/// </summary>
public sealed class DefaultOrderByGenerator : IOrderByGenerator
{
    /// <inheritdoc/>
    public Expression? Generate<TModel>(string orderBy) where TModel : class
    {
        var selection = typeof(TModel).TrySelectProperty(orderBy)
            ?? TrySelectPropertyIgnoringCase(typeof(TModel), orderBy);

        if (selection is null)
            return null;

        var parameter = Expression.Parameter(typeof(TModel), "x");
        var property = selection.GetAccessExpression(parameter);
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(TModel), property.Type);
        var lambda = Expression.Lambda(delegateType, property, parameter);
        return lambda;
    }

    // Fallback quando o match exato (case-sensitive) falha: resolve cada segmento do caminho
    // ("estoque.disponivel", "preco") ignorando case. Limitacoes deliberadas: nomes concatenados em
    // PascalCase ("EstoqueDisponivel") continuam dependendo do case para delimitar os segmentos, e
    // ambiguidade (duas propriedades diferindo apenas por case) mantem o order by como nao suportado.
    private static PropertySelection? TrySelectPropertyIgnoringCase(Type type, string orderBy)
    {
        PropertySelection? selection = null;
        var currentType = type;

        foreach (var segment in orderBy.Split('.', '-'))
        {
            PropertyInfo? info;
            try
            {
                info = currentType.GetProperty(
                    segment,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }

            if (info is null)
                return null;

            selection = selection is null ? new PropertySelection(info) : selection.SelectChild(info);
            currentType = info.PropertyType;
        }

        return selection;
    }
}
