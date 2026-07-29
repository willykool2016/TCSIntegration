Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports RemoteViewing.Vnc

Public Class VncViewerControl
    Inherits UserControl

    Private _client As VncClient
    Private HiddenCursor As Cursor

    Private _localMousePosition As Point
    Private _drawLocalCursor As Boolean = False
    Private _localCursor As Cursor = Cursors.Default

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Client As VncClient
        Get
            Return _client
        End Get
        Set(value As VncClient)

            If _client IsNot Nothing Then
                RemoveHandler _client.FramebufferChanged, AddressOf FramebufferChanged
            End If

            _client = value

            If _client IsNot Nothing Then
                AddHandler _client.FramebufferChanged, AddressOf FramebufferChanged
            End If

            Me.Invalidate()

        End Set
    End Property

    Public Sub New()

        DoubleBuffered = True
        ResizeRedraw = True
        TabStop = True
        HiddenCursor = CreateInvisibleCursor()

    End Sub

#Region "Mouse Support"
    Private Function GetMouseButtonMask() As Integer

        Dim buttons As Integer = 0

        If (Control.MouseButtons And MouseButtons.Left) <> 0 Then
            buttons = buttons Or 1
        End If

        If (Control.MouseButtons And MouseButtons.Middle) <> 0 Then
            buttons = buttons Or 2
        End If

        If (Control.MouseButtons And MouseButtons.Right) <> 0 Then
            buttons = buttons Or 4
        End If

        Return buttons

    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)

        MyBase.OnMouseMove(e)

        _localMousePosition = e.Location
        _localCursor = Cursors.Default

        Invalidate()

        If Client Is Nothing Then Return

        Dim remotePoint = TranslateMousePoint(e.Location)

        Client.SendPointerEvent(
        remotePoint.X,
        remotePoint.Y,
        GetMouseButtonMask())

    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)

        MyBase.OnMouseDown(e)

        Me.Focus()

        If Client Is Nothing Then Return

        Dim remotePoint = TranslateMousePoint(e.Location)

        Client.SendPointerEvent(
        remotePoint.X,
        remotePoint.Y,
        GetMouseButtonMask())

    End Sub


    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)

        MyBase.OnMouseUp(e)

        If Client Is Nothing Then Return

        Dim remotePoint = TranslateMousePoint(e.Location)

        Client.SendPointerEvent(
        remotePoint.X,
        remotePoint.Y,
        GetMouseButtonMask())

    End Sub

    Private Function TranslateMousePoint(p As Point) As Point

        If Client Is Nothing OrElse Client.Framebuffer Is Nothing Then
            Return p
        End If

        Dim fb = Client.Framebuffer

        Dim scale As Single =
        Math.Min(ClientSize.Width / fb.Width,
                 ClientSize.Height / fb.Height)

        Dim drawWidth As Integer = CInt(fb.Width * scale)
        Dim drawHeight As Integer = CInt(fb.Height * scale)

        Dim offsetX As Integer =
        (ClientSize.Width - drawWidth) \ 2

        Dim offsetY As Integer =
        (ClientSize.Height - drawHeight) \ 2


        Dim remoteX As Integer =
        CInt((p.X - offsetX) / scale)

        Dim remoteY As Integer =
        CInt((p.Y - offsetY) / scale)


        Return New Point(remoteX, remoteY)

    End Function

    Protected Overrides Sub OnMouseEnter(e As EventArgs)

        MyBase.OnMouseEnter(e)

        Me.Cursor = HiddenCursor

    End Sub


    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)

        Me.Cursor = Cursors.Default
    End Sub
#End Region

