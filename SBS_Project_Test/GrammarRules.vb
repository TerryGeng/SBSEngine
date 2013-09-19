Public Module GrammarRules
    Public Sub LoadRules(ByVal Rules As List(Of Grammar))

        Rules.Add(New Grammar("STATMENT", "DEFINITION+++LINE_END|||EXPRESSION+++LINE_END|||CONTROLFLOW+++LINE_END|||JUMP+++LINE_END"))
        'Rules.Add(New Grammar("STATMENT", "EXPRESSION+++LINE_END"))

        Rules.Add(New Grammar("NUMBER", AddressOf PackNumber))
        Rules.Add(New Grammar("STRING", AddressOf PackString))
        Rules.Add(New Grammar("NAME", AddressOf PackName))
        Rules.Add(New Grammar("LINE_END", AddressOf PackLineEnd))
        Rules.Add(New Grammar("EXP_OP", "'+'|||'-'|||'*'|||'/'"))

        Rules.Add(New Grammar("EXPRESSION", "EXP_ELEMENT+++*EXP_OP_ELEMENT|||*EXP_OP_ELEMENT|||EXP_ELEMENT"))
        Rules.Add(New Grammar("EXP_ELEMENT", "NUMBER|||STRING|||VARIABLE|||FUNC_CALL|||'('+++EXPRESSION+++')'"))
        Rules.Add(New Grammar("EXP_OP_ELEMENT", "EXP_OP+++EXP_ELEMENT|||EXP_OP+++'('+++EXPRESSION+++')'"))

        Rules.Add(New Grammar("JUDG_OP", "'<='|||'>='|||'='|||'>'|||'<'"))
        Rules.Add(New Grammar("JUDG_OR_EXPR", "JUDG_AND_EXPR+++'Or'+++JUDG_OR_EXPR|||JUDG_AND_EXPR"))
        Rules.Add(New Grammar("JUDG_AND_EXPR", "JUDG_SINGLE+++'And'+++JUDG_SINGLE|||JUDG_SINGLE|||JUDG_AND_EXPR+++'And'+++JUDE_SINGLE"))
        Rules.Add(New Grammar("JUDG_SINGLE", "EXPRESSION+++JUDG_OP+++EXPRESSION|||EXPRESSION|||'('+++JUDG_OR_EXPR+++')'"))

        Rules.Add(New Grammar("VARIABLE", "'$'+++NAME"))
        Rules.Add(New Grammar("FUNC_CALL", "NAME+++'()'|||NAME+++'('+++ARG_LIST+++')'"))
        Rules.Add(New Grammar("ARG_LIST", "*ARG_COMMA+++EXPRESSION|||EXPRESSION"))
        Rules.Add(New Grammar("ARG_COMMA", "EXPRESSION+++','"))

        Rules.Add(New Grammar("DEFINITION", "VAR_DEF|||FUNC_DEF"))
        Rules.Add(New Grammar("VAR_DEF", "VARIABLE+++'='+++EXPRESSION"))

        Rules.Add(New Grammar("CONTROLFLOW", "IF_ELSE|||WHILE|||FOR"))
        Rules.Add(New Grammar("IF_ELSE", "IF_ELSE_HEAD+++ELSE_AND_END"))
        Rules.Add(New Grammar("IF_ELSE_HEAD", "'If '+++JUDG_OR_EXPR+++'Then'+++LINE_END+++*STATMENT"))
        Rules.Add(New Grammar("ELSE_AND_END",
                              "'End If'|||" & _
                              "'Else'+++LINE_END+++*STATMENT+++'End If'|||" & _
                              "*ELSE_IF+++'End If'|||" & _
                              "*ELSE_IF+++'Else'+++LINE_END+++*STATMENT+++'End If'"))
        Rules.Add(New Grammar("ELSE_IF", "'ElseIf '+++JUDG_OR_EXPR+++'Then'+++LINE_END+++*STATMENT"))

        Rules.Add(New Grammar("WHILE", "'While'+++JUDG_OR_EXPR+++LINE_END+++*STATMENT+++'End While'"))
        Rules.Add(New Grammar("FOR",
                              "'For '+++FOR_VAR+++'To '+++EXPRESSION+++'Step '+++EXPRESSION+++LINE_END+++*STATMENT+++'End For'|||" & _
                              "'For '+++FOR_VAR+++'To '+++EXPRESSION+++LINE_END+++*STATMENT+++'End For'"))
        Rules.Add(New Grammar("FOR_VAR", "VAR_DEF|||VARIABLE"))


        Rules.Add(New Grammar("FUNC_DEF",
                              "'Function '+++NAME+++'()'+++LINE_END+++" & _
                              "*STATMENT+++" & _
                              "'End Function'|||" & _
                              "'Function '+++NAME+++'('+++ARG_DEF_LIST+++')'+++LINE_END+++" & _
                              "*STATMENT+++" & _
                              "'End Function'"))
        Rules.Add(New Grammar("ARG_DEF_LIST", "*ARG_DEF_COMMA+++VARIABLE|||VARIABLE"))
        Rules.Add(New Grammar("ARG_DEF_COMMA", "VARIABLE+++','"))

        Rules.Add(New Grammar("JUMP", "'Return '+++EXPRESSION|||'Return'|||'Continue For'|||'Continue While'|||'Exit For'|||'Exit While'"))


    End Sub

    Public Function PackString(ByVal code As TextReader) As CodeSequence
        If code.GetNextChar() = Chr(34) Then
            Dim str As String = String.Empty
            Dim isSpecChar As Boolean
            While True
                Dim mChar As Char = code.GetNextChar()

                If isSpecChar = False Then
                    If mChar = "\" Then
                        isSpecChar = True
                    ElseIf mChar <> Chr(34) Then
                        str += mChar
                    Else
                        Return New CodeSequence("STRING", str)
                    End If
                Else
                    If mChar = "n" Then
                        str += vbCrLf
                    ElseIf mChar = "\" Then
                        str += "\"
                    ElseIf mChar = Chr(34) Then
                        str += "\"
                    End If

                    isSpecChar = False
                End If
            End While

            Return Nothing
        Else
            Return Nothing
        End If
    End Function

    Public Function PackNumber(ByVal code As TextReader) As CodeSequence
        Dim nums As String = String.Empty
        Dim origin_pos As TextReaderPosition = code.Position

        Dim lastChar As Char
        Dim Float As Boolean

        While True
            Dim mChar As Char = code.GetNextChar

            If IsNumeric(mChar) Then
                nums += mChar
            ElseIf mChar = "."c AndAlso Float = False AndAlso lastChar <> Char.MinValue Then
                nums += mChar
            ElseIf nums <> String.Empty AndAlso lastChar <> "."c Then
                code.PositionBack()
                Return New CodeSequence("NUMBER", nums)
            Else
                code.Position = origin_pos
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Public Function PackName(ByVal code As TextReader) As CodeSequence
        Dim name As String = String.Empty
        Dim origin_pos As TextReaderPosition = code.Position
        While True
            Dim mChar As Char = code.GetNextChar

            If IsNameChar(mChar) AndAlso (name.Length <> 0 OrElse (IsNumeric(mChar) <> True)) Then
                name += mChar
            ElseIf name <> String.Empty Then
                code.PositionBack()
                Return New CodeSequence("NAME", name)
            Else
                code.Position = origin_pos
                Return Nothing
            End If
        End While

        Return Nothing
    End Function

    Public Function PackLineEnd(ByVal code As TextReader) As CodeSequence
        Dim origin_pos As TextReaderPosition = code.Position

        If code.IsEOF() OrElse code.GetNextChar() = vbLf Then
            code.RemoveBlankBeforeChar()
            Return New CodeSequence("LINE_END", String.Empty)
        Else
            code.Position = origin_pos
            Return Nothing
        End If
    End Function

    Function IsNameChar(ByVal mChar As Char) As Boolean
        Return Char.IsLetterOrDigit(mChar) OrElse mChar >= ChrW(128)
    End Function

End Module

