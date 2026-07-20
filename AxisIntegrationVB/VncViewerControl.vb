Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports RemoteViewing.Vnc
Imports System.Runtime.InteropServices

Public Class VncViewerControl
    Inherits UserControl

    Private _client As VncClient

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

    End Sub

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

End Class