Imports System.Net
Imports System.Net.Sockets

Public Class P2PConnection

    Private client As TcpClient
    Private listener As TcpListener

    Public Sub ConnectToPeer(ipAddress As String, port As Integer)

        Try

            client = New TcpClient()
            client.Connect(ipAddress, port)
            MessageBox.Show("Connected to peer.")

        Catch ex As Exception

            MessageBox.Show("Connection failed: " & ex.Message)

        End Try


    End Sub

    Public Sub StartListening(port As Integer)

        Try

            listener = New TcpListener(IPAddress.Any, port)
            listener.Start()
            client = listener.AcceptTcpClient()
            MessageBox.Show("Incoming peer call accepted.")

        Catch ex As Exception

            MessageBox.Show("Listener error: " & ex.Message)

        End Try

    End Sub

    Public Sub DisconnectPeer()

        Try

            If client IsNot Nothing Then

                client.Close()
                client.Dispose()
                client = Nothing

            End If

            If listener IsNot Nothing Then

                listener.Stop()
                listener = Nothing

            End If


            MessageBox.Show("Disconnected successfully.")
        Catch ex As Exception
            MessageBox.Show("Error during disconnect: " & ex.Message)
        End Try

    End Sub



End Class
