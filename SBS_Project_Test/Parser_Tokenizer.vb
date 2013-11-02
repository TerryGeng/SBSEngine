Imports System.IO
Imports System.Text
Imports LexiconRules = System.Collections.Generic.Dictionary(Of SBSEngine.TokenizerType.LexiconType, SBSEngine.TokenizerType.LexiconRule)

Public Module TokenizerType
    Public Enum LexiconType
        LInteger
        LFloat
        LName
        LString
        LBlank
        LCrLf
        LSymbol
    End Enum

    Public Structure Token
        Dim Type As LexiconType
        Dim Value As String
    End Structure

    Public Class LexiconRule
        Delegate Function CheckerFunction(ByVal Character As Char, ByVal Position As Integer) As CheckerStatus
        Public Type As LexiconType
        Public Check As CheckerFunction

        Sub New(ByVal Type As LexiconType, ByVal Checker As CheckerFunction)
            Me.Type = Type
            Me.Check = Checker
        End Sub
    End Class

    Enum CheckerStatus
        ''' <summary>
        ''' Current and previous characters still match the rule. Match process continue.
        ''' </summary>
        ''' <remarks></remarks>
        Continued
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
        Container.Add(LexiconType.LInteger, New LexiconRule(LexiconType.LInteger, AddressOf IntegerChecker))
    End Sub

    Function IntegerChecker(ByVal Character As Char, ByVal Position As Integer) As CheckerStatus
        If Char.IsDigit(Character) Then
            Return CheckerStatus.Continued
        End If
        Return CheckerStatus.PreviousFinished
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
        Dim Candidates As List(Of LexiconType) = New List(Of LexiconType)(RulesContainer.Keys)

        Dim TokenBuffer As New StringBuilder()
        Dim BufferLength As Integer
        Dim Character As Char = Reader.NextChar()

        Do
            Dim CandidatePointer As Integer

            While CandidatePointer < Candidates.Count
                Dim Candidate As LexiconType = Candidates(CandidatePointer)
                Select Case RulesContainer(Candidate).Check(Character, BufferLength)
                    Case CheckerStatus.Finished
                        TokenBuffer.Append(Character)
                        Return New Token With {.Type = Candidate, .Value = TokenBuffer.ToString}

                    Case CheckerStatus.PreviousFinished
                        If TokenBuffer.Length > 0 Then
                            Reader.MovePrev()
                            Return New Token With {.Type = Candidate, .Value = TokenBuffer.ToString}
                        Else
                            Candidates.RemoveAt(CandidatePointer)
                            Continue While
                        End If

                    Case CheckerStatus.Continued
                        CandidatePointer += 1
                End Select
            End While

            If Candidates.Count > 0 Then
                TokenBuffer.Append(Character)
            Else
                Throw New UnexpectedCharacterException(Reader.Position, Character)
            End If

        Loop While Not AscW(Character) = 0

        Return Nothing
    End Function

End Class

Class SourceCodeReader
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
        If (Pointer < 0 OrElse Pointer > BaseString.Length) Then
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
