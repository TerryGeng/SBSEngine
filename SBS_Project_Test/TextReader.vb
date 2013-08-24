Public Class TextReader
    Dim Code As String = String.Empty
    Dim Pos As New TextReaderPosition
    Dim DeepestPos As New TextReaderPosition

    Sub New(ByRef code As String)
        LoadText(code)
    End Sub

    Sub LoadText(ByRef mCode As String)
        Code = mCode
        Pos.Position = 0
    End Sub

    Function GetChar(ByVal offset As Integer) As Char
        If offset >= Code.Length Then
            Return String.Empty
        End If

        Return Code.Substring(offset, 1)
    End Function

    Public Function GetNextChar() As Char
        Dim mChar As Char = GetChar(Pos.Position)

        Pos.Position += 1

        If mChar = vbCr Then
            Return GetNextChar()
        ElseIf mChar = vbLf Then
            Pos.Lines += 1
        End If

        If Pos.Position > DeepestPos.Position Then
            DeepestPos.Position = Pos.Position
            DeepestPos.Lines = Pos.Lines
        End If

        Return mChar
    End Function

    Public Function PeekNextChar() As Char
        Return GetChar(Pos.Position)
    End Function

    Public Sub RemoveBlankBeforeLf()
        Dim mChar As Char = PeekNextChar()
        While Char.IsWhiteSpace(mChar) And mChar <> vbLf And mChar <> vbCr
            GetNextChar()
            mChar = PeekNextChar()
        End While
    End Sub

    Public Sub RemoveBlankBeforeChar()
        While Char.IsWhiteSpace(PeekNextChar)
            GetNextChar()
        End While
    End Sub

    Public Function GetPosition() As TextReaderPosition
        Return Pos
    End Function

    Public Function SetPos(ByVal pos As TextReaderPosition) As Boolean
        If pos.Position < Code.Length Then
            pos.Position = pos.Position
            pos.Lines = pos.Lines
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub SetPosition(ByVal position As Integer, ByVal line As Integer)
        Pos.Position = position
        Pos.Lines = line
    End Sub

    Public Sub PositionBack(Optional ByVal len As Integer = 1)
        Pos.Position -= len
    End Sub

    Public Function IsEOF()
        If Pos.Position >= Code.Length Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function IsEOF(ByVal position As Integer)
        If position >= Code.Length Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetLength()
        Return Code.Length
    End Function

    Public Function GetDeepestChar()
        If (IsEOF(DeepestPos.Position - 1)) Then
            Return "END"
        Else
            Return GetChar(DeepestPos.Position - 1)
        End If
    End Function

    Public Function GetDeepestLine()
        Return DeepestPos.Lines
    End Function
End Class

Public Class TextReaderPosition
    Public Position As Integer = 0
    Public Lines As Integer = 1
End Class