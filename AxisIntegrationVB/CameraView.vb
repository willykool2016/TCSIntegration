Imports FFmpeg.AutoGen

'-----------------------
Imports LibVLCSharp.Shared
Imports LibVLCSharp.WinForms

'----------------

Public Class CameraView
    Private connectionString As String = "server=localhost;user=root;password=TMT$olutions;database=vnc_view_schema.list_device;"
    'Private ReadOnly Cams As New List(Of AxAXISMEDIACONTROLLib.AxAxisMediaControl)
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

        '------------------------ Glade

        If _mediaPlayer IsNot Nothing Then
            _mediaPlayer.Dispose()
        End If

        If _libVLC IsNot Nothing Then
            _libVLC.Dispose()
        End If

        '------------------------- Glade
    End Sub

    '----------------------- Glade
    Private _libVLC As LibVLC
    Private _mediaPlayer As MediaPlayer
    '------------------------- Glade

    Private Sub CameraView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.KeyPreview = True

        'New code from Glade ----------------
        'Core.Initialize()
        '_libVLC = New LibVLC()
        '_mediaPlayer = New MediaPlayer(_libVLC)
        'VideoView1.MediaPlayer = _mediaPlayer
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


    Private Sub btnPlay_Click(sender As Object, e As EventArgs)

        Dim username = "willTestCam"
        Dim password = "root"
        Dim ipAddress = "192.168.0.208"
        Dim cameraUrl = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

        Using media As New Media(_libVLC, New Uri(cameraUrl))
            _mediaPlayer.Play(media)
        End Using

    End Sub

    'for when we integrate not needing to press a button
    Private Sub camPlay(user As String, passW As String, ip As String)

        Dim username As String = user
        Dim password As String = passW
        Dim ipAddress As String = ip
        Dim cameraUrl As String = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

        Using media As New Media(_libVLC, New Uri(cameraUrl))
            _mediaPlayer.Play(media)
        End Using
    End Sub


    Public Sub Video_Init()
        'Core.Initialize()
        '_libVLC = New LibVLC()
        '_mediaPlayer = New MediaPlayer(_libVLC)
        ''Dim newAMC As AxAXISMEDIACONTROLLib.AxAxisMediaControl = Globals.CreateCtrl(Of AxAXISMEDIACONTROLLib.AxAxisMediaControl)(CameraPanel)

        'Dim viewFeed As VideoView = Globals.CreateCtrl(Of VideoView)(CameraPanel)
        'viewFeed.MediaPlayer = _mediaPlayer

        'camPlay("willTestCam", "root", "192.168.0.208")

        Dim subForm As VideoFeed = Globals.CreateCtrl(Of VideoFeed)(CameraPanel)
        subForm.TopLevel = False
        subForm.Show()


    End Sub

End Class
