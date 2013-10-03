' My first work for rewriting is modifying here.
' Terry 2013.10.03

Public Class GrammarRule
    Public Structure ElementSequence
        <Obsolete("TODO: Change String to GrammarRuleType")>
        Dim Element() As String
    End Structure

    Public Structure Element
        <FlagsAttribute()> _
        Public Enum MatchOptionList
            PointToRule = 1
            PointToCharaters = 2

            OnlyOne = 4
            LeastOne = 8
            ZeroOrMore = 16
        End Enum

        Dim Type As GrammarRuleType
        Dim Charaters As String
        Dim MatchOption As Boolean
    End Structure

    Enum MatchMethodType
        ViaFunction
        ViaMatcher
    End Enum

    Delegate Function MatchFunc(ByVal code As TextReader) As ParsedTree

    Public Type As GrammarRuleType
    Public Sequences As List(Of ElementSequence) = New List(Of ElementSequence)
    Public SpecifiedFunction As MatchFunc

    Public MatchMethod As MatchMethodType

    Private Shared ReadOnly PatternSeparater() As String = {"|||"}
    Private Shared ReadOnly SequenceSeparater() As String = {"+++"}


    Public Sub New(ByVal Type As GrammarRuleType, ByVal TextPattern As String)
        MatchMethod = MatchMethodType.ViaMatcher
        Me.Type = Type
        Dim Pattern() As String = TextPattern.Split(PatternSeparater, StringSplitOptions.RemoveEmptyEntries)

        For i As Integer = 0 To UBound(Pattern)
            Dim Sequences As New ElementSequence
            Sequences.Element = Pattern(i).Split(SequenceSeparater, StringSplitOptions.RemoveEmptyEntries)
            Me.Sequences.Add(Sequences)
        Next
    End Sub

    Public Sub New(ByVal Type As GrammarRuleType, ByVal SpecifiedFunction As MatchFunc)
        MatchMethod = MatchMethodType.ViaFunction
        Me.Type = Type
        Me.SpecifiedFunction = SpecifiedFunction
    End Sub
End Class

Public Class ParsedTree
    Enum TreeType
        Branch
        Leave
    End Enum

    Public Type As TreeType
    Public RuleName As String
    Public ElementSequence As List(Of ParsedTree) = Nothing
    Public Value As String = Nothing

    Sub New(ByVal RuleName As String, ByVal ElementSequence As List(Of ParsedTree))
        Type = TreeType.Branch
        Me.RuleName = RuleName
        Me.ElementSequence = ElementSequence
    End Sub

    Sub New(ByVal RuleName As String, ByVal Value As String)
        Type = TreeType.Leave
        Me.Value = Value
        Me.RuleName = RuleName
    End Sub
End Class

