Public Class TestForm

    Declare Function GetTickCount Lib "kernel32" () As Long

    Dim origin_window_height As Integer
    Dim origin_textbox_height As Integer

    Dim engine As SBSEngine
    Dim run As Boolean

    Dim LineEnd As Boolean
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
            Dim seq() As String = Input.Text.Split({" < "}, StringSplitOptions.RemoveEmptyEntries)
            Dim command As String = String.Empty
            Dim inputValue As String = String.Empty
            If seq.Length = 3 Then
                command = seq(0).ToLower()
                inputValue = seq(2)
            Else
                command = Input.Text.ToLower()
            End If

            Input.Text = String.Empty

            Select Case command
                Case "pa"
                Case "parse"
                    parseTime = 0
                    Parse()
                    StandardIO.Output(String.Format("Cost time: {0}ms.", parseTime))

                Case "pe"
                Case "perform"
                    If inputValue <> String.Empty Then
                        Buffer = inputValue
                        LineEnd = True
                    End If
                    performTime = 0
                    Perform()
                    StandardIO.Output(String.Format("Cost time: {0}ms.", performTime))

                Case "r"
                Case "run"
                    parseTime = 0
                    performTime = 0

                    If inputValue <> String.Empty Then
                        Buffer = inputValue
                        LineEnd = True
                    End If
                    RunCode()
                    StandardIO.Output(String.Format("Parsing cost: {0}ms, Performing cost: {1}ms.", parseTime, performTime))

                Case "q"
                Case "quit"
                    Application.Exit()

                Case "c"
                Case "clear"
                    DebugText.Clear()

                Case "h"
                Case "help"
                    ListCommand()

            End Select
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
            MsgBox(CStr(Arguments(0)), MsgBoxStyle.OkOnly, "MessageBox")
        End If

        Return Nothing
    End Function
End Class


