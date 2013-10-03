Public Module SBSFunctionLib
    Public Sub LoadFunctions(ByVal funcList As IList(Of LibFunction))
        funcList.Add(New LibFunction("print", AddressOf Func_Print, 1))
        funcList.Add(New LibFunction("readline", AddressOf Func_ReadLine, 0))
        funcList.Add(New LibFunction("strlen", AddressOf Func_StrLen, 1))
        funcList.Add(New LibFunction("asc", AddressOf Func_Asc, 1))
        funcList.Add(New LibFunction("substr", AddressOf Func_SubStr))
        funcList.Add(New LibFunction("fix", AddressOf Func_Fix, 1))
    End Sub

    Public Function Func_ReadLine(args As IList(Of SBSValue)) As SBSValue
        StandardIO.Print(vbCrLf + "> ")
        Dim value As String = StandardIO.GetLine()
        StandardIO.Print(value + vbCrLf)
        Return New SBSValue(vbString, value)
    End Function

    Public Function Func_Print(args As IList(Of SBSValue)) As SBSValue
        StandardIO.Print(CStr(args(0)))
        Return Nothing
    End Function

    Public Function Func_StrLen(args As IList(Of SBSValue)) As SBSValue
        Dim value As SBSValue = args(0)
        If value.Type = vbString Then
            Return New SBSValue(vbDouble, CType(value, String).Length)
        End If
        Error_InvalidArgs("StrLen")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Function Func_Asc(args As IList(Of SBSValue)) As SBSValue
        Dim value As SBSValue = args(0)
        If value.Type = vbString AndAlso CType(value, String).Length = 1 Then
            Return New SBSValue(vbDouble, AscW(CType(value, String)))
        End If
        Error_InvalidArgs("Asc")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Function Func_SubStr(args As IList(Of SBSValue)) As SBSValue
        If args.Count = 2 Then
            Dim value As SBSValue = args(0)
            Dim start As SBSValue = args(1)
            If start.Type = vbDouble AndAlso start.nValue >= 0 AndAlso value.Type = vbString Then
                Return New SBSValue(vbString, CType(value, String).Substring(CInt(start.nValue)))
            End If
        ElseIf args.Count = 3 Then
            Dim value As SBSValue = args(0)
            Dim start As SBSValue = args(1)
            Dim length As SBSValue = args(2)
            If start.Type = vbDouble AndAlso start.nValue >= 0 AndAlso length.Type = vbDouble AndAlso length.nValue >= 0 AndAlso value.Type = vbString Then
                Return New SBSValue(vbString, CType(value, String).Substring(CInt(start.nValue), CInt(length.nValue)))
            End If
        End If
        Error_InvalidArgs("SubStr")
        Return New SBSValue(vbDouble, 0)
    End Function

    Public Function Func_Fix(args As IList(Of SBSValue)) As SBSValue
        If args(0).Type = vbDouble Then
            Return New SBSValue(vbDouble, Fix(args(0).nValue))
        End If
        Error_InvalidArgs("Fix")
        Return New SBSValue(vbDouble, 0)
    End Function

    Sub Error_InvalidArgs(ByVal funcName As String)
        Throw New ApplicationException("Runtime Error: Invalid arguments for '" + funcName + "'.")
    End Sub
End Module
