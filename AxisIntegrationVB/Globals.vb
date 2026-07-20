Imports System.IO
Imports System.Text
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports MySql.Data.MySqlClient
Imports ThoughtWorks.QRCode.Codec
Imports Newtonsoft.Json
Imports RemoteViewing.Vnc
Imports RemoteViewing.Windows.Forms
Imports System.Data

Module Globals
    Public Const STATUS_STRIP_UNITS As String = "* All displayed weights are in lbs, and all times are in minutes unless otherwise noted."
    Public DGV_UPDATE_COLOR As Color = Color.LightBlue
    Public BTN_BACK_COLOR As Color = Color.Red
    Public BTN_FORE_COLOR As Color = Color.White
    Public LBL_FORE_COLOR As Color = Color.Black
    Public GRP_FORE_COLOR As Color = Color.Red
    Public MAX_SPOTS As Integer
    Public TRACK_COUNT As Integer = 13
    Public TRACK_NAME() As String
    Public TRACK_SPOT() As Integer

#Region "Form Routines"

    Public Sub Load_Combo(strSQL As String, ByRef cmb As ComboBox, field_id As String, field_name As String, Optional addBlank As Boolean = False)
        Dim cm As MySqlCommand

        Try
            cmb.DataSource = Nothing

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)

                Using da As New MySqlDataAdapter(cm)
                    Dim dt As New DataTable()
                    da.Fill(dt)

                    If addBlank Then
                        Dim row As DataRow = dt.NewRow()
                        row(0) = 0
                        row(1) = ""
                        dt.Rows.InsertAt(row, 0)
                    End If

                    cmb.DataSource = dt
                    cmb.DisplayMember = field_name
                    cmb.ValueMember = field_id
                End Using
            End Using

        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Sub Load_Combo(strSQL As String, ByRef cmb As ComboBox, field As String, Optional addBlank As Boolean = False)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Try
            cmb.Items.Clear()
            If addBlank Then cmb.Items.Add("")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read
                        cmb.Items.Add(rd.GetString(field))
                    End While
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Sub Load_Combo(strSQL As String, ByRef cmb As ToolStripComboBox, field As String, Optional addBlank As Boolean = False)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Try
            cmb.Items.Clear()
            If addBlank Then cmb.Items.Add("")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read
                        cmb.Items.Add(rd.GetString(field))
                    End While
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Sub Load_Combo_Filter(cmb As ComboBox, tableName As String, Optional addBlank As Boolean = False)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Try
            cmb.Items.Clear()
            If addBlank Then cmb.Items.Add("")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM " & tableName & " LIMIT 1;", cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    For i = 0 To rd.FieldCount - 1
                        cmb.Items.Add(rd.GetName(i))
                    Next
                    cmb.SelectedIndex = 1
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Sub Load_Combo_Filter(cmb As ToolStripComboBox, tableName As String, Optional addBlank As Boolean = False)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Try
            cmb.Items.Clear()
            If addBlank Then cmb.Items.Add("")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM " & tableName & " LIMIT 1;", cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    For i = 0 To rd.FieldCount - 1
                        cmb.Items.Add(rd.GetName(i))
                    Next
                    cmb.SelectedIndex = 1
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

#End Region

#Region "DGV Routines"

    Public Sub DGV_Add_Column_Combo(ByRef DGV As DataGridView, dgv_col_name As String, sql_col_name As String, strSQL As String, Optional addBlank As Boolean = False, Optional comboItem01 As String = "")
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim cmb As DataGridViewComboBoxColumn
        cmb = New DataGridViewComboBoxColumn
        cmb.HeaderText = dgv_col_name
        cmb.Name = dgv_col_name
        Using cn2 As New MySqlConnection(MYSQLCS)
            cm = New MySqlCommand(strSQL, cn2)
            cn2.Open()
            rd = cm.ExecuteReader
            If rd.HasRows Then
                If addBlank Then cmb.Items.Add("")
                While rd.Read()
                    cmb.Items.Add(rd.GetString(sql_col_name))
                End While
                If comboItem01 <> "" Then cmb.Items.Add(comboItem01)
            End If
            rd.Close()
        End Using
        DGV.Columns.Add(cmb)
    End Sub

    Public Sub Add_DGV_Checkbox(ByRef DGV As DataGridView, col_name As String)
        Dim chk As DataGridViewCheckBoxColumn
        chk = New DataGridViewCheckBoxColumn
        chk.HeaderText = col_name
        chk.Name = col_name
        DGV.Columns.Add(chk)
    End Sub

    Public Sub DGV_Del_Row(ByRef DGV As DataGridView)
        If DGV.SelectedRows.Count = 0 Then
            MsgBox("Please select a row", MsgBoxStyle.Information, "DataGridView Error")
        ElseIf IsNothing(DGV.SelectedRows(0).Cells(0).Value) Then
            MsgBox("Please select a row", MsgBoxStyle.Information, "DataGridView Error")
        Else
            For Each row As DataGridViewRow In DGV.SelectedRows
                DGV.Rows.Remove(DGV.SelectedRows(0))
            Next
        End If
    End Sub
