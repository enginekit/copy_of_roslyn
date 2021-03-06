﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    public static class CommandLineProject
    {
        /// <summary>
        /// Create a <see cref="ProjectInfo"/> structure initialized from a compilers command line arguments.
        /// </summary>
        public static ProjectInfo CreateProjectInfo(string projectName, string language, IEnumerable<string> commandLineArgs, string projectDirectory, Workspace workspace = null)
        {
            // TODO (tomat): the method may throw all sorts of exceptions.
            var tmpWorkspace = workspace ?? new CustomWorkspace(DesktopMefHostServices.DefaultServices);
            var languageServices = tmpWorkspace.Services.GetLanguageServices(language);
            if (languageServices == null)
            {
                throw new ArgumentException(WorkspacesResources.UnrecognizedLanguageName);
            }

            var commandLineArgumentsFactory = languageServices.GetRequiredService<ICommandLineArgumentsFactoryService>();
            var commandLineArguments = commandLineArgumentsFactory.CreateCommandLineArguments(commandLineArgs, projectDirectory, isInteractive: false);

            // TODO (tomat): to match csc.exe/vbc.exe we should use CommonCommandLineCompiler.ExistingReferencesResolver to deal with #r's
            var referenceResolver = new MetadataFileReferenceResolver(commandLineArguments.ReferencePaths, commandLineArguments.BaseDirectory);
            var referenceProvider = tmpWorkspace.Services.GetRequiredService<IMetadataService>().GetProvider();
            var xmlFileResolver = new XmlFileResolver(commandLineArguments.BaseDirectory);
            var strongNameProvider = new DesktopStrongNameProvider(commandLineArguments.KeyFileSearchPaths);

            // resolve all metadata references.
            var boundMetadataReferences = commandLineArguments.ResolveMetadataReferences(new AssemblyReferenceResolver(referenceResolver, referenceProvider));
            var unresolvedMetadataReferences = boundMetadataReferences.FirstOrDefault(r => r is UnresolvedMetadataReference);
            if (unresolvedMetadataReferences != null)
            {
                throw new ArgumentException(string.Format(WorkspacesResources.CantResolveMetadataReference, ((UnresolvedMetadataReference)unresolvedMetadataReferences).Reference));
            }

            // resolve all analyzer references.
            var boundAnalyzerReferences = commandLineArguments.ResolveAnalyzerReferences();
            var unresolvedAnalyzerReferences = boundAnalyzerReferences.FirstOrDefault(r => r is UnresolvedAnalyzerReference);
            if (unresolvedAnalyzerReferences != null)
            {
                throw new ArgumentException(string.Format(WorkspacesResources.CantResolveAnalyzerReference, ((UnresolvedAnalyzerReference)unresolvedAnalyzerReferences).Display));
            }

            AssemblyIdentityComparer assemblyIdentityComparer;
            if (commandLineArguments.AppConfigPath != null)
            {
                try
                {
                    using (var appConfigStream = new FileStream(commandLineArguments.AppConfigPath, FileMode.Open, FileAccess.Read))
                    {
                        assemblyIdentityComparer = DesktopAssemblyIdentityComparer.LoadFromXml(appConfigStream);
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(string.Format(WorkspacesResources.ErrorWhileReadingSpecifiedConfigFile, e.Message));
                }
            }
            else
            {
                assemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default;
            }

            var projectId = ProjectId.CreateNewId(debugName: projectName);

            // construct file infos
            var docs = new List<DocumentInfo>();
            foreach (var fileArg in commandLineArguments.SourceFiles)
            {
                var absolutePath = Path.IsPathRooted(fileArg.Path) || string.IsNullOrEmpty(projectDirectory)
                    ? Path.GetFullPath(fileArg.Path)
                    : Path.GetFullPath(Path.Combine(projectDirectory, fileArg.Path));

                var relativePath = FilePathUtilities.GetRelativePath(projectDirectory, absolutePath);
                var isWithinProject = !Path.IsPathRooted(relativePath);

                var folderRoot = isWithinProject ? Path.GetDirectoryName(relativePath) : "";
                var folders = isWithinProject ? GetFolders(relativePath) : null;
                var name = Path.GetFileName(relativePath);
                var id = DocumentId.CreateNewId(projectId, absolutePath);

                var doc = DocumentInfo.Create(
                   id: id,
                   name: name,
                   folders: folders,
                   sourceCodeKind: fileArg.IsScript ? SourceCodeKind.Script : SourceCodeKind.Regular,
                   loader: new FileTextLoader(absolutePath, commandLineArguments.Encoding),
                   filePath: absolutePath);

                docs.Add(doc);
            }

            // construct file infos for additional files.
            var additionalDocs = new List<DocumentInfo>();
            foreach (var fileArg in commandLineArguments.AdditionalStreams)
            {
                var absolutePath = Path.IsPathRooted(fileArg.Path) || string.IsNullOrEmpty(projectDirectory)
                        ? Path.GetFullPath(fileArg.Path)
                        : Path.GetFullPath(Path.Combine(projectDirectory, fileArg.Path));

                var relativePath = FilePathUtilities.GetRelativePath(projectDirectory, absolutePath);
                var isWithinProject = !Path.IsPathRooted(relativePath);

                var folderRoot = isWithinProject ? Path.GetDirectoryName(relativePath) : "";
                var folders = isWithinProject ? GetFolders(relativePath) : null;
                var name = Path.GetFileName(relativePath);
                var id = DocumentId.CreateNewId(projectId, absolutePath);

                var doc = DocumentInfo.Create(
                   id: id,
                   name: name,
                   folders: folders,
                   sourceCodeKind: SourceCodeKind.Regular,
                   loader: new FileTextLoader(absolutePath, commandLineArguments.Encoding),
                   filePath: absolutePath);

                additionalDocs.Add(doc);
            }

            // If /out is not specified and the project is a console app the csc.exe finds out the Main method
            // and names the compilation after the file that contains it. We don't want to create a compilation, 
            // bind Mains etc. here. Besides the msbuild always includes /out in the command line it produces.
            // So if we don't have the /out argument we name the compilation "<anonymous>".
            string assemblyName = (commandLineArguments.OutputFileName != null) ?
                Path.GetFileNameWithoutExtension(commandLineArguments.OutputFileName) : "<anonymous>";

            // TODO (tomat): what should be the assemblyName when compiling a netmodule? Should it be /moduleassemblyname

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName,
                assemblyName,
                language: language,
                compilationOptions: commandLineArguments.CompilationOptions
                    .WithXmlReferenceResolver(xmlFileResolver)
                    .WithAssemblyIdentityComparer(assemblyIdentityComparer)
                    .WithStrongNameProvider(strongNameProvider)
                    .WithMetadataReferenceResolver(new AssemblyReferenceResolver(referenceResolver, referenceProvider)),
                parseOptions: commandLineArguments.ParseOptions,
                documents: docs,
                additionalDocuments: additionalDocs,
                metadataReferences: boundMetadataReferences,
                analyzerReferences: boundAnalyzerReferences);

            return projectInfo;
        }

        /// <summary>
        /// Create a <see cref="ProjectInfo"/> structure initialized with data from a compiler command line.
        /// </summary>
        public static ProjectInfo CreateProjectInfo(string projectName, string language, string commandLine, string baseDirectory, Workspace workspace = null)
        {
            var args = CommandLineParser.SplitCommandLineIntoArguments(commandLine, removeHashComments: true);
            return CreateProjectInfo(projectName, language, args, baseDirectory, workspace);
        }

        private static readonly char[] folderSplitters = new char[] { Path.DirectorySeparatorChar };

        private static IList<string> GetFolders(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                return ImmutableArray.Create<string>();
            }
            else
            {
                return directory.Split(folderSplitters, StringSplitOptions.RemoveEmptyEntries).ToImmutableArray();
            }
        }
    }
}