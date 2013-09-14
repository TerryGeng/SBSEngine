Public Class TestForm

    Dim origin_window_height As Integer
    Dim origin_textbox_height As Integer

    Dim engine As SBSEngine
    Dim run As Boolean

    Dim LineEnd As Boolean
    Dim Buffer As String = String.Empty

    Dim parseTime As Long
    Dim performTime As Long

    Private Sub OnLoaded(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StandardIO.SetIOFunc(New StandardIO.OutputFunction(AddressOf DebugText.AppendText), New StandardIO.InputFunction(AddressOf GetInput))

        StandardIO.PrintLine("SBS Test Console")
        StandardIO.PrintLine(String.Empty)
        ListCommand()
        engine = New SBSEngine()
        WaitForInput()
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        If origin_window_height <> 0 Then
            DebugText.Height = Me.Height - origin_window_height + origin_textbox_height
        End If
        MyBase.OnResize(e)
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
        StandardIO.PrintLine(String.Empty)
    End Sub

    Sub WaitForInput()
        StandardIO.Print(vbCrLf + ">> ")
    End Sub

    Sub ProcessInput()
        If run = False Then
            StandardIO.PrintLine(Input.Text)
            Dim seq() As String = Input.Text.Split({" < "}, StringSplitOptions.RemoveEmptyEntries)
            Dim command As String = String.Empty
            Dim inputValue As String = String.Empty
            If seq.Length = 3 Then
                command = seq(0).ToLower()
                inputValue = seq(2)
            Else
                command = Input.Text.ToLower()
            End If

            Input.Clear()

            If command = "pa" OrElse command = "parse" Then
                parseTime = 0
                Parse()
                StandardIO.Output("Cost time: " + CStr(parseTime) + "ms.")
            ElseIf command = "pe" OrElse command = "perform" Then
                If Not String.IsNullOrEmpty(inputValue) Then
                    Buffer = inputValue
                    LineEnd = True
                End If
                performTime = 0
                Perform()
                StandardIO.Output("Cost time: " + CStr(performTime) + "ms.")
            ElseIf command = "r" OrElse command = "run" Then
                parseTime = 0
                performTime = 0
                If Not String.IsNullOrEmpty(inputValue) Then
                    Buffer = inputValue
                    LineEnd = True
                End If
                RunCode()
                StandardIO.Output("Parsing cost: " + CStr(parseTime) + "ms, Performing cost: " + CStr(performTime) + "ms.")
            ElseIf command = "q" OrElse command = "quit" Then
                Application.Exit()
            ElseIf command = "c" OrElse command = "clear" Then
                DebugText.Clear()
            ElseIf command = "h" OrElse command = "help" Then
                ListCommand()
            End If
            WaitForInput()
        Else
            Buffer = Input.Text
            LineEnd = True
            Input.Clear()
        End If
    End Sub

    Sub RunCode()
        Parse()
        Perform()
    End Sub

    Sub Parse()
        Dim watch As Stopwatch = Stopwatch.StartNew()
        engine.Reset()
        Try
            engine.LoadCode(CodeArea.Text)
        Catch excep As ApplicationException
            StandardIO.PrintError(excep.Message)
        End Try
        watch.Stop()
        parseTime = watch.ElapsedMilliseconds
    End Sub

    Sub Perform()
        run = True
        engine.DeclareFunction(New LibFunction("msgbox", AddressOf Func_msgbox))
        Dim watch = Stopwatch.StartNew()
        Try
            engine.Perform()
        Catch excep As ApplicationException
            run = False
            engine.Reset()
            StandardIO.PrintError(excep.Message)
            watch.Stop()
            performTime = watch.ElapsedMilliseconds
            Return
        End Try
        watch.Stop()
        performTime = watch.ElapsedMilliseconds
        run = False
    End Sub

    Public Function GetInput() As String
        Input.Focus()
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

    Public Function Func_msgbox(Arguments As IList(Of SBSValue)) As SBSValue
        If Arguments IsNot Nothing Then
            MsgBox(CType(Arguments(0), String), MsgBoxStyle.OkOnly, "MessageBox")
        End If

        Return Nothing
    End Function
End Class


