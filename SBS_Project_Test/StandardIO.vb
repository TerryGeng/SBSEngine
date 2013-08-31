Public Class StandardIO
    Public Shared Output As OutputFunction
    Public Shared GetInput As InputFunction

    Public Delegate Sub OutputFunction(ByVal ouput As String)
    Public Delegate Function InputFunction() As String

    Public Shared Sub SetIOFunc(ByVal _output As OutputFunction, ByVal _input As InputFunction)
        Output = _output
        GetInput = _input
    End Sub

    Public Shared Sub Print(ByVal str As String)
        If Output IsNot Nothing Then
            Output(str)
        End If
    End Sub

    Public Shared Sub PrintError(ByVal msg As String)
        If Output IsNot Nothing Then
            Output(msg + vbCrLf)
        End If
    End Sub

    Public Shared Sub PrintLine(ByVal str As String)
        If Output IsNot Nothing Then
            Output(str + vbCrLf)
        End If
    End Sub

    Public Shared Function GetLine() As String
        If GetInput IsNot Nothing Then
            Return GetInput()
        Else
            Return Nothing
        End If
    End Function
End Class
