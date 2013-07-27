Public Class Form1

    Dim origin_window_height As Integer
    Dim origin_textbox_height As Integer

    Dim mPraser As SBSPraser
    Dim sentenceList As New ArrayList()

    Dim run As Boolean = False

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StandardIO.PrintLine("SBS Test Console")
        StandardIO.PrintLine("")
        ListCommand()
        WaitForInput()
    End Sub

    Private Sub Form1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        If origin_window_height <> 0 Then
            DebugText.Height = Me.Height - origin_window_height + origin_textbox_height
        End If
    End Sub

    Private Sub Input_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Input.KeyPress
        If e.KeyChar = vbCr Then
            ProcessInput()
        End If
    End Sub

    Sub ListCommand()
        StandardIO.PrintLine("Commands:")
        StandardIO.PrintLine("Run(r) - Parse and perform the code.")
        StandardIO.PrintLine("Parse(pa) - Parse the code.")
        StandardIO.PrintLine("Perform(pe) - Perform the code.")
        StandardIO.PrintLine("Clear(c) - Clear the debug text.")
        StandardIO.PrintLine("Quit(q) - Quit.")
        StandardIO.PrintLine("Help(h) - Print this list.")
        StandardIO.PrintLine("")
    End Sub

    Sub WaitForInput()
        StandardIO.Print(">> ")
    End Sub

    Sub ProcessInput()
        If run = False Then
            StandardIO.PrintLine(Input.Text)
            Dim seq() As String = Input.Text.Split(" < ")
            Dim command As String = ""
            Dim inputValue As String = ""
            If UBound(seq) Then
                command = seq(0).ToLower()
                inputValue = seq(2)
            Else
                command = Input.Text.ToLower()
            End If

            Input.Text = ""

            If command = "parse" Or command = "pa" Then
                Parse()
            ElseIf command = "perform" Or command = "pe" Then
                If inputValue <> "" Then
                    StandardIO.Buffer = inputValue
                    StandardIO.LineEnd = True
                End If
                Perform()
            ElseIf command = "run" Or command = "r" Then
                If inputValue <> "" Then
                    StandardIO.Buffer = inputValue
                    StandardIO.LineEnd = True
                End If
                RunCode()
            ElseIf command = "quit" Or command = "q" Then
                Application.Exit()
            ElseIf command = "clear" Or command = "c" Then
                DebugText.Text = ""
            ElseIf command = "help" Or command = "h" Then
                ListCommand()
            End If
            WaitForInput()
        Else
            StandardIO.Buffer = Input.Text
            StandardIO.LineEnd = True
            Input.Text = ""
        End If
    End Sub

    Sub RunCode()
        Parse()
        Perform()
    End Sub

    Sub Parse()
        StandardIO.PrintLine("")
        mPraser = New SBSPraser()
        Try
            sentenceList = mPraser.PraseCode(CodeArea.Text)
        Catch excep As ApplicationException
            StandardIO.PrintError(excep.Message)
            Return
        End Try

        StandardIO.PrintLine("")
    End Sub

    Sub Perform()
        run = True
        If sentenceList IsNot Nothing Then
            StandardIO.PrintLine("")
            Dim performer As New SBSPerform(sentenceList)
            performer.Run()
        Else
            StandardIO.PrintLine("Error: No code loaded.")
        End If
        run = False
    End Sub
End Class


