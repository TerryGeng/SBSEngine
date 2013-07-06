' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Planning branch 2013.7

Class GrammarPraser
    Dim SentenceRules As New ArrayList()
    Dim ElementRules As New ArrayList()

    Dim LastMatchPosition As Integer = 0
    Dim CurrentLineNum As Integer = 0

    Public Sub New()
        ElementRules.Add(New Grammar("CONSTANT", "'a'|||'b'|||'b'|||'d'|||'e'|||'f'|||'g'|||'h'|||'i'|||'j'|||'k'|||'m'|||'l'|||'n'"))

        SentenceRules.Add(New Grammar("FUNC_CALL", "*CONSTANT+++'('+++*CONSTANT+++')'"))
        SentenceRules.Add(New Grammar("VAR_DEF", "'$'+++*CONSTANT+++'='+++*CONSTANT"))
    End Sub

    Public Function PraseCode(ByRef code As String)

    End Function

    Public Function MatchSentenceOnce(ByRef code As String) As Sentence
        Dim last_char As Char = ""

        If LastMatchPosition = code.Length Then
            Return Nothing
        End If

        For i As Integer = 0 To SentenceRules.Count - 1
            Dim offset As Integer = 0 ' Which element we are matching
            Dim words As New ArrayList() ' A word list for a sentence to store
            Dim matched_rule_name As String = ""

            words.Add("")

            For j As Integer = 0 To code.Length
                Dim mChar As Char = code.Substring(j, 1)

                If mChar = vbCrLf Then
                    CurrentLineNum = CurrentLineNum + 1
                    Continue For
                End If

                Dim word As String
                Dim element As String = SentenceRules(i).Sequences(0).Element(offset)

                word = words(words.Count - 1) + mChar

                Dim element_first_char As Char = element.Substring(0, 1)

                If element_first_char = "*" Then
                    Dim element_name As String = element.Substring(1, element.Length - 1)
                    If CheckCharType(element_name, mChar) Then
                        words(words.Count - 1) = word
                        ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.
                        Continue For ' Read more chars.
                    ElseIf word.Length = 1 Then
                        Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
                        'The reason of why there isn't any else: If the last char doesn't match this element, it may be the member of the next element.
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
                    last_char = mChar
                End If
            Next

            If matched_rule_name <> "" Then
                Dim sentence As New Sentence
                sentence.RuleName = matched_rule_name
                Return sentence
            End If
        Next

        Debug_Error("Syntax Error: Unexpected '" + last_char + "' on line " + CStr(CurrentLineNum) + ". No rules matched.")
        Return Nothing
    End Function

    Function CheckCharType(ByVal type As String, ByVal mChar As Char) ' Todo: expand to check word type.
        Dim element As Grammar = GetElementRuleByName(type)
        If element IsNot Nothing Then
            For i As Integer = 0 To UBound(element.Sequences)
                If mChar = element.Sequences(i).Element(0) Then
                    Return True
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

    Sub Debug_Error(ByVal error_msg As String)
        Form1.DebugText.AppendText(error_msg)
    End Sub

End Class