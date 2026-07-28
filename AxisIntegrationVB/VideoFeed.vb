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
Imports System.Configuration
Imports Windows.Media.Devices

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

            micDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0.9F

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
        'Dim p2p As New P2PConnection()
        If btnConnection.Text = "Connect" Then
            'p2p.ConnectToPeer(ipAddress, 5060)
            'p2p.StartListening(5060)
            'RaiseEvent ConnectionRequested(ipAddress)

            'Just to test and see if I can make it work differently -------------------------------------------------------------------

            'MessageBox.Show(ipAddress)

            'MakeCall(ipAddress, 5060)


            MakeP2PIntercomCall(ipAddress)

            ' --------------------------------------------------------------------------------------------------------------------------
            'Dim ansCall As New SIPService()
            'nsCall.ActivateVirtualInput(ipAddress)
            'ansCall.AnswerCall()

            btnConnection.BackColor = Color.Firebrick
            btnConnection.Text = "Disconnect"
        Else

            'Dim terminate As New SIPService
            'terminate.HangUp()
            'p2p.DisconnectPeer()
            HangUpCall()
            RaiseEvent DisconnectRequested()
            btnConnection.BackColor = Color.ForestGreen
            btnConnection.Text = "Connect"
        End If
    End Sub

    Private userAgent As SIPUserAgent
    Private voipMediaSession As VoIPMediaSession


    Private Async Function MakeP2PIntercomCall(intercomIpAddress As String) As Task

        Dim destUri As String = $"sip:{ipAddress}"
        Dim sipTransport As New SIPTransport()
        userAgent = New SIPUserAgent(sipTransport, Nothing)

        Dim winAudioEP As New WindowsAudioEndPoint(New AudioEncoder())
        'voipMediaSession = New VoIPMediaSession(winAudioEP.ToMediaEndPoints())

        'voipMediaSession.AcceptRtpFromAny = True

        'Dim callSuccess As Boolean = Await userAgent.Call(destUri, Nothing, Nothing, voipMediaSession)

        'If callSuccess Then
        '    MessageBox.Show("Intercom call connected successfully.")
        '    voipMediaSession.AcceptRtpFromAny = False
        'Else
        '    MessageBox.Show("Intercom call failed to connect.")
        'End If
        Dim audioSourceTrack As New MediaStreamTrack(winAudioEP.GetAudioSourceFormats(), MediaStreamStatusEnum.SendOnly)

        Dim audioSinkTrack As New MediaStreamTrack(winAudioEP.GetAudioSinkFormats(), MediaStreamStatusEnum.RecvOnly)

        Dim customMediaEndPoints As New MediaEndPoints() With {
            .AudioSource = winAudioEP,
            .AudioSink = winAudioEP
        }


        voipMediaSession = New VoIPMediaSession(customMediaEndPoints)
        voipMediaSession.addTrack(audioSourceTrack)
        voipMediaSession.addTrack(audioSinkTrack)


        voipMediaSession.AcceptRtpFromAny = False

        Dim callSuccess As Boolean = Await userAgent.Call(destUri, Nothing, Nothing, voipMediaSession)

        If callSuccess Then
            MessageBox.Show("Intercom call connected successfuly.")
        Else
            MessageBox.Show("Intercom call failed to connect.")
        End If


    End Function


    Dim client As TcpClient

    Sub MakeCall(ipAddress As String, port As Integer)

        Try
            client = New TcpClient()
            client.Connect(ipAddress, port)
            Dim stream As NetworkStream = client.GetStream()
        Catch ex As Exception
            MessageBox.Show("Connection failed: " & ex.Message)
        End Try

    End Sub



    Public Sub DisconnectCall()
        If client IsNot Nothing Then

            If client.Connected Then

                client.GetStream().Close()

            End If

            client.Close()
            client = Nothing

        End If

    End Sub

    Private Sub HangUpCall()

        If userAgent IsNot Nothing Then
            userAgent.Hangup()
        End If

        If voipMediaSession IsNot Nothing Then
            voipMediaSession.Close("Hangup")
            voipMediaSession = Nothing
        End If

    End Sub





    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        Me.Hide()
    End Sub
End Class