Public Class SBSFunctionLib
    Public Shared Sub LoadFunctions(ByRef funcList As ArrayList)
        funcList.Add(New LibFunction("print", AddressOf Print))
    End Sub

    Public Shared Function Print(ByRef argsList As ArrayList) As JumpStatus
        If argsList.Count = 1 Then
            StandardIO.Print(CStr(argsList(0).Value))
        Else
            Error_ArgsAmountNotMatch("Print")
        End If

        Return Nothing
    End Function

    Shared Function Error_ArgsAmountNotMatch(ByVal funcName As String)
        Throw New ApplicationException("Runtime Error: Arguments' amount for '" + funcName + "' doesn't match.")
    End Function
End Class
