' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Developing Branch 2013.7

Class GrammarPraser
    Const Version As Double = 0.1

    Declare Function GetTickCount Lib "kernel32" () As Long

    Dim SentenceRules As New ArrayList()
    Dim ElementRules As New ArrayList()

    Dim LastMatchPosition As Integer = 0
    Dim CurrentLineNum As Integer = 1

    Public Sub New()
        Dim start_time As Long = GetTickCount()
        Debug_Output("SBS Grammar Praser - Version " + CStr(Version) + " - Time " + CStr(start_time))
        Debug_Output("-----------------")
        ElementRules.Add(New Grammar("CONST_NUM", "'0'|||'1'|||'2'|||'3'|||'4'|||'5'|||'6'|||'7'|||'8'|||'9'"))
        ElementRules.Add(New Grammar("CONST_ALP", _
                                     "'A'|||'B'|||'C'|||'D'|||'E'|||'F'|||'G'|||'H'|||'I'|||'J'|||" & _
                                     "'K'|||'L'|||'M'|||'N'|||'O'|||'P'|||'Q'|||'R'|||'S'|||'T'|||'U'|||'V'|||" & _
                                     "'W'|||'X'|||'Y'|||'Z'" & _
                                     "'a'|||'b'|||'c'|||'d'|||'e'|||'f'|||'g'|||'h'|||'i'|||'j'|||'k'|||'l'|||" & _
                                     "'m'|||'n'|||'o'|||'p'|||'q'|||'r'|||'s'|||'t'|||'u'|||'v'|||'w'|||'x'|||" & _
                                     "'y'|||'z'"))
        ElementRules.Add(New Grammar("CONSTANT", "CONST_NUM|||CONST_ALP"))

        SentenceRules.Add(New Grammar("FUNC_CALL", "*CONSTANT+++'('+++*CONSTANT+++')'"))
        SentenceRules.Add(New Grammar("VAR_DEF", "'$'+++*CONSTANT+++'='+++*CONSTANT"))

        Debug_Output("Rules Loaded (ElementR " + CStr(ElementRules.Count) + ", SentenceR " + CStr(SentenceRules.Count) + ")")
        Debug_Output("Total Time: " + CStr(GetTickCount() - start_time))
        Debug_Output("")
    End Sub

    Public Function PraseCode(ByRef code As String)
        Dim start_time As Long = GetTickCount()
        Debug_Output("Prase start at " + CStr(start_time) + ".")

        If code.Length = 0 Then
            Return Nothing
        End If

        Dim sentence_list As New ArrayList()
        Dim mSentence As New Sentence

        While True
            mSentence = MatchSentenceOnce(code)

            If mSentence.RuleName <> "" Then
                sentence_list.Add(mSentence)
            Else
                Debug_Output("Prase end at " + CStr(start_time) + ". Total " + CStr(GetTickCount() - start_time) + "ms.")

                LastMatchPosition = 0
                CurrentLineNum = 1

                Return sentence_list
            End If
        End While

        LastMatchPosition = 0
        CurrentLineNum = 1

        Return Nothing

    End Function

    Public Function MatchSentenceOnce(ByRef code As String) As Sentence
        Dim last_char_offset As Integer
        Dim original_line_num As Integer = CurrentLineNum
        Dim mSentence As New Sentence

        If LastMatchPosition = code.Length Then
            mSentence.RuleName = ""
            Return mSentence
        End If

        For i As Integer = 0 To SentenceRules.Count - 1
            Dim offset As Integer = 0 ' Which element we are matching
            Dim words As New ArrayList() ' A word list for a sentence to store
            Dim matched_rule_name As String = ""
            Dim line_offset As Integer = 0

            words.Add("")

            For j As Integer = LastMatchPosition To code.Length - 1
                Dim mChar As Char = GetChar(code, j)

                If last_char_offset < j Then
                    last_char_offset = j
                End If

                If mChar = vbCr Then
                    line_offset = line_offset + 1

                    If CurrentLineNum < original_line_num + line_offset Then
                        CurrentLineNum = original_line_num + line_offset
                    End If
                End If

                Dim word As String = words(words.Count - 1) + mChar
                Dim element As String = SentenceRules(i).Sequences(0).Element(offset) ' TODO

                Dim element_first_char As Char = element.Substring(0, 1)

                If element_first_char = "*" Then
                    Dim element_name As String = element.Substring(1, element.Length - 1)
                    If CheckCharType(element_name, mChar) Then
                        words(words.Count - 1) = word
                        ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.
                        If j <> code.Length - 1 Then
                            Continue For ' Read more chars.
                        Else
                            words.Add("")
                        End If

                    ElseIf word.Length = 1 Then
                        Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
                    Else
                        words.Add("")
                        word = ""
                        j = j - 1 'The reason of this line: If the last char doesn't match this element, it may be the member of the next element.
                        ' If this sentence doesn't match the rule at all, this char won't match the next element either.
                    End If
                ElseIf element_first_char = "'" Then ' Todo: if there are more chars in a quote
                    Dim quote_char As String = element.Substring(1, element.Length - 2)
                    If mChar = quote_char Then
                        words(words.Count - 1) = quote_char
                        words.Add("")
                        word = ""
                    Else
                        Exit For
                    End If
                Else
                    If CheckCharType(element, word) Then
                        words(words.Count - 1) = word
                        words.Add("")
                        word = ""
                    Else
                        Exit For
                    End If
                End If

                If UBound(SentenceRules(i).Sequences(0).Element) = offset Then
                    matched_rule_name = SentenceRules(i).Name
                    LastMatchPosition = j + 1
                    Exit For
                Else
                offset = offset + 1
                End If
            Next

            If matched_rule_name <> "" Then
                mSentence.RuleName = matched_rule_name
                mSentence.WordsList = words

                Return mSentence
            End If
        Next

        Debug_Error("Syntax Error: Unexpected '" + code.Substring(last_char_offset, 1) + "' on line " + CStr(CurrentLineNum) + ". No rules matched.")
        mSentence.RuleName = ""
        Return mSentence
    End Function

    Function CheckCharType(ByVal type As String, ByVal mChar As Char) ' Todo: expand to check word type.
        Dim element As Grammar = GetElementRuleByName(type)
        If element IsNot Nothing Then
            For i As Integer = 0 To element.Sequences.Count - 1
                Dim rule As String = element.Sequences(i).Element(0)
                If rule.Substring(0, 1) = "'" Then
                    If mChar = rule.Substring(1, 1) Then
                        Return True
                    End If
                Else
                    If CheckCharType(rule, mChar) Then
                        Return True
                    End If
                End If
                
            Next
        End If

        Return False
    End Function

    Function GetElementRuleByName(ByVal name As String) As Grammar
        For i As Integer = 0 To ElementRules.Count - 1
            If ElementRules(i).Name = name Then
                Return ElementRules(i)
            End If
        Next

        Debug_Error("Rules Error: Unknow rule '" + name + "'.")
        Return Nothing
    End Function

    Function GetChar(ByRef str As String, ByVal offset As Integer)
        Return str.Substring(offset, 1)
    End Function

    Sub Debug_Output(ByVal msg As String)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Sub Debug_Error(ByVal msg As String)
        Debug_Output(msg + vbCrLf)
    End Sub

End Class