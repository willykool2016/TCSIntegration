Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Text
Imports System.Security
Imports System.Diagnostics
Imports System.Globalization
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports MySql.Data.MySqlClient
Imports ThoughtWorks.QRCode.Codec
Imports Newtonsoft.Json

Module Universals

    Public TAG_COUNT As Integer = 0
    Public SILO_COUNT As Integer = 0
    Public LANE_COUNT As Integer = 0

    Public RAIL_FORMAT_ADD_ZEROES As Boolean = False
    Public RAIL_FORMAT_ADD_SPACES As Boolean = False
    Public RAIL_FORMAT_ADD_SPACE As Boolean = False

    Public TLSMAINSQL As ToolStripStatusLabel
    Public MYSQLCS As String = String.Format("server={0}; user id={1}; password={2}; database={3}", My.Settings.SQL_ADDRESS, "tmtadmin", "TMT100", My.Settings.DB_NAME)

#Region "Logging Routines"

    Public Sub Messager(message As String, button As MsgBoxStyle, title As String)
        Beep()
        Call MsgBox(message, button, title)
    End Sub

    Public Sub Error_Messager(formName As String, routineName As String, message As String, msgBoxType As MsgBoxStyle, msgBoxTitle As String, Optional suppress As Boolean = False)
        Try
            Beep()
            If suppress Then
                'TLSMAINSQL.BackColor = Color.Red
                'TLSMAINSQL.ToolTipText = "Globals." & System.Reflection.MethodInfo.GetCurrentMethod.Name & " - " & message
            Else
                Call MsgBox(message, msgBoxType, msgBoxTitle)
            End If

            Using fs As New FileStream(My.Settings.DAT_PATH & "\Error.txt", FileMode.Append, FileAccess.Write)
                Using sw As New StreamWriter(fs)
                    sw.WriteLine(DateTime.Now & vbTab & System.Reflection.Assembly.GetEntryAssembly.GetName.Name & vbTab & formName & vbTab & routineName & vbTab & message)
                End Using
            End Using

        Catch ex As Exception
        End Try
    End Sub

#End Region

