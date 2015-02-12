﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System
Imports System.Linq
Imports System.Xml.Linq
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Xunit

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests.FlowAnalysis

    Public Class ImplicitVariableTests : Inherits FlowTestBase

        <Fact>
        Public Sub AnalyzeImplicitVariable()
            VerifyImplicitDeclarationDataFlowAnalysis(<![CDATA[
                [|
                Console.WriteLine(x)
                |]
            ]]>,
            dataFlowsIn:={"x"},
            readInside:={"x"})
        End Sub

        <Fact>
        Public Sub AnalyzeImplicitVariableAsByRefMethodArgument()
            VerifyImplicitDeclarationDataFlowAnalysis(<![CDATA[
                [|
                System.Int32.TryParse("6", CInt(x))
                |]
            ]]>,
            dataFlowsIn:={"x"},
            readInside:={"x"})
        End Sub

        <Fact>
        Public Sub AnalyzeImplicitVariableDeclarationInLambda()
            VerifyImplicitDeclarationDataFlowAnalysis(<![CDATA[
                [|
                Dim f As Func(Of Object) = Function() x
                x = 1|]
            ]]>,
            alwaysAssigned:={"x", "f"},
            captured:={"x"},
            variablesDeclared:={"f"},
            dataFlowsIn:={"x"},
            readInside:={"x"},
            writtenInside:={"f", "x"})
        End Sub

        <Fact>
        Public Sub AnalyzeImplicitVariableDeclarationInOuterScope1()
            VerifyImplicitDeclarationDataFlowAnalysis(<![CDATA[
                [|
                If True Then
                    x = x
                End If|]
                x = 1
            ]]>,
            alwaysAssigned:={"x"},
            dataFlowsIn:={"x"},
            readInside:={"x"},
            writtenInside:={"x"},
            writtenOutside:={"x"})
        End Sub

        <Fact>
        Public Sub AnalyzeImplicitVariableDeclarationInOuterScope2()
            VerifyImplicitDeclarationDataFlowAnalysis(<![CDATA[
                If True Then
                    x = x
                End If
              [|x = 1|]
            ]]>,
            alwaysAssigned:={"x"},
            readOutside:={"x"},
            writtenInside:={"x"},
            writtenOutside:={"x"})
        End Sub

#Region "Helpers"

        Private Sub VerifyImplicitDeclarationDataFlowAnalysis(
                code As XCData,
                Optional alwaysAssigned() As String = Nothing,
                Optional captured() As String = Nothing,
                Optional dataFlowsIn() As String = Nothing,
                Optional dataFlowsOut() As String = Nothing,
                Optional readInside() As String = Nothing,
                Optional readOutside() As String = Nothing,
                Optional variablesDeclared() As String = Nothing,
                Optional writtenInside() As String = Nothing,
                Optional writtenOutside() As String = Nothing)
            VerifyDataFlowAnalysis(Microsoft.CodeAnalysis.VisualBasic.UnitTests.Emit.ImplicitVariableTests.GetSourceXElementFromTemplate(code),
                                   alwaysAssigned,
                                   captured,
                                   dataFlowsIn,
                                   dataFlowsOut,
                                   readInside,
                                   readOutside,
                                   variablesDeclared,
                                   writtenInside,
                                   writtenOutside)
        End Sub

#End Region

    End Class

End Namespace
