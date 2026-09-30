using Sirenix.OdinInspector.Editor.Validation;

// Register this validator for all properties/objects whose value type is IVariantConfig.
[assembly: RegisterValidator(typeof(VariantConfigFallbackValidator))]

public class VariantConfigFallbackValidator : ValueValidator<ISimpleVariantConfig>
{
    protected override void Validate(ValidationResult result)
    {
        if (Value == null)
            return;

        if (Value.ForceFallback && Value.FallbackBase == null)
        {
            result.AddWarning("Fallback base is set to null, this variant will force not to create any product, if used in a car slot.");
        }
    }
}