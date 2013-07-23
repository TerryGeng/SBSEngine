Public Class SBSPerform

    Const Version As String = "0.1"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Public Class StatmentPerformers
        Public Expression As ExpressionPerformer
        Public ControlFlow As ControlFlowPerform
        Public Definition As DefinitionPerform

        Sub New()
            Expression = New ExpressionPerformer()
            ControlFlow = New ControlFlowPerform()
            Definition = New DefinitionPerform()
        End Sub
    End Class

    Dim Performers As StatmentPerformers
    Dim RuntimeData As RuntimeData


    Sub New(ByRef statList As ArrayList)
        Debug_Output("SBS Perform - Version " + Version + " - Time " + CStr(GetTickCount()))
        Debug_Output("-----------------")

        If statList IsNot Nothing And statList(0).GetType().Equals(GetType(CodeSequence)) Then
            RuntimeData = New RuntimeData(statList)
            Performers = New StatmentPerformers()
            Debug_Output("Statments loaded.")
        Else
            Debug_Error("Load Error: Invalid argument type.")
        End If
    End Sub

    Public Function Run() As Integer
        Dim start_time As Long = GetTickCount()
        Debug_Output("Running start at " + CStr(start_time) + ".")

        For i As Integer = 0 To RuntimeData.Statments.Count - 1
            Dim statmBody As CodeSequence = RuntimeData.Statments(i).SeqsList(0)
            Dim statmType As String = statmBody.RuleName
            Dim result As SBSValue

            If statmType = "EXPRESSION" Then
                result = Performers.Expression.Perform(statmBody, RuntimeData)
            Else
                Debug_Error("Internal Error: Unknowed statment type '" + statmType + "'.")
                Return Nothing
            End If

            Debug_Output("[" + CStr(i) + "]Return Val: (Type:" + result.Type + ")" + CStr(result.Value))
        Next

        Debug_Output("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

        Return 0
    End Function


    Sub Debug_Output(ByVal msg As String, Optional ByVal able_not_show As Boolean = False)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Sub Debug_Error(ByVal msg As String)
        Debug_Output(msg + vbCrLf)
    End Sub
End Class


Public Class RuntimeData
    Public Statments As ArrayList

    Sub New(ByRef stat As ArrayList)
        Statments = stat
    End Sub

End Class

Public Class ExpressionPerformer
    Class Calculator
        Dim Value As SBSValue

        Public Sub Calculate(ByVal optr As String, ByVal _value As SBSValue)
            If optr = "+" Then
                CalcuAdd(_value)
            End If
        End Sub

        Public Sub CalcuAdd(ByVal _value As SBSValue)
            If Value.Type = "STRING" Then
                Value.sValue += _value.sValue
            ElseIf Value.Type = "NUMBER" Then
                Value.nValue += _value.nValue
            Else
                Debug_Error("Runtime Error: Used undefine operator '+' between '" + Value.Type + "' and '" + _value.Type + "'.")
            End If
        End Sub

        Public Sub CalcuMulti(ByVal _value As SBSValue)
            Static buffer As Double

            If Value.Type = "NUMBER" Then

            End If

        End Sub

        Public Function GetResult() As SBSValue

        End Function
    End Class

    Function Perform(ByRef statment As CodeSequence, ByRef runtimeData As RuntimeData) As SBSValue
        Dim elementList As ArrayList = statment.SeqsList
        Dim calcu As New Calculator

        Dim firstElement As CodeSequence = elementList(0)
        Dim opEleList As ArrayList


        If firstElement.RuleName = "EXP_ELEMENT" Then
            Dim expElement As CodeSequence = firstElement.SeqsList(0)
            Dim eleValue As New SBSValue(expElement.RuleName, expElement.Value)

            If eleValue.Value = "(" Then
                Dim bracExpr As CodeSequence = firstElement.SeqsList(1)
                Dim result As SBSValue = Perform(bracExpr, runtimeData)
                eleValue = result.Value
            End If

            If elementList.Count > 1 Then
                opEleList = elementList(1).SeqsList
            Else
                Return eleValue
            End If

            calcu.Calculate("+", eleValue)
        Else
            opEleList = elementList(0).SeqsList
        End If

        For i As Integer = 0 To opEleList.Count - 1
            Dim curOp As String = opEleList(i).SeqsList(0).SeqsList(0).Value ' EXP_OP_ELEMENT List -> EXP_OP_ELEMENT -> EXP_OP -> "+","-"...
            Dim curEle As CodeSequence = opEleList(i).SeqsList(1).SeqsList(0) ' EXP_OP_ELEMENT List -> EXP_OP_ELEMENT -> EXP_ELEMENT
            Dim curValue As New SBSValue(curEle.RuleName, curEle.Value)

            If curValue.Value = "(" Then
                curEle = opEleList(i).SeqsList(1).SeqsList(1)
                Dim result As SBSValue = Perform(curEle, runtimeData)

                curValue.Type = result.Type
                curValue = result.Value
            End If

            calcu.Calculate(curOp, curValue)

            'If curOp <> "*" Or curOp <> "/" Then
            '    If curType = "NUMBER" Then
            '        If exprType = "NUMBER" Then
            '            If curOp = "+" Then
            '                exprValueNum += CDbl(curValue)
            '            Else
            '                Debug_Error("Runtime Error: Used undefine operator '" + curOp + "' between '" + exprType + "' and '" + curType + "'.")
            '            End If
            '        ElseIf exprType = "STRING" Then
            '            If curOp = "+" Then
            '                exprValueStr += curValue
            '            Else
            '                Debug_Error("Runtime Error: Used undefine operator '" + curOp + "' between '" + exprType + "' and '" + curType + "'.")
            '            End If
            '        End If
            '    End If
            'End If
        Next

        Return calcu.GetResult()

    End Function

    Sub Debug_Output(ByVal msg As String, Optional ByVal able_not_show As Boolean = False)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Sub Debug_Error(ByVal msg As String)
        Debug_Output(msg + vbCrLf)
    End Sub

End Class

Public Class SBSValue
    Public Type As String = ""
    Public nValue As Double
    Public sValue As String = ""
    Sub New(ByVal _type As String, ByVal _value As String)
        Type = _type
        sValue = _value
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


Public Class ControlFlowPerform

End Class

Public Class DefinitionPerform

End Class