<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VideoFeed
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
        VideoView1 = New LibVLCSharp.WinForms.VideoView()
        btnPlay = New Button()
        CType(VideoView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' VideoView1
        ' 
        VideoView1.BackColor = Color.Black
        VideoView1.Location = New Point(14, 12)
        VideoView1.MediaPlayer = Nothing
        VideoView1.Name = "VideoView1"
        VideoView1.Size = New Size(655, 378)
        VideoView1.TabIndex = 0
        VideoView1.Text = "VideoView1"
        ' 
        ' btnPlay
        ' 
        btnPlay.Location = New Point(256, 396)
        btnPlay.Name = "btnPlay"
        btnPlay.Size = New Size(195, 51)
        btnPlay.TabIndex = 1
        btnPlay.Text = "Play"
        btnPlay.UseVisualStyleBackColor = True
        ' 
        ' VideoFeed
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(685, 450)
        Controls.Add(btnPlay)
        Controls.Add(VideoView1)
        Name = "VideoFeed"
        Text = "VideoFeed"
        CType(VideoView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents VideoView1 As LibVLCSharp.WinForms.VideoView
    Friend WithEvents btnPlay As Button
End Class
