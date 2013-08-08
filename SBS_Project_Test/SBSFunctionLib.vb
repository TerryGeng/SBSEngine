Public Class SBSFunctionLib
    Public Shared Sub LoadFunctions(ByRef funcList As ArrayList)
        funcList.Add(New LibFunction("print", AddressOf Func_Print, 1))
        funcList.Add(New LibFunction("readline", AddressOf Func_ReadLine, 0))
        funcList.Add(New LibFunction("strlen", AddressOf Func_StrLen, 1))
        funcList.Add(New LibFunction("asc", AddressOf Func_Asc, 1))
        funcList.Add(New LibFunction("substr", AddressOf Func_SubStr))
        funcList.Add(New LibFunction("fix", AddressOf Func_Fix, 1))
    End Sub

    Public Shared Function Func_ReadLine(ByRef argsList As ArrayList) As SBSValue
        StandardIO.Print(vbCrLf + "> ")
        Dim value As String = StandardIO.GetLine()
        StandardIO.Print(value + vbCrLf)
        Return New SBSValue("STRING", value)
    End Function

    Public Shared Function Func_Print(ByRef argsList As ArrayList) As SBSValue
        StandardIO.Print(CStr(argsList(0).Value))
        Return Nothing
    End Function

    Public Shared Function Func_StrLen(ByRef argsList As ArrayList) As SBSValue
        Dim value As SBSValue = argsList(0)
        If value.Type = "STRING" Then
            Return New SBSValue("NUMBER", value.sValue.Length)
        End If
        Error_InvalidArgs("StrLen")
        Return New SBSValue("NUMBER", 0)
    End Function

    Public Shared Function Func_Asc(ByRef argsList As ArrayList) As SBSValue
        Dim value As SBSValue = argsList(0)
        If value.Type = "STRING" And value.sValue.Length = 1 Then
            Return New SBSValue("NUMBER", AscW(value.sValue))
        End If
        Error_InvalidArgs("Asc")
        Return New SBSValue("NUMBER", 0)
    End Function

    Public Shared Function Func_SubStr(ByRef argsList As ArrayList) As SBSValue
        If argsList.Count = 2 Then
            Dim value As SBSValue = argsList(0)
            Dim start As SBSValue = argsList(1)
            If start.Type = "NUMBER" And start.nValue >= 0 And value.Type = "STRING" Then
                Return New SBSValue("STRING", value.sValue.Substring(start.nValue))
            End If
        ElseIf argsList.Count = 3 Then
            Dim value As SBSValue = argsList(0)
            Dim start As SBSValue = argsList(1)
            Dim length As SBSValue = argsList(2)
            If start.Type = "NUMBER" And start.nValue >= 0 And length.Type = "NUMBER" And length.nValue >= 0 And value.Type = "STRING" Then
                Return New SBSValue("STRING", value.sValue.Substring(start.nValue, length.nValue))
            End If
        End If
        Error_InvalidArgs("SubStr")
        Return New SBSValue("NUMBER", 0)
    End Function

    Public Shared Function Func_Fix(ByRef argsList As ArrayList) As SBSValue
        If argsList(0).Type = "NUMBER" Then
            Return New SBSValue("NUMBER", Fix(argsList(0).nValue))
        End If
        Error_InvalidArgs("Fix")
        Return New SBSValue("NUMBER", 0)
    End Function

    Shared Function Error_InvalidArgs(ByVal funcName As String)
        Throw New ApplicationException("Runtime Error: Invalid arguments for '" + funcName + "'.")
    End Function
End Class
