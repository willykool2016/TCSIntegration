<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CallNotification
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        IncomingLabel = New Label()
        AnswerButton = New Button()
        DeclineButton = New Button()
        SuspendLayout()
        ' 
        ' IncomingLabel
        ' 
        IncomingLabel.Anchor = AnchorStyles.None
        IncomingLabel.Font = New Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        IncomingLabel.ForeColor = Color.Firebrick
        IncomingLabel.Location = New Point(12, 9)
        IncomingLabel.Name = "IncomingLabel"
        IncomingLabel.Size = New Size(401, 85)
        IncomingLabel.TabIndex = 0
        IncomingLabel.Text = "Incoming Call From: "
        IncomingLabel.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' AnswerButton
        ' 
        AnswerButton.BackColor = SystemColors.ActiveCaption
        AnswerButton.Font = New Font("Segoe UI Semibold", 14.25F, FontStyle.Bold)
        AnswerButton.Location = New Point(12, 97)
        AnswerButton.Name = "AnswerButton"
        AnswerButton.Size = New Size(152, 107)
        AnswerButton.TabIndex = 1
        AnswerButton.Text = "Answer"
        AnswerButton.UseVisualStyleBackColor = False
        ' 
        ' DeclineButton
        ' 
        DeclineButton.Font = New Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        DeclineButton.Location = New Point(261, 97)
        DeclineButton.Name = "DeclineButton"
        DeclineButton.Size = New Size(152, 107)
        DeclineButton.TabIndex = 2
        DeclineButton.Text = "Decline"
        DeclineButton.UseVisualStyleBackColor = True
        ' 
        ' CallNotification
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(425, 216)
        Controls.Add(DeclineButton)
        Controls.Add(AnswerButton)
        Controls.Add(IncomingLabel)
        Name = "CallNotification"
        Text = "CallNotification"
        ResumeLayout(False)
    End Sub

    Friend WithEvents IncomingLabel As Label
    Friend WithEvents AnswerButton As Button
    Friend WithEvents DeclineButton As Button
End Class
