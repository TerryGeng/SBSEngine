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

    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform

    Sub New(ByRef _runtimeData As SBSRuntimeData, ByRef _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Function Perform(ByRef statment As CodeSequence)As SBSValue
        Dim elementList As ArrayList = statment.SeqsList
        Dim calcu As New Calculator

        Dim firstElement As CodeSequence = elementList(0)
        Dim opEleList As ArrayList


        If firstElement.RuleName = "EXP_ELEMENT" Then
            Dim eleValue As SBSValue = GetExpElementValue(firstElement)

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
            Dim expElement As CodeSequence = opEleList(i).SeqsList(1) ' EXP_OP_ELEMENT List -> EXP_OP_ELEMENT -> EXP_ELEMENT
            Dim eleValue As SBSValue = GetExpElementValue(expElement)

            calcu.Calculate(curOp, eleValue)
        Next

        Return calcu.GetResult()

    End Function

    Function GetExpElementValue(ByRef Exp_Element As CodeSequence) As SBSValue
        Dim curValue As SBSValue
        Dim curEle As CodeSequence = Exp_Element.SeqsList(0)

        If curEle.Value IsNot Nothing And curEle.Value = "(" Then
            curEle = Exp_Element.SeqsList(1)
            Dim result As SBSValue = Perform(curEle)

            curValue = result
        ElseIf curEle.RuleName = "FUNC_CALL" Then
            curValue = CallFunction(curEle)
        Else
            curValue = New SBSValue(curEle.RuleName, curEle.Value)
        End If

        Return curValue
    End Function

    Public Function CallFunction(ByRef Func_call As CodeSequence) As SBSValue
        Dim funcName As String = Func_call.SeqsList(0).Value ' FUNC_CALL -> NAME

        If Func_call.SeqsList.Count = 2 Then ' "func()"
            Return RuntimeData.CallFunction(funcName, New ArrayList())
        ElseIf Func_call.SeqsList.Count = 4 Then ' "func(xx)" "func(xx,xx)"
            Dim argList As ArrayList = Func_call.SeqsList(2).SeqsList ' FUNC_CALL -> ARG_LIST
            Dim args As New ArrayList()

            If argList.Count = 1 Then
                args.Add(Perform(argList(0)))
                Return RuntimeData.CallFunction(funcName, args)

            Else
                Dim arg_commas As ArrayList = argList(0).SeqsList ' ARG_LIST -> *ARG_COMMA -> ARG_COMMA List
                For i As Integer = 0 To arg_commas.Count - 1
                    Dim arg_comma As CodeSequence = arg_commas(i)
                    args.Add(Perform(arg_comma.SeqsList(0))) ' ARG_COMMA -> EXPRESSION
                Next

                args.Add(Perform(argList(1)))
                Return RuntimeData.CallFunction(funcName, args)
            End If
        End If

        Return Nothing
    End Function

End Class

Public Class ControlFlowPerform

End Class

Public Class DefinitionPerform

End Class