Public Class SBSFunctionLib
    Public Shared Sub LoadFunctions(ByRef funcList As ArrayList)
        funcList.Add(New LibFunction("print", AddressOf Func_Print, 1))
        funcList.Add(New LibFunction("readline", AddressOf Func_ReadLine, 0))
        funcList.Add(New LibFunction("strlen", AddressOf Func_StrLen, 1))
        funcList.Add(New LibFunction("asc", AddressOf Func_Asc, 1))
        funcList.Add(New LibFunction("substr", AddressOf Func_SubStr))
        funcList.Add(New LibFunction("fix", AddressOf Func_Fix, 1))
    End Sub

    Public Shared Function Func_ReadLine(args As IList(Of SBSValue)) As SBSValue
        StandardIO.Print(vbCrLf + "> ")
        Dim value As String = StandardIO.GetLine()
        StandardIO.Print(value + vbCrLf)
        Return New SBSValue(vbString, value)
    End Function

    Public Shared Function Func_Print(args As IList(Of SBSValue)) As SBSValue
        StandardIO.Print(CStr(args(0).Value))
        Return Nothing
    End Function

    Public Shared Function Func_StrLen(args As IList(Of SBSValue)) As SBSValue
        Dim value As SBSValue = args(0)
        If value.Type = vbString Then
            Return New SBSValue(vbDouble, value.sValue.Length)
        End If
        Error_InvalidArgs("StrLen")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Shared Function Func_Asc(args As IList(Of SBSValue)) As SBSValue
        Dim value As SBSValue = args(0)
        If value.Type = vbString And value.sValue.Length = 1 Then
            Return New SBSValue(vbDouble, AscW(value.sValue))
        End If
        Error_InvalidArgs("Asc")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Shared Function Func_SubStr(args As IList(Of SBSValue)) As SBSValue
        If args.Count = 2 Then
            Dim value As SBSValue = args(0)
            Dim start As SBSValue = args(1)
            If start.Type = vbDouble And start.nValue >= 0 And value.Type = vbString Then
                Return New SBSValue(vbString, value.sValue.Substring(start.nValue))
            End If
        ElseIf args.Count = 3 Then
            Dim value As SBSValue = args(0)
            Dim start As SBSValue = args(1)
            Dim length As SBSValue = args(2)
            If start.Type = vbDouble And start.nValue >= 0 And length.Type = vbDouble And length.nValue >= 0 And value.Type = vbString Then
                Return New SBSValue(vbString, value.sValue.Substring(start.nValue, length.nValue))
            End If
        End If
        Error_InvalidArgs("SubStr")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Shared Function Func_Fix(args As IList(Of SBSValue)) As SBSValue
        If args(0).Type = vbDouble Then
            Return New SBSValue(vbDouble, Fix(args(0).nValue))
        End If
        Error_InvalidArgs("Fix")
        Return New SBSValue(vbDouble, 0)
    End Function

    Shared Function Error_InvalidArgs(ByVal funcName As String)
        Throw New ApplicationException("Runtime Error: Invalid arguments for '" + funcName + "'.")
    End Function
End Class
