Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.Security
Imports System.Net.WebSockets
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports SIPSorcery.Media
Imports SIPSorcery.SIP
Imports SIPSorcery.SIP.App
Imports SIPSorceryMedia.Windows

Public Class IntercomForm
    Dim client As New HttpClient()
    Private webSocket As ClientWebSocket
    Private cts As CancellationTokenSource

    Private sipTransport As SIPTransport
    Private userAgent As SIPUserAgent
    Private windowsAudio As WindowsAudioEndPoint

    Private activeCallAgent As SIPUserAgent
    Private activeServerAgent As SIPServerUserAgent

    Private isAppInitiatingCall As Boolean = False

    Private sipService As SIPService


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        sipService = New SIPService()
        AddHandler sipService.CallStatusChanged, AddressOf UpdateCallStatus
        sipService.StartListening()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' First hang up any active calls
        If sipService IsNot Nothing Then
            sipService.HangUp()
        End If

        ' NOW it is safe to completely shut down the network listener
        If sipTransport IsNot Nothing Then
            sipTransport.Shutdown()
        End If

        ' Force all background threads to die immediately so Port 5060 is released!
        Environment.Exit(0)
    End Sub

    'Updates the Status Label
    Private Sub UpdateCallStatus(status As String)

        If InvokeRequired Then
            Invoke(Sub()
                       lblStatus.Text = status
                   End Sub)
        Else
            lblStatus.Text = status
        End If
        If lblStatus.Text.Equals("Status: Call Active! (Audio Live)") Then
            lblStatus.ForeColor = Color.Green
        ElseIf lblStatus.Text.Equals("Status: Failed to auto-answer.") Or lblStatus.Text.Equals("Status: Failed to answer call.") Then
            lblStatus.ForeColor = Color.Red
            btnCallIntercom.Enabled = True
        ElseIf lblStatus.Text.Equals("Status: INCOMING CALL! (Ringing...)") Then
            lblStatus.ForeColor = Color.Orange
            btnAnswer.Enabled = True
        ElseIf lblStatus.Text.Equals("Status: Connected & Listening") Then
            lblStatus.ForeColor = Color.Green
            btnAnswer.Enabled = False
            btnCallIntercom.Enabled = True
        End If
    End Sub

#Region "Hiding in Background"
    'This forces the form to stay completely invisible on startup
    Protected Overrides Sub SetVisibleCore(ByVal value As Boolean)

        MyBase.SetVisibleCore(False)
    End Sub

    'Clicking on system tray icon opens IntercomForm
    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        Me.WindowState = FormWindowState.Normal
        MyBase.SetVisibleCore(True)
    End Sub
#End Region

#Region "Buttons"
    Private Sub btnAnswer_Click(sender As Object, e As EventArgs) Handles btnAnswer.Click
        ' Safety check
        btnAnswer.Enabled = False
        lblStatus.Text = "Status: Connecting Audio..."
        sipService.AnswerCall()
    End Sub

    Private Sub btnHangUp_Click(sender As Object, e As EventArgs) Handles btnHangUp.Click
        sipService.HangUp()
    End Sub

    Private Async Sub btnCallIntercom_Click(sender As Object, e As EventArgs) Handles btnCallIntercom.Click
        Try
            btnCallIntercom.Enabled = False
            lblStatus.Text = "Status: Waking up Intercom..."
            lblStatus.ForeColor = Color.Orange

            ' Set the flag so the incoming call event knows to auto-answer
            isAppInitiatingCall = True
            Debug.WriteLine("Setting flag TRUE")

            ' Fire the HTTP pulse to trigger the intercom's action rule
            Await sipService.ActivateVirtualInput()
        Catch ex As Exception
            MessageBox.Show($"Error triggering call: {ex.Message}")
            btnCallIntercom.Enabled = True
            isAppInitiatingCall = False
        End Try
    End Sub
#End Region
End Class
