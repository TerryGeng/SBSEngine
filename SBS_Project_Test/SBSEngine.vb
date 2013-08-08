Public Class SBSEngine
    Public Const Version As String = "0.1a"
    Dim Parser As SBSParser
    Dim Performer As SBSPerform
    Dim RuntimeData As SBSRuntimeData

    Public Sub New()
        StandardIO.PrintLine("SBS Engine v" + Version)
        Reset()
        StandardIO.PrintLine("# Components Loaded(Parser v" + SBSParser.Version + " Performer v" + SBSPerform.Version + ")")

    End Sub

    Public Function LoadCode(ByRef code As String) As Range
        Dim reader As New TextReader(code)
        Return RuntimeData.AddStatments(Parser.ParseCode(reader))
    End Function

    Public Sub Perform(Optional ByRef range As Range = Nothing)
        If range Is Nothing Then
            range = New Range(0, RuntimeData.Statments.Count)
        End If

        Dim statments As ArrayList
        statments = RuntimeData.Statments.GetRange(range.rangeStart, range.rangeLength)

        If RuntimeData.Statments.Count <> 0 Then
            Performer.Run(statments)
        End If
    End Sub

    Public Function CallFunction(ByVal funcname As String, Optional ByRef argsList As ArrayList = Nothing) As SBSValue
        Return Performer.CallFunction(funcname, argsList)
    End Function

    Public Sub AddFunction(ByRef libFunc As LibFunction)
        RuntimeData.Functions.AddLibFunction(libFunc)
    End Sub

    Public Sub Reset()
        Parser = New SBSParser()
        RuntimeData = New SBSRuntimeData()
        Performer = New SBSPerform(RuntimeData)
    End Sub

End Class
