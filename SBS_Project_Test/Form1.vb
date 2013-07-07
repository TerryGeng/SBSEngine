Public Class Form1

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
End Class


