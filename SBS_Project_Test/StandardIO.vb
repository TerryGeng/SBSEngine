Public Class StandardIO

    Public Shared Buffer As String = ""
    Public Shared LineEnd As Boolean = False

    Public Shared Sub Print(ByVal str As String)
        Form1.DebugText.AppendText(str)
    End Sub

    Public Shared Sub PrintError(ByVal msg As String)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Public Shared Sub PrintLine(ByVal str As String)
        Form1.DebugText.AppendText(str + vbCrLf)
    End Sub

    Public Shared Function GetLine() As String
        Form1.Input.Focus()
        While True
            Application.DoEvents()
            If LineEnd Then
                LineEnd = False
                Return Buffer
            End If
            System.Threading.Thread.Sleep(10)
        End While
        Return Nothing
    End Function
End Class
