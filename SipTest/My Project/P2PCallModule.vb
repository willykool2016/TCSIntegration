Imports System.Net
Imports System.Threading.Tasks
Imports SIPSorcery.Media
Imports SIPSorcery.SIP.App
Imports SIPSorceryMedia.Windows

Module P2PCallModule

    Public Async Sub MakeCall()

        Dim dest As String = "sip:intercom@192.168.0.208:5060"
        Dim userAgent As New SIPUserAgent()

        Dim winAudio As New WindowsAudioEndPoint(New AudioEncoder())

        Dim voipSession As New VoIPMediaSession(winAudio.ToMediaEndPoints())
        voipSession.AcceptRtpFromAny = True

        Dim callSuccess As Boolean = Await userAgent.Call(dest, Nothing, Nothing, voipSession)

        If callSuccess Then
            MessageBox.Show("Intercom call connected.")
        Else
            MessageBox.Show("Intercom call failed.")
        End If

    End Sub

End Module
