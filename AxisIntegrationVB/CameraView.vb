Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography.X509Certificates
Imports FFmpeg.AutoGen
'--------
Imports MySql.Data.MySqlClient
'--------

Public Class CameraView
    Private connectionString As String = "server=wills_vnc_connection;user=willsVncConnection;password=TMT$olutions;database=vnc_view_schema.list_device;"
    Public Event CameraViewClosed()
    Private CameraPanel As New TableLayoutPanel With {
    .ColumnCount = 1,
        .Dock = System.Windows.Forms.DockStyle.Fill,
        .Location = New System.Drawing.Point(3, 43),
        .Name = "TableLayoutPanel2",
        .RowCount = 1,
        .Size = New System.Drawing.Size(794, 404),
        .TabIndex = 2,
        .BackColor = SystemColors.ControlDarkDark
    }
    Private audioSwitch As Boolean = False

    Public Sub New()
        InitializeComponent()
        Me.IsMdiContainer = True
        CameraPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        CameraPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Try
            Me.Controls.Add(CameraPanel)
        Catch
        End Try
    End Sub

    Private Sub CameraView_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        'EndCall()
        RaiseEvent CameraViewClosed()
    End Sub

    Private Sub CameraView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.KeyPreview = True
    End Sub

    Private Sub CameraView_Shown(sender As Object, e As EventArgs) Handles Me.Shown
    End Sub

    Private Sub CameraView_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        'If Not Cams.Any() Then Return
        'If Cams(0) Is Nothing Then Return
        If e.KeyCode = Keys.D1 Then
            audioSwitch = Not audioSwitch
            If audioSwitch Then
                'StartCall()
            Else
                'EndCall()
            End If
            MessageBox.Show($"Audio On: {audioSwitch}")
        End If
    End Sub


    'rtsp://<willTestCam>:<root>@<host>/axis-media/media.amp
    'rtsp://<willTestCam>:<root>@<host>/axis-media/media.amp?resolution=640x480

    'Public Sub Axis_Init(host As String)
    '    Try
    '        'Create AMC Object In the Viewer
    '        'Dim newAMC As AxAXISMEDIACONTROLLib.AxAxisMediaControl = Globals.CreateCtrl(Of AxAXISMEDIACONTROLLib.AxAxisMediaControl)(CameraPanel)
    '        'Initialized all URLs
    '        'newAMC.AudioConfigURL = $"http://{host}/axis-cgi/view/param.cgi?group=Audio,AudioSource"
    '        'newAMC.AudioReceiveURL = $"http://{host}/axis-cgi/audio/receive.cgi"
    '        'newAMC.AudioTransmitURL = $"http://{host}/axis-cgi/audio/transmit.cgi"
    '        'Add Authentication Parameters
    '        If host.Equals("192.168.0.57") Then
    '            'newAMC.MediaUsername = "tree"
    '            'newAMC.MediaPassword = "pineCone3"
    '        Else
    '            'newAMC.MediaUsername = "willTestCam"
    '            'newAMC.MediaPassword = "root"
    '        End If
    '        'newAMC.MediaURL = $"http://{host}/axis-cgi/mjpg/video.cgi"
    '        Console.WriteLine($"http://{host}/axis-cgi/mjpg/video.cgi")
    '        'Change some Configurations
    '        newAMC.EnableAreaZoom = True
    '        newAMC.ShowStatusBar = True
    '        newAMC.ShowToolbar = True
    '        newAMC.StretchToFit = True
    '        newAMC.MaintainAspectRatio = True
    '        newAMC.Dock = DockStyle.Fill
    '        'Play Camera and Start Recieving Audio
    '        Cams.Add(newAMC)
    '        newAMC.Play()
    '        newAMC.AudioReceiveStart()
    '    Catch ex As Exception
    '        Console.WriteLine("Error initializing axis device. " & ex.Message)
    '    End Try
    'End Sub

    'Private Sub StartCall()
    '    Cams(0).AudioTransmitURL = "http://admin:root@192.168.0.195/axis-cgi/audio/transmit.cgi"
    '    Cams(0).StartTransmitMedia("C:\test.wav", 0)
    'End Sub

    'Private Sub EndCall()
    '    Cams(0).AudioTransmitStop()

    'End Sub

    '-----------------------------------------------------------------------------------------------------------------
    'New code from Glade to make the video appear


    'Private Sub btnPlay_Click(sender As Object, e As EventArgs)

    '    Dim username = "willTestCam"
    '    Dim password = "root"
    '    Dim ipAddress = "192.168.0.208"
    '    Dim cameraUrl = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

    '    Using media As New Media(_libVLC, New Uri(cameraUrl))
    '        _mediaPlayer.Play(media)
    '    End Using

    'End Sub

    'for when we integrate not needing to press a button
    'Private Sub camPlay(user As String, passW As String, ip As String)

    '    Dim username As String = user
    '    Dim password As String = passW
    '    Dim ipAddress As String = ip
    '    Dim cameraUrl As String = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

    '    Using media As New Media(_libVLC, New Uri(cameraUrl))
    '        _mediaPlayer.Play(media)
    '    End Using
    'End Sub

    Public rowNum As Integer
    Public Function Video_Init(rowIndex As Integer, ip As String) As VideoFeed
        'Core.Initialize()
        '_libVLC = New LibVLC()
        '_mediaPlayer = New MediaPlayer(_libVLC)
        ''Dim newAMC As AxAXISMEDIACONTROLLib.AxAxisMediaControl = Globals.CreateCtrl(Of AxAXISMEDIACONTROLLib.AxAxisMediaControl)(CameraPanel)
        'Dim viewFeed As VideoView = Globals.CreateCtrl(Of VideoView)(CameraPanel)
        'viewFeed.MediaPlayer = _mediaPlayer
        'camPlay("willTestCam", "root", "192.168.0.208")
        rowNum = rowIndex
        Dim subForm As VideoFeed = Globals.CreateCtrl(Of VideoFeed)(CameraPanel)
        subForm.TopLevel = False
        subForm.ipAddress = ip
        subForm.FormBorderStyle = FormBorderStyle.None
        subForm.ToolStrip1.Left = subForm.ClientSize.Width - subForm.ToolStrip1.Width
        Dim subformName As String = ""

        '------------------------------------------------------------------------------------

        Dim connString As String = MYSQLCS
        Dim query As String = "SELECT name FROM vnc_view_schema.list_device WHERE id = @id"
        Using conn As New MySqlConnection(connString)
            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@id", rowIndex)
                Try
                    conn.Open()
                    Dim result As Object = cmd.ExecuteScalar()
                    If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                        Dim targetValue As String = result.ToString()
                        subformName = targetValue
                        'MessageBox.Show("Value: " & targetValue)
                    Else
                        MessageBox.Show("No data found.")

                    End If
                Catch ex As Exception
                    MessageBox.Show("Error: " & ex.InnerException.Message)
                End Try
            End Using
        End Using
        '-----------------------------------------------------------------------------------
        subForm.Text = subformName
        subForm.Show()
        Return subForm
    End Function
End Class
