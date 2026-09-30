using System;

public class GenericConfigResolver
{
    public TResolved Resolve<TBase, TVariant, TResolved>(
        TBase baseConfig,
        TVariant variantConfig
    )
        where TBase : IBaseConfig
        where TVariant : ISimpleVariantConfig
        where TResolved : IRuntimeConfig, new()
    {
        // 1. Determine the base to use
        TBase chosenBase = baseConfig;
        if (variantConfig != null)
        {
            if (variantConfig.ForceFallback || chosenBase == null)
            {
                chosenBase = (TBase)variantConfig.FallbackBase;
            }
        }

        if (chosenBase == null)
            throw new Exception($"Variant {variantConfig} has no fallback or null and no base is given.");

        // 2. Create runtime result
        var result = new TResolved();

        // 3. Copy base into runtime (reflection)
        CopyFields(chosenBase, result);
        
        // 4. Apply variant overrides
        if(variantConfig != null)
            ApplyOverrides(result, variantConfig.Overrides);

        return result;
    }

    private void CopyFields(object from, object to)
    {
        var fields = from.GetType().GetFields();
        foreach (var f in fields)
        {
            var target = to.GetType().GetField(f.Name);
            if (target != null)
                target.SetValue(to, f.GetValue(from));
        }
    }

    private void ApplyOverrides(object runtime, IConfigOverrides overrides)
    {
        var oType = overrides.GetType();
        var rType = runtime.GetType();

        foreach (var field in oType.GetFields())
        {
            if (!field.Name.StartsWith("Override")) continue;

            bool shouldOverride = (bool)field.GetValue(overrides);
            if (!shouldOverride) continue;

            // Actual value field: e.g. "Radius" name after "OverrideRadius"
            var name = field.Name.Replace("Override", "");
            var valField = oType.GetField(name);
            var runtimeField = rType.GetField(name);

            runtimeField?.SetValue(runtime, valField?.GetValue(overrides));
        }
    }
}
