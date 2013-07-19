Public Class TextReader
    Dim Code As String = ""
    Dim PosStat As New TextReaderPosStatus
    Dim DeepestPos As New TextReaderPosStatus

    Sub New(ByRef code As String)
        LoadCode(code)
    End Sub

    Sub LoadCode(ByRef mCode As String)
        Code = mCode
        PosStat.Position = 0
    End Sub

    Function GetChar(ByVal offset As Integer) As Char
        If offset >= Code.Length Then
            Return ""
        End If

        Return Code.Substring(offset, 1)
    End Function

    Function GetNextChar() As Char
        Dim mChar As Char = GetChar(PosStat.Position)
        PosStat.Position += 1

        If mChar = vbCr Then
            Return GetNextChar()
        ElseIf mChar = vbLf Then
            PosStat.Lines += 1
        End If

        If PosStat.Position > DeepestPos.Position Then
            DeepestPos.Position = PosStat.Position
            DeepestPos.Lines = PosStat.Lines
        End If

        Return mChar
    End Function

    Function GetPosStat() As TextReaderPosStatus
        Return PosStat
    End Function

    Function SetPosStat(ByVal pos As TextReaderPosStatus) As Boolean
        If pos.Position < Code.Length Then
            PosStat.Position = pos.Position
            PosStat.Lines = pos.Lines
            Return True
        Else
            Return False
        End If
    End Function

    Sub SetPosition(ByVal position As Integer)
        PosStat.Position = position
    End Sub

    Function IsEOF()
        If PosStat.Position >= Code.Length Then
            Return True
        Else
            Return False
        End If
    End Function

    Function GetLength()
        Return Code.Length
    End Function

    Function GetDeepestChar()
        Return GetChar(DeepestPos.Position)
    End Function

    Function GetDeepestLine()
        Return DeepestPos.Lines
    End Function
End Class

Public Class TextReaderPosStatus
    Public Position As Integer = 0
    Public Lines As Integer = 1
End Class