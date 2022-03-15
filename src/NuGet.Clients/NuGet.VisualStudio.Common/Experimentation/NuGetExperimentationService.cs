// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Experimentation;
using Microsoft.VisualStudio.Shell;
using NuGet.Common;

namespace NuGet.VisualStudio
{
    [Export(typeof(INuGetExperimentationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NuGetExperimentationService : INuGetExperimentationService
    {
        private readonly IEnvironmentVariableReader _environmentVariableReader;
        private readonly IExperimentationService _experimentationService;
        private readonly IAsyncServiceProvider _asyncServiceProvider;
        private readonly Microsoft.VisualStudio.Threading.AsyncLazy<IVsFeatureFlags> _ivsFeatureFlags;
        private readonly Dictionary<string, bool> _featureFlagCache;

        [ImportingConstructor]
        public NuGetExperimentationService(IAsyncServiceProvider asyncServiceProvider)
            : this(EnvironmentVariableWrapper.Instance, ExperimentationService.Default, asyncServiceProvider)
        {
            // ensure uniqueness.
            _ivsFeatureFlags = new(() => _asyncServiceProvider.GetServiceAsync<SVsFeatureFlags, IVsFeatureFlags>(), NuGetUIThreadHelper.JoinableTaskFactory);
            _featureFlagCache = new();
        }

        internal NuGetExperimentationService(IEnvironmentVariableReader environmentVariableReader, IExperimentationService experimentationService, IAsyncServiceProvider asyncServiceProvider)
        {
            _environmentVariableReader = environmentVariableReader ?? throw new ArgumentNullException(nameof(environmentVariableReader));
            _experimentationService = experimentationService ?? throw new ArgumentNullException(nameof(experimentationService));
            _asyncServiceProvider = asyncServiceProvider ?? throw new ArgumentNullException(nameof(asyncServiceProvider));
        }

        public bool IsExperimentEnabled(ExperimentationConstants experiment)
        {
            GetEnvironmentVariablesForExperiment(experiment, out bool isExpForcedEnabled, out bool isExpForcedDisabled);

            return !isExpForcedDisabled && (isExpForcedEnabled || _experimentationService.IsCachedFlightEnabled(experiment.FlightFlag));
        }

        private void GetEnvironmentVariablesForExperiment(ExperimentationConstants experiment, out bool isExpForcedEnabled, out bool isExpForcedDisabled)
        {
            isExpForcedEnabled = false;
            isExpForcedDisabled = false;
            if (!string.IsNullOrEmpty(experiment.FlightEnvironmentVariable))
            {
                string envVarOverride = _environmentVariableReader.GetEnvironmentVariable(experiment.FlightEnvironmentVariable);

                isExpForcedDisabled = envVarOverride == "0";
                isExpForcedEnabled = envVarOverride == "1";
            }
        }

        public async Task<bool> IsFeatureEnabledAsync(ExperimentationConstants experiment)
        {
            GetEnvironmentVariablesForExperiment(experiment, out bool isFeatureForcedEnabled, out bool isFeatureForcedDisabled);
            if (_featureFlagCache.TryGetValue(experiment.FlightFlag, out bool featureEnabled))
            {
                var featureFlagService = await _ivsFeatureFlags.GetValueAsync();
                await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                featureEnabled = featureFlagService.IsFeatureEnabled(experiment.FlightFlag, true);
                _featureFlagCache.Add(experiment.FlightFlag, featureEnabled);
            }

            return !isFeatureForcedDisabled && (isFeatureForcedEnabled || featureEnabled);
        }
    }
}
