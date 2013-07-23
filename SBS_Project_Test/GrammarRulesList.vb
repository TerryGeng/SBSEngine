Public Class GrammarRulesList
    Public Shared Sub LoadRules(ByRef Rules As ArrayList)

        Rules.Add(New Grammar("STATMENT", "EXPRESSION+++LINE_END"))

        Rules.Add(New Grammar("NUMBER", AddressOf PackNumber))
        Rules.Add(New Grammar("STRING", AddressOf PackString))
        Rules.Add(New Grammar("NAME", AddressOf PackName))
        Rules.Add(New Grammar("LINE_END", AddressOf PackLineEnd))
        Rules.Add(New Grammar("EXP_OP", "'<='|||'>='|||'+'|||'-'|||'*'|||'/'|||'>'|||'<'"))

        Rules.Add(New Grammar("EXPRESSION", "EXP_ELEMENT+++*EXP_OP_ELEMENT|||*EXP_OP_ELEMENT|||EXP_ELEMENT"))
        Rules.Add(New Grammar("EXP_ELEMENT", "NUMBER|||STRING|||VARIABLE|||FUNC_CALL|||'('+++EXPRESSION+++')'"))
        Rules.Add(New Grammar("EXP_OP_ELEMENT", "EXP_OP+++EXP_ELEMENT|||EXP_OP+++'('+++EXPRESSION+++')'"))

        Rules.Add(New Grammar("VARIABLE", "'$'+++NAME"))
        Rules.Add(New Grammar("FUNC_CALL", "NAME+++'()'|||NAME+++'('+++ARG_LIST+++')'"))
        Rules.Add(New Grammar("ARG_LIST", "*ARG_COMMA+++EXPRESSION|||EXPRESSION"))
        Rules.Add(New Grammar("ARG_COMMA", "EXPRESSION+++','"))

        Rules.Add(New Grammar("VAR_DEF", "VARIABLE+++'='+++EXPRESSION"))
        Rules.Add(New Grammar("FUNC_DEF", "'Function '+++NAME+++(+++ARG_DEF_LIST+++)+++LINE_END+++" & _
                              "*STATMENT+++" & _
                              "'End Function'"))
        Rules.Add(New Grammar("ARG_DEF_LIST", "VARIABLE|||*VARIABLE_COMMA+++VARIABLE"))
        Rules.Add(New Grammar("ARG_COMMA", "VARIABLE+++','"))

        Rules.Add(New Grammar("JUMP", "'Return '+++EXPRESSION|||'Return'|||'Exit'"))


    End Sub

    Public Shared Function PackString(ByRef code As TextReader) As CodeSequence
        If code.GetNextChar() = Chr(34) Then
            Dim str As String = ""
            While True
                Dim mChar As Char = code.GetNextChar()
                If mChar <> Chr(34) Then
                    Str += mChar
                Else
                    Return New CodeSequence("STRING", Str)
                End If
            End While

            Return Nothing
        Else
            Return Nothing
        End If
    End Function

    Public Shared Function PackNumber(ByRef code As TextReader) As CodeSequence
        Dim nums As String = ""
        Dim origin_pos As Integer = code.GetPosition().Position
        While True
            Dim mChar As Char = code.GetNextChar

            If IsNumeric(mChar) Then
                nums += mChar
            ElseIf nums <> "" Then
                code.PositionBack()
                Return New CodeSequence("NUMBER", nums)
            Else
                code.SetPosition(origin_pos)
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Public Shared Function PackName(ByRef code As TextReader) As CodeSequence
        Dim name As String = ""
        Dim origin_pos As Integer = code.GetPosition().Position
        While True
            Dim mChar As Char = code.GetNextChar

            If IsNameChar(mChar) And (name.Length <> 0 Or (IsNumeric(mChar) <> True)) Then
                name += mChar
            ElseIf name <> "" Then
                code.PositionBack()
                Return New CodeSequence("NAME", name)
            Else
                code.SetPosition(origin_pos)
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Public Shared Function PackLineEnd(ByRef code As TextReader) As CodeSequence
        Dim origin_pos As Integer = code.GetPosition().Position
        If code.IsEOF() Or code.GetNextChar() = vbLf Then
            Return New CodeSequence("LINE_END", "")
        Else
            code.SetPosition(origin_pos)
            Return Nothing
        End If
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

