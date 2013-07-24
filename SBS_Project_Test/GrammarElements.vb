' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Developing branch 2013.7

Public Structure GrammarSequence
    Dim Element() As String
End Structure

Public Class Grammar

    Public Const MATCH_METHOD_SPECIFY_FUNC As Short = 1
    Public Const MATCH_METHOD_NORMAL As Short = 0

    Delegate Function MatchFunc(ByRef code As TextReader) As CodeSequence

    Public Name As String
    Public Sequences As New ArrayList()
    Public SpecFunc As MatchFunc

    Public MatchMethod As Short = MATCH_METHOD_NORMAL

    Public Sub New(ByVal mname As String, ByVal text As String)
        Dim element() As String
        Dim separater() As String = {"|||"}
        Name = mname
        element = text.Split(separater, StringSplitOptions.RemoveEmptyEntries)

        For i As Integer = 0 To UBound(element)
            Dim mSequences As New GrammarSequence
            Dim mseparater() As String = {"+++"}
            mSequences.Element = element(i).Split(mseparater, StringSplitOptions.RemoveEmptyEntries)
            Sequences.Add(mSequences)
        Next
    End Sub

    Public Sub New(ByVal mname As String, ByRef func As MatchFunc)
        MatchMethod = MATCH_METHOD_SPECIFY_FUNC
        Name = mname
        SpecFunc = func
    End Sub
End Class

Public Class CodeSequence
    Public Type As String = "Index"
    Public RuleName As String = ""
    Public SeqsList As ArrayList
    Public Value As String

    Sub New(ByVal name As String, ByRef words As ArrayList)
        Type = "Index"
        RuleName = name
        SeqsList = words
    End Sub

    Sub New(ByVal name As String, ByVal val As String)
        Type = "Value"
        Value = val
        RuleName = name
        SeqsList = Nothing
    End Sub

    Function GetSeqType()
        Return Type
    End Function
End Class

