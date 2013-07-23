Public Interface StandardIO
    Sub Print(ByVal str As String)
    Sub PrintLine(ByVal str As String)

    Sub PrintError(ByVal msg As String)
End Interface

Public Class TestStandardIO
    Implements StandardIO

    Public Sub Print(ByVal str As String) Implements StandardIO.Print
        Form1.DebugText.AppendText(str)
    End Sub

    Public Sub PrintError(ByVal msg As String) Implements StandardIO.PrintError
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Public Sub PrintLine(ByVal str As String) Implements StandardIO.PrintLine
        Form1.DebugText.AppendText(str + vbCrLf)
    End Sub
End Class
