// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using JetBrains.Annotations;

namespace Canopy.Dependency;

[MeansImplicitUse(ImplicitUseKindFlags.Assign)]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ResolvedAttribute : Attribute
{
}
