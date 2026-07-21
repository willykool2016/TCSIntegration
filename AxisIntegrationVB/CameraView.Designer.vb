<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CameraView
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        VideoView1 = New LibVLCSharp.WinForms.VideoView()
        btnPlay = New Button()
        CType(VideoView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' VideoView1
        ' 
        VideoView1.BackColor = Color.Black
        VideoView1.Location = New Point(80, 28)
        VideoView1.MediaPlayer = Nothing
        VideoView1.Name = "VideoView1"
        VideoView1.Size = New Size(887, 489)
        VideoView1.TabIndex = 0
        VideoView1.Text = "VideoView1"
        ' 
        ' btnPlay
        ' 
        btnPlay.Location = New Point(350, 540)
        btnPlay.Name = "btnPlay"
        btnPlay.Size = New Size(317, 102)
        btnPlay.TabIndex = 1
        btnPlay.Text = "Play"
        btnPlay.UseVisualStyleBackColor = True
        ' 
        ' CameraView
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1067, 692)
        Controls.Add(btnPlay)
        Controls.Add(VideoView1)
        Margin = New Padding(4, 5, 4, 5)
        Name = "CameraView"
        Text = "Form1"
        CType(VideoView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)

    End Sub

    Friend WithEvents VideoView1 As LibVLCSharp.WinForms.VideoView
    Friend WithEvents btnPlay As Button
End Class
