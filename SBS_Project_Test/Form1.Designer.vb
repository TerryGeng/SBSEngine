﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.DebugText = New System.Windows.Forms.TextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(59, 12)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Code Area"
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(12, 24)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(433, 175)
        Me.TextBox1.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(10, 244)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(71, 12)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Runtime Log"
        '
        'DebugText
        '
        Me.DebugText.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.DebugText.ForeColor = System.Drawing.Color.Lime
        Me.DebugText.Location = New System.Drawing.Point(12, 259)
        Me.DebugText.Multiline = True
        Me.DebugText.Name = "DebugText"
        Me.DebugText.ReadOnly = True
        Me.DebugText.Size = New System.Drawing.Size(433, 124)
        Me.DebugText.TabIndex = 3
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(345, 209)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(99, 24)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "Execute"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(457, 395)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.DebugText)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents DebugText As System.Windows.Forms.TextBox
    Friend WithEvents Button1 As System.Windows.Forms.Button

End Class
