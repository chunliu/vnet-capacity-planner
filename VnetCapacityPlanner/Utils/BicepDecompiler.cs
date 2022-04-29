﻿using Bicep.Core.Configuration;
using Bicep.Core.Features;
using Bicep.Core.FileSystem;
using Bicep.Core.Registry;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.TypeSystem.Az;
using Bicep.Decompiler;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using IOFileSystem = System.IO.Abstractions.FileSystem;

namespace VnetCapacityPlanner.Utils
{
    public record DecompileResult(string BicepFile, string Error);

    public class EmptyModuleRegistryProvider : IModuleRegistryProvider
    {
        public ImmutableArray<IModuleRegistry> Registries => ImmutableArray<IModuleRegistry>.Empty;
    }

    public class BicepDecompiler
    {
        private static readonly IFeatureProvider features = new FeatureProvider();

        private static readonly INamespaceProvider namespaceProvider 
            = new DefaultNamespaceProvider(new AzResourceTypeLoader(), features);

        public static async Task<DecompileResult> Decompile(string jsonContent)
        {
            var jsonUri = new Uri("inmemory:///main.json");

            var fileResolver = new InMemoryFileResolver(new Dictionary<Uri, string>
            {
                [jsonUri] = jsonContent,
            });

            return await Task.Run(() =>
            {
                try
                {
                    var bicepUri = PathHelper.ChangeToBicepExtension(jsonUri);
                    var decompiler = new TemplateDecompiler(features, namespaceProvider, fileResolver,
                        new EmptyModuleRegistryProvider(), new ConfigurationManager(new IOFileSystem()));
                    var (entrypointUri, filesToSave) = decompiler.DecompileFileWithModules(jsonUri, bicepUri);

                    return new DecompileResult(filesToSave[entrypointUri], null);
                }
                catch (Exception exception)
                {
                    return new DecompileResult(null, exception.Message);
                }
            });
        }
    }
}
