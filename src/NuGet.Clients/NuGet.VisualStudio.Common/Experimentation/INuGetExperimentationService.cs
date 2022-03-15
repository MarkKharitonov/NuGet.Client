// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace NuGet.VisualStudio
{
    /// <summary>
    /// A service by which you can check for enabled experiments.
    /// </summary>
    public interface INuGetExperimentationService
    {
        /// <summary>
        /// Determines whether an experiment has been enabled.
        /// </summary>
        /// <param name="experimentation">The experiment info.</param>
        /// <returns>Whether the experiment is enabled.</returns>
        bool IsExperimentEnabled(ExperimentationConstants experimentation);

        /// <summary>
        /// Determines whether a feature has been enabled.
        /// </summary>
        /// <param name="experimentation">The experiment info.</param>
        /// <returns>Whether the feature is enabled.</returns>
        Task<bool> IsFeatureEnabledAsync(ExperimentationConstants experimentation);
    }
}
