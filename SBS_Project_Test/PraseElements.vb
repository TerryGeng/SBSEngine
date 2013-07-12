' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Developing branch 2013.7

Structure GrammarSequence
    Dim Element() As String ' A word only have one element, but a sentence can have more.
End Structure

Class Grammar

    Public Name As String
    Public Sequences As New ArrayList() ' A sentence rule only have one Sequence, but a element can have more.

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
End Class

Class CodeSequence
    Public Type As String = "Index"
    Public RuleName As String = ""
    Public WordsList As ArrayList
    Public Value As String

    Sub New(ByVal name As String, ByRef words As ArrayList)
        Type = "Index"
        RuleName = name
        WordsList = words
    End Sub

    Sub New(ByVal name As String, ByVal val As String)
        Type = "Value"
        Value = val
        RuleName = name
        WordsList = Nothing
    End Sub

    Function GetSeqType()
        Return Type
    End Function
End Class

