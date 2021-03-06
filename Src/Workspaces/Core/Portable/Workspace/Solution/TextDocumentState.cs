﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal partial class TextDocumentState
    {
        protected SolutionServices solutionServices;
        protected DocumentInfo info;

        protected ValueSource<TextAndVersion> textSource;

        protected TextDocumentState(
            SolutionServices solutionServices,
            DocumentInfo info,
            ValueSource<TextAndVersion> textSource)
        {
            this.solutionServices = solutionServices;
            this.info = info;
            this.textSource = textSource;
        }

        public DocumentId Id
        {
            get { return this.info.Id; }
        }

        public string FilePath
        {
            get { return this.info.FilePath; }
        }

        public DocumentInfo Info
        {
            get { return this.info; }
        }

        public IReadOnlyList<string> Folders
        {
            get { return this.info.Folders; }
        }

        public string Name
        {
            get { return this.info.Name; }
        }

        public static TextDocumentState Create(DocumentInfo info, SolutionServices services)
        {
            var textSource = info.TextLoader != null
                ? CreateRecoverableText(info.TextLoader, info.Id, services, catchInvalidDataException: true)
                : CreateStrongText(TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, info.FilePath));

            // remove any initial loader so we don't keep source alive
            info = info.WithTextLoader(null);

            return new TextDocumentState(
                solutionServices: services,
                info: info,
                textSource: textSource);
        }

        protected static ValueSource<TextAndVersion> CreateStrongText(TextAndVersion text)
        {
            return new ConstantValueSource<TextAndVersion>(text);
        }

        protected static ValueSource<TextAndVersion> CreateStrongText(TextLoader loader, DocumentId documentId, SolutionServices services, bool catchInvalidDataException = false)
        {
            return new AsyncLazy<TextAndVersion>(c => LoadTextAsync(loader, documentId, services, c, catchInvalidDataException), cacheResult: true);
        }

        protected static ValueSource<TextAndVersion> CreateRecoverableText(TextAndVersion text, SolutionServices services)
        {
            return new RecoverableTextAndVersion(CreateStrongText(text), services.TemporaryStorage, services.TextCache);
        }

        protected static ValueSource<TextAndVersion> CreateRecoverableText(TextLoader loader, DocumentId documentId, SolutionServices services, bool catchInvalidDataException = false)
        {
            return new RecoverableTextAndVersion(
                new AsyncLazy<TextAndVersion>(c => LoadTextAsync(loader, documentId, services, c, catchInvalidDataException), cacheResult: false),
                services.TemporaryStorage,
                services.TextCache);
        }

        protected static async Task<TextAndVersion> LoadTextAsync(TextLoader loader, DocumentId documentId, SolutionServices services, CancellationToken cancellationToken, bool catchInvalidDataException = false)
        {
            try
            {
                using (ExceptionHelpers.SuppressFailFast())
                {
                    var result = await loader.LoadTextAndVersionAsync(services.Workspace, documentId, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                // if load text is failed due to a cancellation, make sure we propagate it out to the caller
                throw;
            }
            catch (IOException e)
            {
                services.Workspace.OnWorkspaceFailed(new DocumentDiagnostic(WorkspaceDiagnosticKind.Failure, e.Message, documentId));
                return TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, documentId.GetDebuggerDisplay());
            }
            catch (InvalidDataException) if (catchInvalidDataException)
            {
                // For non-text additional files, create an empty text document.
                // TODO: If we add support for non-text additional files in future, remove this catch clause.
                return TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, documentId.GetDebuggerDisplay());
            }
        }

        public bool TryGetText(out SourceText text)
        {
            TextAndVersion textAndVersion;
            if (this.textSource.TryGetValue(out textAndVersion))
            {
                text = textAndVersion.Text;
                return true;
            }
            else
            {
                text = null;
                return false;
            }
        }

        public bool TryGetTextVersion(out VersionStamp version)
        {
            // try fast path first
            if (TryGetTextVersionFromRecoverableTextAndVersion(out version))
            {
                return true;
            }

            TextAndVersion textAndVersion;
            if (this.textSource.TryGetValue(out textAndVersion))
            {
                version = textAndVersion.Version;
                return true;
            }
            else
            {
                version = default(VersionStamp);
                return false;
            }
        }

        protected bool TryGetTextVersionFromRecoverableTextAndVersion(out VersionStamp version)
        {
            version = default(VersionStamp);

            var recoverable = this.textSource as RecoverableTextAndVersion;
            return recoverable != null && recoverable.TryGetTextVersion(out version);
        }

        public async Task<SourceText> GetTextAsync(CancellationToken cancellationToken)
        {
            var textAndVersion = await this.textSource.GetValueAsync(cancellationToken).ConfigureAwait(false);
            return textAndVersion.Text;
        }

        public async Task<VersionStamp> GetTextVersionAsync(CancellationToken cancellationToken)
        {
            // try fast path first
            VersionStamp version;
            if (TryGetTextVersionFromRecoverableTextAndVersion(out version))
            {
                return version;
            }

            TextAndVersion textAndVersion;
            if (this.textSource.TryGetValue(out textAndVersion))
            {
                return textAndVersion.Version;
            }
            else
            {
                textAndVersion = await this.textSource.GetValueAsync(cancellationToken).ConfigureAwait(false);
                return textAndVersion.Version;
            }
        }

        public TextDocumentState UpdateText(TextAndVersion newTextAndVersion, PreservationMode mode)
        {
            if (newTextAndVersion == null)
            {
                throw new ArgumentNullException("newTextAndVesion");
            }

            var newTextSource = mode == PreservationMode.PreserveIdentity
                ? CreateStrongText(newTextAndVersion)
                : CreateRecoverableText(newTextAndVersion, this.solutionServices);

            return new TextDocumentState(
                this.solutionServices,
                this.info,
                newTextSource);
        }

        public TextDocumentState UpdateText(SourceText newText, PreservationMode mode)
        {
            if (newText == null)
            {
                throw new ArgumentNullException("newText");
            }

            var newVersion = this.GetNewerVersion();
            var newTextAndVersion = TextAndVersion.Create(newText, newVersion, this.FilePath);

            var newState = this.UpdateText(newTextAndVersion, mode);
            return newState;
        }

        public TextDocumentState UpdateText(TextLoader loader, PreservationMode mode)
        {
            if (loader == null)
            {
                throw new ArgumentNullException("loader");
            }

            // don't blow up on non-text documents.
            var newTextSource = (mode == PreservationMode.PreserveIdentity)
                ? CreateStrongText(loader, this.Id, this.solutionServices, catchInvalidDataException: true)
                : CreateRecoverableText(loader, this.Id, this.solutionServices, catchInvalidDataException: true);

            return new TextDocumentState(
                this.solutionServices,
                this.info,
                textSource: newTextSource);
        }

        private VersionStamp GetNewerVersion()
        {
            TextAndVersion textAndVersion;
            if (this.textSource.TryGetValue(out textAndVersion))
            {
                return textAndVersion.Version.GetNewerVersion();
            }

            return VersionStamp.Create();
        }

        public virtual async Task<VersionStamp> GetTopLevelChangeTextVersionAsync(CancellationToken cancellationToken)
        {
            TextAndVersion textAndVersion = await this.textSource.GetValueAsync(cancellationToken).ConfigureAwait(false);
            return textAndVersion.Version;
        }
    }
}
