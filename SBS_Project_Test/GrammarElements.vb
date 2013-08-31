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

    Delegate Function MatchFunc(ByVal code As TextReader) As CodeSequence

    Public Name As String
    Public Sequences As List(Of GrammarSequence) = New List(Of GrammarSequence)
    Public SpecFunc As MatchFunc

    Public MatchMethod As Short = MATCH_METHOD_NORMAL

    Private Shared ReadOnly PatternSeparater() As String = {"|||"}
    Private Shared ReadOnly SequenceSeparater() As String = {"+++"}


    Public Sub New(ByVal mname As String, ByVal pattern As String)
        Dim element() As String
        Name = mname
        element = pattern.Split(PatternSeparater, StringSplitOptions.RemoveEmptyEntries)

        For i As Integer = 0 To UBound(element)
            Dim mSequences As New GrammarSequence
            mSequences.Element = element(i).Split(SequenceSeparater, StringSplitOptions.RemoveEmptyEntries)
            Sequences.Add(mSequences)
        Next
    End Sub

    Public Sub New(ByVal mname As String, ByVal func As MatchFunc)
        MatchMethod = MATCH_METHOD_SPECIFY_FUNC
        Name = mname
        SpecFunc = func
    End Sub
End Class

Public Class CodeSequence
    Public Type As String = "Index"
    Public RuleName As String
    Public SeqsList() As CodeSequence
    Public Value As String

    Sub New(ByVal name As String, ByVal words As List(Of CodeSequence))
        Type = "Index"
        RuleName = name
        SeqsList = words.ToArray()
    End Sub

    Sub New(ByVal name As String, ByVal val As String)
        Type = "Value"
        Value = val
        RuleName = name
        SeqsList = Nothing
    End Sub

    Function GetSeqType() As String
        Return Type
    End Function
End Class

