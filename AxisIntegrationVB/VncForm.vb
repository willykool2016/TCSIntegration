Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports MySql.Data.MySqlClient
Imports Mysqlx.XDevAPI
Imports RemoteViewing.Vnc
Imports RemoteViewing.Windows.Forms
Imports System.Drawing.Imaging
Imports System.Drawing.Drawing2D
Partial Public Class VncForm
    Inherits System.Windows.Forms.Form

#Region "Initial Var and Constructors"
    Private client As New VncClient()
    Private connectionString As String = "server=localhost;user=root;password=TMT$olutions;database=vnc_view_schema.list_device;"
    Private ManageDevice As ManageDevice
    Private NumVnc As Integer = 0
    Private MaxVncCount As Integer = 3
    Private TotalPanel As New TableLayoutPanel With {
        .ColumnCount = 1,
        .Dock = System.Windows.Forms.DockStyle.Fill,
        .Location = New System.Drawing.Point(3, 43),
        .Name = "TableLayoutPanel2",
        .RowCount = 1,
        .Size = New System.Drawing.Size(794, 404),
        .TabIndex = 2,
        .BackColor = SystemColors.ControlDarkDark
        }
    Private ReadOnly Clients As New List(Of VncClient)
    Private Structure VncSession
        Public Client As VncClient
        Public Control As VncViewerControl
    End Structure
    Private Sessions As New List(Of VncSession)

    Public Sub New()
        InitializeComponent()
        Me.IsMdiContainer = True
        ' Reference the existing ManageDevice main form if it is already open
        ManageDevice = Application.OpenForms.OfType(Of ManageDevice)().FirstOrDefault()
        If ManageDevice IsNot Nothing Then
            MaxVncCount = If(ManageDevice.dgvDevice.RowCount > 9, 4, 3)
        End If
        Dim type = GetType(RemoteViewing.Vnc.VncClient)
        Debug.WriteLine("Members of " & type.FullName)
        For Each m In type.GetMembers(BindingFlags.[Public] Or BindingFlags.Instance Or BindingFlags.[Static])
            Debug.WriteLine(m.MemberType, " ", m)
        Next
        TotalPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        TotalPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Try
            Me.Controls.Add(TotalPanel)
        Catch
        End Try
    End Sub
#End Region

#Region "Create Connection"
    Public Async Function CreateConnection(ByVal host As String, ByVal pass As String) As Task
        If IsDisposed OrElse Not IsHandleCreated Then
            Return
        End If
        'Dim ignore = Me.Handle
        'Initialize connection parameters
        Dim hostname As String = host
        Dim port As Integer = 5900
        Dim password As String = pass
        Dim options = New RemoteViewing.Vnc.VncClientConnectOptions With {
            .Password = password.ToCharArray()
        }
        ' Run connect asynchronously
        Try
            Dim client As New VncClient()
            Dim VncControl As VncViewerControl = Globals.CreateCtrl(Of VncViewerControl)(TotalPanel)
            Sessions.Add(New VncSession With {
                .Client = client,
                .Control = VncControl
            })
            Debug.WriteLine($"Form: {Me.ClientSize}")
            Debug.WriteLine($"Grid: {TotalPanel.ClientSize}")
            Debug.WriteLine($"VNC: {VncControl.ClientSize}")
            For Each item As VncSession In Sessions
                Console.WriteLine($"Control: {item.Control.ToString()}, Name: {item.Client.ToString()}")
            Next

            If VncControl Is Nothing Then
                MessageBox.Show("Creation Failed! The Monster Emerges!")
            End If
            Dim firstFrameTcs As New TaskCompletionSource(Of Boolean)
            AddHandler client.Connected, Sub(s, ev)
                                             Debug.WriteLine($"FramebufferChanged: disposed={VncControl.IsDisposed}, handle={VncControl.IsHandleCreated}")
                                             SafeInvoke(CType((Sub()
                                                                   Dim fb = client.Framebuffer
                                                                   'If fb IsNot Nothing AndAlso fb.Width > 0 AndAlso fb.Height > 0 Then
                                                                   '    VncControl.Client = client
                                                                   '    'VncControl.Width = fb.Width
                                                                   '    'VncControl.Height = fb.Height
                                                                   '    VncControl.Invalidate()
                                                                   'End If
                                                                   VncControl.Client = client
                                                                   VncControl.Width = fb.Width
                                                                   VncControl.Height = fb.Height
                                                                   VncControl.Invalidate()
                                                               End Sub), Action))
                                         End Sub
            AddHandler client.FramebufferChanged, Sub(s, ev)
                                                      If VncControl.IsDisposed Then Return
                                                      If Not VncControl.IsHandleCreated Then Return
                                                      Dim currentClient = DirectCast(s, VncClient)
                                                      If Not firstFrameTcs.Task.IsCompleted Then
                                                          firstFrameTcs.TrySetResult(True)
                                                      End If
                                                      SafeInvoke(Sub()
                                                                     If VncControl.Client Is Nothing Then
                                                                         VncControl.Client = currentClient
                                                                     End If
                                                                     Dim wid = VncControl.Client.Framebuffer.Width()
                                                                     Dim hei = VncControl.Client.Framebuffer.Width()
                                                                     VncControl.Width = wid
                                                                     VncControl.Height = hei
                                                                     VncControl.Invalidate()
                                                                 End Sub)
                                                  End Sub
            AddHandler client.ConnectionFailed, Sub(s3, ev3)
                                                    Debug.WriteLine(
    $"FramebufferChanged: disposed={VncControl.IsDisposed}, handle={VncControl.IsHandleCreated}"
)
                                                    SafeInvoke(CType((Sub() MessageBox.Show("Connection failed.")), Action))
                                                End Sub
            AddHandler client.Closed, Sub(s4, ev4)
                                          Debug.WriteLine(
    $"FramebufferChanged: disposed={VncControl.IsDisposed}, handle={VncControl.IsHandleCreated}"
)
                                          SafeInvoke(CType((Sub()
                                                                If VncControl.Client Is client Then VncControl.Client = Nothing
                                                            End Sub), Action))
                                      End Sub
            client.Connect(hostname, port, options)
            Await firstFrameTcs.Task
        Catch ex As Exception
            ' rethrow to be handled by awaiting code
            MessageBox.Show(ex.Message)
            Return
        End Try
    End Function
#End Region

#Region "Handles Closing"
    Private Sub SafeInvoke(action As Action)
        If IsDisposed OrElse Disposing Then Return
        If Not IsHandleCreated Then Return
        Try
            BeginInvoke(action)
        Catch ex As InvalidOperationException
            ' Form is closing or handle is gone.
            MessageBox.Show("Form is closing or Handle is gone")
        Catch ex As ObjectDisposedException
            ' Form already disposed.
            MessageBox.Show("Form is already Disposed")
        End Try
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
        For Each val As VncSession In Sessions
            Try
                val.Client?.Dispose()
                val.Control?.Dispose()
            Catch
            End Try
        Next
        Sessions.Clear()
    End Sub
#End Region


End Class