#Region "Get DB Functions"

    Public Function Get_SQL_Value(strSQL As String, Optional suppress As Boolean = False) As String
        Dim cm As New MySqlCommand
        Dim o As Object

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm.CommandText = strSQL
                cm.CommandType = CommandType.Text
                cm.Connection = cn
                cn.Open()
                o = cm.ExecuteScalar
            End Using

            If o Is Nothing Then
                Return ""
            Else
                Return o.ToString
            End If

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)
            Return ""

        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)
            Return ""
        End Try
    End Function

    Public Function Get_SQL_Values(strSQL As String, field As String, Optional suppress As Boolean = False) As List(Of String)
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim result = New List(Of String)()

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        result.Add(rd.GetString(field))
                    End While
                End If
                rd.Close()
            End Using

            If result.Count = 0 Then
                Return Nothing
            Else
                Return result
            End If

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)
            Return Nothing

        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)
            Return Nothing
        End Try
    End Function

    Public Function Get_SQL_Values(strSQL As String, Optional suppress As Boolean = False) As List(Of String)
        Dim cm As New MySqlCommand
        Dim rd As MySqlDataReader
        Dim result As List(Of String) = New List(Of String)
        Dim col As String = ""
        Dim data_type As Type
        Dim col_int As String = ""
        Dim col_date As DateTime
        Dim j As Integer = 0

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read()
                        For i As Integer = 0 To rd.FieldCount - 1
                            col = rd.GetName(i)
                            data_type = rd.GetFieldType(col)

                            If data_type Is GetType(String) Then
                                result.Add(rd.GetString(col))
                            ElseIf data_type Is GetType(Integer) Then
                                col_int = Convert.ToString(rd.GetInt32(col))
                                result.Add(col_int)
                            ElseIf data_type Is GetType(DateTime) Then
                                col_date = Convert.ToString(rd.GetDateTime(col))
                                result.Add(col_date)
                            End If
                        Next
                    End While
                End If
                rd.Close()
            End Using

            If result.Count = 0 Then
                Return Nothing
            Else
                Return result
            End If

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return Nothing
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return Nothing
        End Try
    End Function

    Public Function Get_SQL_Value(strSQL As String, field_count As Integer) As Object()
        Dim cm As MySqlCommand
        Dim objArr(field_count - 1) As Object

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                Using rd = cm.ExecuteReader
                    If rd.HasRows Then
                        rd.Read()
                        rd.GetValues(objArr)
                    End If
                End Using
            End Using

            Return objArr
        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals", True)
            Return objArr
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals", True)
            Return objArr
        End Try
    End Function

    Public Function Get_Next_Ticket() As String
        Dim cm As MySqlCommand
        Dim prefix As String = ""
        Dim value As String = ""

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM config WHERE name='ticket';", cn)
                cn.Open()
                Using rd As MySqlDataReader = cm.ExecuteReader
                    If rd.HasRows Then
                        rd.Read()
                        If Not rd.IsDBNull(rd.GetOrdinal("prefix")) Then prefix = rd.GetString("prefix")
                        value = rd.GetString("value")
                    Else
                        Call MessageBox.Show("No configuration for the ticket number could be found.", "Globals", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                        Return ""
                    End If
                End Using
            End Using

            If Val(value) >= 999 Then
                value = 1
            Else
                value = Val(value) + 1
            End If

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("UPDATE config SET value=@value WHERE name='ticket';", cn)
                cn.Open()
                cm.Parameters.AddWithValue("@value", value)
                cm.ExecuteNonQuery()
            End Using

            value = DateTime.Now.ToString("yyyyMMdd") & "-" & CInt(value).ToString("D3")
            Return prefix & value

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        End Try
    End Function

    Public Function Get_Next_Config(field As String, Optional addDatePrefix As Boolean = True) As String
        Dim cm As MySqlCommand
        Dim prefix As String = ""
        Dim value As Integer = 0

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM config WHERE name='" & field & "';", cn)
                cn.Open()
                Using rd As MySqlDataReader = cm.ExecuteReader
                    If rd.HasRows Then
                        rd.Read()
                        If Not rd.IsDBNull(rd.GetOrdinal("prefix")) Then prefix = rd.GetString("prefix")
                        value = rd.GetString("value")
                    Else
                        Call MessageBox.Show("No configuration for the requested config could be found.", "Globals", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                        Return ""
                    End If
                End Using
            End Using

            If (value + 1).ToString.Length <> value.ToString.Length Then
                value = CInt("1" & Fill_Number(value, "0") & "1")
            Else
                value += 1
            End If

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("UPDATE config SET value=@value WHERE name='" & field & "';", cn)
                cn.Open()
                cm.Parameters.AddWithValue("@value", value)
                cm.ExecuteNonQuery()
            End Using

            If addDatePrefix Then
                Return prefix & DateTime.Now.ToString("yy") & value
            Else
                Return prefix & value
            End If

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        End Try
    End Function

    Public Function Get_Next_UID(customerName As String, rand As Random) As String
        Dim cm As MySqlCommand
        Dim prefix As String = ""
        Dim value As Integer = 0
        Dim intChar As Integer = rand.Next(65, 91)

        Try
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("SELECT * FROM customer WHERE name='" & customerName & "';", cn)
                cn.Open()
                Using rd As MySqlDataReader = cm.ExecuteReader
                    If rd.HasRows Then
                        rd.Read()
                        If Not rd.IsDBNull(rd.GetOrdinal("uid_prefix")) Then prefix = rd.GetString("uid_prefix")
                        value = rd.GetString("uid")
                    Else
                        Call MessageBox.Show("No configuration for the uid could be found.", "Globals", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                        Return ""
                    End If
                End Using
            End Using

            value += 1

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand("UPDATE customer SET uid=@value WHERE name='" & customerName & "';", cn)
                cn.Open()
                cm.Parameters.AddWithValue("@value", value)
                cm.ExecuteNonQuery()
            End Using

            Return prefix & value & Chr(intChar)
        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        End Try
    End Function

    Public Function Get_QR(bol_id As Integer) As String
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim s As New StringBuilder
        Dim sql As New StringBuilder
        Dim qr As New StringBuilder

        Try
            sql.Append("SELECT bol.name, bol.uid, bol.carrier_id, bol.truck_number, bol.trailer_1_number, bol.trailer_2_number, bol.bol_number,")
            sql.Append(" bol.tare_weight, bol.gross_weight, bol.gross_weight - bol.tare_weight AS net_weight, bol.exit_time,")
            sql.Append(" car.name as carrier_name, soh.po_number,")
            sql.Append(" cust.id AS cust_id, cust.name AS cust_name, COALESCE(cust.qr_type, '') AS qr_type,")
            sql.Append(" prod.id AS prod_id, prod.name AS prod_name,")
            sql.Append(" xref.name AS xref_name, xref.description AS xref_desc")
            sql.Append(" FROM ticket AS bol")
            sql.Append(" LEFT JOIN so_header AS soh ON bol.so_id=soh.id")
            sql.Append(" LEFT JOIN carrier as car ON car.id=bol.carrier_id")
            sql.Append(" LEFT JOIN customer AS cust ON soh.customer_id=cust.id")
            sql.Append(" LEFT JOIN product AS prod ON soh.product_id=prod.id")
            sql.Append(" LEFT JOIN product_xref as xref ON soh.customer_id = xref.customer_id AND soh.product_id = xref.product_id")
            sql.Append(" WHERE bol.id=" & bol_id & ";")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()

                    Select Case rd.GetString("qr_type").ToString.ToUpper
                        Case "QR-HAL"
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then qr.Append(rd.GetString("po_number") & "|") Else qr.Append("|")
                            qr.Append("0001" & "|")
                            If Not rd.IsDBNull(rd.GetOrdinal("xref_name")) Then qr.Append(rd.GetString("xref_name") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("xref_desc")) Then qr.Append(rd.GetString("xref_desc") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then qr.Append(rd.GetString("bol_number") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then qr.Append(rd.GetInt32("net_weight") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("uid")) Then qr.Append(rd.GetString("uid") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then qr.Append(rd.GetString("truck_number")) Else qr.Append("|")

                        Case "QR-PTL"
                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then qr.Append("<BOL_KEY>" & rd.GetString("bol_number") & "</BOL_KEY>")
                            qr.Append("<SUPPLIER_KEY>" & "CUSTOMER NAME" & "</SUPPLIER_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("xref_name")) Then qr.Append("<PROPPANT_KEY>" & rd.GetString("xref_name") & "</PROPPANT_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("carrier_name")) Then qr.Append("<CARRIER_KEY>" & rd.GetString("carrier_name") & "</CARRIER_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then qr.Append("<VOLUME_KEY>" & rd.GetString("net_weight") & "</VOLUME_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then qr.Append("<SAND_PO_KEY>" & rd.GetString("po_number") & "</SAND_PO_KEY>")
                            qr.Append("<ORIGIN_KEY>" & "PLANT NAME" & "</ORIGIN_KEY>")
                            qr.Append("<DATE_KEY>" & DateTime.Now.ToString & "</DATE_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then qr.Append("<TRUCK_KEY>" & rd.GetString("truck_number") & "</TRUCK_KEY>")
                            If Not rd.IsDBNull(rd.GetOrdinal("trailer_1_number")) Then qr.Append("<TRAILER_KEY>" & rd.GetString("trailer_1_number") & "</TRAILER_KEY>")

                        Case "QR-PIONEER"
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then qr.Append(rd.GetString("po_number") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then qr.Append(rd.GetInt32("net_weight") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then qr.Append(rd.GetString("bol_number") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("xref_name")) Then qr.Append(rd.GetString("xref_name") & "|") Else qr.Append("|")
                            qr.Append(Format(DateTime.Now, "MM/dd/yyyy"))

                        Case "QR-LIB"
                            qr.Append("SITE NAME" & ",")
                            If Not rd.IsDBNull(rd.GetOrdinal("xref_name")) Then qr.Append(rd.GetString("xref_name") & ",") Else qr.Append(",")
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then qr.Append(rd.GetString("po_number") & ",") Else qr.Append(",")
                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then qr.Append(rd.GetString("bol_number") & ",") Else qr.Append(",")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then qr.Append(rd.GetString("net_weight") & ",") Else qr.Append(",")
                            If Not rd.IsDBNull(rd.GetOrdinal("trailer_1_number")) Then qr.Append(rd.GetString("trailer_1_number") & ",") Else qr.Append(",")
                            If Not rd.IsDBNull(rd.GetOrdinal("trailer_2_number")) Then qr.Append(rd.GetString("trailer_2_number"))

                        Case "QR-PROFRAC"
                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then qr.Append(rd.GetString("bol_number") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then qr.Append(rd.GetString("po_number") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then qr.Append(rd.GetString("net_weight") & "|") Else qr.Append("|")
                            If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then qr.Append(rd.GetString("truck_number")) Else qr.Append("|")

                        Case "QR-PROPX"
                            Dim propx As QR_PROPX = New QR_PROPX

                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then propx.ticket_no = rd.GetString("bol_number")
                            If Not rd.IsDBNull(rd.GetOrdinal("cust_name")) Then propx.customer = rd.GetString("cust_name")
                            If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then propx.po_no = rd.GetString("po_number")
                            If Not rd.IsDBNull(rd.GetOrdinal("prod_name")) Then propx.product = rd.GetString("prod_name")
                            If Not rd.IsDBNull(rd.GetOrdinal("gross_weight")) Then propx.gross = rd.GetInt32("gross_weight")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then propx.net = rd.GetInt32("net_weight")
                            If Not rd.IsDBNull(rd.GetOrdinal("tare_weight")) Then propx.tare = rd.GetInt32("tare_weight")
                            If Not rd.IsDBNull(rd.GetOrdinal("exit_time")) Then propx.exit_time = rd.GetDateTime("exit_time") Else propx.exit_time = DateTime.Now

                            Dim json_str As String = Newtonsoft.Json.JsonConvert.SerializeObject(propx, Newtonsoft.Json.Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                            qr.Append(json_str)
                        Case "QR-OVINTIV"
                            Dim ovintiv As QR_OVINTIV.Rootobject = New QR_OVINTIV.Rootobject
                            ovintiv.ticket = New QR_OVINTIV.Ticket

                            If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then ovintiv.ticket.number = rd.GetString("bol_number")
                            If Not rd.IsDBNull(rd.GetOrdinal("net_weight")) Then ovintiv.ticket.quantity = rd.GetInt32("net_weight")

                            Dim json_str As String = Newtonsoft.Json.JsonConvert.SerializeObject(ovintiv, Newtonsoft.Json.Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                            qr.Append(json_str)
                        Case Else

                    End Select

                    Return qr.ToString
                Else
                    Return ""
                End If

                rd.Close()
            End Using

        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Get_Ticket_Sources(intDBID As Integer) As String
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim s As New StringBuilder
        Dim sql As New StringBuilder

        Try
            sql.Append("SELECT silo.name, tload.net_weight FROM ticket_load AS tload")
            sql.Append(" LEFT JOIN storage AS silo ON tload.storage_id=silo.id")
            sql.Append(" WHERE tload.truck_id=" & intDBID & ";")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    If Not rd.IsDBNull(rd.GetOrdinal("name")) Then s.Append(rd.GetString("name") & " (" & rd.GetInt32("net_weight").ToString("N0") & ")")
                    While rd.Read
                        If Not rd.IsDBNull(rd.GetOrdinal("name")) Then s.Append(", " & rd.GetString("name") & " (" & rd.GetInt32("net_weight").ToString("N0") & ")")
                    End While
                End If
                rd.Close()
            End Using

            Return s.ToString
        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return ""
        End Try
    End Function

    Public Function Get_Storage_Calc_Weight(db_id As Integer, Optional datStartDate As DateTime = #1/1/1970 12:00:01 AM#, Optional datStartTime As DateTime = #1/1/1970 12:00:01 AM#,
                                         Optional datStopDate As DateTime = #12/31/2999 12:59:59 PM#, Optional datStopTime As DateTime = #12/31/2999 12:59:59 PM#) As Integer
        Try
            Dim calc_weight As Integer = 0
            Using cn As New MySqlConnection(MYSQLCS)
                Dim cm As MySqlCommand = New MySqlCommand("GET_STORAGE_CALC_WEIGHT", cn)
                cm.CommandType = CommandType.StoredProcedure
                cm.Parameters.Add("calc_weight", MySqlDbType.Int32).Direction = ParameterDirection.ReturnValue
                cm.Parameters.AddWithValue("@db_id", db_id)
                cm.Parameters.AddWithValue("@start_date", datStartDate.ToString("yyyy-MM-dd") & " " & datStartTime.ToString("HH:mm:ss"))
                cm.Parameters.AddWithValue("@stop_date", datStopDate.ToString("yyyy-MM-dd") & " " & datStopTime.ToString("HH:mm:ss"))

                cn.Open()
                cm.ExecuteNonQuery()

                calc_weight = cm.Parameters("calc_weight").Value
            End Using

            Return calc_weight

        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function Get_Storage_Calc_Weight(db_id As Integer, ByRef silo_in_amount As Long, ByRef silo_out_amount As Long, ByRef truck_in_amount As Long, ByRef truck_out_amount As Long, ByRef offset_amount As Long,
                                         datStartDate As DateTime, datStartTime As DateTime, datStopDate As DateTime, datStopTime As DateTime) As Integer

        Try
            Dim calc_weight As Integer = 0
            Using cn As New MySqlConnection(MYSQLCS)
                Dim cm As MySqlCommand = New MySqlCommand("get_silo_inventory", cn)
                cm.CommandType = CommandType.StoredProcedure
                cm.Parameters.AddWithValue("@db_id", db_id)
                cm.Parameters.AddWithValue("@start_date", datStartDate.ToString("yyyy-MM-dd") & " " & datStartTime.ToString("HH:mm:ss"))
                cm.Parameters.AddWithValue("@stop_date", datStopDate.ToString("yyyy-MM-dd") & " " & datStopTime.ToString("HH:mm:ss"))
                cm.Parameters.Add("calc_weight", MySqlDbType.Int32).Direction = ParameterDirection.Output
                cm.Parameters.Add("silo_in_amount", MySqlDbType.Int32).Direction = ParameterDirection.Output
                cm.Parameters.Add("silo_out_amount", MySqlDbType.Int32).Direction = ParameterDirection.Output
                cm.Parameters.Add("truck_in_amount", MySqlDbType.Int32).Direction = ParameterDirection.Output
                cm.Parameters.Add("truck_out_amount", MySqlDbType.Int32).Direction = ParameterDirection.Output
                cm.Parameters.Add("offset_amount", MySqlDbType.Int32).Direction = ParameterDirection.Output

                cn.Open()
                cm.ExecuteNonQuery()

                calc_weight = cm.Parameters("calc_weight").Value
                silo_in_amount = cm.Parameters("silo_in_amount").Value
                silo_out_amount = cm.Parameters("silo_out_amount").Value
                truck_in_amount = cm.Parameters("truck_in_amount").Value
                truck_out_amount = cm.Parameters("truck_out_amount").Value
                offset_amount = cm.Parameters("offset_amount").Value
            End Using

            Return calc_weight

        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function Get_Lane_Number(storage As String) As String
        Dim result As String = ""

        Try
            If storage <> "" Then
                Select Case Get_SQL_Value("SELECT type FROM storage WHERE active=true AND name='" & storage & "';")
                    Case "SILO"
                        result = Get_SQL_Value("SELECT map.name FROM storage AS stor LEFT JOIN storage_map AS map ON stor.track = map.id WHERE stor.active=true AND stor.name='" & storage & "';")
                    Case Else
                        result = Get_SQL_Value("SELECT map.name FROM storage AS stor LEFT JOIN railcar_map AS map ON stor.track = map.id WHERE stor.active=true AND stor.name='" & storage & "';")
                End Select
            End If

            Return result
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Get_Silo_Number(silo As String) As String
        Dim result As String = ""

        If silo.ToUpper.StartsWith("SILO") Then
            result = Strings.Trim(Strings.Right(silo, Len(silo) - 4))
        End If

        Return result
    End Function

    Public Function Get_Directions(storage As String) As String
        Dim result As String = ""

        Try
            If storage <> "" Then
                Select Case Get_SQL_Value("SELECT type FROM storage WHERE active=true AND name='" & storage & "';")
                    Case "SILO"
                        result = Get_SQL_Value("SELECT map.directions FROM storage AS stor LEFT JOIN storage_map AS map ON stor.track = map.id WHERE stor.active=true AND stor.name='" & storage & "';")
                    Case Else
                        result = Get_SQL_Value("SELECT map.directions FROM storage AS stor LEFT JOIN railcar_map AS map ON stor.track = map.id WHERE stor.active=true AND stor.name='" & storage & "';")
                End Select
            End If

            Return result
        Catch ex As Exception
            Return ""
        End Try
    End Function

#End Region

#Region "Set DB Functions"

    Public Function Create_Table_Row(table As String _
, Optional field01 As String = "", Optional value01 As String = "" _
, Optional field02 As String = "", Optional value02 As String = "" _
, Optional field03 As String = "", Optional value03 As String = "" _
, Optional field04 As String = "", Optional value04 As String = "" _
, Optional field05 As String = "", Optional value05 As String = "" _
, Optional field06 As String = "", Optional value06 As String = "" _
, Optional field07 As String = "", Optional value07 As String = "" _
, Optional field08 As String = "", Optional value08 As String = "" _
, Optional field09 As String = "", Optional value09 As String = "" _
, Optional field10 As String = "", Optional value10 As String = "" _
, Optional field11 As String = "", Optional value11 As String = "" _
, Optional field12 As String = "", Optional value12 As String = "") As Boolean

        Dim cm As MySqlCommand
        Dim sql As New StringBuilder

        Try
            sql.Append("INSERT INTO " & table & "(")
            If field01 <> "" Then sql.Append(field01)
            If field02 <> "" Then sql.Append("," & field02)
            If field03 <> "" Then sql.Append("," & field03)
            If field04 <> "" Then sql.Append("," & field04)
            If field05 <> "" Then sql.Append("," & field05)
            If field06 <> "" Then sql.Append("," & field06)
            If field07 <> "" Then sql.Append("," & field07)
            If field08 <> "" Then sql.Append("," & field08)
            If field09 <> "" Then sql.Append("," & field09)
            If field10 <> "" Then sql.Append("," & field10)
            If field11 <> "" Then sql.Append("," & field11)
            If field12 <> "" Then sql.Append("," & field12)
            sql.Append(") VALUES(")
            If field01 <> "" Then sql.Append("@" & field01)
            If field02 <> "" Then sql.Append(",@" & field02)
            If field03 <> "" Then sql.Append(",@" & field03)
            If field04 <> "" Then sql.Append(",@" & field04)
            If field05 <> "" Then sql.Append(",@" & field05)
            If field06 <> "" Then sql.Append(",@" & field06)
            If field07 <> "" Then sql.Append(",@" & field07)
            If field08 <> "" Then sql.Append(",@" & field08)
            If field09 <> "" Then sql.Append(",@" & field09)
            If field10 <> "" Then sql.Append(",@" & field10)
            If field11 <> "" Then sql.Append(",@" & field11)
            If field12 <> "" Then sql.Append(",@" & field12)
            sql.Append(");")

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                If field01 <> "" Then cm.Parameters.AddWithValue("@" & field01, value01)
                If field02 <> "" Then cm.Parameters.AddWithValue("@" & field02, value02)
                If field03 <> "" Then cm.Parameters.AddWithValue("@" & field03, value03)
                If field04 <> "" Then cm.Parameters.AddWithValue("@" & field04, value04)
                If field05 <> "" Then cm.Parameters.AddWithValue("@" & field05, value05)
                If field06 <> "" Then cm.Parameters.AddWithValue("@" & field06, value06)
                If field07 <> "" Then cm.Parameters.AddWithValue("@" & field07, value07)
                If field08 <> "" Then cm.Parameters.AddWithValue("@" & field08, value08)
                If field09 <> "" Then cm.Parameters.AddWithValue("@" & field09, value09)
                If field10 <> "" Then cm.Parameters.AddWithValue("@" & field10, value10)
                If field11 <> "" Then cm.Parameters.AddWithValue("@" & field11, value11)
                If field12 <> "" Then cm.Parameters.AddWithValue("@" & field12, value12)

                cm.ExecuteNonQuery()
            End Using

            Return True
        Catch ex As MySqlException
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "SQL Error creating the record. " & ex.Message, MsgBoxStyle.Critical, "Universals")
            Return False
        Catch ex As Exception
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "General error creating the record. " & ex.Message, MsgBoxStyle.Critical, "Universals")
            Return False
        End Try
    End Function

    Public Function Update_Table_Row(table As String, id As Integer _
, Optional field01 As String = "", Optional value01 As String = "" _
, Optional field02 As String = "", Optional value02 As String = "" _
, Optional field03 As String = "", Optional value03 As String = "" _
, Optional field04 As String = "", Optional value04 As String = "" _
, Optional field05 As String = "", Optional value05 As String = "" _
, Optional field06 As String = "", Optional value06 As String = "") As Boolean

        Dim cm As MySqlCommand
        Dim sql As New StringBuilder

        Try
            sql.Append("UPDATE " & table & " SET ")
            If field01 <> "" Then sql.Append(field01 & "=@" & field01)
            If field02 <> "" Then sql.Append("," & field02 & "=@" & field02)
            If field03 <> "" Then sql.Append("," & field03 & "=@" & field03)
            If field04 <> "" Then sql.Append("," & field04 & "=@" & field04)
            If field05 <> "" Then sql.Append("," & field05 & "=@" & field05)
            If field06 <> "" Then sql.Append("," & field06 & "=@" & field06)
            sql.Append(" WHERE id=" & id & ";")

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                If field01 <> "" Then cm.Parameters.AddWithValue("@" & field01, value01)
                If field02 <> "" Then cm.Parameters.AddWithValue("@" & field02, value02)
                If field03 <> "" Then cm.Parameters.AddWithValue("@" & field03, value03)
                If field04 <> "" Then cm.Parameters.AddWithValue("@" & field04, value04)
                If field05 <> "" Then cm.Parameters.AddWithValue("@" & field05, value05)
                If field06 <> "" Then cm.Parameters.AddWithValue("@" & field06, value06)

                cm.ExecuteNonQuery()
            End Using

            Return True
        Catch ex As MySqlException
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query. " & ex.Message, MsgBoxStyle.Critical, "Universals")
            Return False
        Catch ex As Exception
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data. " & ex.Message, MsgBoxStyle.Critical, "Universals")
            Return False
        End Try
    End Function

    Public Sub Set_Active(table As String, dbid As Integer, value As Boolean)
        Dim cm As MySqlCommand
        Dim strSQL As String

        Try
            strSQL = "UPDATE " & table &
                    " SET active=@active" &
                    " WHERE id=" & dbid & ";"

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                cm.Parameters.AddWithValue("@active", value)

                cm.ExecuteNonQuery()
            End Using

        Catch ex As MySqlException
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured editing the ticket. " & ex.Message, MsgBoxStyle.Critical, "Universals")
        Catch ex As Exception
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured editing the ticket. " & ex.Message, MsgBoxStyle.Critical, "Universals")
        End Try
    End Sub
    Public Sub Delete_Table_Row(table As String, id As Integer)
        Dim cm As MySqlCommand
        Dim strSQL As String

        Try
            strSQL = "DELETE FROM " & table & " WHERE id=" & id & ";"
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                cm.ExecuteNonQuery()
            End Using

        Catch ex As MySqlException
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured editing the ticket. " & ex.Message, MsgBoxStyle.Critical, "Universals")
        Catch ex As Exception
            Call Error_Messager("Universals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured editing the ticket. " & ex.Message, MsgBoxStyle.Critical, "Universals")
        End Try
    End Sub

#End Region

#Region "DGV Routines"

    Public Sub Load_DGV(SQL As String, ByRef DGV As DataGridView, table As String, Optional read As Boolean = False, Optional suppress As Boolean = False)
        Dim Myadapter As MySqlDataAdapter
        Dim MyBuilder As MySqlCommandBuilder
        Dim MyDataTable As DataTable
        Dim MyDataset As DataSet
        Dim cm As MySqlCommand

        Try
            Myadapter = New MySqlDataAdapter
            MyDataTable = New DataTable
            MyDataset = New DataSet

            DGV.DataSource = Nothing
            DGV.Rows.Clear()
            DGV.Columns.Clear()

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(SQL, cn)
                Myadapter.SelectCommand = cm
                MyBuilder = New MySqlCommandBuilder(Myadapter)
                Myadapter.Fill(MyDataset, table)
                MyDataTable = MyDataset.Tables(table)
                DGV.DataSource = MyDataTable
            End Using

            DGV.ReadOnly = read
            DGV.ClearSelection()

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the DGV. " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)

        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the DGV. " & ex.Message, MsgBoxStyle.Critical, "Globals", suppress)
        End Try
    End Sub

    Public Sub Load_DGV(ByRef DGV As DataGridView, ByVal fileName As String, ByVal starting As DateTime, ByVal ending As DateTime)
        Dim i As Integer
        Dim header() As String
        Dim temp() As String

        DGV.DataSource = Nothing
        DGV.Rows.Clear()
        DGV.Columns.Clear()

        Try
            Using fs As New FileStream(My.Settings.DAT_PATH & fileName, FileMode.Open, FileAccess.Read)
                Using sr As New StreamReader(fs)
                    header = sr.ReadLine.Split(vbTab)
                    For i = 0 To header.Length - 1
                        DGV.Columns.Add(header(i), header(i))
                    Next i
                    While sr.Peek <> -1
                        temp = sr.ReadLine.Split(vbTab)
                        If DateTime.Parse(temp(0)) >= starting And DateTime.Parse(temp(0)) <= ending Then
                            DGV.Rows.Add(temp)
                        End If
                    End While
                    DGV.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)
                End Using
            End Using

        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error populating the DGV data table for " & fileName & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Sub Load_DGV_CSV(filePath As String, ByRef DGV As DataGridView)
        Dim i As Integer
        Dim header() As String
        Dim currentRow() As String

        DGV.Rows.Clear()
        DGV.Columns.Clear()
        Try
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Using sr As New StreamReader(fs)
                    header = sr.ReadLine.Split(",")
                    For i = 0 To header.Length - 1
                        header(i) = header(i).Replace("""", String.Empty)
                        DGV.Columns.Add(header(i), header(i))
                    Next i
                End Using
            End Using

            Using myReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(filePath)
                myReader.TextFieldType = FileIO.FieldType.Delimited
                myReader.SetDelimiters(",")
                currentRow = myReader.ReadFields()
                While Not myReader.EndOfData
                    currentRow = myReader.ReadFields()
                    DGV.Rows.Add(currentRow)
                End While
            End Using
            DGV.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)

        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error populating the DGV data table for " & filePath & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

    Public Function Load_DGV_Excel(filePath As String, ByRef DGV As DataGridView, strSQL As String) As Boolean
        Dim dt As New DataTable
        'Dim cs As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & filePath & ";Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1"";"
        Dim cs As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & filePath & ";Extended Properties=""Excel 12.0 Xml;HDR=YES"";"

        DGV.DataSource = Nothing
        DGV.Rows.Clear()
        DGV.Columns.Clear()

        Try
            Using cn As New System.Data.OleDb.OleDbConnection(cs)
                Using cmd As New System.Data.OleDb.OleDbDataAdapter(strSQL, cn)
                    cn.Open()
                    cmd.Fill(dt)
                End Using
                cn.Close()
            End Using

            DGV.DataSource = dt
            DGV.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)
            DGV.Refresh()

            Return True
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error populating the DGV data table for " & filePath & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        End Try
    End Function

    Public Sub Load_DGV_Array(strSQL As String, ByRef DGV As DataGridView, table As String)
        Dim i As Integer
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim strarr(DGV.ColumnCount - 1) As String

        Try
            DGV.Rows.Clear()

            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(strSQL, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    While rd.Read
                        For i = 0 To DGV.ColumnCount - 1
                            If rd.IsDBNull(rd.GetOrdinal(DGV.Columns(i).Name)) Then
                                strarr(i) = ""
                            Else
                                Select Case rd.GetFieldType(i).ToString.ToUpper
                                    Case "SYSTEM.INT32"
                                        strarr(i) = rd.GetInt32(DGV.Columns(i).Name)
                                    Case "SYSTEM.INT64"
                                        strarr(i) = rd.GetInt64(DGV.Columns(i).Name)
                                    Case "SYSTEM.STRING"
                                        strarr(i) = rd.GetString(DGV.Columns(i).Name)
                                    Case "SYSTEM.DECIMAL"
                                        strarr(i) = rd.GetDecimal(DGV.Columns(i).Name)
                                    Case "SYSTEM.DATETIME"
                                        strarr(i) = rd.GetDateTime(DGV.Columns(i).Name)
                                End Select

                            End If
                        Next

                        DGV.Rows.Add(strarr)
                    End While
                End If
            End Using

        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A SQL error occured loading the DGV. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "A general error occured loading the DGV. " & ex.Message, MsgBoxStyle.Critical, "Globals")
        End Try
    End Sub

#End Region

#Region "BOL Printing"

    Public Function Save_Receipt_PDF(ticket As String, Optional timestamp As String = "") As Boolean
        Dim filePath As String = ""
        Dim fileName As String = ""
        Dim doc As PdfDocument = New PdfDocument
        Dim page As PdfPage = doc.AddPage
        Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
        Dim font0 As XFont = New XFont("Callibri", 12, XFontStyleEx.Bold)
        Dim font1 As XFont = New XFont("Callibri", 18, XFontStyleEx.Bold)
        Dim font2 As XFont = New XFont("Callibri", 20, XFontStyleEx.Regular)
        Dim font3 As XFont = New XFont("Callibri", 25, XFontStyleEx.Regular)
        Dim font4 As XFont = New XFont("Callibri", 35, XFontStyleEx.Regular)
        Dim font5 As XFont = New XFont("Callibri", 50, XFontStyleEx.Regular)
        Dim spacing As Integer = 30
        Dim format1 As XStringFormat = New XStringFormat()
        Dim x() As Integer = {0, 115, 180, 260, 300, 420, 450}
        Dim y() As Integer = {0, 30, 140, 600, 245, 275, 305, 335}
        Dim objQRCode As QRCodeEncoder = New QRCodeEncoder()
        Dim qrImg As Image
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim sql As New StringBuilder
        Try
            sql.Append("SELECT bol.name,bol.uid,bol.target_weight,bol.truck_number, COALESCE(CONCAT(bol.trailer_1_number,")
            sql.Append(" if(length(bol.trailer_2_number) > 0, ',', ''), COALESCE(bol.trailer_2_number, '')), '') AS trailer,")
            sql.Append(" bol.driver_name,bol.assign, so.so_number,so.po_number,so.po_line, cust.name AS cust_name,")
            sql.Append(" prod.name AS prod_name,prod.description, car.name AS car_name, COALESCE(bol.language, 'en') AS language,")
            sql.Append(" COALESCE(prod.description, '') AS description, COALESCE(bol.tare_weight,'0') AS tare_weight")
            sql.Append(" FROM ticket AS bol ")
            sql.Append(" LEFT JOIN so_header AS so ON bol.so_id=so.id")
            sql.Append(" LEFT JOIN customer AS cust ON so.customer_id=cust.id")
            sql.Append(" LEFT JOIN product AS prod ON so.product_id=prod.id")
            sql.Append(" LEFT JOIN carrier AS car ON bol.carrier_id=car.id")
            sql.Append(" WHERE bol.active=true AND bol.name='" & ticket & "';")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    If Not Directory.Exists(My.Settings.DAT_PATH & "\Pick") Then Directory.CreateDirectory(My.Settings.DAT_PATH & "\Pick")
                    filePath = My.Settings.DAT_PATH & "\Pick\"
                    fileName = rd.GetString("name") & timestamp & ".pdf"
                    doc.Info.Title = fileName
                    format1.Alignment = XStringAlignment.Center
                    gfx.DrawString("www.tmtsolutions.com", font1, XBrushes.Black, New XRect(x(3), y(0), 100, 100), format1)
                    ' Print the QR string if present
                    If rd.IsDBNull(rd.GetOrdinal("name")) OrElse rd.GetString("name") = "" Then
                    Else
                        objQRCode.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE
                        objQRCode.QRCodeScale = 6
                        objQRCode.QRCodeVersion = 1
                        objQRCode.QRCodeErrorCorrect = ThoughtWorks.QRCode.Codec.QRCodeEncoder.ERROR_CORRECTION.L
                        qrImg = objQRCode.Encode(rd.GetString("name"))
                        Dim ms As New MemoryStream()
                        qrImg.Save(ms, Imaging.ImageFormat.Png)
                        ms.Position = 0
                        Dim imgQR As PdfSharp.Drawing.XImage = PdfSharp.Drawing.XImage.FromStream(ms)
                        gfx.DrawImage(imgQR, x(3), y(1), 100, 100)
                    End If
                    format1.Alignment = XStringAlignment.Far
                    gfx.DrawString("Ticket:", font2, XBrushes.Black, New XRect(x(2), y(2), 100, 100), format1)
                    gfx.DrawString("Load Num:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 1), 100, 100), format1)
                    gfx.DrawString("SO Num:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 2), 100, 100), format1)
                    gfx.DrawString("PO Num:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 3), 100, 100), format1)
                    gfx.DrawString("Customer:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 4), 100, 100), format1)
                    gfx.DrawString("Carrier:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 5), 100, 100), format1)
                    gfx.DrawString("Truck:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 6), 100, 100), format1)
                    gfx.DrawString("Trailer:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 7), 100, 100), format1)
                    gfx.DrawString("Driver:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 8), 100, 100), format1)
                    gfx.DrawString("Product:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 9), 100, 100), format1)
                    gfx.DrawString("Description:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 10), 100, 100), format1)
                    gfx.DrawString("Target:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 11), 100, 100), format1)
                    gfx.DrawString("Tare:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 12), 100, 100), format1)
                    gfx.DrawString("Printed:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 13), 100, 100), format1)
                    gfx.DrawString("Assigned:", font2, XBrushes.Black, New XRect(x(2), y(2) + (spacing * 14), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Near
                    If Not rd.IsDBNull(rd.GetOrdinal("name")) Then gfx.DrawString(rd.GetString("name"), font2, XBrushes.Black, New XRect(x(4), y(2), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("uid")) Then gfx.DrawString(rd.GetString("uid"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 1), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("so_number")) Then gfx.DrawString(rd.GetString("so_number"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 2), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then gfx.DrawString(rd.GetString("po_number"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 3), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("cust_name")) Then gfx.DrawString(rd.GetString("cust_name"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 4), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("car_name")) Then gfx.DrawString(rd.GetString("car_name"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 5), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then gfx.DrawString(rd.GetString("truck_number"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 6), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("trailer")) Then gfx.DrawString(rd.GetString("trailer"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 7), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("driver_name")) Then gfx.DrawString(rd.GetString("driver_name"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 8), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("prod_name")) Then gfx.DrawString(rd.GetString("prod_name"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 9), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("description")) Then gfx.DrawString(rd.GetString("description"), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 10), 100, 100), format1)
                    gfx.DrawString(Convert.ToInt32(rd.GetString("target_Weight")).ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 11), 100, 100), format1)
                    gfx.DrawString(Convert.ToInt32(rd.GetString("tare_weight")).ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 12), 100, 100), format1)
                    gfx.DrawString(DateTime.Now.ToString(New CultureInfo("en-US")), font2, XBrushes.Black, New XRect(x(4), y(2) + (spacing * 13), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Center
                    If rd.GetString("assign").ToLower.Contains("xload") Then
                        gfx.DrawString("", font3, XBrushes.Black, New XRect(x(3), y(2) + (spacing * 17), 100, 100), format1)
                    Else
                        ' Lane
                        gfx.DrawString(SecurityElement.Escape(Get_Lane_Number(rd.GetString("assign"))), font5, XBrushes.Black, New XRect(x(3), y(2) + (spacing * 15), 100, 100), format1)
                    End If
                    ' Silo
                    gfx.DrawString(rd.GetString("assign"), font5, XBrushes.Black, New XRect(x(3), y(2) + (spacing * 17), 100, 100), format1)
                    ' Directions
                    format1.Alignment = XStringAlignment.Near
                    gfx.DrawString(SecurityElement.Escape(Get_Directions(rd.GetString("assign"))), font2, XBrushes.Black, New XRect(x(0) + 20, y(2) + (spacing * 19), 100, 100), format1)
                    doc.Save(filePath & fileName)
                    gfx = Nothing
                    Return True
                Else
                    Return False
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Exclamation, "Globals")
            Return False
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Exclamation, "Globals")
            Return False
        End Try
    End Function

    Public Function Save_BOL_PDF(bol_id As Integer, Optional timestamp As String = "") As Boolean
        Dim filePath As String = ""
        Dim fileName As String = ""
        Dim signPath As String = ""
        Dim disclaimer(3) As String
        Dim doc As PdfDocument = New PdfDocument
        Dim page As PdfPage = doc.AddPage
        Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
        Dim font1 As XFont = New XFont("Callibri", 10, XFontStyleEx.Bold)
        Dim font2 As XFont = New XFont("Callibri", 10, XFontStyleEx.Regular)
        Dim font3 As XFont = New XFont("Callibri", 6, XFontStyleEx.Regular)
        Dim logo As XImage
        Dim sign As XImage
        Dim format1 As XStringFormat = New XStringFormat()
        Dim x() As Integer = {0, 105, 155, 260, 315, 420, 450}
        Dim y() As Integer = {25, 45, 65, 85, 105, 125, 145, 165, 185, 205, 225, 245, 255, 265}
        Dim strRailID As String = ""
        Dim objQRCode As QRCodeEncoder = New QRCodeEncoder()
        Dim qrImg As Image
        Dim cm As MySqlCommand
        Dim rd As MySqlDataReader
        Dim s As New StringBuilder
        Dim sql As New StringBuilder
        Dim strTrailers As String = ""
        Dim sngImgRatio As Single
        Try
            disclaimer(0) = "WARNING: Contains silica sand.  Prolonged inhalation can cause silicosis and other health risks.  Use adequate respiratory protective equipment."
            disclaimer(1) = ""
            disclaimer(2) = ""
            disclaimer(3) = ""
            sql.Append("SELECT bol.name, bol.uid, bol.truck_number, bol.trailer_1_number, bol.trailer_2_number, bol.driver_name, bol.bol_number, bol.qr_string, bol.signature_path, bol.qr_string,")
            sql.Append(" bol.target_weight, bol.tare_weight, bol.gross_weight, bol.gross_weight - bol.tare_weight AS net_weight,")
            sql.Append(" COALESCE(bol.entry_time, '') AS entry_time, COALESCE(bol.exit_time, '') AS exit_time,")
            sql.Append(" cust.name AS cust_name, so.consignee, so.so_number, so.po_number, so.po_line, so.well_name, so.operator, car.name AS carr_name, prod.name AS prod_name, prod.description AS prod_desc,")
            sql.Append(" IF (LENGTH(so.well_site) > 0, so.well_site, bol.well_site) AS well_site")
            sql.Append(" FROM ticket AS bol")
            sql.Append(" LEFT JOIN so_header AS so ON bol.so_id=so.id")
            sql.Append(" LEFT JOIN customer AS cust ON so.customer_id=cust.id")
            sql.Append(" LEFT JOIN carrier AS car ON bol.carrier_id=car.id")
            sql.Append(" LEFT JOIN product AS prod ON so.product_id=prod.id")
            sql.Append(" WHERE bol.id=" & bol_id & ";")
            Using cn As New MySqlConnection(MYSQLCS)
                cm = New MySqlCommand(sql.ToString, cn)
                cn.Open()
                rd = cm.ExecuteReader
                If rd.HasRows Then
                    rd.Read()
                    ' Get the file directory
                    If Directory.Exists(My.Settings.DAT_PATH & "\BOL") Then
                        filePath = My.Settings.DAT_PATH & "\BOL\"
                    Else
                        filePath = ""
                    End If
                    ' Get the filename
                    If rd.IsDBNull(rd.GetOrdinal("bol_number")) Then
                        fileName = rd.GetString("name") & timestamp & ".pdf"
                    Else
                        fileName = rd.GetString("bol_number") & timestamp & ".pdf"
                    End If
                    ' Set the trailer field
                    If Not rd.IsDBNull(rd.GetOrdinal("trailer_1_number")) Then strTrailers = rd.GetString("trailer_1_number")
                    If Not rd.IsDBNull(rd.GetOrdinal("trailer_2_number")) AndAlso rd.GetString("trailer_2_number") <> "" Then strTrailers &= ", " & rd.GetString("trailer_2_number")
                    doc.Info.Title = fileName
                    sngImgRatio = My.Resources.Logo.Width / My.Resources.Logo.Height
                    Dim ms As New MemoryStream()
                    My.Resources.Logo.Save(ms, Imaging.ImageFormat.Png)
                    ms.Position = 0
                    Dim logoImg As XImage = XImage.FromStream(ms)
                    gfx.DrawImage(logoImg, 20, 20, sngImgRatio * 45, 45)
                    ' Print the signature if present
                    format1.Alignment = XStringAlignment.Far
                    If Not rd.IsDBNull(rd.GetOrdinal("signature_path")) AndAlso rd.GetString("signature_path") <> "" Then
                        If File.Exists(My.Settings.DAT_PATH & "\BOL\Signatures\" & rd.GetString("signature_path")) Then
                            signPath = My.Settings.DAT_PATH & "\BOL\Signatures\" & rd.GetString("signature_path")
                            sign = XImage.FromFile(signPath)
                            gfx.DrawImage(sign, x(5) + 10, y(7), 100, 50)
                        End If
                    End If
                    gfx.DrawLine(XPens.Black, x(5), y(9) + 10, x(4) + 260, y(9) + 10)
                    gfx.DrawString("Signature:", font1, XBrushes.Black, New XRect(x(4), y(9), 100, 100), format1)
                    ' Print the QR string if present
                    If Not rd.IsDBNull(rd.GetOrdinal("qr_string")) AndAlso rd.GetString("qr_string") <> "" Then
                        objQRCode.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE
                        objQRCode.QRCodeScale = 6
                        objQRCode.QRCodeVersion = Get_QR_Version(rd.GetString("qr_string").Length)
                        objQRCode.QRCodeErrorCorrect = ThoughtWorks.QRCode.Codec.QRCodeEncoder.ERROR_CORRECTION.M
                        qrImg = objQRCode.Encode(rd.GetString("qr_string"))
                        Dim ns As New MemoryStream()
                        qrImg.Save(ns, Imaging.ImageFormat.Png)
                        ns.Position = 0
                        Dim imgQR As PdfSharp.Drawing.XImage = PdfSharp.Drawing.XImage.FromStream(ns)
                        gfx.DrawImage(imgQR, x(6) + 50, y(3), 75, 75)
                    End If
                    format1.Alignment = XStringAlignment.Near
                    gfx.DrawString(Get_SQL_Value("SELECT prefix FROM config WHERE name='site_address';"), font3, XBrushes.Black, New XRect(30, 75, 100, 100), format1)
                    format1.Alignment = XStringAlignment.Far
                    gfx.DrawString("SO Number:", font1, XBrushes.Black, New XRect(x(0), y(3), 100, 100), format1)
                    gfx.DrawString("PO Number:", font1, XBrushes.Black, New XRect(x(0), y(4), 100, 100), format1)
                    gfx.DrawString("PO Line:", font1, XBrushes.Black, New XRect(x(0), y(5), 100, 100), format1)
                    gfx.DrawString("UID:", font1, XBrushes.Black, New XRect(x(0), y(6), 100, 100), format1)
                    gfx.DrawString("Customer:", font1, XBrushes.Black, New XRect(x(0), y(7), 100, 100), format1)
                    gfx.DrawString("Consignee:", font1, XBrushes.Black, New XRect(x(0), y(8), 100, 100), format1)
                    gfx.DrawString("Well Site:", font1, XBrushes.Black, New XRect(x(0), y(9), 100, 100), format1)
                    gfx.DrawString("Well Name:", font1, XBrushes.Black, New XRect(x(0), y(10), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Near
                    If Not rd.IsDBNull(rd.GetOrdinal("so_number")) Then gfx.DrawString(rd.GetString("so_number"), font2, XBrushes.Black, New XRect(x(1), y(3), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("po_number")) Then gfx.DrawString(rd.GetString("po_number"), font2, XBrushes.Black, New XRect(x(1), y(4), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("po_line")) Then gfx.DrawString(rd.GetString("po_line"), font2, XBrushes.Black, New XRect(x(1), y(5), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("uid")) Then gfx.DrawString(Strings.Left(rd.GetString("uid"), 18), font2, XBrushes.Black, New XRect(x(1), y(6), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("cust_name")) Then gfx.DrawString(Strings.Left(rd.GetString("cust_name"), 18), font2, XBrushes.Black, New XRect(x(1), y(7), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("consignee")) Then gfx.DrawString(Strings.Left(rd.GetString("consignee"), 18), font2, XBrushes.Black, New XRect(x(1), y(8), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("well_site")) Then gfx.DrawString(Strings.Left(rd.GetString("well_site"), 45), font2, XBrushes.Black, New XRect(x(1), y(9), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("well_name")) Then gfx.DrawString(Strings.Left(rd.GetString("well_name"), 45), font2, XBrushes.Black, New XRect(x(1), y(10), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Center
                    gfx.DrawString("BILL OF LADING", font1, XBrushes.Black, New XRect(x(3) - 50, y(0), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Far
                    gfx.DrawString("BOL Number:", font1, XBrushes.Black, New XRect(x(2), y(1), 100, 100), format1)
                    gfx.DrawString("Carrier:", font1, XBrushes.Black, New XRect(x(2), y(2), 100, 100), format1)
                    gfx.DrawString("Truck:", font1, XBrushes.Black, New XRect(x(2), y(3), 100, 100), format1)
                    gfx.DrawString("Trailer:", font1, XBrushes.Black, New XRect(x(2), y(4), 100, 100), format1)
                    gfx.DrawString("Driver:", font1, XBrushes.Black, New XRect(x(2), y(5), 100, 100), format1)
                    gfx.DrawString("Product:", font1, XBrushes.Black, New XRect(x(2), y(6), 100, 100), format1)
                    gfx.DrawString("Desc:", font1, XBrushes.Black, New XRect(x(2), y(7), 100, 100), format1)
                    gfx.DrawString("Source(s):", font1, XBrushes.Black, New XRect(x(2), y(8), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Near
                    If Not rd.IsDBNull(rd.GetOrdinal("bol_number")) Then gfx.DrawString(rd.GetString("bol_number"), font2, XBrushes.Black, New XRect(x(3), y(1), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("carr_name")) Then gfx.DrawString(rd.GetString("carr_name"), font2, XBrushes.Black, New XRect(x(3), y(2), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("truck_number")) Then gfx.DrawString(Strings.Left(rd.GetString("truck_number"), 22), font2, XBrushes.Black, New XRect(x(3), y(3), 100, 100), format1)
                    gfx.DrawString(Strings.Left(strTrailers, 20), font2, XBrushes.Black, New XRect(x(3), y(4), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("driver_name")) Then gfx.DrawString(Strings.Left(rd.GetString("driver_name"), 20), font2, XBrushes.Black, New XRect(x(3), y(5), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("prod_name")) Then gfx.DrawString(Strings.Left(rd.GetString("prod_name"), 20), font2, XBrushes.Black, New XRect(x(3), y(6), 100, 100), format1)
                    If Not rd.IsDBNull(rd.GetOrdinal("prod_desc")) Then gfx.DrawString(Strings.Left(rd.GetString("prod_desc"), 20), font2, XBrushes.Black, New XRect(x(3), y(7), 100, 100), format1)
                    gfx.DrawString(Get_Ticket_Sources(bol_id), font2, XBrushes.Black, New XRect(x(3), y(8), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Far
                    gfx.DrawString("Entrance Time:", font1, XBrushes.Black, New XRect(x(4), y(0), 100, 100), format1)
                    gfx.DrawString("Exit Time:", font1, XBrushes.Black, New XRect(x(4), y(1), 100, 100), format1)
                    gfx.DrawString("Target:", font1, XBrushes.Black, New XRect(x(4), y(3), 100, 100), format1)
                    gfx.DrawString("Gross:", font1, XBrushes.Black, New XRect(x(4), y(4), 100, 100), format1)
                    gfx.DrawString("Tare:", font1, XBrushes.Black, New XRect(x(4), y(5), 100, 100), format1)
                    gfx.DrawLine(XPens.Black, x(4) + 60, y(6) - 3, x(4) + 160, y(6) - 3)
                    gfx.DrawString("Net:", font1, XBrushes.Black, New XRect(x(4), y(6), 100, 100), format1)
                    format1.Alignment = XStringAlignment.Near
                    gfx.DrawString(rd.GetDateTime("entry_time").ToString(New CultureInfo("en-US")), font2, XBrushes.Black, New XRect(x(5), y(0), 100, 100), format1)
                    gfx.DrawString(rd.GetDateTime("exit_time").ToString(New CultureInfo("en-US")), font2, XBrushes.Black, New XRect(x(5), y(1), 100, 100), format1)
                    gfx.DrawString(rd.GetInt32("target_Weight").ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(5), y(3), 100, 100), format1)
                    gfx.DrawString(rd.GetInt32("gross_weight").ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(5), y(4), 100, 100), format1)
                    gfx.DrawString(rd.GetInt32("tare_weight").ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(5), y(5), 100, 100), format1)
                    gfx.DrawString(rd.GetInt32("net_weight").ToString("N0", New CultureInfo("en-US")) & " lbs", font2, XBrushes.Black, New XRect(x(5), y(6), 100, 100), format1)
                    For i As Integer = 0 To disclaimer.Length - 1
                        If disclaimer(i) <> "" Then
                            gfx.DrawString(disclaimer(i), font3, XBrushes.Black, New XRect(30, y(11) + (i * 10), page.Width.Point, page.Height.Point), XStringFormats.TopLeft)
                        End If
                    Next
                    doc.Save(filePath & fileName)
                    If System.Reflection.Assembly.GetEntryAssembly.GetName.Name.ToUpper.Contains("TERMINAL") Then
                        Process.Start(filePath & fileName)
                    End If
                    gfx = Nothing
                    Return True
                Else
                    Return False
                End If
                rd.Close()
            End Using
        Catch ex As MySqlException
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error with the SQL Connection/Query " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        Catch ex As Exception
            Call Error_Messager("Globals", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Error retrieving SQL data " & ex.Message, MsgBoxStyle.Critical, "Globals")
            Return False
        End Try
    End Function

#End Region

#Region "Other"

    Public Function Fill_Number(number As Integer, fillChar As String) As String
        Dim i As Integer
        Dim result As String = ""
        For i = 1 To number.ToString.Length - 2
            result &= "0"
        Next
        Return result
    End Function

    Public Function Format_MySQL_Date(dateString As String) As Object
        Dim result As Object
        Dim dateValue As Date
        If dateString = "" Then
            result = DBNull.Value
        ElseIf Date.TryParse(dateString, dateValue) = False Then
            result = DBNull.Value
        Else
            result = Format(dateValue, "yyyy-MM-dd HH:mm:ss")
        End If
        Return result
    End Function

    Public Sub WaitForM(ByVal interval As Integer)
        Dim sw As New Stopwatch
        sw.Start()
        Do While sw.ElapsedMilliseconds < interval
            Application.DoEvents()
        Loop
        sw.Stop()
    End Sub

    Public Function Match_Railcar(railcar_name As String) As Integer
        Dim i As Integer
        Dim db_id As Integer = 0
        Try
            For i = 0 To railcar_name.Length - 1
                If IsNumeric(railcar_name.Substring(i, 1)) Then
                    Exit For
                End If
            Next
            If i > 0 Then
                db_id = Val(Universals.Get_SQL_Value("SELECT id FROM storage WHERE active=true AND type <>'SILO' AND name LIKE '%" & Strings.Left(railcar_name, i).Trim & "%' AND name LIKE '%" & CInt(Strings.Right(railcar_name, Strings.Len(railcar_name) - i)) & "%';"))
            End If
            Return db_id
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function Format_Rail_Name(strRail As String, addZeroes As Boolean, addSpaces As Boolean, addSpace As Boolean) As String
        Dim i As Integer
        Dim pre As String = ""
        Dim suf As String = ""
        Dim mid As Integer = 0
        Try
            For i = 0 To strRail.Length - 1
                If IsNumeric(strRail.Substring(i, 1)) Then
                    mid = i
                    Exit For
                End If
            Next
            If mid > 0 Then
                pre = strRail.Substring(0, mid)
                suf = strRail.Substring(mid, strRail.Length - mid)
            End If
            If addZeroes Then
                suf = CInt(suf).ToString("D6")
            Else
                suf = CInt(suf).ToString
            End If
            If addSpaces Then
                pre = pre.PadRight(4)
            Else
                pre = pre.Trim
            End If
            If addSpace Then
                Return pre & " " & suf
            Else
                Return pre & suf
            End If
        Catch ex As Exception
            Return strRail
        End Try
    End Function

    Private Function Get_QR_Version(qr_length As Integer) As Integer
        If qr_length > 650 Then
            Return 25
        ElseIf qr_length > 620 Then
            Return 20
        ElseIf qr_length > 555 Then
            Return 19
        ElseIf qr_length > 500 Then
            Return 18
        ElseIf qr_length > 445 Then
            Return 17
        ElseIf qr_length > 405 Then
            Return 16
        ElseIf qr_length > 360 Then
            Return 15
        ElseIf qr_length > 325 Then
            Return 14
        ElseIf qr_length > 285 Then
            Return 13
        ElseIf qr_length > 245 Then
            Return 12
        ElseIf qr_length > 205 Then
            Return 11
        ElseIf qr_length > 175 Then
            Return 10
        ElseIf qr_length > 145 Then
            Return 9
        ElseIf qr_length > 115 Then
            Return 8
        ElseIf qr_length > 100 Then
            Return 7
        ElseIf qr_length > 80 Then
            Return 6
        Else
            Return 5
        End If
    End Function

#End Region

End Module
