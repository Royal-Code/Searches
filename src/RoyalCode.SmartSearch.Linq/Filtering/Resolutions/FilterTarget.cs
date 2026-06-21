using RoyalCode.Extensions.PropertySelection;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class FilterTarget
{
    // implicit convert Type to FilterTarget
    public static implicit operator FilterTarget(Type modelType) => new(modelType);

    public FilterTarget(Type modelType)
    {
        ModelType = modelType;
        TargetType = modelType;
    }

    public FilterTarget(Type modelType, Type targetType, PropertySelection? parentSelection = null)
    {
        ModelType = modelType;
        TargetType = targetType;
        ParentSelection = parentSelection;
    }

    public PropertySelection? ParentSelection { get; }

    public Type ModelType { get;}

    public Type TargetType { get; }

    public PropertySelection? TrySelectProperty(string propertyPath)
    {
        if (ParentSelection is null)
            return TargetType.TrySelectProperty(propertyPath);

        return ParentSelection.SelectChild(propertyPath, false);
    }
}