#End Region

#Region "Get DB Functions"

    Public Function Get_Sum_Field(strSQL As String) As Integer
        Dim cm As New MySqlCommand
        Dim o As Object
        Dim result As Integer = 0
        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm.CommandText = strSQL
                cm.CommandType = CommandType.Text
                cm.Connection = cn
                cn.Open()
                o = cm.ExecuteScalar
            End Using
            If o Is Nothing Then
                Return 0
            Else
                Return Val(o.ToString)
            End If
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return 0
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return 0
        End Try
    End Function

    Public Function Get_Truck_Current_Weight(truckID As Integer) As Integer
        Dim cm As New MySqlCommand
        Dim o As Object
        Dim result As Integer = 0
        Dim strSQL As String
        Dim intWeightTicketLoad As Integer
        Dim intWeightStorageLoad As Integer
        Try
            ' Calculate truck load net weight
            strSQL = "SELECT SUM(net_weight) FROM ticket_load WHERE truck_id=" & truckID & ";"
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                o = cm.ExecuteScalar
            End Using
            If o Is Nothing Then intWeightTicketLoad = 0 Else intWeightTicketLoad = Val(o.ToString)
            ' Calculate truck transfer net weight
            strSQL = "SELECT SUM(net_weight) FROM storage_load WHERE truck_id=" & truckID & ";"
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                o = cm.ExecuteScalar
            End Using
            If o Is Nothing Then intWeightStorageLoad = 0 Else intWeightStorageLoad = Val(o.ToString)
            Return intWeightTicketLoad - intWeightStorageLoad
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return intWeightTicketLoad - intWeightStorageLoad
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return intWeightTicketLoad - intWeightStorageLoad
        End Try
    End Function

    Public Function Get_SQL_Lane_Query(laneNumber As Integer) As String
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim result As String = ""
        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT name FROM storage WHERE active=true AND type='SILO' AND track=" & laneNumber & ";", cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    result &= " OR UPPER(assign)='" & rd.GetString("name").ToString.ToUpper & "'"
                End If
                rd.Close()
            End Using
            Return result
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        End Try
    End Function

    Public Sub Get_Mat_Silo(ByRef cmb As ComboBox, product_name As String, lane As Integer)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim sql As StringBuilder
        Dim group_name As String
        cmb.Items.Clear()
        If product_name = "" Then Exit Sub
        cmb.Items.Add("")
        sql = New StringBuilder
        sql.Append("SELECT grp.name")
        sql.Append(" FROM product as prod")
        sql.Append(" LEFT JOIN product AS grp ON prod.group_id = grp.id")
        sql.Append(" WHERE prod.active=true AND prod.name='" & product_name & "';")
        group_name = Universals.Get_SQL_Value(sql.ToString)
        Try
            sql = New StringBuilder
            sql.Append("SELECT id, name, product_name, track from storage WHERE active=true AND type='SILO' AND (product_name='" & product_name & "' OR product_name='" & group_name & "') ORDER BY id;")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read
                        If lane = 0 Then
                            cmb.Items.Add(rd.GetString("name"))
                        ElseIf lane = rd.GetInt32("track") Then
                            cmb.Items.Add(rd.GetString("name"))
                        End If
                    End While
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the filter combo box. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Function Read_HH_Scan()
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim strSQL As String
        Dim result As String = ""
        strSQL = "SELECT name FROM railcar_scan WHERE active=true ORDER BY created_time DESC;"
        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    result = rd.GetString("name")
                End If
                rd.Close()
            End Using
            Return result
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured reading handheld records. " & ex.Message, MsgBoxStyle.Critical, "Globals", True)
            Return ""
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured reading handheld records. " & ex.Message, MsgBoxStyle.Critical, "Globals", True)
            Return ""
        End Try
    End Function

    Public Function Get_Ticket_Info(db_id As Integer) As String
        Dim s As New StringBuilder
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim sql As New StringBuilder
        Try
            sql.Append("SELECT bol.name, bol.truck_number, bol.trailer_1_number, bol.trailer_2_number, bol.driver_name, bol.bol_number,")
            sql.Append(" bol.target_weight, bol.tare_weight, bol.gross_weight,")
            sql.Append(" bol.entry_time, bol.tare_time, bol.scan_time,")
            sql.Append(" soh.so_number, soh.po_number, soh.well_site, soh.well_name")
            sql.Append(" FROM ticket AS bol")
            sql.Append(" LEFT JOIN so_header AS soh ON bol.so_id=soh.id")
            sql.Append(" WHERE bol.id=" & db_id & ";")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        s.Append("Ticket: " & rd.GetString("name"))
                        s.Append(vbCr & "SO: ") : If Not rd.IsDBNull(rd.GetOrdinal("so_number")) Then s.Append(rd.GetString("so_number"))
                        s.Append(vbCr & "PO: ") : If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then s.Append(rd.GetString("po_number"))
                        s.Append(vbCr & "Well Site: ") : If Not rd.IsDBNull(rd.GetOrdinal("well_site")) Then s.Append(rd.GetString("well_site"))
                        s.Append(vbCr & "Well Name: ") : If Not rd.IsDBNull(rd.GetOrdinal("well_name")) Then s.Append(rd.GetString("well_name"))
                        s.Append(vbCr & "Truck: ") : If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then s.Append(rd.GetString("truck_number"))
                        s.Append(vbCr & "Trailer/Box 1: ") : If Not rd.IsDBNull(rd.GetOrdinal("trailer_1_number")) Then s.Append(rd.GetString("trailer_1_number"))
                        s.Append(vbCr & "Trailer/Box 2: ") : If Not rd.IsDBNull(rd.GetOrdinal("trailer_2_number")) Then s.Append(rd.GetString("trailer_2_number"))
                        s.Append(vbCr & "Driver: ") : If Not rd.IsDBNull(rd.GetOrdinal("driver_name")) Then s.Append(rd.GetString("driver_name"))
                        s.Append(vbCr & "BOL: ") : If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then s.Append(rd.GetString("bol_number"))
                        s.Append(vbCr & "Entry Time: ") : If Not rd.IsDBNull(rd.GetOrdinal("entry_time")) Then s.Append(rd.GetString("entry_time"))
                        s.Append(vbCr & "Scan Time: ") : If Not rd.IsDBNull(rd.GetOrdinal("scan_time")) Then s.Append(rd.GetString("scan_time"))
                        s.Append(vbCr & "Tare Time: ") : If Not rd.IsDBNull(rd.GetOrdinal("tare_time")) Then s.Append(rd.GetString("tare_time"))
                        s.Append(vbCr & "Tare Weight: " & rd.GetInt32("tare_weight").ToString("N0"))
                    End While
                End If
                rd.Close()
            End Using
            Return s.ToString
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Get_Lane_Info(lane As Integer) As String
        Dim s As New StringBuilder
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim sql As New StringBuilder
        Dim strSilo As String
        Dim strProd As String
        Dim strCust As String
        Try
            sql.Append("SELECT silo.id, silo.name AS silo_name, silo.spot, silo.product_name AS prod_name, silo.current_weight, cust.name AS cust_name")
            sql.Append(" FROM storage AS silo")
            sql.Append(" LEFT JOIN customer AS cust ON silo.customer_id = cust.id")
            sql.Append(" WHERE silo.active=true AND type='SILO'")
            If lane = 0 Then
                sql.Append(";")
            Else
                sql.Append(" AND silo.track=" & lane & ";")
            End If
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        If rd.IsDBNull(rd.GetOrdinal("silo_name")) Then strSilo = "" Else strSilo = rd.GetString("silo_name")
                        If rd.IsDBNull(rd.GetOrdinal("prod_name")) Then strProd = "" Else strProd = rd.GetString("prod_name")
                        If rd.IsDBNull(rd.GetOrdinal("cust_name")) Then strCust = "" Else strCust = rd.GetString("cust_name")
                        s.Append(strSilo & vbTab)
                        s.Append(Strings.Left(strProd, 15) & vbTab)
                        s.Append(Strings.Left(strCust, 20) & vbTab)
                        's.Append(Val(plc.TagGet("SILO_INV_" & rd.GetInt32("spot"))).ToString("N0"))
                        s.Append(vbCr)
                    End While
                End If
                rd.Close()
            End Using
            Return s.ToString
        Catch ex As Exception
            'Form1.ToolStripStatusLabel3.BackColor = Color.Red
            'Form1.ToolStripStatusLabel3.ToolTipText = "Globals." & System.Reflection.MethodInfo.GetCurrentMethod.Name & " - " & ex.Message
            Return ""
        End Try
    End Function

