<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TestForm
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
        Dim CodeAreaLabel As System.Windows.Forms.Label
        Dim RuntimeLogLabel As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TestForm))
        Me.CodeArea = New System.Windows.Forms.TextBox()
        Me.DebugText = New System.Windows.Forms.TextBox()
        Me.Input = New System.Windows.Forms.TextBox()
        CodeAreaLabel = New System.Windows.Forms.Label()
        RuntimeLogLabel = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'CodeAreaLabel
        '
        CodeAreaLabel.AutoSize = True
        CodeAreaLabel.Location = New System.Drawing.Point(12, 9)
        CodeAreaLabel.Name = "CodeAreaLabel"
        CodeAreaLabel.Size = New System.Drawing.Size(59, 12)
        CodeAreaLabel.TabIndex = 0
        CodeAreaLabel.Text = "Code Area"
        '
        'CodeArea
        '
        Me.CodeArea.Font = New System.Drawing.Font("Consolas", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CodeArea.Location = New System.Drawing.Point(12, 24)
        Me.CodeArea.Multiline = True
        Me.CodeArea.Name = "CodeArea"
        Me.CodeArea.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.CodeArea.Size = New System.Drawing.Size(433, 392)
        Me.CodeArea.TabIndex = 1
        '
        'RuntimeLogLabel
        '
        RuntimeLogLabel.AutoSize = True
        RuntimeLogLabel.Location = New System.Drawing.Point(460, 9)
        RuntimeLogLabel.Name = "RuntimeLogLabel"
        RuntimeLogLabel.Size = New System.Drawing.Size(71, 12)
        RuntimeLogLabel.TabIndex = 2
        RuntimeLogLabel.Text = "Runtime Log"
        '
        'DebugText
        '
        Me.DebugText.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.DebugText.Font = New System.Drawing.Font("Consolas", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.DebugText.ForeColor = System.Drawing.Color.Lime
        Me.DebugText.Location = New System.Drawing.Point(462, 24)
        Me.DebugText.Multiline = True
        Me.DebugText.Name = "DebugText"
        Me.DebugText.ReadOnly = True
        Me.DebugText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.DebugText.Size = New System.Drawing.Size(433, 362)
        Me.DebugText.TabIndex = 3
        '
        'Input
        '
        Me.Input.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.Input.Font = New System.Drawing.Font("Consolas", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Input.ForeColor = System.Drawing.Color.Cyan
        Me.Input.Location = New System.Drawing.Point(462, 392)
        Me.Input.Name = "Input"
        Me.Input.Size = New System.Drawing.Size(433, 24)
        Me.Input.TabIndex = 6
        '
        'TestForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(907, 431)
        Me.Controls.Add(Me.Input)
        Me.Controls.Add(Me.DebugText)
        Me.Controls.Add(RuntimeLogLabel)
        Me.Controls.Add(Me.CodeArea)
        Me.Controls.Add(CodeAreaLabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "TestForm"
        Me.Text = "SBS Tester"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents CodeArea As System.Windows.Forms.TextBox
    Friend WithEvents DebugText As System.Windows.Forms.TextBox
    Friend WithEvents Input As System.Windows.Forms.TextBox

End Class
