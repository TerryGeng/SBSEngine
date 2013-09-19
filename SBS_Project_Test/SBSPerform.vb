Public Class SBSPerform

    Public Const Version As String = "0.1a"

    Public Class StatmentPerformers
        Public Expression As ExpressionPerformer
        Public ControlFlow As ControlFlowPerform
        Public Definition As DefinitionPerform
        Public Jump As JumpPerform

        Sub New(ByVal RuntimeData As SBSRuntimeData, ByVal Performer As SBSPerform)
            Expression = New ExpressionPerformer(RuntimeData, Performer)
            ControlFlow = New ControlFlowPerform(RuntimeData, Performer)
            Definition = New DefinitionPerform(RuntimeData, Performer)
            Jump = New JumpPerform(RuntimeData, Performer)
        End Sub
    End Class

    Public Performers As StatmentPerformers
    Dim RuntimeData As SBSRuntimeData

    Sub New(ByVal _RuntimeData As SBSRuntimeData)
        RuntimeData = _RuntimeData
        Performers = New StatmentPerformers(RuntimeData, Me)
    End Sub

    'Public Function Run() As JumpStatus
    '    Dim start_time As Long = GetTickCount()

    '    StandardIO.PrintLine("Running start at " + CStr(start_time) + ".")
    '    Dim return_val As JumpStatus = PerformStatments(RuntimeData.Statments)

    '    If return_val IsNot Nothing Then
    '        Throw New ApplicationException("Runtime Error: Unexpected jump statment '" + return_val.JumpType + "'.")
    '    End If

    '    StandardIO.PrintLine("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

    '    Return return_val
    'End Function

    Public Function Run(ByVal statments As IList(Of CodeSequence), Optional ByVal arguments As Tuple(Of String(), IList(Of SBSValue)) = Nothing, Optional ByVal AutoStackManage As Boolean = True, Optional ByVal VarBlackBox As Boolean = False) As JumpStatus
        If AutoStackManage Then RuntimeData.RecordCurrentStackStatus(VarBlackBox)

        If arguments IsNot Nothing Then
            Dim argsName() As String = arguments.Item1
            Dim argsValue As IList(Of SBSValue) = arguments.Item2

            For i As Integer = 0 To argsName.Count - 1
                RuntimeData.Variables.SetVariable(argsName(i), argsValue(i))
            Next
        End If

        Dim return_val As JumpStatus

        return_val = PerformStatments(statments)
        If AutoStackManage Then RuntimeData.StackStatusBack()

        Return return_val
    End Function

    Function PerformStatments(ByVal statments As IList(Of CodeSequence)) As JumpStatus
        Dim jumpstat As JumpStatus = Nothing

        For i As Integer = 0 To statments.Count - 1
            Dim statmBody As CodeSequence = statments(i).SeqsList(0)
            Dim statmType As String = statmBody.RuleName
            Dim result As SBSValue

            If statmType = "EXPRESSION" Then
                result = Performers.Expression.ExprPerform(statmBody)
            ElseIf statmType = "DEFINITION" Then
                result = Performers.Definition.Perform(statmBody)
            ElseIf statmType = "CONTROLFLOW" Then
                jumpstat = Performers.ControlFlow.Perform(statmBody)
            ElseIf statmType = "JUMP" Then
                jumpstat = Performers.Jump.Perform(statmBody)
            Else
                Throw New ApplicationException("Internal Error: Unknowed statment type '" + statmType + "'.")
                Return Nothing
            End If

            'StandardIO.PrintLine("[" + CStr(i) + "]Return Val: (Type:" + result.Type + ")" + CStr(result.Value))
        Next

        Return jumpstat
    End Function

    Public Function CallFunction(ByVal funcName As String, ByVal args As IList(Of SBSValue)) As SBSValue
        Dim userFunc As UsersFunction = RuntimeData.Functions.GetUsersFunction(funcName)
        Dim return_val As JumpStatus

        If userFunc IsNot Nothing Then
            Dim argsName() As String = userFunc.ArgumentList
            If argsName.Count <> args.Count Then
                Throw New ApplicationException("Runtime Error: Arguments' amount for '" + funcName + "' doesn't match.")
            End If

            Dim arguments As New Tuple(Of String(), IList(Of SBSValue))(argsName, args)

            return_val = Run(userFunc.Statments.SeqsList, arguments, True, True)
        Else
            Dim libFunc As LibFunction
            libFunc = RuntimeData.Functions.GetLibFunction(funcName)
            If libFunc IsNot Nothing Then
                If libFunc.ArgumentsCount = 0 OrElse libFunc.ArgumentsCount = args.Count Then
                    Dim value As SBSValue = libFunc.Func(args)
                    If value IsNot Nothing Then
                        return_val = New JumpStatus("Return ", value)
                    Else
                        return_val = New JumpStatus("Return")
                    End If
                Else
                    Throw New ApplicationException("Runtime Error: Arguments' amount for '" + funcName + "' doesn't match.")
                End If
            Else
                Throw New ApplicationException("Runtime Error: Undefined function '" + funcName + "'.")
                Return Nothing
            End If
        End If

        If return_val IsNot Nothing Then
            If return_val.JumpType = "Return " OrElse return_val.JumpType = "Return" Then
                Return return_val.ExtraValue
            Else
                Throw New ApplicationException("Runtime Error: Unexpected jump statment '" + return_val.JumpType + "' in function '" + funcName + "'.")
            End If
        End If

        Return Nothing

    End Function


End Class

Public Class SBSValue
    Public Type As VariantType
    Public nValue As Double
    Friend sValue As String

    Sub New(ByVal _type As String)
        Me.New(_type, String.Empty)
    End Sub

    Sub New(ByVal _type As VariantType)
        Me.New(_type, Nothing)
    End Sub

    Sub New(ByVal _type As VariantType, ByVal _value As Object)
        If _type = vbDouble Then
            Type = _type
            nValue = CDbl(_value)
        Else
            Type = _type
            sValue = CStr(_value)
        End If
    End Sub

    Sub New(ByVal _type As String, ByVal _value As Object)
        Me.New(CType(IIf(_type = "NUMBER", vbDouble, vbString), VariantType), _value)
    End Sub

    Public Shared Widening Operator CType(ByVal value As SBSValue) As String
        Select Case value.Type
            Case vbString
                Return value.sValue

            Case vbDouble
                Return value.nValue.ToString()

            Case Else
                Return Nothing
        End Select
    End Operator

    <Obsolete("No need to use this function - there are operators converting values automatically")>
    Public Function Value() As Object
        If Type = vbString Then
            Return sValue
        ElseIf Type = vbDouble Then
            Return nValue
        End If
        Return (Nothing)
    End Function
End Class