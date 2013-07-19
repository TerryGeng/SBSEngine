' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Developing Branch 2013.7

Public Class GrammarPraser
    Const Version As String = "0.2a"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Dim Rules As New ArrayList()

    Public Sub New()
        Dim start_time As Long = GetTickCount()
        Debug_Output("SBS Grammar Praser - Version " + Version + " - Time " + CStr(start_time))
        Debug_Output("-----------------")

        GrammarRulesList.LoadRules(Rules)

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

        Dim code_reader As New TextReader(code)

        Dim sentence_list As New ArrayList()
        Dim mSentence As CodeSequence

        Dim is_error As Boolean = False

        While code_reader.IsEOF() <> True
            mSentence = MatchGrammarRule("STATMENT", code_reader)

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

    Function MatchGrammarRule(ByVal rulename As String, ByRef code As TextReader) As CodeSequence
        Debug_Output("Try to match " + rulename + " on " + CStr(code.GetPosStat().Position))
        Dim rule As Grammar = GetRuleByName(rulename)

        If rule Is Nothing Then
            Return Nothing
        End If

        If rule.MatchMethod = Grammar.MATCH_METHOD_NORMAL Then

            Dim seq As ArrayList = rule.Sequences

            For seq_offset As Integer = 0 To seq.Count - 1
                Dim match_result As ArrayList = MatchGrammarSequence(seq(seq_offset), code)

                If match_result IsNot Nothing Then
                    Debug_Output("Done on matching " + rulename)
                    Return New CodeSequence(rule.Name, match_result)
                End If
            Next
            Debug_Output("Fault on matching " + rulename)
            Return Nothing
        ElseIf rule.MatchMethod = Grammar.MATCH_METHOD_SPECIFY_FUNC Then
            Debug_Output("Try to use specify function to match " + rulename)
            Return rule.SpecFunc(code)
        End If
        Return Nothing
    End Function

    Function MatchGrammarSequence(ByVal sequence As GrammarSequence, ByRef code As TextReader) As ArrayList
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

Public Class GrammarRulesList
    Public Shared Sub LoadRules(ByRef Rules As ArrayList)

        Rules.Add(New Grammar("STATMENT", "VARIABLE"))

        Rules.Add(New Grammar("NUMBER", AddressOf PackNumber))
        Rules.Add(New Grammar("STRING", AddressOf PackString))
        Rules.Add(New Grammar("NAME", AddressOf PackName))
        Rules.Add(New Grammar("CONST_ALP", _
                                     "'A'|||'B'|||'C'|||'D'|||'E'|||'F'|||'G'|||'H'|||'I'|||'J'|||" & _
                                     "'K'|||'L'|||'M'|||'N'|||'O'|||'P'|||'Q'|||'R'|||'S'|||'T'|||'U'|||'V'|||" & _
                                     "'W'|||'X'|||'Y'|||'Z'|||" & _
                                     "'a'|||'b'|||'c'|||'d'|||'e'|||'f'|||'g'|||'h'|||'i'|||'j'|||'k'|||'l'|||" & _
                                     "'m'|||'n'|||'o'|||'p'|||'q'|||'r'|||'s'|||'t'|||'u'|||'v'|||'w'|||'x'|||" & _
                                     "'y'|||'z'"))
        Rules.Add(New Grammar("CHAR_LF", "'" + vbLf + "'"))
        Rules.Add(New Grammar("tCONSTANT", "NUMBER|||CONST_ALP"))
        Rules.Add(New Grammar("EXP_OP", "'+'|||'-'|||'*'|||'/'|||'>'|||'<'|||'<='|||'>='"))

        Rules.Add(New Grammar("EXPRESSION", "EXP_ELEMENT+++*EXP_OP_ELEMENT|||EXP_ELEMENT"))
        Rules.Add(New Grammar("EXP_ELEMENT", "NUMBER|||VARIABLE"))
        Rules.Add(New Grammar("EXP_OP_ELEMENT", "EXP_OP+++EXP_ELEMENT"))

        Rules.Add(New Grammar("FUNC_CALL", "NAME+++'('+++*tCONSTANT+++')'"))
        Rules.Add(New Grammar("VARIABLE", "'$'+++NAME"))
        Rules.Add(New Grammar("VAR_DEF", "VARIABLE+++'='+++EXPRESSION"))

    End Sub

    Public Shared Function PackString(ByRef code As TextReader) As CodeSequence
        Dim str As String = ""
        While True
            Dim mChar As Char = code.GetNextChar
            If mChar <> Chr(34) Then
                str += mChar
            Else
                Return New CodeSequence("STRING", str)
            End If
        End While

        Return Nothing
    End Function

    Public Shared Function PackNumber(ByRef code As TextReader) As CodeSequence
        Dim nums As String = ""
        While True
            Dim mChar As Char = code.GetNextChar

            If IsNumeric(mChar) Then
                nums += mChar
            ElseIf nums <> "" Then
                Return New CodeSequence("NUMBER", nums)
            Else
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Public Shared Function PackName(ByRef code As TextReader) As CodeSequence
        Dim name As String = ""
        While True
            Dim mChar As Char = code.GetNextChar

            If IsNameChar(mChar) And (name.Length <> 0 Or (IsNumeric(mChar) <> True)) Then
                name += mChar

            ElseIf name <> "" Then
                Return New CodeSequence("NAME", name)
            Else
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Shared Function IsNameChar(ByVal mChar As Char) As Boolean
        Dim ascii As Integer = AscW(mChar)

        If IsNumeric(mChar) Or _
            ascii >= 65 And ascii <= 90 Or _
            ascii >= 97 And ascii <= 122 Or _
            ascii >= 128 Then
            Return True
        Else
            Return False
        End If
    End Function

End Class

