<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ManageDevice
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
        components = New ComponentModel.Container()
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ManageDevice))
        dgvDevice = New DataGridView()
        Timer1 = New Timer(components)
        TableLayoutPanel1 = New TableLayoutPanel()
        ToolStrip1 = New ToolStrip()
        tsbRefresh = New ToolStripButton()
        ToolStripSeparator1 = New ToolStripSeparator()
        tsbPing = New ToolStripButton()
        ToolStripSeparator3 = New ToolStripSeparator()
        VncAllButton = New ToolStripButton()
        ToolStripSeparator4 = New ToolStripSeparator()
        IntercomAllButton = New ToolStripButton()
        ToolStripSeparator5 = New ToolStripSeparator()
        ToolStripLabel1 = New ToolStripLabel()
        tsbPrinted = New ToolStripTextBox()
        tsbSave = New ToolStripButton()
        ToolStripSeparator2 = New ToolStripSeparator()
        StatusStrip1 = New StatusStrip()
        ToolStripStatusLabel2 = New ToolStripStatusLabel()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        Timer2 = New Timer(components)
        CType(dgvDevice, ComponentModel.ISupportInitialize).BeginInit()
        TableLayoutPanel1.SuspendLayout()
        ToolStrip1.SuspendLayout()
        StatusStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' dgvDevice
        ' 
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = SystemColors.Control
        DataGridViewCellStyle1.Font = New Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle1.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvDevice.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvDevice.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = SystemColors.Window
        DataGridViewCellStyle2.Font = New Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle2.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvDevice.DefaultCellStyle = DataGridViewCellStyle2
        dgvDevice.Dock = DockStyle.Fill
        dgvDevice.Location = New Point(4, 5)
        dgvDevice.Margin = New Padding(4, 5, 4, 5)
        dgvDevice.Name = "dgvDevice"
        dgvDevice.RowHeadersWidth = 51
        dgvDevice.Size = New Size(1608, 720)
        dgvDevice.TabIndex = 0
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 1000
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 1
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.Controls.Add(dgvDevice, 0, 0)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 39)
        TableLayoutPanel1.Margin = New Padding(4, 5, 4, 5)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 702F))
        TableLayoutPanel1.Size = New Size(1616, 730)
        TableLayoutPanel1.TabIndex = 1
        ' 
        ' ToolStrip1
        ' 
        ToolStrip1.ImageScalingSize = New Size(32, 32)
        ToolStrip1.Items.AddRange(New ToolStripItem() {tsbRefresh, ToolStripSeparator1, tsbPing, ToolStripSeparator3, VncAllButton, ToolStripSeparator4, IntercomAllButton, ToolStripSeparator5, ToolStripLabel1, tsbPrinted, tsbSave, ToolStripSeparator2})
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.Size = New Size(1616, 39)
        ToolStrip1.TabIndex = 2
        ToolStrip1.Text = "ToolStrip1"
        ' 
        ' tsbRefresh
        ' 
        tsbRefresh.AutoSize = False
        tsbRefresh.ImageTransparentColor = Color.Magenta
        tsbRefresh.Name = "tsbRefresh"
        tsbRefresh.Size = New Size(100, 36)
        tsbRefresh.Text = "Refresh"
        tsbRefresh.ToolTipText = "Reload the device information"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(6, 39)
        ' 
        ' tsbPing
        ' 
        tsbPing.AutoSize = False
        tsbPing.ImageTransparentColor = Color.Magenta
        tsbPing.Name = "tsbPing"
        tsbPing.Size = New Size(100, 36)
        tsbPing.Text = "Ping"
        tsbPing.ToolTipText = "Get device status"
        ' 
        ' ToolStripSeparator3
        ' 
        ToolStripSeparator3.Name = "ToolStripSeparator3"
        ToolStripSeparator3.Size = New Size(6, 39)
        ' 
        ' VncAllButton
        ' 
        VncAllButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        VncAllButton.Image = CType(resources.GetObject("VncAllButton.Image"), Image)
        VncAllButton.ImageTransparentColor = Color.Magenta
        VncAllButton.Name = "VncAllButton"
        VncAllButton.Size = New Size(64, 36)
        VncAllButton.Text = "VNC All"
        ' 
        ' ToolStripSeparator4
        ' 
        ToolStripSeparator4.Name = "ToolStripSeparator4"
        ToolStripSeparator4.Size = New Size(6, 39)
        ' 
        ' IntercomAllButton
        ' 
        IntercomAllButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        IntercomAllButton.Image = CType(resources.GetObject("IntercomAllButton.Image"), Image)
        IntercomAllButton.ImageTransparentColor = Color.Magenta
        IntercomAllButton.Name = "IntercomAllButton"
        IntercomAllButton.Size = New Size(94, 36)
        IntercomAllButton.Text = "Intercom All"
        ' 
        ' ToolStripSeparator5
        ' 
        ToolStripSeparator5.Name = "ToolStripSeparator5"
        ToolStripSeparator5.Size = New Size(6, 39)
        ' 
        ' ToolStripLabel1
        ' 
        ToolStripLabel1.Name = "ToolStripLabel1"
        ToolStripLabel1.Size = New Size(141, 36)
        ToolStripLabel1.Text = "Add length (inches):"
        ' 
        ' tsbPrinted
        ' 
        tsbPrinted.Name = "tsbPrinted"
        tsbPrinted.Size = New Size(132, 39)
        ' 
        ' tsbSave
        ' 
        tsbSave.DisplayStyle = ToolStripItemDisplayStyle.Image
        tsbSave.ImageAlign = ContentAlignment.MiddleLeft
        tsbSave.ImageTransparentColor = Color.Magenta
        tsbSave.Name = "tsbSave"
        tsbSave.Size = New Size(29, 36)
        tsbSave.ToolTipText = "Save changes"
        ' 
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        ToolStripSeparator2.Size = New Size(6, 39)
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.ImageScalingSize = New Size(20, 20)
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel2, ToolStripStatusLabel1})
        StatusStrip1.Location = New Point(0, 769)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Padding = New Padding(1, 0, 19, 0)
        StatusStrip1.Size = New Size(1616, 26)
        StatusStrip1.TabIndex = 3
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripStatusLabel2
        ' 
        ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        ToolStripStatusLabel2.Size = New Size(1443, 20)
        ToolStripStatusLabel2.Spring = True
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(153, 20)
        ToolStripStatusLabel1.Text = "ToolStripStatusLabel1"
        ' 
        ' Timer2
        ' 
        Timer2.Enabled = True
        Timer2.Interval = 1000
        ' 
        ' ManageDevice
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1616, 795)
        Controls.Add(TableLayoutPanel1)
        Controls.Add(ToolStrip1)
        Controls.Add(StatusStrip1)
        Margin = New Padding(4, 5, 4, 5)
        Name = "ManageDevice"
        Text = "Kiosk Management"
        CType(dgvDevice, ComponentModel.ISupportInitialize).EndInit()
        TableLayoutPanel1.ResumeLayout(False)
        ToolStrip1.ResumeLayout(False)
        ToolStrip1.PerformLayout()
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub
    Friend WithEvents dgvDevice As System.Windows.Forms.DataGridView
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents tsbSave As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents tsbRefresh As ToolStripButton
    Friend WithEvents tsbPrinted As ToolStripTextBox
    Friend WithEvents tsbPing As ToolStripButton
    Friend WithEvents ToolStripSeparator3 As ToolStripSeparator
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripStatusLabel2 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents Timer2 As Timer
    Friend WithEvents VncAllButton As ToolStripButton
    Friend WithEvents ToolStripSeparator4 As ToolStripSeparator
    Friend WithEvents IntercomAllButton As ToolStripButton
    Friend WithEvents ToolStripSeparator5 As ToolStripSeparator
End Class

