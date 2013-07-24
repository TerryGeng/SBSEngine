Public Class SBSPerform

    Const Version As String = "0.1"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Public Class StatmentPerformers
        Public Expression As ExpressionPerformer
        Public ControlFlow As ControlFlowPerform
        Public Definition As DefinitionPerform

        Sub New(ByRef RuntimeData As SBSRuntimeData, ByRef Performer As SBSPerform)
            Expression = New ExpressionPerformer(RuntimeData, Performer)
            ControlFlow = New ControlFlowPerform()
            Definition = New DefinitionPerform(RuntimeData, Performer)
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
        Dim result As JumpStatus = Run(RuntimeData.Statments)
        StandardIO.PrintLine("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

        Return result
    End Function

    Public Function Run(ByRef statments As ArrayList) As JumpStatus
        For i As Integer = 0 To statments.Count - 1
            Dim statmBody As CodeSequence = statments(i).SeqsList(0)
            Dim statmType As String = statmBody.RuleName
            Dim result As SBSValue

            If statmType = "EXPRESSION" Then
                result = Performers.Expression.ExprPerform(statmBody)
            ElseIf statmType = "DEFINITION" Then
                result = Performers.Definition.Perform(statmBody)
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
    Sub New(ByVal _type As String, ByVal _value As Double)
        Type = _type
        nValue = _value
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

Public Class JumpStatus
    Public JumpType As String
    Public ExtraValue As SBSValue

    Sub New(ByVal _jumpType As String, ByRef _extraValue As SBSValue)
        JumpType = _jumpType
        ExtraValue = _extraValue
    End Sub
End Class