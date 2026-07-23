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
        TableLayoutPanel1 = New TableLayoutPanel()
        btnMute = New Button()
        btnConnection = New Button()
        CType(VideoView1, ComponentModel.ISupportInitialize).BeginInit()
        TableLayoutPanel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' VideoView1
        ' 
        VideoView1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        VideoView1.BackColor = Color.Black
        TableLayoutPanel1.SetColumnSpan(VideoView1, 2)
        VideoView1.Location = New Point(3, 2)
        VideoView1.Margin = New Padding(3, 2, 3, 2)
        VideoView1.MediaPlayer = Nothing
        VideoView1.Name = "VideoView1"
        VideoView1.Size = New Size(396, 170)
        VideoView1.TabIndex = 0
        VideoView1.Text = "VideoView1"
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 2
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        TableLayoutPanel1.Controls.Add(VideoView1, 0, 0)
        TableLayoutPanel1.Controls.Add(btnMute, 0, 1)
        TableLayoutPanel1.Controls.Add(btnConnection, 1, 1)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 84.05797F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 15.942029F))
        TableLayoutPanel1.Size = New Size(402, 207)
        TableLayoutPanel1.TabIndex = 1
        ' 
        ' btnMute
        ' 
        btnMute.Dock = DockStyle.Fill
        btnMute.Location = New Point(3, 177)
        btnMute.Name = "btnMute"
        btnMute.Size = New Size(195, 27)
        btnMute.TabIndex = 1
        btnMute.Text = "Mute"
        btnMute.UseVisualStyleBackColor = True
        ' 
        ' btnConnection
        ' 
        btnConnection.BackColor = Color.ForestGreen
        btnConnection.Dock = DockStyle.Fill
        btnConnection.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConnection.ForeColor = SystemColors.Control
        btnConnection.Location = New Point(204, 177)
        btnConnection.Name = "btnConnection"
        btnConnection.Size = New Size(195, 27)
        btnConnection.TabIndex = 2
        btnConnection.Text = "Connect"
        btnConnection.UseVisualStyleBackColor = False
        ' 
        ' VideoFeed
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(402, 207)
        Controls.Add(TableLayoutPanel1)
        Margin = New Padding(3, 2, 3, 2)
        Name = "VideoFeed"
        Text = "VideoFeed"
        CType(VideoView1, ComponentModel.ISupportInitialize).EndInit()
        TableLayoutPanel1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents VideoView1 As LibVLCSharp.WinForms.VideoView
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents btnMute As Button
    Friend WithEvents btnConnection As Button




End Class
