Imports System.Net
Imports System.Net.Http
'----------------------------------------
Imports System.Net.Sockets
Imports System.Threading.Tasks
Imports SIPSorcery.Net
Imports System.Drawing
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


    '---------------------------------------------------------------

    Dim overlay As New StillOnCall

    '---------------------------------------------------------------

    Private Sub VideoFeed_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Core.Initialize()
        _libVLC = New LibVLC()
        _mediaPlayer = New MediaPlayer(_libVLC)
        VideoView1.MediaPlayer = _mediaPlayer
        Me.TopLevel = False
        Me.Dock = DockStyle.Fill
        videoPlay()

        '-----------------------------------------------------------


        overlay.StartPosition = FormStartPosition.Manual
        overlay.Location = New Point(25, 25)
        overlay.Size = New Size(50, 150)





        '-----------------------------------------------------------

    End Sub


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
        Dim cameraUrl As String = "rtsp://" & username & ":" & password & "@" & ipAddress & "/axis-media/media.amp?videocodec=h264&camera=1&resolution=640x480"
        Using media As New Media(_libVLC, New Uri(cameraUrl))
            _mediaPlayer.Play(media)
            _mediaPlayer.Mute = True
        End Using
    End Sub

    Private Sub VideoView1_Click(sender As Object, e As EventArgs) Handles VideoView1.Click
    End Sub

    Private Sub btnMute_Click(sender As Object, e As EventArgs) Handles btnMute.Click
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
        If btnConnection.Text = "Connect" Then
            MakeP2PIntercomCall(ipAddress)
            btnConnection.BackColor = Color.Firebrick
            btnConnection.Text = "Disconnect"
        Else
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


    Private Sub HangUpCall()
        If userAgent IsNot Nothing Then
            userAgent.Hangup()
        End If
        If voipMediaSession IsNot Nothing Then
            voipMediaSession.Close("Hangup")
            voipMediaSession = Nothing
        End If
    End Sub


    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs)
    End Sub

    Dim docked = True
    Private WithEvents popUpForm As Form
    Dim cams As New CameraView()


    Private isBackgroundVisible As Boolean = False


    Private Async Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        If docked = True Then
            popUpForm = New Form()
            CameraView.CameraPanel.Controls.Remove(Me)
            popUpForm.Text = "Detached Window"
            popUpForm.ClientSize = New Drawing.Size(300, 200)
            Me.Location = New Drawing.Point(50, 50)
            popUpForm.Controls.Add(Me)
            popUpForm.Show()
            docked = False
            Dim btnImage As Bitmap = CType(ToolStripButton2.Image.Clone(), Bitmap)
            btnImage.RotateFlip(RotateFlipType.Rotate180FlipNone)
            ToolStripButton2.Image = btnImage
        Else
            Dim targetForm As CameraView = CType(Application.OpenForms("CameraView"), CameraView)
            If targetForm IsNot Nothing Then
                Me.TopLevel = False
                Me.FormBorderStyle = FormBorderStyle.None
                Me.Dock = DockStyle.Fill
                targetForm.CameraPanel.Controls.Add(Me, 0, 0)
                Me.Show()
                popUpForm.Close()
            End If
            docked = True
            Dim btnImage As Bitmap = CType(ToolStripButton2.Image.Clone(), Bitmap)
            btnImage.RotateFlip(RotateFlipType.Rotate180FlipNone)
            ToolStripButton2.Image = btnImage
        End If
    End Sub

    Private Sub ToolStripButton3_Click(sender As Object, e As EventArgs) Handles ToolStripButton3.Click
        Dim targetForm As CameraView = CType(Application.OpenForms("CameraView"), CameraView)
        RemovePanelAndShiftUp(targetForm.CameraPanel, Me, ManageDevice)
    End Sub








    Public Sub RemovePanelAndShiftUp(tlp As TableLayoutPanel, panelToRemove As Control, mainForm As ManageDevice)
        popUpForm = New Form()
        CameraView.CameraPanel.Controls.Remove(Me)
        popUpForm.Text = "Detached Window"
        popUpForm.ClientSize = New Drawing.Size(300, 200)
        Me.Hide()
        Me.Location = New Drawing.Point(50, 50)
        popUpForm.Controls.Add(Me)
        popUpForm.Close()
        docked = False

        RemoveStuff(mainForm)
        'MessageBox.Show(ipAddress.ToString)

        'Dim remove As New ManageDevice
        'remove = ParentForm
        'If remove.openFeeds.ContainsKey(ipAddress.ToString) Then
        '    MessageBox.Show("Has yet to be removed.")
        'Else
        '    MessageBox.Show("Can't find it :(")
        'End If


        'remove.openFeeds.Remove(ipAddress)

        'If remove.openFeeds.ContainsKey(ipAddress.ToString) Then
        '    MessageBox.Show("It's still here!")
        'Else
        '    MessageBox.Show("Probably successfully removed.")
        'End If


    End Sub


    Private _mainForm As ManageDevice

    Public Sub RemoveStuff(ByVal parentForm As ManageDevice)

        ' This call is required by the designer.
        InitializeComponent()
        _mainForm = parentForm
        ' Add any initialization after the InitializeComponent() call.





        _mainForm.openFeeds.Remove(ipAddress)



    End Sub



    Private Sub ToolStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ToolStrip1.ItemClicked
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick



        'If btnMute.Text.ToString.Contains("Mute Mic") Then
        If btnMute.Text = "Mute Mic" Then
            overlay.BackColor = Color.LimeGreen
            overlay.TransparencyKey = Color.LimeGreen
            overlay.PictureBox1.BackColor = Color.LimeGreen
        Else
            overlay.BackColor = Color.Red
            overlay.TransparencyKey = Color.Red
            overlay.PictureBox1.BackColor = Color.Red
        End If


        'If btnConnection.Text.ToString.Contains("Disconnect") Then
        If btnConnection.Text = "Disconnect" Then
            'MessageBox.Show("hi")
            overlay.Show()
        Else
            overlay.Hide()
        End If


    End Sub
End Class