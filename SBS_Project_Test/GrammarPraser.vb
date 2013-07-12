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

    Dim LastMatchPosition As New CodeReaderPosStatus

    Public Sub New()
        Dim start_time As Long = GetTickCount()
        Debug_Output("SBS Grammar Praser - Version " + Version + " - Time " + CStr(start_time))
        Debug_Output("-----------------")

        Rules.Add(New Grammar("ENTRANCE", "EXPRESSION"))

        Rules.Add(New Grammar("CONST_NUM", "'0'|||'1'|||'2'|||'3'|||'4'|||'5'|||'6'|||'7'|||'8'|||'9'"))
        Rules.Add(New Grammar("CONST_ALP", _
                                     "'A'|||'B'|||'C'|||'D'|||'E'|||'F'|||'G'|||'H'|||'I'|||'J'|||" & _
                                     "'K'|||'L'|||'M'|||'N'|||'O'|||'P'|||'Q'|||'R'|||'S'|||'T'|||'U'|||'V'|||" & _
                                     "'W'|||'X'|||'Y'|||'Z'" & _
                                     "'a'|||'b'|||'c'|||'d'|||'e'|||'f'|||'g'|||'h'|||'i'|||'j'|||'k'|||'l'|||" & _
                                     "'m'|||'n'|||'o'|||'p'|||'q'|||'r'|||'s'|||'t'|||'u'|||'v'|||'w'|||'x'|||" & _
                                     "'y'|||'z'"))
        Rules.Add(New Grammar("CONST_LF", vbLf))
        Rules.Add(New Grammar("CONSTANT", "CONST_NUM|||CONST_ALP"))
        Rules.Add(New Grammar("EXP_OP", "'+'|||'-'|||'*'|||'/'|||'>'|||'<'|||'<='|||'>='"))

        Rules.Add(New Grammar("EXPRESSION", "EXP_ELEMENT+++CONST_LF|||EXP_ELEMENT+++*EXP_OP_ELEMENT+++CONST_LF"))
        Rules.Add(New Grammar("EXP_ELEMENT", "*CONST_NUM|||VARIABLE"))
        Rules.Add(New Grammar("EXP_OP_ELEMENT", "ELE_OP+++EXPRESSION"))

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

        While code_reader.IsEOF() <> True
            mSentence = MatchGrammarRule("ENTRANCE", code_reader)

            If mSentence.RuleName <> "" Then
                sentence_list.Add(mSentence)
            Else
                Debug_Output("Prase end at " + CStr(start_time) + ". Total " + CStr(GetTickCount() - start_time) + "ms.")

                LastMatchPosition = New CodeReaderPosStatus

                Return sentence_list
            End If
        End While

        LastMatchPosition = New CodeReaderPosStatus

        Return Nothing

    End Function

    'Public Function MatchSentence(ByVal rule_name As String, ByRef code As CodeReader)
    '    Dim last_char_offset As Integer
    '    Dim original_line_num As Integer = CurrentLineNum
    '    Dim mSentence As New CodeSequence

    '    If LastMatchPosition = code.Length Then
    '        mSentence.RuleName = ""
    '        Return mSentence
    '    End If

    '    For i As Integer = 0 To SentenceRules.Count - 1
    '        Dim offset As Integer = 0 ' Which element we are matching
    '        Dim words As New ArrayList() ' A word list for a sentence to store
    '        Dim matched_rule_name As String = ""
    '        Dim line_offset As Integer = 0

    '        words.Add("")

    '        For j As Integer = LastMatchPosition To code.Length - 1
    '            Dim mChar As Char = GetChar(code, j)

    '            If last_char_offset < j Then
    '                last_char_offset = j
    '            End If

    '            If mChar = vbCr Then
    '                line_offset = line_offset + 1

    '                If CurrentLineNum < original_line_num + line_offset Then
    '                    CurrentLineNum = original_line_num + line_offset
    '                End If
    '            End If

    '            Dim word As String = words(words.Count - 1) + mChar
    '            Dim element As String = SentenceRules(i).Sequences(0).Element(offset) ' TODO

    '            Dim element_first_char As Char = element.Substring(0, 1)

    '            If element_first_char = "*" Then
    '                Dim element_name As String = element.Substring(1, element.Length - 1)
    '                If CheckCharType(element_name, mChar) Then
    '                    words(words.Count - 1) = word
    '                    ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.
    '                    If j <> code.Length - 1 Then
    '                        Continue For ' Read more chars.
    '                    Else
    '                        words.Add("")
    '                    End If

    '                ElseIf word.Length = 1 Then
    '                    Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
    '                Else
    '                    words.Add("")
    '                    word = ""
    '                    j = j - 1 'The reason of this line: If the last char doesn't match this element, it may be the member of the next element.
    '                    ' If this sentence doesn't match the rule at all, this char won't match the next element either.
    '                End If
    '            ElseIf element_first_char = "'" Then ' Todo: if there are more chars in a quote
    '                Dim quote_char As String = element.Substring(1, element.Length - 2)
    '                If mChar = quote_char Then
    '                    words(words.Count - 1) = quote_char
    '                    words.Add("")
    '                    word = ""
    '                Else
    '                    Exit For
    '                End If
    '            Else
    '                If CheckCharType(element, word) Then
    '                    words(words.Count - 1) = word
    '                    words.Add("")
    '                    word = ""
    '                Else
    '                    Exit For
    '                End If
    '            End If

    '            If UBound(SentenceRules(i).Sequences(0).Element) = offset Then
    '                matched_rule_name = SentenceRules(i).Name
    '                LastMatchPosition = j + 1
    '                Exit For
    '            Else
    '                offset = offset + 1
    '            End If
    '        Next

    '        If matched_rule_name <> "" Then
    '            mSentence.RuleName = matched_rule_name
    '            mSentence.WordsList = words

    '            Return mSentence
    '        End If
    '    Next

    '    Debug_Error("Syntax Error: Unexpected '" + code.Substring(last_char_offset, 1) + "' on line " + CStr(CurrentLineNum) + ". No rules matched.")
    '    mSentence.RuleName = ""
    '    Return mSentence
    'End Function

    Function MatchGrammarRule(ByVal rulename As String, ByRef code As CodeReader)
        Dim rule As Grammar = GetRuleByName(rulename)
        Dim seq As ArrayList = rule.Sequences

        For seq_offset As Integer = 0 To seq.Count - 1
            Dim match_result As ArrayList = MatchGrammarSequence(seq(seq_offset), code)

            If match_result IsNot Nothing Then
                Return New CodeSequence(rule.Name, match_result)
            End If
        Next

        Return Nothing
    End Function

    Function MatchGrammarSequence(ByVal sequence As GrammarSequence, ByRef code As CodeReader) As ArrayList
        Dim words As New ArrayList()
        Dim start_position As CodeReaderPosStatus = LastMatchPosition
        Dim unmatch As Boolean = False

        words.Add("")

        For offset As Integer = 0 To sequence.Element.Count - 1
            Dim ele As String = sequence.Element(offset)
            Dim element_first_char As Char = ele.Substring(0, 1)
            Dim word As New ArrayList()

            If element_first_char = "*" Then
                Dim element_name As String = ele.Substring(1, ele.Length - 1)

                While True
                    Dim origin_pos As CodeReaderPosStatus = code.GetPosStat()
                    Dim matched_element As CodeSequence = MatchGrammarRule(element_name, code)
                    If matched_element.RuleName <> "" Then
                        word.Add(matched_element)
                        ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.

                    ElseIf word.Count = 0 Then
                        unmatch = True
                        Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
                    Else
                        code.SetPosStat(origin_pos)
                        Exit While
                        ' The reason of this line: If the last char doesn't match this element, it may be the member of the next element.So we should roll back.
                        ' If this sentence doesn't match the rule at all, this char won't match the next element either.
                    End If
                End While

            ElseIf element_first_char = "'" Then
                Dim expected_str As String = ele.Substring(1, ele.Length - 2)
                Dim mChar As Char = ""
                If expected_str.Length = 1 Then
                    mChar = code.GetNextChar
                    If mChar = expected_str Then
                        word.Add(expected_str)
                    Else
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
                    If unmatch = False Then
                        word.Add(New CodeSequence("-KEYWORD-", expected_str))
                    Else
                        Exit For
                    End If
                End If
            Else
                Dim matched_element As CodeSequence = MatchGrammarRule(ele, code)
                If matched_element.RuleName <> "" Then
                    word.Add(matched_element)
                Else
                    unmatch = True
                    Exit For
                End If
            End If

            words.Add(New CodeSequence(ele, word))

        Next

        If unmatch = False Then
            Return words
        Else
            code.SetPosStat(start_position)
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

    Sub Debug_Output(ByVal msg As String)
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

    Sub LoadCode(ByRef code As String)
        code = code
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
            DeepestPos = PosStat
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
    Public Lines As Integer = 0
End Class