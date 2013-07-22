Public Class SBSPerform

    Const Version As String = "0.1"

    Declare Function GetTickCount Lib "kernel32" () As Long

    Public Class PerformerPtr
        Delegate Function StatPerformer(ByRef statment As CodeSequence, ByRef runtimeData As RuntimeData)

        Public RuleName As String
        Public Performer As StatPerformer

        Sub New(ByVal name As String, ByRef perfFunc As StatPerformer)
            RuleName = name
            Performer = perfFunc
        End Sub

    End Class

    Dim Performers As ArrayList
    Dim RuntimeData As RuntimeData


    Sub New(ByRef statList As ArrayList)
        Debug_Output("SBS Perform - Version " + Version + " - Time " + CStr(GetTickCount()))
        Debug_Output("-----------------")

        If statList IsNot Nothing And statList(0).GetType().Equals(GetType(CodeSequence)) Then
            RuntimeData = New RuntimeData(statList)
            LoadPerformers()
            Debug_Output("Statments loaded.")
        Else
            Debug_Error("Load Error: Invalid argument type.")
        End If
    End Sub

    Public Function Run() As Integer
        Dim start_time As Long = GetTickCount()
        Debug_Output("Running start at " + CStr(start_time) + ".")

        For i As Integer = 0 To RuntimeData.Statments.Count - 1
            Dim statment As CodeSequence = RuntimeData.Statments(i)
            Dim return_val As ReturnValue = PerformStatment(statment.SeqsList(0))
            Debug_Output("[" + CStr(i) + "]Return Val: (Type:" + return_val.Type + ")" + CStr(return_val.Value))
        Next

        Debug_Output("Running end at " + CStr(GetTickCount()) + ". Total cost " + CStr(GetTickCount() - start_time) + "ms.")

        Return 0
    End Function

    Public Function PerformStatment(ByVal statment As CodeSequence)
        Dim performer As PerformerPtr = GetPerformer(statment.RuleName)
        Return performer.Performer(statment, RuntimeData)
    End Function

    Function GetPerformer(ByVal ruleName As String) As PerformerPtr
        For i As Integer = 0 To Performers.Count - 1
            If Performers(i).RuleName = ruleName Then
                Return Performers(i)
            End If
        Next

        Debug_Error("Performs Error: Unknow performer '" + ruleName + "'.")
        Return Nothing
    End Function

    '-----------------
    Sub LoadPerformers()
        Performers = New ArrayList()
        Performers.Add(New PerformerPtr("EXPRESSION", AddressOf ExpressionPerformer))
    End Sub

    Function ExpressionPerformer(ByRef statment As CodeSequence, ByRef runtimeData As RuntimeData)
        Dim seqsList As ArrayList = statment.SeqsList
        Dim exprtype As String = ""
        Dim exprValueNum As Double = 0
        Dim exprValueStr As String = ""

        Dim firstSeq As CodeSequence = seqsList(0)


        If firstSeq.RuleName = "EXP_ELEMENT" Then
            Dim eleType As String = firstSeq.SeqsList(0).RuleName
            Dim eleValue As String = firstSeq.SeqsList(0).Value
            If eleValue = "(" Then
                Dim result As ReturnValue = ExpressionPerformer(firstSeq.SeqsList(1), runtimeData)
                If result.Type = "NUMBER" Then
                    eleType = "NUMBER"
                ElseIf result.Type = "STRING" Then
                    eleType = "STRING"
                End If
                eleValue = result.Value
            End If
            exprtype = eleType
            If eleType = "NUMBER" Then
                exprValueNum = CDbl(eleValue)
            ElseIf eleType = "STRING" Then
                exprValueStr = eleValue
            End If
        End If
        If seqsList.Count > 1 Then
            Dim eleList As ArrayList = seqsList(1).SeqsList

            For i As Integer = 0 To eleList.Count - 1
                Dim curOp As String = eleList(i).SeqsList(0).SeqsList(0).Value
                Dim curEle As CodeSequence = eleList(i).SeqsList(1).SeqsList(0)
                Dim curType As String = curEle.RuleName
                Dim curValue As String = curEle.Value

                If curValue = "(" Then
                    curEle = eleList(i).SeqsList(1).SeqsList(1)
                    Dim result As ReturnValue = ExpressionPerformer(curEle, runtimeData)
                    If result.Type = "NUMBER" Then
                        curType = "NUMBER"
                    ElseIf result.Type = "STRING" Then
                        curType = "STRING"
                    End If
                    curValue = result.Value
                End If

                If curOp <> "*" Or curOp <> "/" Then
                    If curType = "NUMBER" Then
                        If exprtype = "NUMBER" Then
                            If curOp = "+" Then
                                exprValueNum += CDbl(curValue)
                            Else
                                Debug_Error("Runtime Error: Used undefine operator '" + curOp + "' between '" + exprtype + "' and '" + curType + "'.")
                            End If
                        ElseIf exprtype = "STRING" Then
                            If curOp = "+" Then
                                exprValueStr += curValue
                            Else
                                Debug_Error("Runtime Error: Used undefine operator '" + curOp + "' between '" + exprtype + "' and '" + curType + "'.")
                            End If
                        End If
                    End If
                End If
            Next
        End If

        If exprtype = "NUMBER" Then
            Return New ReturnValue(exprtype, exprValueNum)
        ElseIf exprtype = "STRING" Then
            Return New ReturnValue(exprtype, exprValueStr)
        End If

        Return Nothing

    End Function
    '-----------------

    Sub Debug_Output(ByVal msg As String, Optional ByVal able_not_show As Boolean = False)
        Form1.DebugText.AppendText(msg + vbCrLf)
    End Sub

    Sub Debug_Error(ByVal msg As String)
        Debug_Output(msg + vbCrLf)
    End Sub
End Class


Public Class RuntimeData
    Public Statments As ArrayList

    Sub New(ByRef stat As ArrayList)
        Statments = stat
    End Sub

End Class

Public Class ReturnValue
    Public Type As String = ""
    Public nValue As Double
    Public sValue As String = ""
    Sub New(ByVal _type As String, ByVal _value As String)
        Type = _type
        sValue = _value
    End Sub
    Sub New(ByVal _type As String, ByVal _value As Double)
        Type = _type
        nValue = _value
    End Sub

    Public Function Value()
        If Type = "STRING" Then
            Return sValue
        ElseIf Type = "NUMBER" Then
            Return nValue
        End If

        Return Nothing
    End Function
End Class