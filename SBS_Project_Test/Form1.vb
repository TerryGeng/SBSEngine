Public Class Form1

    Dim origin_window_height As Integer
    Dim origin_textbox_height As Integer

    Dim mPraser As GrammarPraser
    Dim sentenceList As New ArrayList()

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        mPraser = New GrammarPraser()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        sentenceList = mPraser.PraseCode(CodeArea.Text)
        Debug_Output("")
    End Sub

    Sub Debug_Output(ByVal msg As String)
        DebugText.AppendText(msg + vbCrLf)
    End Sub

    Private Sub Form1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        If origin_window_height <> 0 Then
            DebugText.Height = Me.Height - origin_window_height + origin_textbox_height
        End If
    End Sub

    Private Sub Form1_Activated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        origin_textbox_height = DebugText.Height
        origin_window_height = Me.Height
    End Sub
End Class