#End Region

#Region "Will's Grid Functions"
    Public Function CreateCtrl(Of T As {Control, New})(grid As TableLayoutPanel) As T
        Dim ctrl As New T()
        ctrl.Dock = DockStyle.None
        ctrl.Anchor = AnchorStyles.None
        ctrl.BackColor = SystemColors.ControlDark
        ResizeCtrl(ctrl, grid)
        If (Not ctrl.IsHandleCreated) Then
            ctrl.CreateControl()
        End If
        Return ctrl
    End Function

    Public Sub ResizeCtrl(ctrl As Control, grid As TableLayoutPanel)
        grid.SuspendLayout()
        Dim count As Integer = grid.Controls.Count + 1
        If count < 2 Then
            grid.ColumnCount = 1
            grid.RowCount = 1
        ElseIf count < 3 Then
            grid.ColumnCount = 2
            grid.RowCount = 1
        ElseIf count < 5 Then
            grid.ColumnCount = 2
            grid.RowCount = 2
        ElseIf count < 7 Then
            grid.ColumnCount = 3
            grid.RowCount = 2
        ElseIf count < 10 Then
            grid.ColumnCount = 3
            grid.RowCount = 3
        ElseIf count < 13 Then
            grid.ColumnCount = 4
            grid.RowCount = 3
        ElseIf count < 17 Then
            grid.ColumnCount = 4
            grid.RowCount = 4
        Else
            grid.ColumnCount = 5
            grid.RowCount = 5
        End If
        If grid Is Nothing Then Return
        If grid.IsDisposed Then Return
        If Not grid.IsHandleCreated Then Return
        Try
            grid.Invoke(Sub()
                            grid.GrowStyle = TableLayoutPanelGrowStyle.FixedSize
                            grid.ColumnStyles.Clear()
                            grid.RowStyles.Clear()
                            grid.AutoSize = False
                            For i As Integer = 1 To grid.ColumnCount
                                grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0! / grid.ColumnCount))
                            Next
                            For i As Integer = 1 To grid.RowCount
                                grid.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0! / grid.RowCount))
                            Next
                            grid.Controls.Add(ctrl, -1, -1)
                            Debug.WriteLine($"Added Control Hash: {ctrl.GetHashCode()}")
                            Debug.WriteLine($"Form: {Form1.ClientSize}")
                            Debug.WriteLine($"Grid: {grid.ClientSize}")
                            Debug.WriteLine($"VNC: {ctrl.ClientSize}")
                            grid.ResumeLayout(True)
                            grid.PerformLayout()
                        End Sub)
            ctrl.BeginInvoke(Sub()
                                 ctrl.Refresh()
                             End Sub)
        Catch ex As InvalidOperationException
            Debug.WriteLine("grid handle is gone.")
        Catch ex As ObjectDisposedException
            Debug.WriteLine("grid disposed.")
        End Try
    End Sub
