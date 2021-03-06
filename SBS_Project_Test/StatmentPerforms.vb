﻿Public Class ExpressionPerformer
    Class Calculator
        Dim Value As SBSValue

        Dim buffer As Double
        Dim buffer_available As Boolean

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
            If Value.Type = vbString Then
                If _value.Type = vbDouble Then
                    Value.sValue += _value.nValue.ToString()
                ElseIf _value.Type = vbString Then
                    Value.sValue += CType(_value, String)
                End If
            ElseIf Value.Type = vbDouble Then
                If _value.Type = vbString Then
                    Value.Type = vbString
                    Value.sValue = GetResult().nValue.ToString()
                    Value.sValue += CType(_value, String)

                ElseIf _value.Type = vbDouble Then
                    Value.nValue += buffer
                    buffer = _value.nValue
                End If
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '+' between '" & Value.Type & "' and '" & _value.Type & "'.")
            End If
        End Sub

        Public Sub CalcuSub(ByVal _value As SBSValue)
            If Value.Type = vbDouble Then
                Value.nValue += buffer
                buffer = -(_value.nValue)
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '+' between '" & Value.Type & "' and '" & _value.Type & "'.")
            End If
        End Sub

        Public Sub CalcuMulti(ByVal _value As SBSValue)
            If Value.Type = vbDouble Then
                buffer *= _value.nValue
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '*' between '" & Value.Type & "' and '" & _value.Type & "'.")
            End If
        End Sub

        Public Sub CalcuDiv(ByVal _value As SBSValue)
            If Value.Type = vbDouble Then
                buffer /= _value.nValue
            Else
                Throw New ApplicationException("Runtime Error: Used undefine operation '*' between '" & Value.Type & "' and '" & _value.Type & "'.")
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

    Sub New(ByVal _runtimeData As SBSRuntimeData, ByVal _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Function ExprPerform(ByVal statment As CodeSequence) As SBSValue
        Dim elementList As IList(Of CodeSequence) = statment.SeqsList
        Dim calcu As New Calculator

        Dim firstElement As CodeSequence = elementList(0)
        Dim opEleList As IList(Of CodeSequence)


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

    Function GetExpElementValue(ByVal Exp_Element As CodeSequence) As SBSValue
        Dim curValue As SBSValue
        Dim curEle As CodeSequence = Exp_Element.SeqsList(0)

        If curEle.Value IsNot Nothing AndAlso curEle.Value = "(" Then
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

    Function CallFunction(ByVal Func_call As CodeSequence) As SBSValue
        Dim funcName As String = Func_call.SeqsList(0).Value ' FUNC_CALL -> NAME

        If Func_call.SeqsList.Count = 2 Then ' "func()"
            Return MainPerformer.CallFunction(funcName, New List(Of SBSValue)())
        ElseIf Func_call.SeqsList.Count = 4 Then ' "func(xx)" "func(xx,xx)"
            Dim argList As IList(Of CodeSequence) = Func_call.SeqsList(2).SeqsList ' FUNC_CALL -> ARG_LIST
            Dim args As New List(Of SBSValue)

            If argList.Count = 1 Then
                Dim arg As SBSValue = ExprPerform(argList(0))
                If arg IsNot Nothing Then
                    args.Add(arg)
                    Return MainPerformer.CallFunction(funcName, args)
                Else
                    Throw New ApplicationException("Runtime Error: Cannot use nothing as argument.")
                End If

            Else
                Dim arg_commas As IList(Of CodeSequence) = argList(0).SeqsList ' ARG_LIST -> *ARG_COMMA -> ARG_COMMA List
                For i As Integer = 0 To arg_commas.Count - 1
                    Dim arg_comma As CodeSequence = arg_commas(i)
                    Dim mArg As SBSValue = ExprPerform(arg_comma.SeqsList(0))
                    If mArg IsNot Nothing Then
                        args.Add(mArg) ' ARG_COMMA -> EXPRESSION
                    Else
                        Throw New ApplicationException("Runtime Error: Cannot use nothing as argument.")
                    End If

                Next
                Dim _arg As SBSValue = ExprPerform(argList(1))
                If _arg IsNot Nothing Then
                    args.Add(_arg)
                Else
                    Throw New ApplicationException("Runtime Error: Cannot use nothing as argument.")
                End If
                Return MainPerformer.CallFunction(funcName, args)
            End If
        End If

        Return Nothing
    End Function

    '====================

    Public Function JudgOrExprPerform(ByVal judg_or_expr As CodeSequence) As SBSValue
        Dim firstEle As CodeSequence = judg_or_expr.SeqsList(0)
        Dim firstValue As SBSValue = JudgAndExpPerform(firstEle)

        If CBool(firstValue.nValue) Then
            Return firstValue
        Else
            If judg_or_expr.SeqsList.Count = 1 Then
                Return New SBSValue(vbDouble, 0)
            End If
        End If

        If judg_or_expr.SeqsList.Count = 3 Then
            Dim secondEle As CodeSequence = judg_or_expr.SeqsList(2)
            Dim secondValue As SBSValue = JudgOrExprPerform(secondEle)

            If CBool(secondValue.nValue) Then
                Return New SBSValue(vbDouble, 1)
            Else
                Return New SBSValue(vbDouble, 0)
            End If
        End If

        Return Nothing
    End Function

    Public Function JudgAndExpPerform(ByVal judg_and_expr As CodeSequence) As SBSValue
        Dim firstEle As CodeSequence = judg_and_expr.SeqsList(0)
        Dim firstValue As SBSValue = Nothing

        If firstEle.RuleName = "JUDG_SINGLE" Then
            firstValue = JudgSinglePerform(firstEle)
        ElseIf firstEle.RuleName = "JUDG_AND_EXPR" Then
            firstValue = JudgAndExpPerform(firstEle)
        End If

        If judg_and_expr.SeqsList.Count = 3 Then
            Dim secondEle As CodeSequence = judg_and_expr.SeqsList(2)
            Dim secondValue As SBSValue = JudgSinglePerform(secondEle)

            If CBool(firstValue.nValue) AndAlso CBool(secondValue.nValue) Then
                Return New SBSValue(vbDouble, 1)
            Else
                Return New SBSValue(vbDouble, 0)
            End If
        ElseIf judg_and_expr.SeqsList.Count = 1 Then
            Return firstValue
        End If

        Return Nothing
    End Function

    Public Function JudgSinglePerform(ByVal judg_single As CodeSequence) As SBSValue
        Dim firstEle As CodeSequence = judg_single.SeqsList(0)
        Dim firstValue As SBSValue

        If firstEle.RuleName = "EXPRESSION" Then
            firstValue = ExprPerform(firstEle)
        ElseIf firstEle.Value = "(" Then
            Return JudgOrExprPerform(firstEle)
        Else
            Return Nothing
        End If

        If judg_single.SeqsList.Count = 1 Then
            If CBool(firstValue.nValue) Then
                Return firstValue
            Else
                Return New SBSValue(vbDouble, 0)
            End If
        End If

        Dim judgOp As String = judg_single.SeqsList(1).SeqsList(0).Value ' JUDG_SINGLE -> JUDG_OP -> -KEYWORD-
        Dim secondEle As CodeSequence = judg_single.SeqsList(2)
        Dim secondValue As SBSValue = ExprPerform(secondEle)
        If firstValue.Type = secondValue.Type Then
            If firstValue.Type = vbDouble Then
                If judgOp = "=" Then
                    If firstValue.nValue = secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                ElseIf judgOp = ">" Then
                    If firstValue.nValue > secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                ElseIf judgOp = "<" Then
                    If firstValue.nValue < secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                ElseIf judgOp = ">=" Then
                    If firstValue.nValue >= secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                ElseIf judgOp = "<=" Then
                    If firstValue.nValue <= secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                Else
                    Throw New ApplicationException("Runtime Error: Undefined operator '" + judgOp + "' while compare NUMBER.")
                End If
            ElseIf firstValue.Type = vbString Then
                If judgOp = "=" Then
                    If firstValue.nValue = secondValue.nValue Then
                        Return New SBSValue(vbDouble, 1)
                    Else
                        Return New SBSValue(vbDouble, 0)
                    End If
                Else
                    Throw New ApplicationException("Runtime Error: Undefined operator '" + judgOp + "' while compare STRING.")
                End If
            End If
        Else
            Throw New ApplicationException("Runtime Error: Cannot compare two values with different types.")
        End If

        Return Nothing
    End Function
End Class

Public Class DefinitionPerform
    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform
    Dim ExprPerf As ExpressionPerformer

    Sub New(ByVal _runtimeData As SBSRuntimeData, ByVal _mainPerformer As SBSPerform)
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

    Public Function VarDefine(ByVal var_def As CodeSequence) As SBSValue
        If ExprPerf Is Nothing Then
            ExprPerf = MainPerformer.Performers.Expression
        End If

        Dim varName As String = var_def.SeqsList(0).SeqsList(1).Value ' VAR_DEF -> VARIABLE -> NAME
        Dim varValue As SBSValue = ExprPerf.ExprPerform(var_def.SeqsList(2))

        RuntimeData.Variables.SetVariable(varName, varValue)

        Return Nothing
    End Function

    Function FuncDefine(ByVal func_def As CodeSequence) As SBSValue
        Dim funcName As String = func_def.SeqsList(1).Value

        If func_def.SeqsList.Count = 6 Then ' "Function func() ..."
            RuntimeData.Functions.DeclareUsersFunction(New UsersFunction(funcName, New String() {}, func_def.SeqsList(4)))
        ElseIf func_def.SeqsList.Count = 8 Then ' "func(xx)" "func(xx,xx)"
            Dim argList As IList(Of CodeSequence) = func_def.SeqsList(3).SeqsList ' FUNC_DEF -> ARG_DEF_LIST
            Dim args As New List(Of String)

            If argList.Count = 1 Then
                args.Add(argList(0).SeqsList(1).Value) ' FUNC_DEF -> ARG_DEF_LIST -> VARIABLE -> NAME
                RuntimeData.Functions.DeclareUsersFunction(New UsersFunction(funcName, args.ToArray(), func_def.SeqsList(6)))
            Else
                Dim arg_commas As IList(Of CodeSequence) = argList(0).SeqsList ' ARG_DEF_LIST -> *ARG_EDF_COMMA -> ARG_DEF_COMMA List
                For i As Integer = 0 To arg_commas.Count - 1
                    Dim arg_comma As CodeSequence = arg_commas(i)
                    args.Add(arg_comma.SeqsList(0).SeqsList(1).Value) ' ARG_DEF_COMMA -> VARIABLE -> NAME
                Next

                args.Add(argList(1).SeqsList(1).Value)
                RuntimeData.Functions.DeclareUsersFunction(New UsersFunction(funcName, args.ToArray(), func_def.SeqsList(6)))
            End If
        End If

        Return Nothing

    End Function
End Class

Public Class ControlFlowPerform
    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform
    Dim ExprPerf As ExpressionPerformer

    Sub New(ByVal _runtimeData As SBSRuntimeData, ByVal _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Public Function Perform(ByVal statment As CodeSequence) As JumpStatus
        ExprPerf = MainPerformer.Performers.Expression

        If statment.SeqsList(0).RuleName = "IF_ELSE" Then
            Return IfElseBlock(statment.SeqsList(0))
        ElseIf statment.SeqsList(0).RuleName = "WHILE" Then
            Return WhileBlock(statment.SeqsList(0))
        ElseIf statment.SeqsList(0).RuleName = "FOR" Then
            Return ForBlock(statment.SeqsList(0))
        End If

        Return Nothing
    End Function

    Function IfElseBlock(ByVal if_else As CodeSequence) As JumpStatus
        Dim if_else_head As CodeSequence = if_else.SeqsList(0)
        Dim else_and_end As CodeSequence = if_else.SeqsList(1)

        ' Dealing with IF_ELSE_HEAD and judge whether enter this If.

        Dim condition As CodeSequence = if_else_head.SeqsList(1)

        If CBool(ExprPerf.JudgOrExprPerform(condition).nValue) Then
            Dim statments As IList(Of CodeSequence) = if_else_head.SeqsList(4).SeqsList
            Return MainPerformer.Run(statments)
        Else
            Dim firstEle As CodeSequence = else_and_end.SeqsList(0)
            Dim firstValue As String = firstEle.Value

            If firstEle.RuleName = "*ELSE_IF" Then
                For i As Integer = 0 To firstEle.SeqsList.Count - 1
                    Dim curEle As CodeSequence = firstEle.SeqsList(i)
                    Dim cond As CodeSequence = curEle.SeqsList(1)
                    If CBool(ExprPerf.JudgOrExprPerform(cond).nValue) Then
                        Dim statments As IList(Of CodeSequence) = curEle.SeqsList(4).SeqsList
                        Return MainPerformer.Run(statments)
                    End If
                Next
            End If

            If firstValue <> "End If" Then
                Dim elseStatments As IList(Of CodeSequence) = Nothing

                If firstValue IsNot Nothing AndAlso firstValue = "Else" Then
                    elseStatments = else_and_end.SeqsList(2).SeqsList
                Else
                    elseStatments = else_and_end.SeqsList(3).SeqsList
                End If

                MainPerformer.Run(elseStatments)
                Return Nothing
            End If
        End If

        Return Nothing
    End Function

    Function WhileBlock(ByVal _while As CodeSequence) As JumpStatus
        Dim condition As CodeSequence = _while.SeqsList(1)
        Dim statments As CodeSequence = _while.SeqsList(3)
        Dim jumpstat As JumpStatus

        While CBool(ExprPerf.JudgOrExprPerform(condition).nValue)
            jumpstat = MainPerformer.Run(statments.SeqsList)
            If jumpstat IsNot Nothing Then
                If jumpstat.JumpType = "Continue While" Then
                    Continue While
                ElseIf jumpstat.JumpType = "Exit While" Then
                    Exit While
                Else
                    Return jumpstat
                End If
            End If
        End While

        Return Nothing
    End Function

    Function ForBlock(ByVal _for As CodeSequence) As JumpStatus
        Dim for_var As CodeSequence = _for.SeqsList(1)
        Dim varName As String = String.Empty
        If for_var.SeqsList(0).RuleName = "VAR_DEF" Then
            varName = for_var.SeqsList(0).SeqsList(0).SeqsList(1).Value
            MainPerformer.Performers.Definition.VarDefine(for_var.SeqsList(0))
        Else
            varName = for_var.SeqsList(0).SeqsList(1).Value
        End If

        Dim endValue As SBSValue = ExprPerf.ExprPerform(_for.SeqsList(3))
        If endValue.Type <> vbDouble Then
            Throw New ApplicationException("Runtime Error: Cannot use '" & endValue.Type & "' as counter for 'FOR'.")
        End If

        Dim for_step As SBSValue
        Dim for_body As CodeSequence

        If _for.SeqsList.Count = 9 Then
            for_step = ExprPerf.ExprPerform(_for.SeqsList(5))
            If for_step.Type <> vbDouble Then
                Throw New ApplicationException("Runtime Error: Cannot use '" & for_step.Type & "' as step length for 'FOR'.")
            End If
            for_body = _for.SeqsList(7)
        Else
            for_step = New SBSValue(vbDouble, 1)
            for_body = _for.SeqsList(5)
        End If

        While True
            Dim varValue As SBSValue = RuntimeData.Variables.GetVariable(varName)
            If varValue.Type <> vbDouble Then
                Throw New ApplicationException("Runtime Error: Cannot use '" & varValue.Type & "' as the counter for 'FOR'.")
            End If

            Dim jumpstat As JumpStatus = Nothing

            MainPerformer.Run(for_body.SeqsList)

            If jumpstat IsNot Nothing Then
                If jumpstat.JumpType = "Continue For" Then
                    'Do nothing
                ElseIf jumpstat.JumpType = "Exit For" Then
                    Exit While
                Else
                    Return jumpstat
                End If
            End If

            If varValue.nValue <> endValue.nValue Then
                RuntimeData.Variables.SetVariable(varName, New SBSValue(vbDouble, varValue.nValue + for_step.nValue))
            Else
                Return Nothing
            End If
        End While
        Return Nothing
    End Function

End Class

Public Class JumpPerform
    Dim RuntimeData As SBSRuntimeData
    Dim MainPerformer As SBSPerform
    Dim ExprPerf As ExpressionPerformer

    Sub New(ByVal _runtimeData As SBSRuntimeData, ByVal _mainPerformer As SBSPerform)
        RuntimeData = _runtimeData
        MainPerformer = _mainPerformer
    End Sub

    Public Function Perform(ByVal statment As CodeSequence) As JumpStatus
        ExprPerf = MainPerformer.Performers.Expression
        Dim Type As String = statment.SeqsList(0).Value
        Dim ExtraValue As SBSValue = Nothing
        If statment.SeqsList.Count > 1 Then
            ExtraValue = ExprPerf.ExprPerform(statment.SeqsList(1))
        End If

        Return New JumpStatus(Type, ExtraValue)
    End Function
End Class