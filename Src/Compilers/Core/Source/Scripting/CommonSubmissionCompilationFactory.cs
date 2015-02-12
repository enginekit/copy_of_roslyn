using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;

namespace Roslyn.Scripting
{
    internal abstract class CommonSubmissionCompilationFactory
    {
        // global references applicable to all code compiled with this engine
        private readonly ReadOnlyArray<MetadataReference> initialAssemblyReferences;

        // global usings applicable to all code compiled with this engine:
        private readonly ReadOnlyArray<string> initialNamespaceImports;

        protected readonly MetadataFileProvider metadataFileProvider;

        /// <summary>
        /// Unique prefix for generated uncollectible assemblies.
        /// </summary>
        /// <remarks>
        /// The full names of uncollectible assemblies generated by this context must be unique,
        /// so that we can resolve references among them. Note that CLR can load two different assemblies of the very 
        /// identity into the same load context.
        /// 
        /// We are using a certain naming scheme for the generated assemblies (a fixed name prefix followed by a number). 
        /// If we allowed the compiled code to add references that match this exact pattern it migth happen that 
        /// the user supplied reference identity conflicts with the identity we use for our generated assemblies and 
        /// the AppDomain assembly resolve event won't be able to correctly identify the target assembly.
        /// 
        /// To avoid this problem we use a prefix for assemblies we generate that is unlikely to conflict with user specified references.
        /// We also check that no user provided references are allowed to be used in the compiled code and report an error ("reserved assembly name").
        /// </remarks>
        private static readonly string globalAssemblyNamePrefix;
        private static int engineIdDispenser;
        private int submissionIdDispenser = -1;
        private readonly string assemblyNamePrefix;

        static CommonSubmissionCompilationFactory()
        {
            globalAssemblyNamePrefix = "\u211B*" + Guid.NewGuid().ToString() + "-";
        }

        internal CommonSubmissionCompilationFactory(IEnumerable<string> importedNamespaces, MetadataFileProvider metadataFileProvider)
        {
            if (metadataFileProvider == null)
            {
                metadataFileProvider = MetadataFileProvider.Default;
            }

            this.initialAssemblyReferences = new MetadataReference[]
            { 
                metadataFileProvider.GetReference(typeof(object).Assembly.Location, MetadataReferenceProperties.Assembly),
                metadataFileProvider.GetReference(typeof(Session).Assembly.Location, MetadataReferenceProperties.Assembly)
            }.AsReadOnlyWrap();

            this.initialNamespaceImports = importedNamespaces.AsReadOnlyOrEmpty();
            this.assemblyNamePrefix = globalAssemblyNamePrefix + "#" + Interlocked.Increment(ref engineIdDispenser);
            this.metadataFileProvider = metadataFileProvider;
        }

        internal string AssemblyNamePrefix
        {
            get { return assemblyNamePrefix; }
        }

        internal static bool IsReservedAssemblyName(AssemblyIdentity identity)
        {
            return identity.Name.StartsWith(globalAssemblyNamePrefix);
        }

        internal int GenerateSubmissionId(out string assemblyName, out string typeName)
        {
            int id = Interlocked.Increment(ref submissionIdDispenser);
            assemblyName = assemblyNamePrefix + id;
            typeName = "Submission#" + id;
            return id;
        }

        internal ReadOnlyArray<string> InitialNamespaceImports
        {
            get { return initialNamespaceImports; }
        }

        internal MetadataFileProvider MetadataFileProvider
        {
            get { return metadataFileProvider; }
        }

        internal IEnumerable<MetadataReference> GetReferences(Session session)
        {
            var previousSubmission = (session != null) ? session.LastSubmission : null;

            IEnumerable<MetadataReference> references;
            if (previousSubmission != null)
            {
                references = previousSubmission.References;
            }
            else
            {
                references = initialAssemblyReferences.AsEnumerable();
            }

            if (session != null)
            {
                references = references.Concat(session.PendingReferences.AsEnumerable());
            }

            return references;
        }

        internal ReadOnlyArray<string> GetImportedNamespaces(Session session)
        {
            // TODO (tomat): bound imports should be reused from previous submission instead of passing 
            // them to every submission in the chain. See bug #7802.
            //
            //if (session != null && session.LastSubmission != null)
            //{
            //    // engine wide namespaces have already been imported by the first submission:
            //    return session.PendingNamespaces;
            //}
            //else 

            if (session != null)
            {
                // this is the first submission of a session:
                return initialNamespaceImports.Concat(session.PendingNamespaces);
            }
            else
            {
                // this is a standalone submission:
                return initialNamespaceImports;
            }
        }

        internal abstract CommonCompilation CreateCompilation(IText text, string path, bool isInteractive, Session session, Type returnType, DiagnosticBag localDiagnostics);
    }
}
