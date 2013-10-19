Imports System.IO
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
        Delegate Function PackerFunction(ByVal Character As Char, ByVal Position As Integer) As PackerStatus
        Public Type As LexiconType
        Public Pack As PackerFunction

        Sub New(ByVal Type As LexiconType, ByVal Packer As PackerFunction)
            Me.Type = Type
            Me.Pack = Packer
        End Sub
    End Class

    Enum PackerStatus
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
        Public Overrides ReadOnly Property Message As String
            Get
                Return "Unexpected Character (" & AscW(Character).ToString & ")'" & Character & "' at " & Position.ToString
            End Get
        End Property

        Sub New(ByVal Position As Integer, ByVal Character As Char)
            Me.Position = Position
            Me.Character = Character
        End Sub
    End Class
End Module

Module TokenizerRules
    Sub LoadLexicalRules(ByVal Container As LexiconRules)
        Container.Add(LexiconType.LInteger, New LexiconRule(LexiconType.LInteger, AddressOf IntegerPacker))
    End Sub

    Function IntegerPacker(ByVal Character As Char, ByVal Position As Integer) As PackerStatus
        If Char.IsDigit(Character) Then
            Return PackerStatus.Continued
        End If
        Return PackerStatus.PreviousFinished
    End Function
End Module

Public Class Tokenizer
    Dim Reader As CharsReader
    Dim RulesContainer As LexiconRules

    Sub New(ByVal Code As String)
        Reader = New CharsReader(Code)
        RulesContainer = New LexiconRules
        TokenizerRules.LoadLexicalRules(RulesContainer)
    End Sub

    Function NextToken() As Token
        Dim Candidates As List(Of LexiconType) = New List(Of LexiconType)(RulesContainer.Keys)

        Dim TokenBuffer As String = String.Empty
        Dim BufferLength As Integer
        Dim Character As Char = Reader.NextChar()

        Do
            Dim CandidatePointer As Integer

            While CandidatePointer < Candidates.Count
                Dim Candidate As LexiconType = Candidates(CandidatePointer)
                Select Case RulesContainer(Candidate).Pack(Character, BufferLength)
                    Case PackerStatus.Finished
                        TokenBuffer &= Character
                        Return New Token With {.Type = Candidate, .Value = TokenBuffer}

                    Case PackerStatus.PreviousFinished
                        If Not TokenBuffer.Length = 0 Then
                            Reader.MovePrev()
                            Return New Token With {.Type = Candidate, .Value = TokenBuffer}
                        Else
                            Candidates.RemoveAt(CandidatePointer)
                            Continue While
                        End If

                    Case PackerStatus.Continued
                        CandidatePointer += 1
                End Select
            End While

            If Candidates.Count > 0 Then
                TokenBuffer &= Character
            Else
                Throw New UnexpectedCharacterException(Reader.Position, Character)
            End If

        Loop While CBool(AscW(Character))

        Return Nothing
    End Function

End Class

Class CharsReader
    Dim BaseString As String
    Dim Pointer As Integer

    Public ReadOnly Property Position As Integer
        Get
            Return Pointer
        End Get
    End Property

    Sub New(ByRef BaseString As String)
        Me.BaseString = BaseString
        Pointer = -1
    End Sub

    Public Function Current() As Char
        Try
            Return BaseString(Pointer)
        Catch ex As IndexOutOfRangeException
            Return ChrW(0)
        End Try
    End Function

    Public Function NextChar() As Char
        MoveNext()
        Return Current()
    End Function

    Public Sub MoveNext()
        Pointer += 1
    End Sub

    Public Sub MovePrev()
        Pointer -= 1
    End Sub
End Class