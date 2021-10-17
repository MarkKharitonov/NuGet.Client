// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCORE5_0
using System;
#endif
using System.IO;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.Packaging
{
    /// <summary>
    /// A V3 path resolver.
    /// </summary>
    public class VersionFolderPathResolver
    {
        /// <summary>
        /// Gets the packages directory root folder.
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        /// Gets a flag indicating whether or not package ID's and versions are made lowercase.
        /// </summary>
        public bool IsLowerCase { get; }

        /// <summary>
        /// Initializes a new <see cref="VersionFolderPathResolver" /> class.
        /// </summary>
        /// <param name="rootPath">The packages directory root folder.</param>
        public VersionFolderPathResolver(string rootPath) : this(rootPath, isLowercase: true)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="VersionFolderPathResolver" /> class.
        /// </summary>
        /// <param name="rootPath">The packages directory root folder.</param>
        /// <param name="isLowercase"><c>true</c> if package ID's and versions are made lowercase;
        /// otherwise <c>false</c>.</param>
        public VersionFolderPathResolver(string rootPath, bool isLowercase)
        {
            RootPath = rootPath;
            IsLowerCase = isLowercase;
        }

        /// <summary>
        /// Gets the package install path.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The package install path.</returns>
        public virtual string GetInstallPath(string packageId, NuGetVersion version)
        {
#if NETCORE5_0
            return Path.Join(
                RootPath.AsSpan(),
                GetPackageDirectory(packageId, version));
#else
            return Path.Combine(
                RootPath,
                GetPackageDirectory(packageId, version));
#endif
        }

        /// <summary>
        /// Gets the package version list path.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <returns>The package version list path.</returns>
        public string GetVersionListPath(string packageId)
        {
#if NETCORE5_0
            return Path.Join(
                RootPath.AsSpan(),
                GetVersionListDirectory(packageId).AsSpan());
#else
            return Path.Combine(
                RootPath,
                GetVersionListDirectory(packageId));
#endif

        }

        /// <summary>
        /// Gets the package file path.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The package file path.</returns>
        public string GetPackageFilePath(string packageId, NuGetVersion version)
        {
#if NETCORE5_0
            return Path.Join(
                GetInstallPath(packageId, version).AsSpan(),
                GetPackageFileName(packageId, version).AsSpan());
#else
            return Path.Combine(
                GetInstallPath(packageId, version),
                GetPackageFileName(packageId, version));
#endif
        }

        /// <summary>
        /// Gets the manifest file path.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The manifest file path.</returns>
        public string GetManifestFilePath(string packageId, NuGetVersion version)
        {
            packageId = Normalize(packageId);
#if NETCORE5_0            
            return Path.Join(
                GetInstallPath(packageId, version).AsSpan(),
                GetManifestFileName(packageId, version).AsSpan());
#else
            return Path.Combine(
                GetInstallPath(packageId, version),
                GetManifestFileName(packageId, version));
#endif
        }

        /// <summary>
        /// Gets the hash file path.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The hash file path.</returns>
        public string GetHashPath(string packageId, NuGetVersion version)
        {
#if NETCORE5_0
            return Path.Join(
                GetInstallPath(packageId, version).AsSpan(),
                GetHashFileName(packageId, version).AsSpan());
#else
            return Path.Combine(
                GetInstallPath(packageId, version),
                GetHashFileName(packageId, version));
#endif
        }

        /// <summary>
        /// Gets the hash file name.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The hash file name.</returns>
        public string GetHashFileName(string packageId, NuGetVersion version)
        {
            return $"{Normalize(packageId)}.{Normalize(version)}{PackagingCoreConstants.HashFileExtension}";
        }

        /// <summary>
        /// Gets the new hash file path which represents the original hash of the package.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The hash file path.</returns>
        public string GetNupkgMetadataPath(string packageId, NuGetVersion version)
        {
#if NETCORE5_0
            return Path.Join(
                GetInstallPath(packageId, version).AsSpan(),
                PackagingCoreConstants.NupkgMetadataFileExtension.AsSpan());
#else
            return Path.Combine(
                GetInstallPath(packageId, version),
                PackagingCoreConstants.NupkgMetadataFileExtension);
#endif
        }

        /// <summary>
        /// Gets the version list directory.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <returns>The version list directory.</returns>
        public virtual string GetVersionListDirectory(string packageId)
        {
            return Normalize(packageId);
        }

        /// <summary>
        /// Gets the package directory.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The package directory.</returns>
        public virtual string GetPackageDirectory(string packageId, NuGetVersion version)
        {
#if NETCORE5_0
            return Path.Join(
                GetVersionListDirectory(packageId).AsSpan(),
                Normalize(version).AsSpan());
#else
            return Path.Combine(
                GetVersionListDirectory(packageId),
                Normalize(version));
#endif
        }

        /// <summary>
        /// Gets the package file name.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The package file name.</returns>
        public virtual string GetPackageFileName(string packageId, NuGetVersion version)
        {
            return $"{Normalize(packageId)}.{Normalize(version)}{PackagingCoreConstants.NupkgExtension}";
        }

        /// <summary>
        /// Gets the package download marker file name.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <returns>The package download marker file name.</returns>
        public string GetPackageDownloadMarkerFileName(string packageId)
        {
            return $"{Normalize(packageId)}{PackagingCoreConstants.PackageDownloadMarkerFileExtension}";
        }

        /// <summary>
        /// Gets the manifest file name.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The manifest file name.</returns>
        public virtual string GetManifestFileName(string packageId, NuGetVersion version)
        {
            return $"{Normalize(packageId)}{PackagingCoreConstants.NuspecExtension}";
        }

        private string Normalize(NuGetVersion version)
        {
            var versionString = version.ToNormalizedString();

            if (IsLowerCase)
            {
                versionString = versionString.ToLowerInvariant();
            }

            return versionString;
        }

        private string Normalize(string packageId)
        {
            if (IsLowerCase)
            {
                packageId = packageId.ToLowerInvariant();
            }

            return packageId;
        }
    }
}