#Region "Keyboard Support"
    'Protected Overrides Sub OnKeyDown(e As KeyEventArgs)

    '    MyBase.OnKeyDown(e)

    '    If Client Is Nothing Then Return

    '    Client.SendKeyEvent(True, CInt(e.KeyCode))

    '    'Client.SendKeyEvent(
    '    'True,
    '    'e.KeyCode)
    '    '    'changed from e.KeyValue to e.KeyCode for testing purposes
    'End Sub


    'Protected Overrides Sub OnKeyUp(e As KeyEventArgs)

    '    MyBase.OnKeyUp(e)

    '    If Client Is Nothing Then Return

    '    Client.SendKeyEvent(False, CInt(e.KeyCode))


    '    'Client.SendKeyEvent(
    '    'False,
    '    'e.KeyCode)
    '    'changed from e.KeyValue to e.KeyCode for testing purposes
    'End Sub

    'Glade test stuff---------------------------------------------------------

    'Protected Overrides Function IsInputKey(keyData As Keys) As Boolean

    '    Return True

    'End Function


    'Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
    '    If _client Is Nothing Then Return MyBase.ProcessCmdKey(msg, keyData)


    '    Dim keyCode As Integer = CInt(keyData And Keys.KeyCode)

    '    _client.SendKeyEvent(True, keyCode)

    '    Return True
    'End Function


    'Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
    '    MyBase.OnKeyUp(e)

    '    If _client Is Nothing Then Return

    '    Dim keyCode As Integer = CInt(e.KeyCode)
    '    _client.SendKeyEvent(False, keyCode)

    'End Sub


    ' Safe Windows API imports to decode precise character layouts natively
    <DllImport("user32.dll")>
    Private Shared Function MapVirtualKey(uCode As UInteger, uMapType As UInteger) As UInteger
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ToUnicode(wVirtKey As UInteger, wScanCode As UInteger, lpKeyState As Byte(), <Out, MarshalAs(UnmanagedType.LPWStr)> pwszBuff As System.Text.StringBuilder, cchBuff As Integer, wFlags As UInteger) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetKeyboardState(lpKeyState As Byte()) As Boolean
    End Function

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        ' Instructs Windows Forms to let navigation/control keys bypass standard selection
        Return True
    End Function

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        ' FIX 1: Change '_client' to the active 'Client' property
        If Client Is Nothing Then Return MyBase.ProcessCmdKey(msg, keyData)

        Dim key As Keys = keyData And Keys.KeyCode

        ' Capture command, functional, and layout-altering structural keys immediately
        If IsSpecialKey(key) Then
            Dim keysym As Integer = GetSpecialKeySym(key)
            If keysym <> 0 Then
                ' FIX 2: Parameter order swapped to match RemoteViewing signature (KeySym, Pressed)
                Client.SendKeyEvent(CType(keysym, KeySym), True)
                Return True
            End If
        End If

        ' Drop alphanumeric typing strings cleanly down into standard Windows layout chains
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        ' FIX 1: Evaluation mapped to 'Client' property directly
        If Client Is Nothing Then Return

        Dim keysym As Integer = GetKeySymFromEventArgs(e)
        If keysym <> 0 Then
            ' FIX 2: Correct signature tracking applied
            Client.SendKeyEvent(CType(keysym, KeySym), True)
            e.Handled = True
        End If
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        ' FIX 1: Evaluation mapped to 'Client' property directly
        If Client Is Nothing Then Return

        Dim keysym As Integer = If(IsSpecialKey(e.KeyCode), GetSpecialKeySym(e.KeyCode), GetKeySymFromEventArgs(e))
        If keysym <> 0 Then
            ' FIX 2: Correct signature tracking applied
            Client.SendKeyEvent(CType(keysym, KeySym), False)
            e.Handled = True
        End If
    End Sub

    ' Segregates structural layout behaviors from standard alphanumeric inputs
    Private Function IsSpecialKey(key As Keys) As Boolean
        Select Case key
            Case Keys.Back, Keys.Tab, Keys.Enter, Keys.Escape, Keys.Delete, Keys.Insert,
                 Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown,
                 Keys.Left, Keys.Up, Keys.Right, Keys.Down,
                 Keys.F1 To Keys.F24, Keys.ShiftKey, Keys.ControlKey, Keys.Menu, Keys.Capital
                Return True
            Case Else
                Return False
        End Select
    End Function

    ' Translates system structural keys cleanly to standard X11 codes expected by VNC
    Private Function GetSpecialKeySym(key As Keys) As Integer
        Select Case key
            Case Keys.Back : Return &HFF08
            Case Keys.Tab : Return &HFF09
            Case Keys.Enter : Return &HFF0D
            Case Keys.Escape : Return &HFF1B
            Case Keys.Delete : Return &HFFFF
            Case Keys.Insert : Return &HFF63
            Case Keys.Home : Return &HFF50
            Case Keys.End : Return &HFF57
            Case Keys.PageUp : Return &HFF55
            Case Keys.PageDown : Return &HFF56
            Case Keys.Left : Return &HFF51
            Case Keys.Up : Return &HFF52
            Case Keys.Right : Return &HFF53
            Case Keys.Down : Return &HFF54
            Case Keys.ShiftKey : Return &HFFE1
            Case Keys.ControlKey : Return &HFFE3
            Case Keys.Menu : Return &HFFE9
            Case Keys.Capital : Return &HFFE5 ' Caps Lock
            Case Keys.F1 To Keys.F12
                Return &HFFBE + (key - Keys.F1)
            Case Else
                Return 0
        End Select
    End Function

    ' Extracts proper shifted or raw Unicode structures out of standard typing keystrokes
    Private Function GetKeySymFromEventArgs(e As KeyEventArgs) As Integer
        Dim scanCode As UInteger = MapVirtualKey(CUInt(e.KeyCode), 0)
        Dim keyState(255) As Byte
        GetKeyboardState(keyState)

        Dim sb As New System.Text.StringBuilder(2)
        Dim result As Integer = ToUnicode(CUInt(e.KeyCode), scanCode, keyState, sb, sb.Capacity, 0)

        If result > 0 Then
            Return AscW(sb(0))
        End If

        Return CInt(e.KeyCode)
    End Function




