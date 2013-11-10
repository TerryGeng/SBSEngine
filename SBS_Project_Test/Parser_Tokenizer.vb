Imports System.IO
Imports System.Text
Imports LexiconRules = System.Collections.Generic.Dictionary(Of SBSEngine.TokenizerType.LexiconType, SBSEngine.TokenizerType.LexiconRule)

Public Module TokenizerType
    Public Enum LexiconType
        Undefined
        LInteger
        LFloat
        LName
        LString
        LBlank
        LCrLf
        LSymbol
    End Enum

    Public Structure Token
        Public Type As LexiconType
        Public Value As String
    End Structure

    Public Class LexiconRule
        Delegate Function ScannerFunction(ByVal Character As Char, ByVal Position As Integer) As ScannerStatus
        Public Type As LexiconType
        Public Scan As ScannerFunction

        Sub New(ByVal Type As LexiconType, ByVal Scanner As ScannerFunction)
            Me.Type = Type
            Me.Scan = Scanner
        End Sub
    End Class

    Enum ScannerStatus
        ''' <summary>
        ''' Current and previous characters still match the rule. Match process continue.
        ''' </summary>
        ''' <remarks></remarks>
        Continued
        ''' <summary>
        ''' The emergence of current char indicated this combination unmatched the rule.
        ''' </summary>
        ''' <remarks></remarks>
        Unmatch
        ''' <summary>
        ''' Current and previous characters matchs the rule. Match process end.
        ''' </summary>
        ''' <remarks></remarks>
        Finished
        ''' <summary>
        ''' Only previous characters match the rule. Match process end.
        ''' </summary>
        ''' <remarks></remarks>
        PreviousFinished

    End Enum

    Public Class UnexpectedCharacterException
        Inherits ApplicationException

        Public Position As Integer
        Public Character As Char

        Sub New(ByVal Position As Integer, ByVal Character As Char)
            MyBase.New(String.Format("Unexpected Character '{0}'({1}) at {2}.", Character, Convert.ToInt32(Character), Position))
            Me.Position = Position
            Me.Character = Character
        End Sub
    End Class
End Module

Module TokenizerRules
    Sub LoadLexicalRules(ByVal Container As LexiconRules)
        Container.Add(LexiconType.LInteger, New LexiconRule(LexiconType.LInteger, AddressOf IntegerScanner))
        Container.Add(LexiconType.LBlank, New LexiconRule(LexiconType.LBlank, AddressOf BlankScanner))
        Container.Add(LexiconType.LName, New LexiconRule(LexiconType.LName, AddressOf NameScanner))
        Container.Add(LexiconType.LCrLf, New LexiconRule(LexiconType.LCrLf, AddressOf CrLfScanner))

    End Sub

    Function IntegerScanner(ByVal Character As Char, ByVal Position As Integer) As ScannerStatus
        If Char.IsDigit(Character) Then Return ScannerStatus.Continued
        Return ScannerStatus.PreviousFinished
    End Function

    Function BlankScanner(ByVal Character As Char, ByVal Position As Integer) As ScannerStatus
        ' Character is neither vbCr nor vbLf
        If (Not Char.IsControl(Character)) AndAlso _ 
            Char.IsWhiteSpace(Character) Then _
            Return ScannerStatus.Continued
        Return ScannerStatus.PreviousFinished
    End Function

    Function NameScanner(ByVal Character As Char, ByVal Position As Integer) As ScannerStatus
        If Character = "_"c Then
            Return ScannerStatus.Continued
        Else
            If (Position = 0 AndAlso Char.IsLetter(Character)) OrElse _
                (Char.IsLetterOrDigit(Character)) Then _
                Return ScannerStatus.Continued
        End If

        Return ScannerStatus.PreviousFinished
    End Function

    Function CrLfScanner(ByVal Character As Char, ByVal Position As Integer) As ScannerStatus
        Select Case Position
            Case 0
                If Character = vbCr Then Return ScannerStatus.Continued
            Case 1
                If Character = vbLf Then Return ScannerStatus.Finished
        End Select
        Return ScannerStatus.Unmatch
    End Function
End Module

Public Class Tokenizer
    Dim Reader As SourceCodeReader
    Dim RulesContainer As LexiconRules

    Sub New(ByVal Code As String)
        Reader = New SourceCodeReader(Code)
        RulesContainer = New LexiconRules
        TokenizerRules.LoadLexicalRules(RulesContainer)
    End Sub

    Function NextToken() As Token
        If AscW(Reader.Peek()) = 0 Then Return Nothing
        Dim Rules = RulesContainer.Keys
        Dim Matches As New BitArray(Rules.Count, True)

        Dim TokenBuffer As New StringBuilder()
        Dim Character As Char

        Do
            Dim CandidatePos As Integer
            Character = Reader.NextChar()

            While CandidatePos < Rules.Count
                If Not Matches(CandidatePos) Then CandidatePos += 1 : Continue While
                Dim Candidate As LexiconType = Rules(CandidatePos)
                Select Case RulesContainer(Candidate).Scan(Character, TokenBuffer.Length)
                    Case ScannerStatus.Continued
                        Exit Select
                    Case ScannerStatus.Finished
                        TokenBuffer.Append(Character)
                        Return New Token With {.Type = Candidate, .Value = TokenBuffer.ToString}

                    Case ScannerStatus.PreviousFinished
                        If TokenBuffer.Length > 0 Then
                            Reader.MovePrev()
                            Return New Token With {.Type = Candidate, .Value = TokenBuffer.ToString}
                        Else
                            Matches.Set(CandidatePos, False)
                        End If
                    Case ScannerStatus.Unmatch
                        Matches.Set(CandidatePos, False)
                End Select
                CandidatePos += 1
            End While

            'Judge if there's still some candidates are true.
            For Each IsMatch As Boolean In Matches
                If IsMatch Then
                    TokenBuffer.Append(Character)
                    Continue Do
                End If
            Next

            Throw New UnexpectedCharacterException(Reader.Position, Character)
        Loop While Not AscW(Character) = 0

        Return Nothing
    End Function

End Class

Public Class SourceCodeReader
    Dim BaseString As String
    Dim Pointer As Integer

    Public ReadOnly Property Position As Integer
        Get
            Return Pointer
        End Get
    End Property

    Sub New(ByRef BaseString As String)
        Me.BaseString = BaseString
    End Sub

    Public Function Peek() As Char
        If Pointer < 0 OrElse Pointer >= BaseString.Length Then
            Return ChrW(0)
        Else
            Return BaseString(Pointer)
        End If
    End Function

    Public Overloads Function NextChar() As Char
        Dim result As Char = Peek()
        MoveNext()
        Return result
    End Function

    'Reserve
    Public Overloads Function NextChar(ByVal ch As Char) As Boolean
        If Peek() = ch Then
            MoveNext()
            Return True
        Else : Return False
        End If
    End Function

    Public Sub MoveNext()
        Pointer += 1
    End Sub

    Public Sub MovePrev()
        Pointer -= 1
    End Sub
End Class
