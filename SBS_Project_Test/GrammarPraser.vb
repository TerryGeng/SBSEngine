' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Developing Branch 2013.7

Class GrammarPraser
    Const Version As String = "0.2a"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Dim Rules As New ArrayList()

    Public Sub New()
        Dim start_time As Long = GetTickCount()
        Debug_Output("SBS Grammar Praser - Version " + Version + " - Time " + CStr(start_time))
        Debug_Output("-----------------")

        Rules.Add(New Grammar("ENTRANCE", "EXPRESSION"))

        Rules.Add(New Grammar("CONST_NUM", "'0'|||'1'")) '|||'2'|||'3'|||'4'|||'5'|||'6'|||'7'|||'8'|||'9'"))
        Rules.Add(New Grammar("CONST_ALP", _
                                     "'A'|||'B'|||'C'|||'D'|||'E'|||'F'|||'G'|||'H'|||'I'|||'J'|||" & _
                                     "'K'|||'L'|||'M'|||'N'|||'O'|||'P'|||'Q'|||'R'|||'S'|||'T'|||'U'|||'V'|||" & _
                                     "'W'|||'X'|||'Y'|||'Z'" & _
                                     "'a'|||'b'|||'c'|||'d'|||'e'|||'f'|||'g'|||'h'|||'i'|||'j'|||'k'|||'l'|||" & _
                                     "'m'|||'n'|||'o'|||'p'|||'q'|||'r'|||'s'|||'t'|||'u'|||'v'|||'w'|||'x'|||" & _
                                     "'y'|||'z'"))
        Rules.Add(New Grammar("CONST_LF", "'" + vbLf + "'"))
        'Rules.Add(New Grammar("CONSTANT", "CONST_NUM|||CONST_ALP"))
        Rules.Add(New Grammar("CONSTANT", "CONST_NUM"))
        Rules.Add(New Grammar("EXP_OP", "'+'|||'-'|||'*'|||'/'|||'>'|||'<'|||'<='|||'>='"))

        Rules.Add(New Grammar("EXPRESSION", "EXP_ELEMENT+++CONST_LF|||EXP_ELEMENT+++*EXP_OP_ELEMENT+++CONST_LF"))
        Rules.Add(New Grammar("EXP_ELEMENT", "*CONST_NUM|||VARIABLE"))
        Rules.Add(New Grammar("EXP_OP_ELEMENT", "EXP_OP+++EXP_ELEMENT"))

        Rules.Add(New Grammar("FUNC_CALL", "*CONSTANT+++'('+++*CONSTANT+++')'"))
        Rules.Add(New Grammar("VARIABLE", "'$'+++*CONSTANT"))
        Rules.Add(New Grammar("VAR_DEF", "VARIABLE+++'='+++*CONSTANT"))

        Debug_Output(CStr(Rules.Count) + " Rules Loaded")
        Debug_Output("Total Time: " + CStr(GetTickCount() - start_time))
        Debug_Output("")
    End Sub

    Public Function PraseCode(ByRef code As String)
        Dim start_time As Long = GetTickCount()
        Debug_Output("Prase start at " + CStr(start_time) + ".")

        If code.Length = 0 Then
            Return Nothing
        End If

        Dim code_reader As New CodeReader(code)

        Dim sentence_list As New ArrayList()
        Dim mSentence As CodeSequence

        Dim is_error As Boolean = False

        While code_reader.IsEOF() <> True
            mSentence = MatchGrammarRule("ENTRANCE", code_reader)

            If mSentence IsNot Nothing Then
                sentence_list.Add(mSentence)
            Else
                Debug_Error("Syntax Error: Unexpected '" + code_reader.GetDeepestChar + "' on line " + CStr(code_reader.GetDeepestLine))
                is_error = True
                Exit While
            End If
        End While

        Debug_Output("Prase end at " + CStr(start_time) + ". Total " + CStr(GetTickCount() - start_time) + "ms.")
        If is_error Then
            Return Nothing
        Else
            Return sentence_list
        End If

    End Function

    Function MatchGrammarRule(ByVal rulename As String, ByRef code As CodeReader)
        'Debug_Output("Try to match " + rulename + " on " + CStr(code.GetPosStat().Position))
        Dim rule As Grammar = GetRuleByName(rulename)

        If rule Is Nothing Then
            Return Nothing
        End If

        Dim seq As ArrayList = rule.Sequences

        For seq_offset As Integer = 0 To seq.Count - 1
            Dim match_result As ArrayList = MatchGrammarSequence(seq(seq_offset), code)

            If match_result IsNot Nothing Then
                'Debug_Output("Done on matching " + rulename)
                Return New CodeSequence(rule.Name, match_result)
            End If
        Next
        'Debug_Output("Fault on matching " + rulename)
        Return Nothing
    End Function

    Function MatchGrammarSequence(ByVal sequence As GrammarSequence, ByRef code As CodeReader) As ArrayList
        Dim words As New ArrayList()
        Dim start_position As Integer = code.GetPosStat().Position
        Dim unmatch As Boolean = False

        For offset As Integer = 0 To sequence.Element.Count - 1
            Dim ele As String = sequence.Element(offset)
            Dim element_first_char As Char = ele.Substring(0, 1)

            If element_first_char = "*" Then
                Dim word As New ArrayList()
                Dim element_name As String = ele.Substring(1, ele.Length - 1)

                While True
                    Dim origin_pos As Integer = code.GetPosStat().Position
                    Dim matched_element As CodeSequence = MatchGrammarRule(element_name, code)
                    If matched_element IsNot Nothing Then
                        word.Add(matched_element)
                        ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.

                    ElseIf word.Count = 0 Then
                        unmatch = True
                        Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
                    Else
                        code.SetPosition(origin_pos)
                        Exit While
                        ' The reason of this line: If the last char doesn't match this element, it may be the member of the next element.So we should roll back.
                        ' If this sentence doesn't match the rule at all, this char won't match the next element either.
                    End If

                    words.Add(New CodeSequence(ele, word))
                End While

            ElseIf element_first_char = "'" Then
                Dim expected_str As String = ele.Substring(1, ele.Length - 2)
                Dim mChar As Char = ""
                If expected_str.Length = 1 Then
                    mChar = code.GetNextChar
                    If mChar <> expected_str Then
                        unmatch = True
                        Exit For
                    End If
                Else
                    Dim mWord As String = ""
                    For j As Integer = 1 To expected_str.Length
                        mChar = code.GetNextChar()
                        mWord += mChar

                        If mWord = expected_str.Substring(0, j) Then
                            Continue For
                        Else
                            unmatch = True
                            Exit For
                        End If
                    Next
                End If
                If unmatch = False Then
                    words.Add(New CodeSequence("-KEYWORD-", expected_str))
                Else
                    Exit For
                End If
            Else
                Dim matched_element As CodeSequence = MatchGrammarRule(ele, code)
                If matched_element IsNot Nothing Then
                    words.Add(matched_element)
                Else
                    unmatch = True
                    Exit For
                End If
            End If

        Next

        If unmatch = False Then
            Return words
        Else
            code.SetPosition(start_position)
            Return Nothing
        End If

    End Function

    Function GetRuleByName(ByVal name As String) As Grammar
        For i As Integer = 0 To Rules.Count - 1
            If Rules(i).Name = name Then
                Return Rules(i)
            End If
        Next

        Debug_Error("Rules Error: Unknow rule '" + name + "'.")
        Return Nothing
    End Function

    Sub Debug_Output(ByVal msg As String, Optional ByVal able_not_show As Boolean = False)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Sub Debug_Error(ByVal msg As String)
        Debug_Output(msg + vbCrLf)
    End Sub

