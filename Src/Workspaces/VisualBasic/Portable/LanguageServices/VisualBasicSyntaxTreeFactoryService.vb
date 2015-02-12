﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Composition
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.CodeAnalysis.Host
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic
    <ExportLanguageServiceFactory(GetType(ISyntaxTreeFactoryService), LanguageNames.VisualBasic), [Shared]>
    Partial Friend Class VisualBasicSyntaxTreeFactoryServiceFactory
        Implements ILanguageServiceFactory

        Public Function CreateLanguageService(provider As HostLanguageServices) As ILanguageService Implements ILanguageServiceFactory.CreateLanguageService
            Return New VisualBasicSyntaxTreeFactoryService(provider)
        End Function

        Partial Friend Class VisualBasicSyntaxTreeFactoryService
            Inherits AbstractSyntaxTreeFactoryService

            Public Sub New(languageServices As HostLanguageServices)
                MyBase.New(languageServices)
            End Sub

            Public Overloads Overrides Function GetDefaultParseOptions() As ParseOptions
                Return VisualBasicParseOptions.Default
            End Function

            Public Overloads Overrides Function ParseSyntaxTree(fileName As String, options As ParseOptions, text As SourceText, cancellationToken As CancellationToken) As SyntaxTree
                If options Is Nothing Then
                    options = GetDefaultParseOptions()
                End If
                Return SyntaxFactory.ParseSyntaxTree(text, options, fileName, cancellationToken)
            End Function

            Public Overloads Overrides Function CreateSyntaxTree(fileName As String, options As ParseOptions, node As SyntaxNode, encoding As Encoding) As SyntaxTree
                If options Is Nothing Then
                    options = GetDefaultParseOptions()
                End If
                Return SyntaxFactory.SyntaxTree(node, options, fileName, encoding)
            End Function

            Public Overrides Function CreateRecoverableTree(filePath As String, optionsOpt As ParseOptions, text As ValueSource(Of TextAndVersion), root As SyntaxNode, reparse As Boolean) As SyntaxTree
                Return RecoverableSyntaxTree.CreateRecoverableTree(Me, filePath, If(optionsOpt, GetDefaultParseOptions()), text, DirectCast(root, CompilationUnitSyntax), reparse)
            End Function

            Public Overrides Function DeserializeNodeFrom(stream As Stream, cancellationToken As CancellationToken) As SyntaxNode
                Return VisualBasicSyntaxNode.DeserializeFrom(stream, cancellationToken)
            End Function
        End Class
    End Class
End Namespace
