Public Class StandardIO

    Public Shared Sub Print(ByVal str As String)
        Form1.DebugText.AppendText(str)
    End Sub

    Public Shared Sub PrintError(ByVal msg As String)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Public Shared Sub PrintLine(ByVal str As String)
        Form1.DebugText.AppendText(str + vbCrLf)
    End Sub
End Class
