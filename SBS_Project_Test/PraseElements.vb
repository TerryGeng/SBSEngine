' SBS - Simple Basic Script
' -------------------------
' This file is a part of SBS
' project.
' =========================
' XVG Planning branch 2013.7

Class Grammar
    Structure GrammarSequence
        Dim Element() As String ' A word only have one element, but a sentence can have more.
    End Structure

    Public Name As String
    Public Sequences() As GrammarSequence ' A sentence rule only have one Sequence, but a element can have more.

    Public Sub New(ByVal mname As String, ByVal text As String)
        Dim element() As String
        Name = mname
        element = text.Split("|||")

        For i As Integer = 0 To UBound(element)
            Sequences(i).Element = element(i).Split("+++")
        Next
    End Sub


End Class

Structure Sentence
    Dim RuleName As String
    Dim WordsList As ArrayList
End Structure

