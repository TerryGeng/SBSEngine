Public Class SBSEngine
    Dim Parser As SBSPraser
    Dim Performer As SBSPerform
    Dim RuntimeData As SBSRuntimeData

    Public Sub New()
        Parser = New SBSPraser()
        RuntimeData = New SBSRuntimeData()
        Performer = New SBSPerform(RuntimeData)
    End Sub

    Public Sub LoadCode(ByRef code As String)
        Dim reader As New TextReader(code)
        RuntimeData.AddStatments(Parser.PraseCode(reader))
    End Sub

    Public Sub Perform()
        If RuntimeData.Statments.Count <> 0 Then
            Performer.Run()
        End If
    End Sub

    Public Function CallFunction(ByVal funcname As String, Optional ByRef argsList As ArrayList = Nothing) As SBSValue
        Return Performer.CallFunction(funcname, argsList)
    End Function

    Public Sub AddFunction(ByRef libFunc As LibFunction)
        RuntimeData.Functions.AddLibFunction(libFunc)
    End Sub

    Public Sub ResetData()
        RuntimeData = New SBSRuntimeData()
    End Sub

End Class
