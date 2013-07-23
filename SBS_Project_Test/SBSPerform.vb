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
    Dim StdIO As StandardIO

    Sub New(ByRef statList As ArrayList, ByRef _stdIO As StandardIO)
        If _stdIO IsNot Nothing Then
            StdIO = _stdIO
            StdIO.PrintLine("SBS Perform - Version " + Version + " - Time " + CStr(GetTickCount()))
            StdIO.PrintLine("-----------------")
        Else
            Throw New ApplicationException("Internal Error: Invalid argument.")
        End If

        If statList IsNot Nothing And statList(0).GetType().Equals(GetType(CodeSequence)) Then
            RuntimeData = New RuntimeData(statList)
            StdIO = _stdIO
            Performers = New StatmentPerformers()
            StdIO.PrintLine("Statments loaded.")
        Else
            Throw New ApplicationException("Internal Error: Invalid argument.")
        End If
    End Sub

    Public Function Run() As Integer
        Dim start_time As Long = GetTickCount()
        StdIO.PrintLine("Running start at " + CStr(start_time) + ".")

        For i As Integer = 0 To RuntimeData.Statments.Count - 1
            Dim statmBody As CodeSequence = RuntimeData.Statments(i).SeqsList(0)
            Dim statmType As String = statmBody.RuleName
            Dim result As SBSValue

            If statmType = "EXPRESSION" Then
                result = Performers.Expression.Perform(statmBody, RuntimeData)
            Else
                Throw New ApplicationException("Internal Error: Unknowed statment type '" + statmType + "'.")
                Return Nothing
            End If

            StdIO.PrintLine("[" + CStr(i) + "]Return Val: (Type:" + result.Type + ")" + CStr(result.Value))
        Next

        StdIO.PrintLine("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

        Return 0
    End Function

End Class


Public Class RuntimeData
    Public Statments As ArrayList

    Sub New(ByRef stat As ArrayList)
        Statments = stat
    End Sub

End Class

Public Class ExpressionPerformer
    Class Calculator
        Dim Value As SBSValue = Nothing

        Dim buffer As Double = 0
        Dim buffer_available As Boolean = False

        Public Sub Calculate(ByVal optr As String, ByVal _value As SBSValue)
            If Value Is Nothing Then
                Value = New SBSValue(_value.Type)
            End If

            If optr = "+" Then
                CalcuAdd(_value)
            ElseIf optr = "-" Then
                CalcuSub(_value)
            ElseIf optr = "*" Then
                CalcuMulti(_value)
            ElseIf optr = "/" Then
                CalcuDiv(_value)
            End If
        End Sub

        Public Sub CalcuAdd(ByVal _value As SBSValue)
            If Value.Type = "STRING" Then
                Value.sValue += _value.sValue
            ElseIf Value.Type = "NUMBER" Then
                Value.nValue += buffer
                buffer = _value.nValue
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '+' between '" + Value.Type + "' and '" + _value.Type + "'.")
            End If
        End Sub

        Public Sub CalcuSub(ByVal _value As SBSValue)
            If Value.Type = "NUMBER" Then
                Value.nValue += buffer
                buffer = -(_value.nValue)
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '+' between '" + Value.Type + "' and '" + _value.Type + "'.")
            End If
        End Sub

        Public Sub CalcuMulti(ByVal _value As SBSValue)
            If Value.Type = "NUMBER" Then
                buffer *= _value.nValue
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '*' between '" + Value.Type + "' and '" + _value.Type + "'.")
            End If
        End Sub

        Public Sub CalcuDiv(ByVal _value As SBSValue)
            If Value.Type = "NUMBER" Then
                buffer /= _value.nValue
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '*' between '" + Value.Type + "' and '" + _value.Type + "'.")
            End If
        End Sub

        Public Function GetResult() As SBSValue
            If buffer <> 0 Then
                Value.nValue += buffer
            End If
            Return Value
        End Function

    End Class

    Function Perform(ByRef statment As CodeSequence, ByRef runtimeData As RuntimeData) As SBSValue
        Dim elementList As ArrayList = statment.SeqsList
        Dim calcu As New Calculator

        Dim firstElement As CodeSequence = elementList(0)
        Dim opEleList As ArrayList


        If firstElement.RuleName = "EXP_ELEMENT" Then
            Dim expElement As CodeSequence = firstElement.SeqsList(0)
            Dim eleValue As SBSValue

            If expElement.Value = "(" Then
                Dim bracExpr As CodeSequence = firstElement.SeqsList(1)
                Dim result As SBSValue = Perform(bracExpr, runtimeData)
                eleValue = result
            Else
                eleValue = New SBSValue(expElement.RuleName, expElement.Value)
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
            Dim curValue As SBSValue

            If curEle.Value = "(" Then
                curEle = opEleList(i).SeqsList(1).SeqsList(1)
                Dim result As SBSValue = Perform(curEle, runtimeData)

                curValue = result
            Else
                curValue = New SBSValue(curEle.RuleName, curEle.Value)
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


Public Class ControlFlowPerform

End Class

Public Class DefinitionPerform

End Class