﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.UnitTests.Diagnostics
{
    public partial class SuppressMessageAttributeTests
    {
        protected const string TestDiagnosticCategory = "Test";
        protected const string TestDiagnosticMessageTemplate = "{0}";

        protected class WarningOnCompilationEndedAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "CompilationEnded";
            private static DiagnosticDescriptor rule = GetRule(Id);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterCompilationEndAction(
                    (context) =>
                        {
                            context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, Location.None, messageArgs: Id));
                        }
                    );
            }
        }

        // Produces a warning on the declaration of any symbol whose name starts with a specified prefix
        protected class WarningOnNamePrefixDeclarationAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "Declaration";
            private static DiagnosticDescriptor rule = GetRule(Id);

            private string errorSymbolPrefix;

            public WarningOnNamePrefixDeclarationAnalyzer(string errorSymbolPrefix)
            {
                this.errorSymbolPrefix = errorSymbolPrefix;
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterSymbolAction(
                    (context) =>
                        {
                            if (context.Symbol.Name.StartsWith(this.errorSymbolPrefix))
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.Symbol.Locations.First(), messageArgs: context.Symbol.Name));
                            }
                        },
                    SymbolKind.Event,
                    SymbolKind.Field,
                    SymbolKind.Method,
                    SymbolKind.NamedType,
                    SymbolKind.Namespace,
                    SymbolKind.Property);
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }
        }

        // Produces a warning on the declaration of any named type
        protected class WarningOnTypeDeclarationAnalyzer : DiagnosticAnalyzer
        {
            public const string TypeId = "TypeDeclaration";
            private static DiagnosticDescriptor rule = GetRule(TypeId);

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterSymbolAction(
                    (context) =>
                        {
                            context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.Symbol.Locations.First(), messageArgs: context.Symbol.Name));
                        },
                    SymbolKind.NamedType);
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }
        }

        // Produces a warning for the end of every code body and every invocation expression within that code body
        protected class WarningOnCodeBodyAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "CodeBody";
            private static DiagnosticDescriptor rule = GetRule(Id);

            private string language;

            public WarningOnCodeBodyAnalyzer(string language)
            {
                this.language = language;
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                if (this.language == LanguageNames.CSharp)
                {
                    analysisContext.RegisterCodeBlockStartAction<CSharp.SyntaxKind>(new CSharpCodeBodyAnalyzer().Initialize);
                }
                else
                {
                    analysisContext.RegisterCodeBlockStartAction<VisualBasic.SyntaxKind>(new BasicCodeBodyAnalyzer().Initialize);
                }
            }

            protected class CSharpCodeBodyAnalyzer
            {
                public void Initialize(CodeBlockStartAnalysisContext<CSharp.SyntaxKind> analysisContext)
                {
                    analysisContext.RegisterCodeBlockEndAction(
                        (context) =>
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.OwningSymbol.Locations.First(), messageArgs: context.OwningSymbol.Name + ":end"));
                            });

                    analysisContext.RegisterSyntaxNodeAction(
                        (context) =>
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.Node.GetLocation(), messageArgs: context.Node.ToFullString()));
                            },
                        CSharp.SyntaxKind.InvocationExpression);
                }
            }

            protected class BasicCodeBodyAnalyzer
            {
                public void Initialize(CodeBlockStartAnalysisContext<VisualBasic.SyntaxKind> analysisContext)
                {
                    analysisContext.RegisterCodeBlockEndAction(
                        (context) =>
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.OwningSymbol.Locations.First(), messageArgs: context.OwningSymbol.Name + ":end"));
                            });

                    analysisContext.RegisterSyntaxNodeAction(
                        (context) =>
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, context.Node.GetLocation(), messageArgs: context.Node.ToFullString()));
                            },
                        VisualBasic.SyntaxKind.InvocationExpression);
                }
            }
        }

        // Produces a warning for each comment trivium in a syntax tree
        protected class WarningOnCommentAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "Comment";
            private static DiagnosticDescriptor rule = GetRule(Id);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterSyntaxTreeAction(
                    (context) =>
                        {
                             var comments = context.Tree.GetRoot().DescendantTrivia()
                                .Where(t =>
                                    t.CSharpKind() == CSharp.SyntaxKind.SingleLineCommentTrivia ||
                                    t.CSharpKind() == CSharp.SyntaxKind.MultiLineCommentTrivia ||
                                    t.VBKind() == VisualBasic.SyntaxKind.CommentTrivia);

                            foreach (var comment in comments)
                            {
                                context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, comment.GetLocation(), messageArgs: comment.ToFullString()));
                            }
                        });
            }
        }

        // Produces a warning for each token overlapping the given span in a syntax tree
        protected class WarningOnTokenAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "Token";
            private static DiagnosticDescriptor rule = GetRule(Id);
            private IList<TextSpan> spans;

            public WarningOnTokenAnalyzer(IList<TextSpan> spans)
            {
                this.spans = spans;
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterSyntaxTreeAction(
                    (context) =>
                        {
                            foreach (var nodeOrToken in context.Tree.GetRoot().DescendantNodesAndTokens())
                            {
                                if (nodeOrToken.IsToken && this.spans.Any(s => s.OverlapsWith(nodeOrToken.FullSpan)))
                                {
                                    context.ReportDiagnostic(CodeAnalysis.Diagnostic.Create(rule, nodeOrToken.GetLocation(), messageArgs: nodeOrToken.ToString()));
                                }
                            }
                        });
            }
        }

        // Throws an exception on every AnalyzeSymbol on named types
        protected class ThrowExceptionForEachNamedTypeAnalyzer : DiagnosticAnalyzer
        {
            public const string Id = "ThrowException";
            private static DiagnosticDescriptor rule = GetRule(Id);
            
            public ThrowExceptionForEachNamedTypeAnalyzer()
            {
            }

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    return ImmutableArray.Create(rule);
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
                analysisContext.RegisterSymbolAction(
                    (context) =>
                    {
                        throw new Exception("ThrowExceptionAnalyzer exception");
                    },
                    SymbolKind.NamedType);
            }
        }

        protected static DiagnosticDescriptor GetRule(string id)
        {
            return new DiagnosticDescriptor(
                id,
                id,
                TestDiagnosticMessageTemplate,
                TestDiagnosticCategory,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);
        }
    }
}