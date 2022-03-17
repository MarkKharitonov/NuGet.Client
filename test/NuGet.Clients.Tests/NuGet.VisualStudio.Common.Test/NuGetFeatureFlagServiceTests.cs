// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Microsoft.VisualStudio.Shell;
using Moq;
using NuGet.Common;
using Test.Utility;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace NuGet.VisualStudio.Common.Test
{
    [Collection(MockedVS.Collection)]
    public class NuGetFeatureFlagServiceTests
    {
        private GlobalServiceProvider _globalProvider;

        public NuGetFeatureFlagServiceTests(GlobalServiceProvider sp)
        {
            sp.Reset();
            _globalProvider = sp;
        }

        [Fact]
        public void Constructor_WithNullWrapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NuGetFeatureFlagService(null, AsyncServiceProvider.GlobalProvider));
        }

        [Fact]
        public void Constructor_WithNullAsyncServiceProvider_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NuGetFeatureFlagService(new TestEnvironmentVariableReader(new Dictionary<string, string>()), null));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]

        public async Task IsFeatureEnabledAsync_WithoutFlag_ReturnsDefaultValueFromConstant(bool featureFlagDefault)
        {
            var featureFlagConstant = new NuGetFeatureFlagConstants("featureFlag", "featureEnvVar", defaultFeatureFlag: featureFlagDefault);
            var vsFeatureFlags = Mock.Of<IVsFeatureFlags>();

            Mock.Get(vsFeatureFlags)
                .Setup(x => x.IsFeatureEnabled(
                    It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(featureFlagDefault);

            _globalProvider.AddService(typeof(SVsFeatureFlags), vsFeatureFlags);
            var service = new NuGetFeatureFlagService(new EnvironmentVariableWrapper(), AsyncServiceProvider.GlobalProvider);
            (await service.IsFeatureEnabledAsync(featureFlagConstant)).Should().Be(featureFlagDefault);
        }

        [Fact]
        public async Task IsFeatureEnabledAsync_WithEnabledFeatureFlagAndForcedEnabledEnvVar_ReturnsTrue()
        {
            var featureFlagConstant = new NuGetFeatureFlagConstants("featureFlag", "featureEnvVar", defaultFeatureFlag: false);
            var envVars = new Dictionary<string, string>()
            {
                { featureFlagConstant.FeatureEnvironmentVariable, "1" },
            };
            var envVarWrapper = new TestEnvironmentVariableReader(envVars);

            var vsFeatureFlags = Mock.Of<IVsFeatureFlags>();

            Mock.Get(vsFeatureFlags)
                .Setup(x => x.IsFeatureEnabled(
                    It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(true);

            _globalProvider.AddService(typeof(SVsFeatureFlags), vsFeatureFlags);
            var service = new NuGetFeatureFlagService(new EnvironmentVariableWrapper(), AsyncServiceProvider.GlobalProvider);
            (await service.IsFeatureEnabledAsync(featureFlagConstant)).Should().Be(true);

        }

        [Theory]
        [InlineData("2")]
        [InlineData("randomValue")]
        public async Task IsFeatureEnabledAsync_WithEnvVarWithIncorrectValue_WithEnvironmentVariable__ReturnsFalse(string value)
        {
            var featureFlagConstant = new NuGetFeatureFlagConstants("featureFlag", "featureEnvVar", defaultFeatureFlag: false);
            var envVars = new Dictionary<string, string>()
            {
                { featureFlagConstant.FeatureEnvironmentVariable, value },
            };
            var envVarWrapper = new TestEnvironmentVariableReader(envVars);

            var vsFeatureFlags = Mock.Of<IVsFeatureFlags>();

            Mock.Get(vsFeatureFlags)
                .Setup(x => x.IsFeatureEnabled(
                    It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(false);

            _globalProvider.AddService(typeof(SVsFeatureFlags), vsFeatureFlags);
            var service = new NuGetFeatureFlagService(new EnvironmentVariableWrapper(), AsyncServiceProvider.GlobalProvider);
            (await service.IsFeatureEnabledAsync(featureFlagConstant)).Should().Be(false);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task IsFeatureEnabledAsync_WithEnvVarNotSetWithEnabledFeatureFromWithFeatureFlagService_ReturnsExpectedResult(bool isFeatureEnabled, bool expectedResult)
        {
            var featureFlagConstant = new NuGetFeatureFlagConstants("featureFlag", "featureEnvVar", defaultFeatureFlag: false);
            var envVarWrapper = new TestEnvironmentVariableReader(new Dictionary<string, string>());

            var vsFeatureFlags = Mock.Of<IVsFeatureFlags>();

            Mock.Get(vsFeatureFlags)
                .Setup(x => x.IsFeatureEnabled(
                    It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(isFeatureEnabled);

            _globalProvider.AddService(typeof(SVsFeatureFlags), vsFeatureFlags);
            var service = new NuGetFeatureFlagService(new EnvironmentVariableWrapper(), AsyncServiceProvider.GlobalProvider);
            (await service.IsFeatureEnabledAsync(featureFlagConstant)).Should().Be(expectedResult);
        }

        [Fact]
        public void IsEnabled_WithEnvVarEnabled_WithExperimentalServiceDisabled_ReturnsTrue()
        {
            var constant = ExperimentationConstants.PackageManagerBackgroundColor;
            var flightsEnabled = new Dictionary<string, bool>()
            {
                { constant.FlightFlag, false },
            };

            var envVars = new Dictionary<string, string>()
            {
                { constant.FlightEnvironmentVariable, "1" },
            };
            var envVarWrapper = new TestEnvironmentVariableReader(envVars);

            var service = new NuGetExperimentationService(envVarWrapper, new TestVisualStudioExperimentalService(flightsEnabled));

            service.IsExperimentEnabled(ExperimentationConstants.PackageManagerBackgroundColor).Should().BeTrue();
        }

        [Fact]
        public void IsEnabled_WithEnvVarDisabled_WithExperimentalServiceEnabled_ReturnsFalse()
        {
            var constant = ExperimentationConstants.PackageManagerBackgroundColor;
            var flightsEnabled = new Dictionary<string, bool>()
            {
                { constant.FlightFlag, true },
            };

            var envVars = new Dictionary<string, string>()
            {
                { constant.FlightEnvironmentVariable, "0" },
            };
            var envVarWrapper = new TestEnvironmentVariableReader(envVars);

            var service = new NuGetExperimentationService(envVarWrapper, new TestVisualStudioExperimentalService(flightsEnabled));

            service.IsExperimentEnabled(ExperimentationConstants.PackageManagerBackgroundColor).Should().BeFalse();
        }

        [Fact]
        public void IsEnabled_WithNullEnvironmentVariableForConstant_HandlesGracefully()
        {
            var service = new NuGetExperimentationService(new EnvironmentVariableWrapper(), new TestVisualStudioExperimentalService());
            service.IsExperimentEnabled(new ExperimentationConstants("flag", null)).Should().BeFalse();
        }

        [Fact]
        public void IsEnabled_MultipleExperimentsOverriddenWithDifferentEnvVars_DoNotConflict()
        {
            var forcedOffExperiment = new ExperimentationConstants("TestExp1", "TEST_EXP_1");
            var forcedOnExperiment = new ExperimentationConstants("TestExp2", "TEST_EXP_2");
            var noOverrideExperiment = new ExperimentationConstants("TestExp3", "TEST_EXP_3");
            var flightsEnabled = new Dictionary<string, bool>()
            {
                { forcedOffExperiment.FlightFlag, true },
                { forcedOnExperiment.FlightFlag, true },
                { noOverrideExperiment.FlightFlag, true },
            };
            var envVars = new Dictionary<string, string>()
            {
                { forcedOnExperiment.FlightEnvironmentVariable, "1" },
                { forcedOffExperiment.FlightEnvironmentVariable, "0" },
            };
            var envVarWrapper = new TestEnvironmentVariableReader(envVars);
            var service = new NuGetExperimentationService(envVarWrapper, new TestVisualStudioExperimentalService(flightsEnabled));

            service.IsExperimentEnabled(forcedOffExperiment).Should().BeFalse();
            service.IsExperimentEnabled(forcedOnExperiment).Should().BeTrue();
            service.IsExperimentEnabled(noOverrideExperiment).Should().BeTrue();
        }
    }
}
