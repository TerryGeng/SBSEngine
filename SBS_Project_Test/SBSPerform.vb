Public Class SBSPerform

    Const Version As String = "0.1"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Public Class StatmentPerformers
        Public Expression As ExpressionPerformer
        Public ControlFlow As ControlFlowPerform
        Public Definition As DefinitionPerform
        Public Jump As JumpPerform

        Sub New(ByRef RuntimeData As SBSRuntimeData, ByRef Performer As SBSPerform)
            Expression = New ExpressionPerformer(RuntimeData, Performer)
            ControlFlow = New ControlFlowPerform(RuntimeData, Performer)
            Definition = New DefinitionPerform(RuntimeData, Performer)
            Jump = New JumpPerform(RuntimeData, Performer)
        End Sub
    End Class

    Public Performers As StatmentPerformers
    Dim RuntimeData As SBSRuntimeData

    Sub New(ByRef statList As ArrayList)
        StandardIO.PrintLine("SBS Perform - Version " + Version + " - Time " + CStr(GetTickCount()))
        StandardIO.PrintLine("-----------------")

        If statList IsNot Nothing And statList(0).GetType().Equals(GetType(CodeSequence)) Then
            RuntimeData = New SBSRuntimeData(statList, Me)
            Performers = New StatmentPerformers(RuntimeData, Me)
            StandardIO.PrintLine("Statments loaded.")
        Else
            Throw New ApplicationException("Internal Error: Invalid argument.")
        End If
    End Sub

    Public Function Run() As JumpStatus
        Dim start_time As Long = GetTickCount()

        StandardIO.PrintLine("Running start at " + CStr(start_time) + ".")
        Dim result As JumpStatus = PerformStatments(RuntimeData.Statments)
        StandardIO.PrintLine("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

        Return result
    End Function

    Public Function Run(ByRef statments As ArrayList, Optional ByRef arguments() As ArrayList = Nothing, Optional ByVal VarBlackBox As Boolean = False)
        If arguments IsNot Nothing Then
            Dim argsName As ArrayList = arguments(0)
            Dim argsValue As ArrayList = arguments(1)

            RuntimeData.RecordCurrentStackStatus(VarBlackBox)

            For i As Integer = 0 To argsName.Count - 1
                RuntimeData.Variables.SetVariable(argsName(i), argsValue(i))
            Next
        Else
            RuntimeData.RecordCurrentStackStatus()
        End If

        Dim return_val As JumpStatus

        return_val = PerformStatments(statments)
        RuntimeData.StackStatusBack()

        Return return_val
    End Function

    Function PerformStatments(ByRef statments As ArrayList) As JumpStatus
        For i As Integer = 0 To statments.Count - 1
            Dim statmBody As CodeSequence = statments(i).SeqsList(0)
            Dim statmType As String = statmBody.RuleName
            Dim result As SBSValue
            Dim jumpstat As JumpStatus

            If statmType = "EXPRESSION" Then
                result = Performers.Expression.ExprPerform(statmBody)
            ElseIf statmType = "DEFINITION" Then
                result = Performers.Definition.Perform(statmBody)
            ElseIf statmType = "CONTROLFLOW" Then
                result = Performers.ControlFlow.Perform(statmBody)
            ElseIf statmType = "JUMP" Then
                jumpstat = Performers.Jump.Perform(statmBody)
                Return jumpstat
            Else
                Throw New ApplicationException("Internal Error: Unknowed statment type '" + statmType + "'.")
                Return Nothing
            End If

            'StandardIO.PrintLine("[" + CStr(i) + "]Return Val: (Type:" + result.Type + ")" + CStr(result.Value))
        Next

        Return Nothing ' TODO
    End Function

End Class

Public Class SBSValue
    Public Type As String = ""
    Public nValue As Double = 0
    Public sValue As String = ""

    Sub New(ByVal _type As String)
        Type = _type
    End Sub

    Sub New(ByVal _type As String, ByVal _value As String)
        If _type = "NUMBER" Then
            Type = _type
            nValue = CDbl(_value)
        Else
            Type = _type
            sValue = _value
        End If
    End Sub

    Public Function Value()
        If Type = "STRING" Then
            Return sValue
        ElseIf Type = "NUMBER" Then
            Return nValue
        End If

        Return Nothing
    End Function
End Class