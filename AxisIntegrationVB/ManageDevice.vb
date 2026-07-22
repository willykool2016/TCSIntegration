Imports System.IO
Imports System.Text
Imports MySql.Data.MySqlClient
Imports Mysqlx.XDevAPI

Public Class ManageDevice
    'Public Property ParentFormRef As VncForm
    Dim blnDGVUpdate As Boolean
    Dim WithEvents tkbTrack As New TrackBar
    Dim ToolStripProgressBar1 As New ToolStripProgressBar
    Dim frm1 = Application.OpenForms.OfType(Of VncForm).FirstOrDefault()
    Dim intercomView = Application.OpenForms.OfType(Of CameraView).FirstOrDefault()
    Dim notification As New CallNotification()

#Region "Form Routines"

    Private Sub ManageDevices_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Me.MdiParent = Form1
        Try
            Call DGV_Init()
            ToolStripStatusLabel1.Text = ""
            tkbTrack.Minimum = 0
            tkbTrack.Maximum = 60
            tkbTrack.Value = 30
            tkbTrack.TickFrequency = 10
            tkbTrack.AutoSize = False
            tkbTrack.Height = 10
            tkbTrack.TickStyle = TickStyle.None
            StatusStrip1.Items.Add(New ToolStripControlHost(tkbTrack))
            StatusStrip1.Items.Add(ToolStripProgressBar1)
            ToolStripStatusLabel1.Text = "Refresh: " & tkbTrack.Value & " seconds"
            ToolStripProgressBar1.Minimum = 0
            ToolStripProgressBar1.Value = 0
            ToolStripProgressBar1.Maximum = tkbTrack.Value
            'backForm.Show()
        Catch ex As Exception
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error loading the form. " & ex.Message, MsgBoxStyle.Critical, Me.Text)
        End Try
    End Sub

    Private Sub DGV_Init()
    End Sub

#End Region

