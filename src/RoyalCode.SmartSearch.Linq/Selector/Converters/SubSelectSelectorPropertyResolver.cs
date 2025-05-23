using RoyalCode.Extensions.PropertySelection;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Selector.Converters;

internal sealed class SubSelectSelectorPropertyResolver : ISelectorPropertyResolver
{
    public bool CanConvert(PropertyMatch selection, ISelectResolver resolver, out ISelectorPropertyConverter? converter)
    {
        // check if the target (entity) property and the origin (DTO) property
        // are classes but not IEnumerable.
        var areClasses = selection.TargetSelection!.PropertyType.IsClass
            && selection.OriginProperty.PropertyType.IsClass
            && selection.TargetSelection.PropertyType.IsAssignableTo(typeof(IEnumerable)) is false
            && selection.OriginProperty.PropertyType.IsAssignableTo(typeof(IEnumerable)) is false;

        if (areClasses && resolver.GetResolutions(
                selection.TargetSelection.PropertyType,
                selection.OriginProperty.PropertyType,
                out var resolutions,
                out var ctor))
        {
            converter = new Converter(resolutions, ctor, selection.TargetSelection);
            return true;
        }

        converter = null;
        return false;
    }

    private sealed class Converter : ISelectorPropertyConverter
    {
        private readonly IEnumerable<SelectResolution> resolutions;
        private readonly ConstructorInfo ctor;
        private readonly PropertySelection targetSelection;

        public Converter(
            IEnumerable<SelectResolution> resolutions,
            ConstructorInfo ctor,
            PropertySelection targetSelection)
        {
            this.resolutions = resolutions;
            this.ctor = ctor;
            this.targetSelection = targetSelection;
        }

        public Expression GetExpression(PropertyMatch selection, Expression parameter)
        {
            // get the target member
            var targetParameter = targetSelection.GetAccessExpression(parameter);

            // generate de bindings for create the new DTO
            var bindings = resolutions
                .Select(r => Expression.Bind(r.Match.OriginProperty, r.Converter.GetExpression(r.Match, targetParameter)));

            // create the new DTO with the bindings.
            var newDto = Expression.MemberInit(Expression.New(ctor), bindings);

            return newDto;
        }
    }
}
