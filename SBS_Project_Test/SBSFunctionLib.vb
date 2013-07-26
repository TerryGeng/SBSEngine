Public Class SBSFunctionLib
    Public Shared Sub LoadFunctions(ByRef funcList As ArrayList)
        funcList.Add(New LibFunction("print", AddressOf Func_Print))
        funcList.Add(New LibFunction("strlen", AddressOf Func_StrLen))
        funcList.Add(New LibFunction("asc", AddressOf Func_Asc))
        funcList.Add(New LibFunction("substr", AddressOf Func_SubStr))
    End Sub

    Public Shared Function Func_Print(ByRef argsList As ArrayList) As JumpStatus
        If argsList.Count = 1 Then
            StandardIO.Print(CStr(argsList(0).Value))
        Else
            Error_InvalidArgs("Print")
        End If

        Return Nothing
    End Function

    Public Shared Function Func_StrLen(ByRef argsList As ArrayList) As JumpStatus
        If argsList.Count = 1 Then
            Dim value As SBSValue = argsList(0)
            If value.Type = "STRING" Then
                Return New JumpStatus("Return ", New SBSValue("NUMBER", value.sValue.Length))
            End If
        End If
        Error_InvalidArgs("StrLen")
        Return New JumpStatus("Return ", New SBSValue("NUMBER", 0))
    End Function

    Public Shared Function Func_Asc(ByRef argsList As ArrayList) As JumpStatus
        If argsList.Count = 1 Then
            Dim value As SBSValue = argsList(0)
            If value.Type = "STRING" And value.sValue.Length = 1 Then
                Return New JumpStatus("Return ", New SBSValue("NUMBER", AscW(value.sValue)))
            End If
        End If
        Error_InvalidArgs("Asc")
        Return New JumpStatus("Return ", New SBSValue("NUMBER", 0))
    End Function

    Public Shared Function Func_SubStr(ByRef argsList As ArrayList) As JumpStatus
        If argsList.Count = 2 Then
            Dim value As SBSValue = argsList(0)
            Dim start As SBSValue = argsList(1)
            If start.Type = "NUMBER" And start.nValue >= 0 And value.Type = "STRING" Then
                Return New JumpStatus("Return ", New SBSValue("STRING", value.sValue.Substring(start.nValue)))
            End If
        ElseIf argsList.Count = 3 Then
            Dim value As SBSValue = argsList(0)
            Dim start As SBSValue = argsList(1)
            Dim length As SBSValue = argsList(2)
            If start.Type = "NUMBER" And start.nValue >= 0 And length.Type = "NUMBER" And length.nValue >= 0 And value.Type = "STRING" Then
                Return New JumpStatus("Return ", New SBSValue("STRING", value.sValue.Substring(start.nValue, length.nValue)))
            End If
        End If
        Error_InvalidArgs("SubStr")
        Return New JumpStatus("Return ", New SBSValue("NUMBER", 0))
    End Function

    Shared Function Error_InvalidArgs(ByVal funcName As String)
        Throw New ApplicationException("Runtime Error: Invalid arguments for '" + funcName + "'.")
    End Function
End Class
