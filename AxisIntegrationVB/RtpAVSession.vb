Imports System.Net.Sockets

Friend Class RtpAVSession
    Private audio As SIPSorcery.Net.SDPMediaTypesEnum
    Private sDPApplicationMediaFormat As SIPSorcery.Net.SDPApplicationMediaFormat
    Private interNetwork As AddressFamily

    Public Sub New(audio As SIPSorcery.Net.SDPMediaTypesEnum, sDPApplicationMediaFormat As SIPSorcery.Net.SDPApplicationMediaFormat, interNetwork As AddressFamily)
        Me.audio = audio
        Me.sDPApplicationMediaFormat = sDPApplicationMediaFormat
        Me.interNetwork = interNetwork
    End Sub
End Class
