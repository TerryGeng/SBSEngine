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

    Public Sub RecordCurrentStackStatus(Optional ByVal ShieldOriginalVars As Boolean = False)
        StackStatus.Add(New StackStatus(Variables.AvailableRange, Functions.AvailableRange))
        If ShieldOriginalVars Then
            Variables.AvailableRange.rangeStart = Variables.AvailableRange.rangeStart + Variables.AvailableRange.rangeLength
            Variables.AvailableRange.rangeLength = 0
        End If
    End Sub

    Public Sub StackStatusBack()
        Dim stat As StackStatus = StackStatus(StackStatus.Count - 1)
        Variables.AvailableRange.rangeStart = stat.varAvailableRange.rangeStart
        Variables.AvailableRange.rangeLength = stat.varAvailableRange.rangeLength
        Functions.AvailableRange.rangeStart = stat.funcAvailableRange.rangeStart
        Functions.AvailableRange.rangeLength = stat.funcAvailableRange.rangeLength

        StackStatus.RemoveAt(StackStatus.Count - 1)
    End Sub

End Class

Public Class SBSFunctionList
    Dim UsersFunctions As ArrayList
    Dim LibraryFunctions As ArrayList

    Public AvailableRange As Range

    Delegate Function LibFunc(ByRef Arguments As ArrayList) As SBSValue

    Sub New()
        LibraryFunctions = New ArrayList()
        UsersFunctions = New ArrayList()
        AvailableRange = New Range(0, 0)
        SBSFunctionLib.LoadFunctions(LibraryFunctions)
    End Sub

    Sub AddUsersFunction(ByVal func As UsersFunction)
        func.Name = func.Name.ToLower()

        Dim nextOffset As Integer = AvailableRange.rangeStart + AvailableRange.rangeLength

        If nextOffset = UsersFunctions.Count Then
            UsersFunctions.Add(func)
        Else
            UsersFunctions(nextOffset) = func
        End If

        AvailableRange.rangeLength += 1
    End Sub

    Public Function GetUsersFunction(ByVal funcName As String) As UsersFunction
        funcName = funcName.ToLower()
        Dim lastOffset = AvailableRange.rangeStart + AvailableRange.rangeLength - 1

        For i As Integer = lastOffset To AvailableRange.rangeStart Step -1
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

    Public AvailableRange As Range

    Sub New()
        varPtrs = New ArrayList()
        varTable = New ArrayList()
        AvailableRange = New Range(0, 0)
    End Sub

    Public Sub SetVariable(ByVal name As String, ByRef value As SBSValue)
        Dim varPtr As VariablePtr = GetVarPtr(name)
        If varPtr IsNot Nothing Then
            varTable(varPtr.Offset) = value
        Else
            name = name.ToLower()
            Dim length As Integer = AvailableRange.rangeStart + AvailableRange.rangeLength
            Dim nextOffset As Integer
            If length > 0 Then
                nextOffset = varPtrs(length - 1).Offset + 1
            Else
                nextOffset = 0
            End If

            If length = varPtrs.Count Then
                varPtrs.Add(New VariablePtr(name, nextOffset))
            Else
                varPtrs(length) = New VariablePtr(name, nextOffset)
            End If

            If nextOffset = varTable.Count Then
                varTable.Add(value)
            Else
                varTable(nextOffset) = value
            End If

            AvailableRange.rangeLength += 1
            End If
    End Sub

    Function GetVarPtr(ByVal name As String) As VariablePtr
        name = name.ToLower()

        Dim lastOffset As Integer = AvailableRange.rangeStart + AvailableRange.rangeLength - 1

        For i As Integer = lastOffset To AvailableRange.rangeStart Step -1
            Dim var As VariablePtr = varPtrs(i)
            If var.Name = name Then
                Return var
            End If
        Next

        Return Nothing
    End Function

    Public Function GetVariable(ByVal name As String) As SBSValue
        name = name.ToLower()
        Dim varPtr As VariablePtr = GetVarPtr(name)

        If varPtr IsNot Nothing Then
            Return varTable(varPtr.Offset)
        End If

        Return Nothing
    End Function

End Class

Public Class StackStatus
    Public varAvailableRange As Range
    Public funcAvailableRange As Range

    Sub New(ByVal varsLength As Integer, ByRef funcsLength As Integer)
        varAvailableRange = New Range(0, varsLength)
    End Sub

    Sub New(ByVal varsRange As Range, ByVal funcsRange As Range)
        varAvailableRange = New Range(varsRange.rangeStart, varsRange.rangeLength)
        funcAvailableRange = New Range(funcsRange.rangeStart, funcsRange.rangeLength)
    End Sub
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

Public Class JumpStatus
    Public JumpType As String
    Public ExtraValue As SBSValue

    Sub New(ByVal _jumpType As String, Optional ByRef _extraValue As SBSValue = Nothing)
        JumpType = _jumpType
        ExtraValue = _extraValue
    End Sub
End Class

Public Class Range
    Public rangeStart As Integer
    Public rangeLength As Integer

    Sub New(ByVal _rangeStart As Integer, ByVal _rangeLength As Integer)
        rangeStart = _rangeStart
        rangeLength = _rangeLength
    End Sub
End Class