' My first work for rewriting is modifying here.
' Terry 2013.10.03

#Const OUTPUT_MATCH_PROCESS = False

Public Class SBSParser
    Public Const Version As String = "0.3devel"

    Dim Rules As List(Of Grammar) = New List(Of Grammar)

    Public Sub New()
        GrammarRules.LoadRules(Rules)
    End Sub

    Public Function ParseCode(ByVal code_reader As TextReader) As List(Of CodeSequence)
        If code_reader.GetLength() = 0 Then
            Return Nothing
        End If

        code_reader.RemoveBlankBeforeChar()
        RemoveNonCode(code_reader)

        Dim sentence_list As New List(Of CodeSequence)
        Dim mSentence As CodeSequence
        Dim is_error As Boolean

        While code_reader.IsEOF() <> True
            mSentence = MatchGrammarRule("STATMENT", code_reader)

            If mSentence IsNot Nothing Then
                sentence_list.Add(mSentence)
            Else
                Throw New ApplicationException("Syntax Error: Unexpected '" + code_reader.GetDeepestChar() + "' on line " + CStr(code_reader.GetDeepestLine))
                is_error = True
                Exit While
            End If
        End While

        If is_error Then
            Return Nothing
        Else
            Return sentence_list
        End If

    End Function

    Function MatchGrammarRule(ByVal rulename As String, ByVal code As TextReader) As CodeSequence

#If OUTPUT_MATCH_PROCESS Then
        StandardIO.PrintLine("Try to match " + rulename + " on " + CStr(code.GetPosition().Position))
#End If

        Dim rule As Grammar = GetRuleByName(rulename)

        If rule Is Nothing Then
            Return Nothing
        End If

        If rule.MatchMethod = Grammar.MATCH_METHOD_NORMAL Then

            Dim seq As List(Of GrammarSequence) = rule.Sequences

            For seq_offset As Integer = 0 To seq.Count - 1
                'RemoveBlankAndComments(code)
                Dim match_result As List(Of CodeSequence) = MatchGrammarSequence(seq(seq_offset), code)

                If match_result IsNot Nothing Then

#If OUTPUT_MATCH_PROCESS Then
                    StandardIO.PrintLine("Done on matching " + rulename)
#End If
                    Return New CodeSequence(rule.Name, match_result)
                End If
            Next

#If OUTPUT_MATCH_PROCESS Then
            StandardIO.PrintLine("Fault on matching " + rulename)
#End If
            Return Nothing
        ElseIf rule.MatchMethod = Grammar.MATCH_METHOD_SPECIFY_FUNC Then
            'RemoveBlankAndComments(code)

#If OUTPUT_MATCH_PROCESS Then
            StandardIO.PrintLine("Try to use specify function to match " + rulename)
#End If
            Return rule.SpecFunc(code)
        End If
        Return Nothing
    End Function

    Function MatchGrammarSequence(ByVal sequence As GrammarSequence, ByVal code As TextReader) As List(Of CodeSequence)
        Dim words As New List(Of CodeSequence)
        Dim start_position As TextReaderPosition = code.Position
        Dim unmatch As Boolean

        For offset As Integer = 0 To sequence.Element.Count - 1
            Dim ele As String = sequence.Element(offset)
            Dim element_first_char As Char = ele.Substring(0, 1)(0)

            If element_first_char = "*"c Then
                Dim word As New List(Of CodeSequence)
                Dim element_name As String = ele.Substring(1, ele.Length - 1)

                While True
                    Dim origin_pos As TextReaderPosition = code.Position
                    RemoveNonCode(code)
                    Dim matched_element As CodeSequence = MatchGrammarRule(element_name, code)
                    If matched_element IsNot Nothing Then
                        word.Add(matched_element)
                        ' *XXXXX is a set of XXXXXs. If some chars match the XXXXX, then they match *XXXXX.

                    ElseIf word.Count = 0 Then
                        unmatch = True
                        Exit For ' If the first char of the sentence don't match the element, the sentence cannot match the rule naturally.
                    Else
                        code.Position = origin_pos
                        Exit While
                        ' The reason of this line: If the last char doesn't match this element, it may be the member of the next element.So we should roll back.
                        ' If this sentence doesn't match the rule at all, this char won't match the next element either.
                    End If
                End While

                words.Add(New CodeSequence(ele, word))

            ElseIf element_first_char = "'"c Then
                code.RemoveBlankBeforeLf()

                Dim expected_str As String = ele.Substring(1, ele.Length - 2)
                Dim mChar As Char


                Dim mWord As String = String.Empty
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
                    words.Add(New CodeSequence("-KEYWORD-", expected_str))
                Else
                    Exit For
                End If
            Else
                RemoveNonCode(code)
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
            code.RemoveBlankBeforeLf()
            Return words
        Else
            code.Position = start_position
            Return Nothing
        End If

    End Function

    Function GetRuleByName(ByVal name As String) As Grammar
        For i As Integer = 0 To Rules.Count - 1
            If Rules(i).Name = name Then
                Return Rules(i)
            End If
        Next

        StandardIO.PrintLine("Internal Error: Unknow rule '" + name + "'.")
        Return Nothing
    End Function

    Sub RemoveNonCode(ByVal code As TextReader)
        code.RemoveBlankBeforeLf()
        While code.PeekNextChar() = "'"c
            Dim mChar As Char = code.GetNextChar()
            While mChar <> vbLf AndAlso code.IsEOF() <> True
                mChar = code.GetNextChar()
            End While
            code.RemoveBlankBeforeChar()
        End While
    End Sub
End Class


