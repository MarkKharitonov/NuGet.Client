// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace NuGet.VisualStudio
{
    public interface INuGetFeatureFlagService
    {
        /// <summary>
        /// Determines whether a feature has been enabled.
        /// </summary>
        /// <param name="experimentation">The experiment info.</param>
        /// <returns>Whether the feature is enabled.</returns>
        Task<bool> IsFeatureEnabledAsync(NuGetFeatureFlagConstants experimentation);
    }
}
