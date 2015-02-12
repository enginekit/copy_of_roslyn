' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
'-----------------------------------------------------------------------------
' Contains the definition of the BlockContext
'-----------------------------------------------------------------------------

Namespace Microsoft.CodeAnalysis.VisualBasic.Syntax.InternalSyntax

    Friend NotInheritable Class CatchPartContext
        Inherits ExecutableStatementContext

        Friend Sub New(statement As StatementSyntax, prevContext As BlockContext)
            MyBase.New(SyntaxKind.CatchBlock, statement, prevContext)

            Debug.Assert(statement.Kind = SyntaxKind.CatchStatement)
        End Sub

        Friend Overrides Function ProcessSyntax(node As VisualBasicSyntaxNode) As BlockContext
            Debug.Assert(node IsNot Nothing)

            Select Case node.Kind
                Case SyntaxKind.CatchStatement, SyntaxKind.FinallyStatement
                    Dim context = PrevBlock.ProcessSyntax(CreateBlockSyntax(Nothing))
                    Debug.Assert(context Is PrevBlock)
                    Return context.ProcessSyntax(node)
            End Select

            Return MyBase.ProcessSyntax(node)
        End Function

        Friend Overrides Function TryLinkSyntax(node As VisualBasicSyntaxNode, ByRef newContext As BlockContext) As LinkResult
            newContext = Nothing
            Select Case node.Kind

                Case _
                    SyntaxKind.CatchStatement,
                    SyntaxKind.FinallyStatement

                    Return UseSyntax(node, newContext)

                Case Else
                    Return MyBase.TryLinkSyntax(node, newContext)
            End Select
        End Function

        Friend Overrides Function CreateBlockSyntax(statement As StatementSyntax) As VisualBasicSyntaxNode
            Debug.Assert(statement Is Nothing)
            Debug.Assert(BeginStatement IsNot Nothing)

            Dim result = SyntaxFactory.CatchBlock(DirectCast(BeginStatement, CatchStatementSyntax), Me.Body())

            FreeStatements()

            Return result
        End Function

        Friend Overrides Function EndBlock(statement As StatementSyntax) As BlockContext
            Dim context = PrevBlock.ProcessSyntax(CreateBlockSyntax(Nothing))
            Debug.Assert(context Is PrevBlock)
            Return context.EndBlock(statement)
        End Function

    End Class

End Namespace
