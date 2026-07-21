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
Imports Org.BouncyCastle.Asn1.Cmp
Imports SIPSorcery.Media
Imports SIPSorcery.SIP
Imports SIPSorcery.SIP.App
Imports SIPSorceryMedia.Windows

Public Class SIPService
    Dim client As New HttpClient()
    Private webSocket As ClientWebSocket
    Private cts As CancellationTokenSource

    Private sipTransport As SIPTransport
    Private userAgent As SIPUserAgent
    Private windowsAudio As WindowsAudioEndPoint

    Private activeCallAgent As SIPUserAgent
    Private activeServerAgent As SIPServerUserAgent

    Private isAppInitiatingCall As Boolean = False

    Public Event CallStatusChanged(status As String)
    Public Event IncomingCallReceived()

    'Automatically listens for incoming traffic
    Public Sub StartListening()
        Try
            ' 1. Set up the SIP Transport
            sipTransport = New SIPTransport()

            ' Explicitly tell SIPSorcery to open and listen on UDP Port 5060
            Dim sipChannel As New SIPUDPChannel(New Net.IPEndPoint(Net.IPAddress.Any, 5060))
            sipTransport.AddSIPChannel(sipChannel)
            Debug.WriteLine("Listening on UDP 5060")

            ' 2. Create the SIP User Agent (Your Softphone)
            userAgent = New SIPUserAgent(sipTransport, Nothing)

            AddHandler userAgent.ClientCallFailed, Sub(ua, err, res)
                                                       MessageBox.Show($"Call Failed! Reason: {err}")
                                                   End Sub

            ' 3. Tell the agent what to do when an incoming call arrives
            'AddHandler userAgent.OnIncomingCall, AddressOf Intercom_Ringing
            AddHandler userAgent.OnIncomingCall,
                Sub(ua, req)
                    Debug.WriteLine("=== SIP USER AGENT RECEIVED INCOMING CALL ===")
                    Intercom_Ringing(ua, req)
                End Sub
        Catch ex As Exception
            MessageBox.Show($"Failed to start listening: {ex.Message}")
        End Try
    End Sub

    ' Update the Ringing event to HOLD the call instead of answering it
    Public Async Sub Intercom_Ringing(ua As SIPUserAgent, req As SIPRequest)
        Debug.WriteLine("Incoming call detected!")
        Try
            activeCallAgent = ua
            activeServerAgent = ua.AcceptCall(req)

            If isAppInitiatingCall Then
                ' We triggered this via HTTP, so auto-answer the audio immediately!
                isAppInitiatingCall = False

                RaiseEvent CallStatusChanged("Status: Connecting Audio...")

                windowsAudio = New WindowsAudioEndPoint(New AudioEncoder)
                Dim voipMediaSession As New VoIPMediaSession(windowsAudio.ToMediaEndPoints)
                Dim answered = Await activeCallAgent.Answer(activeServerAgent, voipMediaSession)

                If answered Then
                    RaiseEvent CallStatusChanged("Status: Call Active! (Audio Live)")
                Else
                    RaiseEvent CallStatusChanged("Status: Failed to auto-answer.")
                End If

            Else
                ' Normal incoming call (someone physically pushed the button outside)
                RaiseEvent CallStatusChanged("Status: INCOMING CALL! (Ringing...)")
                RaiseEvent IncomingCallReceived()
            End If
        Catch ex As Exception
            RaiseEvent CallStatusChanged($"Status: Ring Error - {ex.Message}")
            Debug.WriteLine($"Error I'm Getting: {ex.Message}")
        End Try
    End Sub

    'Activate Led when call made by computer
    Public Async Function ActivateVirtualInput() As Task
        Dim deactivateUrl As String = "http://192.168.0.208/axis-cgi/virtualinput/deactivate.cgi?schemaversion=1&port=1"
        Dim activateUrl As String = "http://192.168.0.208/axis-cgi/virtualinput/activate.cgi?schemaversion=1&port=1"

        Dim handler As New HttpClientHandler()
        handler.Credentials = New NetworkCredential("willTestCam", "root")

        Using client As New HttpClient(handler)
            Try
                ' 1. Force the switch OFF just in case it got stuck ON previously
                Await client.GetAsync(deactivateUrl)

                ' Slight delay to let the Axis state machine process the transition
                Await Task.Delay(200)

                ' 2. Turn the switch ON (This triggers the Axis Action Rule to call us)
                Dim response = Await client.GetAsync(activateUrl)
                Dim body = Await response.Content.ReadAsStringAsync()

                Debug.WriteLine("Virtual Input Activate Response:")
                Debug.WriteLine(body)

                If Not response.IsSuccessStatusCode Then
                    MessageBox.Show($"Failed to trigger intercom: {response.StatusCode} - {body}")
                End If

            Catch ex As Exception
                MessageBox.Show($"HTTP Request Error: {ex.Message}")
                ' Reset flag if the HTTP request failed so we don't accidentally auto-answer a real visitor
                isAppInitiatingCall = False
            End Try
        End Using
    End Function

    'Answer the incoming call
    Public Async Sub AnswerCall()
        If activeCallAgent Is Nothing OrElse activeServerAgent Is Nothing Then Return
        Try
            ' Lock the button so it can't be double-clicked

            ' 1. Set up the Audio endpoints (Mic and Speakers)
            windowsAudio = New WindowsAudioEndPoint(New AudioEncoder)
            Dim voipMediaSession As New VoIPMediaSession(windowsAudio.ToMediaEndPoints)
            ' 2. Formally Answer the call and open the 2-way audio!
            Dim answered = Await activeCallAgent.Answer(activeServerAgent, voipMediaSession)
            If answered Then
                RaiseEvent CallStatusChanged("Status: Call Active! (Audio Live)")
            Else
                RaiseEvent CallStatusChanged("Status: Failed to answer call.")
            End If
        Catch ex As Exception
            MessageBox.Show($"Error answering call: {ex.Message}")
            RaiseEvent CallStatusChanged("Status: Failed to answer call.")
        End Try
    End Sub

    'Where the code to make the hang up button and form closing hangup occur
    Public Sub HangUp()
        Try
            ' 1. Hang up only the active call, do NOT shut down the sipTransport!
            If activeCallAgent IsNot Nothing Then
                activeCallAgent.Hangup()
            End If

            ' 2. Clean up the audio devices
            If windowsAudio IsNot Nothing Then
                windowsAudio.CloseAudio()
            End If

            ' 3. Reset our variables for the next call
            activeCallAgent = Nothing
            activeServerAgent = Nothing

            ' 4. Reset the UI
            RaiseEvent CallStatusChanged("Status: Connected & Listening")

        Catch ex As Exception
            MessageBox.Show($"Error hanging up: {ex.Message}")
        End Try
    End Sub
End Class
