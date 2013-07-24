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
                If _value.Type = "NUMBER" Then
                    Value.sValue += CStr(_value.nValue)
                ElseIf _value.Type = "STRING" Then
                    Value.sValue += _value.sValue
                End If
            ElseIf Value.Type = "NUMBER" Then
                If _value.Type = "STRING" Then
                    Value.Type = "STRING"
                    Value.sValue = CStr(GetResult().nValue)
                    Value.sValue += _value.sValue
                ElseIf _value.Type = "NUMBER" Then
                    Value.nValue += buffer
                    buffer = _value.nValue
                End If
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

    '---------------------------------

    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform

    Sub New(ByRef _runtimeData As SBSRuntimeData, ByRef _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Function ExprPerform(ByRef statment As CodeSequence) As SBSValue
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

            If eleValue IsNot Nothing Then
                calcu.Calculate(curOp, eleValue)
            Else
                Throw New ApplicationException("Runtime warning: Attemp to use 'Nothing' to calculate.")
            End If
        Next

        Return calcu.GetResult()

    End Function

    Function GetExpElementValue(ByRef Exp_Element As CodeSequence) As SBSValue
        Dim curValue As SBSValue
        Dim curEle As CodeSequence = Exp_Element.SeqsList(0)

        If curEle.Value IsNot Nothing And curEle.Value = "(" Then
            curEle = Exp_Element.SeqsList(1)
            Dim result As SBSValue = ExprPerform(curEle)

            curValue = result
        ElseIf curEle.RuleName = "FUNC_CALL" Then
            curValue = CallFunction(curEle)
        ElseIf curEle.RuleName = "VARIABLE" Then
            curValue = GetVarValue(curEle)
        Else
            curValue = New SBSValue(curEle.RuleName, curEle.Value)
        End If

        Return curValue
    End Function

    Public Function GetVarValue(ByVal variable As CodeSequence) As SBSValue
        Dim value As SBSValue = RuntimeData.Variables.GetVariable(variable.SeqsList(1).Value)
        If value Is Nothing Then
            Throw New ApplicationException("Runtime Error: Undefined variable '" + variable.SeqsList(1).Value + "'.")
        End If

        Return value
    End Function

    Function CallFunction(ByRef Func_call As CodeSequence) As SBSValue
        Dim funcName As String = Func_call.SeqsList(0).Value ' FUNC_CALL -> NAME

        If Func_call.SeqsList.Count = 2 Then ' "func()"
            Return RuntimeData.CallFunction(funcName, New ArrayList())
        ElseIf Func_call.SeqsList.Count = 4 Then ' "func(xx)" "func(xx,xx)"
            Dim argList As ArrayList = Func_call.SeqsList(2).SeqsList ' FUNC_CALL -> ARG_LIST
            Dim args As New ArrayList()

            If argList.Count = 1 Then
                args.Add(ExprPerform(argList(0)))
                Return RuntimeData.CallFunction(funcName, args)

            Else
                Dim arg_commas As ArrayList = argList(0).SeqsList ' ARG_LIST -> *ARG_COMMA -> ARG_COMMA List
                For i As Integer = 0 To arg_commas.Count - 1
                    Dim arg_comma As CodeSequence = arg_commas(i)
                    args.Add(ExprPerform(arg_comma.SeqsList(0))) ' ARG_COMMA -> EXPRESSION
                Next

                args.Add(ExprPerform(argList(1)))
                Return RuntimeData.CallFunction(funcName, args)
            End If
        End If

        Return Nothing
    End Function

End Class

Public Class DefinitionPerform
    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform
    Dim ExprPerf As ExpressionPerformer

    Sub New(ByRef _runtimeData As SBSRuntimeData, ByRef _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Public Function Perform(ByVal statment As CodeSequence) As SBSValue
        ExprPerf = MainPerformer.Performers.Expression
        If statment.SeqsList(0).RuleName = "VAR_DEF" Then
            Return VarDefine(statment.SeqsList(0))
        ElseIf statment.SeqsList(0).RuleName = "FUNC_DEF" Then
            Return FuncDefine(statment.SeqsList(0))
        End If

        Return Nothing
    End Function

    Function VarDefine(ByVal var_def As CodeSequence) As SBSValue
        Dim varName As String = var_def.SeqsList(0).SeqsList(1).Value ' VAR_DEF -> VARIABLE -> NAME
        Dim varValue As SBSValue = ExprPerf.ExprPerform(var_def.SeqsList(2))

        RuntimeData.Variables.AddVariable(varName, varValue)

        Return Nothing
    End Function

    Function FuncDefine(ByVal func_def As CodeSequence) As SBSValue
        Dim funcName As String = func_def.SeqsList(1).Value

        If func_def.SeqsList.Count = 6 Then ' "Function func() ..."
            RuntimeData.Functions.AddUsersFunction(New UsersFunction(funcName, New ArrayList(), func_def.SeqsList(4)))
        ElseIf func_def.SeqsList.Count = 8 Then ' "func(xx)" "func(xx,xx)"
            Dim argList As ArrayList = func_def.SeqsList(3).SeqsList ' FUNC_DEF -> ARG_DEF_LIST
            Dim args As New ArrayList()

            If argList.Count = 1 Then
                args.Add(argList(0).SeqsList(1).Value) ' FUNC_DEF -> ARG_DEF_LIST -> VARIABLE -> NAME
                RuntimeData.Functions.AddUsersFunction(New UsersFunction(funcName, args, func_def.SeqsList(6)))
            Else
                Dim arg_commas As ArrayList = argList(0).SeqsList ' ARG_DEF_LIST -> *ARG_EDF_COMMA -> ARG_DEF_COMMA List
                For i As Integer = 0 To arg_commas.Count - 1
                    Dim arg_comma As CodeSequence = arg_commas(i)
                    args.Add(arg_comma.SeqsList(0).SeqsList(1).Value) ' ARG_DEF_COMMA -> VARIABLE -> NAME
                Next

                args.Add(argList(1).SeqsList(1).Value)
                RuntimeData.Functions.AddUsersFunction(New UsersFunction(funcName, args, func_def.SeqsList(6)))
            End If
        End If

        Return Nothing

    End Function
End Class

Public Class ControlFlowPerform

End Class