#End Region

#Region "Other"

    Public Function Check_Spot_Open(track As Integer, spot As Integer, Optional dbid As Integer = 0) As Boolean
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim strSQL As String
        Dim result As Boolean = False
        Try
            If track = 0 Or spot = 0 Then
                result = True
            Else
                If dbid = 0 Then
                    strSQL = "SELECT * FROM storage WHERE active=true AND type<>'SILO' AND track=" & track & " AND spot=" & spot & " AND active=true;"
                Else
                    strSQL = "SELECT * FROM storage WHERE active=true AND type<>'SILO' AND id <>" & dbid & " AND track=" & track & " AND spot=" & spot & " AND active=true;"
                End If
                Using cn As New MySqlConnection(MYSQLCS)
                    cm = New MySqlCommand(strSQL, cn)
                    cn.Open()
                    rd = cm.ExecuteReader
                    If rd.HasRows Then
                        result = False
                    Else
                        result = True
                    End If
                    rd.Close()
                End Using
            End If
            Return result
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        End Try
    End Function

    Public Sub Load_Map()
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim count As Integer = 0
        Dim index As Integer = 0
        Try
            ' Create the track and spot array dimensions
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT COUNT(id) AS id FROM railcar_map;", cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    count = rd.GetInt32("id")
                End If
                rd.Close()
            End Using
            TRACK_NAME = New String(count - 1) {}
            TRACK_SPOT = New Integer(count - 1) {}
            ' Fill the track and spot arrays
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM railcar_map AS map;", cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        TRACK_SPOT(index) = rd.GetInt32("spot_count")
                        If rd.IsDBNull(rd.GetOrdinal("name")) Then
                            TRACK_NAME(index) = rd.GetInt32("id")
                        Else
                            TRACK_NAME(index) = rd.GetString("name")
                        End If
                        If TRACK_SPOT(index) > MAX_SPOTS Then
                            MAX_SPOTS = TRACK_SPOT(index)
                        End If
                        index += 1
                    End While
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Function Check_Tag_Write(tagName As String, value As String, Optional waitSeconds As Integer = 5)
        Dim i As Integer = 0
        Dim result As Boolean = False
        'Wait for response from PLC, If no response from PLC within 5 seconds display message and respond to Operator choice
        'For i = 1 To 5
        '    Universals.WaitForM(1000)
        '    If plc.TagGet(tagName) = value Then
        '        result = True
        '        Exit For
        '    End If
        '    If My.Settings.PLC_ADDRESS = "NULL" Then
        '        plc.TagSet(tagName, value)
        '    End If
        'Next
        Return result
    End Function

    Public Sub Set_Panel_Ctl_ForeColor(ByRef layoutPanel As TableLayoutPanel)
        For Each ctl As Control In layoutPanel.Controls
            If TypeOf ctl Is Label Then
                If Not ctl.Name.Contains("lbl_") Then
                    ctl.ForeColor = Globals.LBL_FORE_COLOR
                End If
            ElseIf TypeOf ctl Is RadioButton Then
                ctl.ForeColor = Globals.LBL_FORE_COLOR
            ElseIf TypeOf ctl Is CheckBox Then
                ctl.ForeColor = Globals.LBL_FORE_COLOR
            End If
        Next
    End Sub

    Public Function Check_Rail_Duplicates(ByRef duplicateList As String) As Boolean
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim strSQL As String
        Dim strList(1000000) As String
        Dim intListCount As Integer = 0
        Dim result As Boolean = False
        Try
            strSQL = "SELECT name FROM storage WHERE active=true AND type <>'SILO';"
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        strList(intListCount) = rd.GetString("name")
                        For i = 1 To intListCount - 1
                            If rd.GetString("name") = strList(i) Then
                                result = True
                                duplicateList = duplicateList & rd.GetString("name") & ", "
                            End If
                        Next
                        intListCount += 1
                    End While
                End If
                rd.Close()
            End Using
            Return result
        Catch ex As MySqlException
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        Catch ex As Exception
            Call Universals.Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the map information. " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        End Try
    End Function

    Public Sub releaseObject(ByVal obj As Object)
        Try
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
            obj = Nothing
        Catch ex As Exception
            obj = Nothing
        Finally
            GC.Collect()
        End Try
    End Sub

    Public Function SQL_Column_Exists(ByRef reader As MySqlDataReader, col_name As String) As Boolean
        Try
            For i = 0 To reader.FieldCount - 1
                If reader.GetName(i).ToLower = col_name.ToLower Then
                    Return True
                End If
            Next
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function Parse_Rail_Name(rail_name As String) As String
        Dim i As Integer
        Dim pre As String = ""
        Dim suf As String = ""
        Dim mid As Integer = 0
        Try
            For i = 0 To rail_name.Length - 1
                If IsNumeric(rail_name.Substring(i, 1)) Then
                    mid = i
                    Exit For
                End If
            Next
            If mid > 0 Then
                pre = rail_name.Substring(0, mid)
                suf = rail_name.Substring(mid, rail_name.Length - mid)
                Return pre & vbCrLf & suf
            Else
                Return rail_name
            End If

        Catch ex As Exception
            Return rail_name
        End Try
    End Function
#End Region

End Module
