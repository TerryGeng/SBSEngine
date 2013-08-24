Public Class TestForm

    Declare Function GetTickCount Lib "kernel32" () As Long

    Dim origin_window_height As Integer
    Dim origin_textbox_height As Integer

    Dim engine As SBSEngine
    Dim run As Boolean = False

    Dim LineEnd As Boolean = False
    Dim Buffer As String = String.Empty

    Dim parseTime As Long
    Dim performTime As Long

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StandardIO.SetIOFunc(New StandardIO.OutputFunction(AddressOf DebugText.AppendText), New StandardIO.InputFunction(AddressOf GetInput))

        StandardIO.PrintLine("SBS Test Console")
        StandardIO.PrintLine(String.Empty)
        ListCommand()
        engine = New SBSEngine()
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
        StandardIO.PrintLine(String.Empty)
    End Sub

    Sub WaitForInput()
        StandardIO.Print(vbCrLf + ">> ")
    End Sub

    Sub ProcessInput()
        If run = False Then
            StandardIO.PrintLine(Input.Text)
            Dim seq() As String = Input.Text.Split(" < ")
            Dim command As String = String.Empty
            Dim inputValue As String = String.Empty
            If UBound(seq) = 2 Then
                command = seq(0).ToLower()
                inputValue = seq(2)
            Else
                command = Input.Text.ToLower()
            End If

            Input.Text = String.Empty

            If command = "parse" Or command = "pa" Then
                parseTime = 0
                Parse()
                StandardIO.Output("Cost time: " + CStr(parseTime) + "ms.")
            ElseIf command = "perform" Or command = "pe" Then
                If inputValue <> String.Empty Then
                    Buffer = inputValue
                    LineEnd = True
                End If
                performTime = 0
                Perform()
                StandardIO.Output("Cost time: " + CStr(performTime) + "ms.")
            ElseIf command = "run" Or command = "r" Then
                parseTime = 0
                performTime = 0
                If inputValue <> String.Empty Then
                    Buffer = inputValue
                    LineEnd = True
                End If
                RunCode()
                StandardIO.Output("Parsing cost: " + CStr(parseTime) + "ms, Performing cost: " + CStr(performTime) + "ms.")
            ElseIf command = "quit" Or command = "q" Then
                Application.Exit()
            ElseIf command = "clear" Or command = "c" Then
                DebugText.Text = String.Empty
            ElseIf command = "help" Or command = "h" Then
                ListCommand()
            End If
            WaitForInput()
        Else
            Buffer = Input.Text
            LineEnd = True
            Input.Text = String.Empty
        End If
    End Sub

    Sub RunCode()
        Parse()
        Perform()
    End Sub

    Sub Parse()
        Dim startTime As Long = GetTickCount()
        engine.Reset()
        Try
            engine.LoadCode(CodeArea.Text)
        Catch excep As ApplicationException
            StandardIO.PrintError(excep.Message)
            parseTime = GetTickCount() - startTime
            Return
        End Try
        parseTime = GetTickCount() - startTime
    End Sub

    Sub Perform()
        run = True
        engine.AddFunction(New LibFunction("msgbox", AddressOf Func_msgbox))
        Dim startTime As Long = GetTickCount()
        Try
            engine.Perform()
        Catch excep As ApplicationException
            run = False
            engine.Reset()
            StandardIO.PrintError(excep.Message)
            performTime = GetTickCount() - startTime
            Return
        End Try
        performTime = GetTickCount() - startTime
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
            MsgBox(CStr(Arguments(0).Value), MsgBoxStyle.OkOnly, "MessageBox")
        End If

        Return Nothing
    End Function
End Class