#End Region

#Region "Rendering"
    Private Sub FramebufferChanged(sender As Object, e As EventArgs)

        If Me.IsHandleCreated Then

            BeginInvoke(Sub()
                            Me.Invalidate()
                        End Sub)

        End If

    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        e.Graphics.Clear(Color.Black)

        If Client Is Nothing Then Return
        If Client.Framebuffer Is Nothing Then Return

        Using bmp = FramebufferToBitmap(Client.Framebuffer)

            Dim scale As Single =
            Math.Min(ClientSize.Width / bmp.Width,
                     ClientSize.Height / bmp.Height)

            Dim w = CInt(bmp.Width * scale)
            Dim h = CInt(bmp.Height * scale)

            Dim x = (ClientSize.Width - w) \ 2
            Dim y = (ClientSize.Height - h) \ 2

            e.Graphics.DrawImage(
            bmp,
            New Rectangle(x, y, w, h))

        End Using

        If _localCursor IsNot Nothing Then

            _localCursor.Draw(
        e.Graphics,
        New Rectangle(
            _localMousePosition.X,
            _localMousePosition.Y,
            _localCursor.Size.Width,
            _localCursor.Size.Height))

        End If

    End Sub

    Private Function FramebufferToBitmap(fb As VncFramebuffer) As Bitmap

        Dim bmp As New Bitmap(
            fb.Width,
            fb.Height,
            Imaging.PixelFormat.Format32bppArgb)

        Dim buffer = fb.GetBuffer()

        Dim data = bmp.LockBits(
            New Rectangle(0, 0, fb.Width, fb.Height),
            Imaging.ImageLockMode.WriteOnly,
            Imaging.PixelFormat.Format32bppArgb)

        Try
            Dim destStride As Integer = data.Stride
            Dim destSize As Integer = destStride * fb.Height

            Dim dest(destSize - 1) As Byte

            Dim bytesPerPixel As Integer = fb.PixelFormat.BitsPerPixel \ 8

            For y As Integer = 0 To CInt(fb.Height) - 1

                For x As Integer = 0 To CInt(fb.Width) - 1

                    Dim srcIndex As Integer =
            (y * fb.Stride) + (x * bytesPerPixel)

                    Dim pixel As UInteger = 0

                    If fb.PixelFormat.IsLittleEndian Then

                        For i As Integer = 0 To bytesPerPixel - 1
                            pixel = pixel Or (CUInt(buffer(srcIndex + i)) << (8 * i))
                        Next

                    Else

                        For i As Integer = 0 To bytesPerPixel - 1
                            pixel = (pixel << 8) Or CUInt(buffer(srcIndex + i))
                        Next

                    End If


                    Dim redMask As UInteger =
            CUInt((1 << fb.PixelFormat.RedBits) - 1)

                    Dim greenMask As UInteger =
            CUInt((1 << fb.PixelFormat.GreenBits) - 1)

                    Dim blueMask As UInteger =
            CUInt((1 << fb.PixelFormat.BlueBits) - 1)


                    Dim r As Byte =
            CByte((((pixel >> fb.PixelFormat.RedShift) And redMask) * 255) \ redMask)

                    Dim g As Byte =
            CByte((((pixel >> fb.PixelFormat.GreenShift) And greenMask) * 255) \ greenMask)

                    Dim b As Byte =
            CByte((((pixel >> fb.PixelFormat.BlueShift) And blueMask) * 255) \ blueMask)


                    Dim destIndex As Integer =
            (y * data.Stride) + (x * 4)

                    dest(destIndex) = b
                    dest(destIndex + 1) = g
                    dest(destIndex + 2) = r
                    dest(destIndex + 3) = 255

                Next

            Next


            Marshal.Copy(
                dest,
                0,
                data.Scan0,
                dest.Length)

        Finally

            bmp.UnlockBits(data)

        End Try


        Return bmp

    End Function
#End Region

    Private Function CreateInvisibleCursor() As Cursor

        Dim bmp As New Bitmap(32, 32)

        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
        End Using

        Return New Cursor(bmp.GetHicon())

    End Function
End Class