Imports System.Net
Imports System.Net.Http
'----------------------------------------
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports SIPSorcery.Net
'----------------------------------------
Imports LibVLCSharp.Shared
Imports LibVLCSharp.WinForms
Imports MySql.Data.MySqlClient
Imports Mysqlx
Imports NAudio.CoreAudioApi
Imports SIPSorcery.Media
Imports SIPSorcery.SIP
Imports SIPSorcery.SIP.App
Imports SIPSorceryMedia.Windows
Imports Windows.ApplicationModel.Calls
Imports Windows.Media.Capture
Imports Windows.Security.Authentication.Identity.Core
Imports SIPSorceryMedia.Abstractions

Public Class VideoFeed

    Private _libVLC As LibVLC
    Private _mediaPlayer As MediaPlayer
    Public Property ipAddress As String
    Public Event ConnectionRequested(ipAddress As String)
    Public Event DisconnectRequested()
    Private Sub VideoFeed_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Core.Initialize()
        _libVLC = New LibVLC()
        _mediaPlayer = New MediaPlayer(_libVLC)
        VideoView1.MediaPlayer = _mediaPlayer
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        videoPlay()
    End Sub



    'Private Sub btnPlay_Click(sender As Object, e As EventArgs) Handles btnPlay.Click


    '    Dim username As String = "willTestCam"
    '    Dim password As String = "root"
    '    Dim ipAddress As String = "192.168.0.208"
    '    Dim cameraUrl As String = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

    '    Using media As New Media(_libVLC, New Uri(cameraUrl))
    '        _mediaPlayer.Play(media)
    '    End Using

    'End Sub



    Private Sub VideoFeed_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If _mediaPlayer IsNot Nothing Then
            _mediaPlayer.Dispose()
        End If

        If _libVLC IsNot Nothing Then
            _libVLC.Dispose()
        End If
    End Sub

    Private Sub videoPlay()
        If ipAddress Is Nothing Then
            ipAddress = "1234"
        End If

        Dim username As String = "willTestCam"
        Dim password As String = "root"
        'Dim ipAddress As String = "192.168.0.208"
        Dim cameraUrl As String = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"

        Using media As New Media(_libVLC, New Uri(cameraUrl))
            _mediaPlayer.Play(media)
            _mediaPlayer.Mute = True
        End Using

    End Sub

    Private Sub VideoView1_Click(sender As Object, e As EventArgs) Handles VideoView1.Click

    End Sub

    Private Sub btnMute_Click(sender As Object, e As EventArgs) Handles btnMute.Click

        'Dim muteObj As New SIPService()
        'Dim result As New String(muteObj.ControlAudioSub())
        'btnMute.Text = result

        Try

            Dim enumerator As New MMDeviceEnumerator()

            Dim micDevice As MMDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia)

            Dim currentMute As Boolean = micDevice.AudioEndpointVolume.Mute
            micDevice.AudioEndpointVolume.Mute = Not currentMute

            If micDevice.AudioEndpointVolume.Mute Then
                btnMute.Text = "Unmute Mic"
            Else
                btnMute.Text = "Mute Mic"
            End If


        Catch ex As Exception
            MessageBox.Show("Error toggling microphone: " & ex.Message)
        End Try


    End Sub

    Dim supService As SIPService


    Private Async Sub btnConnection_Click(sender As Object, e As EventArgs) Handles btnConnection.Click
        If btnConnection.Text = "Connect" Then
            'RaiseEvent ConnectionRequested(ipAddress)

            'Just to test and see if I can make it work differently -------------------------------------------------------------------






            MakeP2PIntercomCall(ipAddress)

            ' --------------------------------------------------------------------------------------------------------------------------
            'Dim ansCall As New SIPService()
            'nsCall.ActivateVirtualInput(ipAddress)
            'ansCall.AnswerCall()

            btnConnection.BackColor = Color.Firebrick
            btnConnection.Text = "Disconnect"
        Else
            RaiseEvent DisconnectRequested()
            btnConnection.BackColor = Color.ForestGreen
            btnConnection.Text = "Connect"
        End If
    End Sub


    Private Async Function MakeP2PIntercomCall(intercomIpAddress As String) As Task

        Dim destUri As String = $"sip:{ipAddress}"
        Dim sipTransport As New SIPTransport()
        Dim userAgent As New SIPUserAgent(sipTransport, Nothing)

        Dim winAudioEP As New WindowsAudioEndPoint(New AudioEncoder())
        Dim voipMediaSession As New VoIPMediaSession(winAudioEP.ToMediaEndPoints())

        voipMediaSession.AcceptRtpFromAny = True

        Dim callSuccess As Boolean = Await userAgent.Call(destUri, Nothing, Nothing, voipMediaSession)

        If callSuccess Then
            MessageBox.Show("Intercom call connected successfully.")
        Else
            MessageBox.Show("Intercom call failed to connect.")
        End If



    End Function




    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Me.Hide()
    End Sub
End Class