End Class

Class CodeReader
    Dim Code As String = ""
    Dim PosStat As New CodeReaderPosStatus
    Dim DeepestPos As New CodeReaderPosStatus

    Sub New(ByRef code As String)
        LoadCode(code)
    End Sub

    Sub LoadCode(ByRef mCode As String)
        Code = mCode
        PosStat.Position = 0
    End Sub

    Function GetChar(ByVal offset As Integer) As Char
        If offset >= Code.Length Then
            Return ""
        End If

        Return Code.Substring(offset, 1)
    End Function

    Function GetNextChar() As Char
        Dim mChar As Char = GetChar(PosStat.Position)
        PosStat.Position += 1

        If mChar = vbCr Then
            Return GetNextChar()
        ElseIf mChar = vbLf Then
            PosStat.Lines += 1
        End If

        If PosStat.Position > DeepestPos.Position Then
            DeepestPos.Position = PosStat.Position
            DeepestPos.Lines = PosStat.Lines
        End If

        Return mChar
    End Function

    Function GetPosStat() As CodeReaderPosStatus
        Return PosStat
    End Function

    Function SetPosStat(ByVal pos As CodeReaderPosStatus) As Boolean
        If pos.Position < Code.Length Then
            PosStat.Position = pos.Position
            PosStat.Lines = pos.Lines
            Return True
        Else
            Return False
        End If
    End Function

    Sub SetPosition(ByVal position As Integer)
        PosStat.Position = position
    End Sub

    Function IsEOF()
        If PosStat.Position >= Code.Length Then
            Return True
        Else
            Return False
        End If
    End Function

    Function GetLength()
        Return Code.Length
    End Function

    Function GetDeepestChar()
        Return GetChar(DeepestPos.Position)
    End Function

    Function GetDeepestLine()
        Return DeepestPos.Lines
    End Function
End Class

Class CodeReaderPosStatus
    Public Position As Integer = 0
    Public Lines As Integer = 1
End Class