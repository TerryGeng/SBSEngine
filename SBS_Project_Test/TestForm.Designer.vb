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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(TestForm))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CodeArea = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.DebugText = New System.Windows.Forms.TextBox()
        Me.Input = New System.Windows.Forms.TextBox()
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
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(460, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(71, 12)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Runtime Log"
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
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.CodeArea)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "SBS Tester"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CodeArea As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents DebugText As System.Windows.Forms.TextBox
    Friend WithEvents Input As System.Windows.Forms.TextBox

End Class
