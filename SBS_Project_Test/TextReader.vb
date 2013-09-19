Public Class TextReader
    Dim Code As String = String.Empty
    Dim Pos As TextReaderPosition
    Dim DeepestPos As New TextReaderPosition

    Sub New(ByVal code As String)
        LoadText(code)
    End Sub

    Sub LoadText(ByVal code As String)
        Me.Code = code
        Pos = New TextReaderPosition
        Pos.Lines = 1
    End Sub

    Function GetChar(ByVal offset As Integer) As Char
        If offset >= Code.Length Then
            Return New Char()
        End If

        Return Code.Substring(offset, 1)(0)
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
        While Char.IsWhiteSpace(mChar) AndAlso mChar <> vbLf AndAlso mChar <> vbCr
            GetNextChar()
            mChar = PeekNextChar()
        End While
    End Sub

    Public Sub RemoveBlankBeforeChar()
        While Char.IsWhiteSpace(PeekNextChar)
            GetNextChar()
        End While
    End Sub

    Public Property Position As TextReaderPosition
        Get
            Return Pos
        End Get
        Set(value As TextReaderPosition)
            If value.Position < Code.Length Then Pos = value
        End Set
    End Property

    <Obsolete("Please use Position property")>
    Public Function GetPosition() As TextReaderPosition
        Return Pos
    End Function

    <Obsolete("Please use Position property")>
    Public Function SetPos(ByVal pos As TextReaderPosition) As Boolean
        If pos.Position < Code.Length Then
            Me.Pos.Position = pos.Position
            Me.Pos.Lines = pos.Lines
            Return True
        Else
            Return False
        End If
    End Function

    <Obsolete("Please use Position property")>
    Public Sub SetPosition(ByVal position As Integer, ByVal line As Integer)
        Pos.Position = position
        Pos.Lines = line
    End Sub

    Public Sub PositionBack(Optional ByVal len As Integer = 1)
        Pos.Position -= len
    End Sub

    Public Function IsEOF() As Boolean
        Return Pos.Position >= Code.Length
    End Function

    Public Function IsEOF(ByVal position As Integer) As Boolean
        Return position >= Code.Length
    End Function

    Public Function GetLength() As Integer
        Return Code.Length
    End Function

    Public Function GetDeepestChar() As Char
        If (IsEOF(DeepestPos.Position - 1)) Then
            Return Nothing
        Else
            Return GetChar(DeepestPos.Position - 1)
        End If
    End Function

    Public Function GetDeepestLine() As Integer
        Return DeepestPos.Lines
    End Function
End Class

Public Structure TextReaderPosition
    Public Position As Integer
    Public Lines As Integer
End Structure