#Region "Form Controls"
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim strErr As String = ""
        Dim btn As DataGridViewButtonColumn
        'Dim txt As DataGridViewTextBoxColumn
        Dim col As DGVProgressColumn
        SaveOrder()
        Try
            Call DGV_Load("SELECT * FROM vnc_view_schema.list_device
                            WHERE active=true
                            ORDER BY CASE
                            When vnc_order IS NULL THEN 2
                            WHEN vnc_order = 0 THEN 1 
                            ELSE 0 END, 
                            vnc_order;")
            For Each colu As DataGridViewColumn In dgvDevice.Columns
                colu.ReadOnly = True
                If colu.Equals(dgvDevice.Columns("vnc_order")) Then
                    colu.ReadOnly = False
                End If
            Next
            'Add VNC Button and Column after populating table
            btn = New DataGridViewButtonColumn
            btn.HeaderText = "Add VNC"
            btn.Name = "VncViewer"
            dgvDevice.Columns.Add(btn)
            dgvDevice.Columns("VncViewer").Width = 70
            'Add VNC Focus Button and Column after populating table
            btn = New DataGridViewButtonColumn
            btn.HeaderText = "VNC Focus"
            btn.Name = "VncBigscreen"
            dgvDevice.Columns.Add(btn)
            dgvDevice.Columns("VncBigscreen").Width = 70
            'Add Intercom Pullup Button after populating table
            btn = New DataGridViewButtonColumn
            btn.HeaderText = "Intercom View"
            btn.Name = "IntercomView"
            dgvDevice.Columns.Add(btn)
            dgvDevice.Columns("IntercomView").Width = 70
            'Add Zero Button and Column after populating table
            btn = New DataGridViewButtonColumn
            'btn.HeaderCell.
            btn.Name = "Zero"
            dgvDevice.Columns.Add(btn)
            dgvDevice.Columns("Zero").Width = 50
            col = New DGVProgressColumn
            col.HeaderText = "paper_usage"
            col.Name = "progress"
            dgvDevice.Columns.Insert(10, col)
            Dim num As Integer = dgvDevice.RowCount
            For Each row As DataGridViewRow In dgvDevice.Rows
                ' Update progress bar
                If row.Cells("paper_length").Value = 0 Then
                    row.Cells("progress").Value = 0
                ElseIf row.Cells("length").Value = 0 Then
                    row.Cells("progress").Value = 0
                Else
                    row.Cells("progress").Value = (row.Cells("length").Value / row.Cells("paper_length").Value) * 100
                End If
                ' Update printer status
                If row.Cells("printer").Value Then
                    If row.Cells("printer_status").Value = "ONLINE" Then
                        row.Cells("printer_status").Style.BackColor = Color.LightGreen
                    ElseIf row.Cells("printer_status").Value = "TICKET OUT | " Then
                        row.Cells("printer_status").Style.BackColor = Color.Gold
                    ElseIf row.Cells("printer_status").Value = "" Then
                        row.Cells("printer_status").Style.BackColor = Nothing
                    Else
                        row.Cells("printer_status").Style.BackColor = Color.LightCoral
                    End If
                End If
                ' Update heartbeat status
                If row.Cells("last_update").Value IsNot DBNull.Value AndAlso row.Cells("last_update").Value <> "" Then
                    If DateTime.Now.Subtract(row.Cells("last_update").Value).totalhours < 2 Then
                        row.Cells("name").Style.BackColor = Color.LightGreen
                        row.Cells("last_update").Style.BackColor = Color.LightGreen
                    Else
                        row.Cells("name").Style.BackColor = Color.LightCoral
                        row.Cells("last_update").Style.BackColor = Color.LightCoral
                    End If
                End If
            Next
            Timer1.Interval = tkbTrack.Value * 1000
            ToolStripProgressBar1.Value = 0
            ToolStripProgressBar1.Maximum = tkbTrack.Value - 1
        Catch ex As Exception
            Timer1.Interval = 180000
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error loading the dgv. " & ex.Message, MsgBoxStyle.Critical, Me.Text, True)
        End Try
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If Timer1.Enabled And ToolStripProgressBar1.Value < ToolStripProgressBar1.Maximum Then ToolStripProgressBar1.Increment(1)
    End Sub

    Private Async Sub dgvDevice_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvDevice.CellContentClick
        Try
            If dgvDevice.Columns(e.ColumnIndex).Name = "Zero" Then
                If dgvDevice.Rows(e.RowIndex).Cells("printer").Value = "True" Then
                    If MessageBox.Show("Are you sure you want to zero the printer?  This will set the printer count value to zero.", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = System.Windows.Forms.DialogResult.No Then Exit Sub
                    Call Universals.Update_Table_Row("list_device", dgvDevice.Rows(e.RowIndex).Cells("id").Value, "count", "0", "length", "0")
                    Call tsbRefresh.PerformClick()
                End If
            ElseIf dgvDevice.Columns(e.ColumnIndex).Name = "VncViewer" Then
                'MessageBox.Show("This will open a VNC Viewer Window.")
                Await OpenVnc(e.RowIndex, False)
            ElseIf dgvDevice.Columns(e.ColumnIndex).Name = "VncBigscreen" Then
                'MessageBox.Show("This will open a VNC Viewer Window.")
                Await OpenVnc(e.RowIndex, True)
            ElseIf dgvDevice.Columns(e.ColumnIndex).Name = "IntercomView" Then
                Await OpenIntercom(e.RowIndex)
            End If
        Catch ex As Exception
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error Zeroing the printer. " & ex.Message, MsgBoxStyle.Critical, Me.Text)
        End Try
    End Sub

    Private Sub tkbTrack_Scroll(sender As Object, e As EventArgs) Handles tkbTrack.Scroll
        ToolStripStatusLabel1.Text = "Refresh: " & tkbTrack.Value & " seconds"
        If tkbTrack.Value = 0 Then
            Timer1.Enabled = False
        Else
            Timer1.Enabled = True
            Timer1.Interval = tkbTrack.Value * 1000
            ToolStripProgressBar1.Maximum = tkbTrack.Value - 1
            ToolStripProgressBar1.Value = 0
        End If
    End Sub

    Private Sub tsbRefresh_Click(sender As Object, e As EventArgs) Handles tsbRefresh.Click
        Timer1.Interval = 1000
        SaveOrder()
    End Sub

    Private Sub tsbSave_Click(sender As Object, e As EventArgs) Handles tsbSave.Click
        Dim refresh As Boolean = True
        Try
            Timer1.Enabled = False
            If dgvDevice.SelectedRows.Count = 0 Then
                MsgBox("Please select a row", MsgBoxStyle.Information, Me.Name)
            ElseIf IsNothing(dgvDevice.SelectedRows(0).Cells(0).Value) Then
                MsgBox("Please select a row", MsgBoxStyle.Information, Me.Name)
            ElseIf dgvDevice.SelectedRows.Count > 1 Then
                MsgBox("Please select only a single row", MsgBoxStyle.Information, Me.Name)
            ElseIf dgvDevice.SelectedRows(0).Cells("printer").Value = "False" Then
                MsgBox("Please select a row with a printer", MsgBoxStyle.Information, Me.Name)
            Else
                If IsNumeric(tsbPrinted.Text) Then
                    If tsbPrinted.Text <> 0 Then
                        If MessageBox.Show("Are you sure you want to add " & tsbPrinted.Text & " inches to paper_usage?", Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = System.Windows.Forms.DialogResult.No Then Exit Sub
                        Call Universals.Update_Table_Row("list_device", dgvDevice.SelectedRows(0).Cells("id").Value, "count", CDec(dgvDevice.SelectedRows(0).Cells("count").Value) + 1, "length", CDec(dgvDevice.SelectedRows(0).Cells("length").Value) + CDec(tsbPrinted.Text))
                        refresh = False
                        Call tsbRefresh.PerformClick()
                    Else
                        MsgBox("Please enter the amount of inches you want to add to paper_usage", MsgBoxStyle.Information, Me.Name)
                    End If
                Else
                    MsgBox("Please enter the amount of inches you want to add to paper_usage", MsgBoxStyle.Information, Me.Name)
                End If
            End If
        Catch ex As Exception
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error adding to paper usage. " & ex.Message, MsgBoxStyle.Critical, Me.Text)
        Finally
            Timer1.Enabled = True
            If refresh = True Then
                ToolStripProgressBar1.Value = 0
            End If
        End Try
    End Sub

    Private Sub tsbPing_Click(sender As Object, e As EventArgs) Handles tsbPing.Click
        Dim strDest As String
        For Each row As DataGridViewRow In dgvDevice.Rows
            strDest = ""
            If row.Cells("name").Value <> "" Then
                strDest = row.Cells("name").Value
            ElseIf row.Cells("address").Value <> "" Then
                strDest = row.Cells("address").Value
            End If
            If strDest <> "" Then
                If My.Computer.Network.Ping(strDest) Then
                    row.Cells("address").Style.BackColor = Color.LightGreen
                    row.Cells("status").Value = True
                Else
                    row.Cells("address").Style.BackColor = Color.LightCoral
                    row.Cells("status").Value = False
                End If
            End If
        Next
    End Sub

#End Region

#Region "Functions and Routines"

    Private Sub DGV_Load(strSQL As String)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim i As Integer
        blnDGVUpdate = False
        dgvDevice.Rows.Clear()
        dgvDevice.Columns.Clear()
        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                Dim cells(rd.FieldCount) As String
                For i = 0 To rd.FieldCount - 1
                    Select Case rd.GetName(i)
                        Case "status", "active", "printer"
                            Call Globals.Add_DGV_Checkbox(dgvDevice, rd.GetName(i))
                        Case Else
                            dgvDevice.Columns.Add(rd.GetName(i), rd.GetName(i))
                    End Select
                    dgvDevice.Columns(i).ReadOnly = True
                Next
                If rd.HasRows Then
                    While rd.Read()
                        Array.Clear(cells, 0, cells.Length)
                        For i = 0 To rd.FieldCount - 1
                            If rd.IsDBNull(i) Then
                                cells(i) = ""
                            Else
                                cells(i) = rd.GetValue(i)
                            End If
                        Next
                        dgvDevice.Rows.Add(cells)
                    End While
                End If
                rd.Close()
            End Using
            dgvDevice.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)
            dgvDevice.AllowUserToAddRows = False
            dgvDevice.AllowUserToResizeRows = False
            blnDGVUpdate = True
        Catch ex As MySqlException
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, Me.Text)
        Catch ex As Exception
            Call Universals.Error_Messager(Me.Name, System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, Me.Text)
        End Try
    End Sub

#End Region

#Region "Will's Functions and Routines"
    Private Sub SaveOrder()
        Using cn As New MySqlConnection(MYSQLCS)
            cn.Open()
            For id As Integer = 0 To dgvDevice.Rows.Count - 1
                Dim sql As String = "UPDATE vnc_view_schema.list_device SET vnc_order = @vnc_order WHERE ID = @ID"
                Using cmd As New MySqlCommand(sql, cn)
                    Dim value = dgvDevice.Rows(id).Cells("vnc_order").Value
                    If value Is Nothing OrElse IsDBNull(value) OrElse String.IsNullOrWhiteSpace(value.ToString()) Then
                        cmd.Parameters.AddWithValue("@vnc_order", DBNull.Value)
                    Else
                        cmd.Parameters.AddWithValue("@vnc_order", dgvDevice.Rows(id).Cells("vnc_order").Value)
                    End If
                    cmd.Parameters.AddWithValue("@ID", id)
                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                    Console.WriteLine($"{rowsAffected} row(s) updated")
                End Using
            Next
        End Using
    End Sub

    Private Async Sub VncAllButton_ClickAsync(sender As Object, e As EventArgs) Handles VncAllButton.Click
        'Find Highest Order
        Dim zerosList = New List(Of Integer)
        Dim orderList = New List(Of Integer)
        For i As Integer = 0 To dgvDevice.Rows.Count - 1
            SaveOrder()
            If dgvDevice.Rows(i).Cells("vnc_order").Value.Equals("") Then Return
            Await OpenVnc(i, False)
        Next
    End Sub

    Private Async Sub IntercomAllButton_ClickAsync(sender As Object, e As EventArgs) Handles IntercomAllButton.Click
        'Find Highest Order
        Dim zerosList = New List(Of Integer)
        Dim orderList = New List(Of Integer)
        For i As Integer = 0 To dgvDevice.Rows.Count - 1
            SaveOrder()
            If dgvDevice.Rows(i).Cells("vnc_order").Value.Equals("") Then Return
            Await OpenIntercom(i)
        Next
    End Sub
    'This below is part of the Add VNC function, and VNC view, and VNC All
    Private Async Function OpenVnc(rowIndex As Integer, openOwn As Boolean) As Task
        Dim curAddress = dgvDevice.Rows(rowIndex).Cells("address").Value.ToString()
        If openOwn Then
            Dim frm2 As New VncForm
            frm2.Show()
            Await frm2.CreateConnection(dgvDevice.Rows(rowIndex).Cells("address").Value.ToString(), dgvDevice.Rows(rowIndex).Cells("vnc_password").Value.ToString())
            frm2.BringToFront()
            frm2.Focus()
            Return
        End If
        If frm1 Is Nothing OrElse frm1.IsDisposed Then
            frm1 = New VncForm()
            frm1.Show()
        End If
        Await frm1.CreateConnection(dgvDevice.Rows(rowIndex).Cells("address").Value.ToString(), dgvDevice.Rows(rowIndex).Cells("vnc_password").Value.ToString())
        frm1.BringToFront()
        frm1.Focus()
    End Function

    Private Async Function OpenIntercom(rowIndex As Integer) As Task
        Dim camAddress = dgvDevice.Rows(rowIndex).Cells("intercom_address").Value.ToString()
        Console.WriteLine($"Intercom Address: {camAddress}")
        If intercomView Is Nothing OrElse intercomView.IsDisposed Then
            intercomView = New CameraView
            intercomView.Show()
        End If
        'intercomView.Axis_Init(camAddress)
        intercomView.Video_Init()
        intercomView.BringToFront()
        intercomView.Focus()
    End Function

    Private Sub ManageDevice_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        SaveOrder()
    End Sub
#End Region
#Region "Pipeline Requests (Will & Glade)"
    Private Sub ShowCallNotification()

        Dim notification As New CallNotification()

        AddHandler notification.AnswerRequested,
        Sub()
            MessageBox.Show("Answer requested")
        End Sub

        AddHandler notification.HangupRequested,
        Sub()
            MessageBox.Show("Hangup requested")
        End Sub

        notification.Show()

    End Sub
#End Region
End Class