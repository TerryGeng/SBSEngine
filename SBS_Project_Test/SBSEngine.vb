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

    Public Function LoadCode(ByVal code As String) As Range
        Dim reader As New TextReader(code)
        Return RuntimeData.AddStatments(Parser.ParseCode(reader))
    End Function

    Public Sub Perform(Optional ByRef range As Range = Nothing, Optional ByVal autoStackManage As Boolean = True)
        If range Is Nothing Then
            range = New Range(0, RuntimeData.Statments.Count)
        End If

        Dim statments As List(Of CodeSequence)
        statments = RuntimeData.Statments.GetRange(range.rangeStart, range.rangeLength)

        If RuntimeData.Statments.Count <> 0 Then
            Performer.Run(statments, Nothing, autoStackManage)
        End If
    End Sub

    Public Function CallFunction(ByVal funcname As String, Optional ByVal argsList As IList(Of SBSValue) = Nothing) As SBSValue
        Return Performer.CallFunction(funcname, argsList)
    End Function

    Public Sub DeclareFunction(ByVal libFunc As LibFunction)
        RuntimeData.Functions.DeclareLibFunction(libFunc)
    End Sub

    Public Sub Reset()
        Parser = New SBSParser()
        RuntimeData = New SBSRuntimeData()
        Performer = New SBSPerform(RuntimeData)
    End Sub

    Public Sub RecordStackTop()
        RuntimeData.RecordCurrentStackStatus()
    End Sub

    Public Sub StackTopResetToPrevious()
        RuntimeData.StackStatusBack()
    End Sub

End Class
