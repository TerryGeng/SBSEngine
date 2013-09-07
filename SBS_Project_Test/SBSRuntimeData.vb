Public Class SBSRuntimeData
    Public Statments As List(Of CodeSequence)
    Public Functions As SBSFunctionList
    Public Variables As SBSVariableList

    Dim StackStatus As List(Of StackStatus)

    Sub New()
        Functions = New SBSFunctionList()
        Variables = New SBSVariableList()
        StackStatus = New List(Of StackStatus)
        Statments = New List(Of CodeSequence)
    End Sub

    Sub New(ByVal _statments As List(Of CodeSequence))
        Statments = _statments
        Functions = New SBSFunctionList()
        Variables = New SBSVariableList()
        StackStatus = New List(Of StackStatus)
    End Sub

    Public Function AddStatments(ByVal _statments As List(Of CodeSequence)) As Range
        Dim start As Integer = Statments.Count
        If _statments IsNot Nothing Then
            Statments.AddRange(_statments)
            Return New Range(start, _statments.Count)
        Else
            Return New Range(start, 0)
        End If
    End Function

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
    Dim UsersFunctions As List(Of UsersFunction)
    Dim LibraryFunctions As List(Of LibFunction)

    Public AvailableRange As Range

    Sub New()
        LibraryFunctions = New List(Of LibFunction)
        UsersFunctions = New List(Of UsersFunction)
        AvailableRange = New Range(0, 0)
        SBSFunctionLib.LoadFunctions(LibraryFunctions)
    End Sub

    Sub AddLibFunction(ByVal libFunc As LibFunction)
        LibraryFunctions.Add(libFunc)
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
    Dim varPtrs As List(Of VariablePtr)
    Dim varTable As List(Of SBSValue)

    Public AvailableRange As Range

    Sub New()
        varPtrs = New List(Of VariablePtr)
        varTable = New List(Of SBSValue)
        AvailableRange = New Range(0, 0)
    End Sub

    Public Sub SetVariable(ByVal name As String, ByVal value As SBSValue)
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

    Sub New(ByVal varsLength As Integer, ByVal funcsLength As Integer)
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
    Public ArgumentList() As String
    Public Statments As CodeSequence

    Sub New(ByVal _name As String, ByVal _argumentList() As String, ByVal _statments As CodeSequence)
        Name = _name
        ArgumentList = _argumentList
        Statments = _statments
    End Sub
End Class

Public Class LibFunction
    Delegate Function LibraryFunction(args As IList(Of SBSValue)) As SBSValue

    Public Name As String
    Public ArgumentsCount? As Integer
    Public Func As LibraryFunction

    Sub New(ByVal _name As String, ByVal _func As LibraryFunction, Optional ByVal _argumentsCount? As Integer = Nothing)
        Name = _name
        Func = _func
        ArgumentsCount = _argumentsCount
    End Sub

End Class

Public Structure JumpStatus
    Public JumpType As String
    Public ExtraValue As SBSValue

    Sub New(ByVal _jumpType As String, Optional ByVal _extraValue As SBSValue = Nothing)
        JumpType = _jumpType
        ExtraValue = _extraValue
    End Sub
End Structure

Public Class Range
    Public rangeStart As Integer
    Public rangeLength As Integer

    Sub New(ByVal _rangeStart As Integer, ByVal _rangeLength As Integer)
        rangeStart = _rangeStart
        rangeLength = _rangeLength
    End Sub
End Class