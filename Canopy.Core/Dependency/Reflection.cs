// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;

namespace Canopy.Dependency;

public static class Reflection
{
    public static void ResolveDependencies(object target)
    {
        var type = target.GetType();
        var currentType = type;

        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<ResolvedAttribute>() == null)
                    continue;

                var service = DependencyContainer.Get(field.FieldType);
                field.SetValue(target, service);
            }

            currentType = currentType.BaseType;
        }
    }
}
