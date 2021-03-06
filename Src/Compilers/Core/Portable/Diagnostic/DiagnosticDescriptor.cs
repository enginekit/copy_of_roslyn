﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Roslyn.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Provides a description about a <see cref="Diagnostic"/>
    /// </summary>
    public class DiagnosticDescriptor
    {
        /// <summary>
        /// An unique identifier for the diagnostic.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// A short localizable title describing the diagnostic.
        /// </summary>
        public LocalizableString Title { get; private set; }

        /// <summary>
        /// An optional longer localizable description for the diagnostic.
        /// </summary>
        public LocalizableString Description { get; private set; }

        /// <summary>
        /// An optional hyperlink that provides more detailed information regarding the diagnostic.
        /// </summary>
        public string HelpLink { get; private set; }

        /// <summary>
        /// A localizable format message string, which can be passed as the first argument to <see cref="String.Format(string, object[])"/> when creating the diagnostic message with this descriptor.
        /// </summary>
        /// <returns></returns>
        public LocalizableString MessageFormat { get; private set; }

        /// <summary>
        /// The category of the diagnostic (like Design, Naming etc.)
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// The default severity of the diagnostic.
        /// </summary>
        public DiagnosticSeverity DefaultSeverity { get; private set; }

        /// <summary>
        /// Returns true if the diagnostic is enabled by default.
        /// </summary>
        public bool IsEnabledByDefault { get; private set; }

        /// <summary>
        /// Custom tags for the diagnostic.
        /// </summary>
        public IEnumerable<string> CustomTags { get; private set; }

        /// <summary>
        /// Create a DiagnosticDescriptor, which provides description about a <see cref="Diagnostic"/>.
        /// </summary>
        /// <param name="id">A unique identifier for the diagnostic. For example, code analysis diagnostic ID "CA1001".</param>
        /// <param name="title">A short localizable title describing the diagnostic. For example, for CA1001: "Types that own disposable fields should be disposable".</param>
        /// <param name="messageFormat">A localizable format message string, which can be passed as the first argument to <see cref="String.Format(string, object[])"/> when creating the diagnostic message with this descriptor.
        /// For example, for CA1001: "Implement IDisposable on '{0}' because it creates members of the following IDisposable types: '{1}'."</param>
        /// <param name="category">The category of the diagnostic (like Design, Naming etc.). For example, for CA1001: "Microsoft.Design".</param>
        /// <param name="defaultSeverity">Default severity of the diagnostic.</param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default.</param>
        /// <param name="description">An optional longer localizable description of the diagnostic.</param>
        /// <param name="helpLink">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags"/> for some well known tags.</param>
        /// <remarks>Example descriptor for rule CA1001:
        ///     internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
        ///         new LocalizableResourceString(nameof(FxCopRulesResources.TypesThatOwnDisposableFieldsShouldBeDisposable), FxCopRulesResources.ResourceManager, typeof(FxCopRulesResources)),
        ///         new LocalizableResourceString(nameof(FxCopRulesResources.TypeOwnsDisposableFieldButIsNotDisposable), FxCopRulesResources.ResourceManager, typeof(FxCopRulesResources)),
        ///         FxCopDiagnosticCategory.Design,
        ///         DiagnosticSeverity.Warning,
        ///         isEnabledByDefault: true,
        ///         helpLink: "http://msdn.microsoft.com/library/ms182172.aspx",
        ///         customTags: DiagnosticCustomTags.Microsoft);
        /// </remarks>
        public DiagnosticDescriptor(
            string id,
            LocalizableString title,
            LocalizableString messageFormat,
            string category, 
            DiagnosticSeverity defaultSeverity, 
            bool isEnabledByDefault,
            LocalizableString description = null, 
            string helpLink = null,
            params string[] customTags)
            : this(id, title, messageFormat, category, defaultSeverity, isEnabledByDefault, description, helpLink, customTags.AsImmutableOrEmpty())
        {
        }

        internal DiagnosticDescriptor(
            string id,
            LocalizableString title,
            LocalizableString messageFormat,
            string category,
            DiagnosticSeverity defaultSeverity,
            bool isEnabledByDefault,
            LocalizableString description,
            string helpLink,
            ImmutableArray<string> customTags)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(CodeAnalysisResources.DiagnosticIdCantBeNullOrWhitespace, nameof(id));
            }

            if (messageFormat == null)
            {
                throw new ArgumentNullException(nameof(messageFormat));
            }

            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            this.Id = id;
            this.Title = title;
            this.Category = category;
            this.MessageFormat = messageFormat;
            this.DefaultSeverity = defaultSeverity;
            this.IsEnabledByDefault = isEnabledByDefault;
            this.Description = description ?? string.Empty;
            this.HelpLink = helpLink ?? string.Empty;
            this.CustomTags = customTags.AsImmutableOrEmpty();
        }

        public override bool Equals(object obj)
        {
            var other = obj as DiagnosticDescriptor;
            return other != null &&
                this.Category == other.Category &&
                this.DefaultSeverity == other.DefaultSeverity &&
                this.Description == other.Description &&
                this.HelpLink == other.HelpLink &&
                this.Id == other.Id &&
                this.IsEnabledByDefault == other.IsEnabledByDefault &&
                this.MessageFormat == other.MessageFormat &&
                this.Title == other.Title;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(this.Category.GetHashCode(),
                Hash.Combine(this.DefaultSeverity.GetHashCode(),
                Hash.Combine(this.Description.GetHashCode(),
                Hash.Combine(this.HelpLink.GetHashCode(),
                Hash.Combine(this.Id.GetHashCode(),
                Hash.Combine(this.IsEnabledByDefault.GetHashCode(),
                Hash.Combine(this.MessageFormat.GetHashCode(),
                    this.Title.GetHashCode())))))));
        }
    }
}
