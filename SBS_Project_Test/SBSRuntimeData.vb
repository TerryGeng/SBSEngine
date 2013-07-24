Public Class SBSRuntimeData
    Public Statments As ArrayList
    Public Functions As SBSFunctionList
    Public Variables As SBSVariableList
    Dim MainPerformer As SBSPerform

    Dim StackStatus As ArrayList

    Sub New(ByRef _statments As ArrayList, ByRef _mainPerformer As SBSPerform)
        Statments = _statments
        MainPerformer = _mainPerformer
        Functions = New SBSFunctionList()
        Variables = New SBSVariableList()
        StackStatus = New ArrayList()
    End Sub

    Public Function CallFunction(ByVal funcName As String, ByRef args As ArrayList) As SBSValue
        Dim userFunc As UsersFunction = Functions.GetUsersFunction(funcName)
        Dim return_val As JumpStatus

        If userFunc IsNot Nothing Then
            Dim argsName As ArrayList = userFunc.ArgumentList
            If argsName.Count <> args.Count Then
                Throw New ApplicationException("Runtime Error: Arguments' amount for '" + funcName + "' doesn't match.")
            End If

            RecordCurrentStackStatus()

            For i As Integer = 0 To argsName.Count - 1
                Variables.AddVariable(argsName(i), args(i))
            Next

            return_val = MainPerformer.Run(userFunc.Statments.SeqsList)
            StackStatusBack()
        Else
            Dim libFunc As LibFunction
            libFunc = Functions.GetLibFunction(funcName)
            If libFunc IsNot Nothing Then
                return_val = libFunc.Func(args)
            Else
                Throw New ApplicationException("Runtime Error: Undefined function '" + funcName + "'.")
                Return Nothing
            End If
        End If

        If return_val IsNot Nothing Then
            If return_val.JumpType = "RETURN" Then
                Return return_val.ExtraValue
            Else
                Throw New ApplicationException("Runtime Error: Unexpected jump statment '" + return_val.JumpType + "' in function '" + funcName + "'.")
            End If
        End If

        Return Nothing

    End Function

    Public Sub RecordCurrentStackStatus()
        Dim funcOffset As Integer = Functions.LastUsrFuncOffset
        Dim varOffset() As Integer = Variables.LastVarOffset
        Dim stat(3) As Integer
        stat(0) = funcOffset
        stat(1) = varOffset(0)
        stat(2) = varOffset(1)

        StackStatus.Add(stat)
    End Sub

    Public Sub StackStatusBack()
        Dim stat() As Integer = StackStatus(StackStatus.Count - 1)
        StackStatus.RemoveAt(StackStatus.Count - 1)

        Dim funcOffset As Integer
        Dim varOffset(2) As Integer
        funcOffset = stat(0)
        varOffset(0) = stat(1)
        varOffset(1) = stat(2)

        Variables.LastVarOffset = varOffset
        Functions.LastUsrFuncOffset = funcOffset
    End Sub

End Class

Public Class SBSFunctionList
    Dim UsersFunctions As ArrayList
    Dim LibraryFunctions As ArrayList

    Public LastUsrFuncOffset As Integer = -1

    Delegate Function LibFunc(ByRef Arguments As ArrayList) As SBSValue

    Sub New()
        LibraryFunctions = New ArrayList()
        UsersFunctions = New ArrayList()
        SBSFunctionLib.LoadFunctions(LibraryFunctions)
    End Sub

    Sub AddUsersFunction(ByVal func As UsersFunction)
        func.Name = func.Name.ToLower()

        LastUsrFuncOffset += 1

        If LastUsrFuncOffset = UsersFunctions.Count Then
            UsersFunctions.Add(func)
        Else
            UsersFunctions(LastUsrFuncOffset) = func
        End If
    End Sub

    Public Function GetUsersFunction(ByVal funcName As String) As UsersFunction
        funcName = funcName.ToLower()

        For i As Integer = LastUsrFuncOffset To 0 Step -1
            Dim Func As UsersFunction = UsersFunctions(i)
            If Func.Name = funcName Then
                Return Func
            End If
        Next

        Return Nothing
    End Function

    Public Function GetLibFunction(ByVal funcName As String) As LibFunction
        funcName = funcName.ToLower()

        For i As Integer = 0 To LibraryFunctions.Count - 1
            Dim func As LibFunction = LibraryFunctions(i)
            If func.Name = funcName Then
                Return func
            End If
        Next

        Return Nothing
    End Function

End Class

Public Class SBSVariableList
    Dim varPtrs As ArrayList
    Dim varTable As ArrayList

    Public LastVarOffset() As Integer = {-1, -1}

    Sub New()
        varPtrs = New ArrayList()
        varTable = New ArrayList()
    End Sub

    Public Sub AddVariable(ByVal name As String, ByRef value As SBSValue)
        lastVarOffset(0) += 1
        LastVarOffset(1) += 1
        name = name.ToLower()

        If lastVarOffset(1) = varTable.Count Then
            varTable.Add(value)
        Else
            varTable(lastVarOffset(1)) = value
        End If

        If lastVarOffset(0) = varPtrs.Count Then
            varPtrs.Add(New VariablePtr(name, lastVarOffset(1)))
        Else
            varPtrs(lastVarOffset(0)) = New VariablePtr(name, lastVarOffset(1))
        End If
    End Sub

    Public Function GetVariable(ByVal name As String) As SBSValue
        name = name.ToLower()

        For i As Integer = LastVarOffset(0) To 0 Step -1
            Dim var As VariablePtr = varPtrs(i)
            If var.Name = name Then
                Return varTable(var.Offset)
            End If
        Next

        Return Nothing
    End Function

End Class

Public Class VariablePtr
    Public Name As String
    Public Offset As Integer

    Sub New(ByVal _name As String, ByVal _offset As Integer)
        Name = _name
        Offset = _offset
    End Sub
End Class

Public Class UsersFunction
    Public Name As String
    Public ArgumentList As ArrayList
    Public Statments As CodeSequence

    Sub New(ByVal _name As String, ByVal _argumentList As ArrayList, ByVal _statments As CodeSequence)
        Name = _name
        ArgumentList = _argumentList
        Statments = _statments
    End Sub
End Class

Public Class LibFunction
    Delegate Function LibraryFunction(ByRef args As ArrayList) As JumpStatus

    Public Name As String
    Public Func As LibraryFunction

    Sub New(ByVal _name As String, ByVal _func As LibraryFunction)
        Name = _name
        Func = _func
    End Sub

End Class

'Public Class Range
'    Public RangeStart As Integer
'    Public RangeEnd As Integer

'    Sub New(ByVal _rangeStart As Integer, ByVal _rangeEnd As Integer)
'        RangeStart = _rangeStart
'        RangeEnd = _rangeEnd
'    End Sub
'End Class