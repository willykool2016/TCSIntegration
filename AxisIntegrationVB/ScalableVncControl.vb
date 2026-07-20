Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Class ScalableVncControl
    Inherits Control

    Private _framebuffer As Bitmap
    Public Property Framebuffer As Bitmap
        Get
            Return _framebuffer
        End Get
        Set(value As Bitmap)
            If _framebuffer IsNot Nothing AndAlso _framebuffer IsNot value Then
                _framebuffer.Dispose()
            End If
            _framebuffer = value
            Me.Invalidate()
            UpdateControlSizeFromZoom()
        End Set
    End Property

    ' Zoom factor (1.0 = 100%). Set >1 for larger than native.
    Private _zoom As Single = 1.0F
    Public Property Zoom As Single
        Get
            Return _zoom
        End Get
        Set(value As Single)
            If value <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(Zoom))
            End If
            _zoom = value
            UpdateControlSizeFromZoom()
            Me.Invalidate()
        End Set
    End Property

    ' If True, ignore Zoom and scale to fit control while preserving aspect.
    Public Property FitToControl As Boolean = False

    ' When True, the control will resize itself to the scaled framebuffer size
    ' (framebuffer.Width * Zoom). Place this control inside a Panel with
    ' AutoScroll = True to allow scrolling when the scaled size exceeds the
    ' parent bounds. Ignored when FitToControl = True or when no framebuffer.
    Public Property AutoResizeToZoom As Boolean = False

    Public Sub New()
        Me.DoubleBuffered = True
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint, True)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g = e.Graphics
        g.Clear(Me.BackColor)
        If _framebuffer Is Nothing Then Return
        ' choose scale
        Dim destW As Single = _framebuffer.Width * Zoom
        Dim destH As Single = _framebuffer.Height * Zoom
        If FitToControl Then
            Dim sx = Me.ClientSize.Width / CSng(_framebuffer.Width)
            Dim sy = Me.ClientSize.Height / CSng(_framebuffer.Height)
            Dim s = Math.Min(sx, sy)
            destW = _framebuffer.Width * s
            destH = _framebuffer.Height * s
        End If
        ' preserve aspect and center
        Dim destX As Single = (Me.ClientSize.Width - destW) / 2
        Dim destY As Single = (Me.ClientSize.Height - destH) / 2
        Dim destRect As New RectangleF(destX, destY, destW, destH)
        ' high-quality scaling (choose performance/quality tradeoff)
        g.InterpolationMode = InterpolationMode.HighQualityBicubic
        g.PixelOffsetMode = PixelOffsetMode.HighQuality
        g.CompositingQuality = CompositingQuality.HighSpeed
        g.DrawImage(_framebuffer, destRect)
    End Sub

    ' Map control coordinates -> framebuffer coordinates (for sending mouse events)
    Public Function ControlToFramebuffer(p As Point) As Point?
        If _framebuffer Is Nothing Then Return Nothing
        Dim destRect = GetDestinationRect()
        If Not destRect.Contains(p) Then Return Nothing
        Dim nx = (p.X - destRect.X) / destRect.Width
        Dim ny = (p.Y - destRect.Y) / destRect.Height
        Return New Point(CInt(nx * _framebuffer.Width), CInt(ny * _framebuffer.Height))
    End Function

    Private Function GetDestinationRect() As RectangleF
        If _framebuffer Is Nothing Then Return RectangleF.Empty
        Dim destW As Single = _framebuffer.Width * Zoom
        Dim destH As Single = _framebuffer.Height * Zoom
        If FitToControl Then
            Dim sx = Me.ClientSize.Width / CSng(_framebuffer.Width)
            Dim sy = Me.ClientSize.Height / CSng(_framebuffer.Height)
            Dim s = Math.Min(sx, sy)
            destW = _framebuffer.Width * s
            destH = _framebuffer.Height * s
        End If
        Dim destX As Single = (Me.ClientSize.Width - destW) / 2
        Dim destY As Single = (Me.ClientSize.Height - destH) / 2
        Return New RectangleF(destX, destY, destW, destH)
    End Function

    Private Sub UpdateControlSizeFromZoom()
        If FitToControl OrElse _framebuffer Is Nothing OrElse Not AutoResizeToZoom Then
            Return
        End If
        ' Calculate new size based on zoom while preventing overflow
        Dim newW As Integer = CInt(Math.Min(Integer.MaxValue, Math.Max(1, _framebuffer.Width * Zoom)))
        Dim newH As Integer = CInt(Math.Min(Integer.MaxValue, Math.Max(1, _framebuffer.Height * Zoom)))
        ' Only update if changed to avoid layout thrash
        If Me.Width <> newW OrElse Me.Height <> newH Then
            ' Ensure size update runs on UI thread
            If Me.IsHandleCreated Then
                If Me.InvokeRequired Then
                    Me.BeginInvoke(New MethodInvoker(Sub()
                                                         If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                                                             Me.Size = New Size(newW, newH)
                                                         End If
                                                     End Sub))
                Else
                    Me.Size = New Size(newW, newH)
                End If
            Else
                ' Defer until handle is created to avoid BeginInvoke throwing
                Dim handler As EventHandler = Nothing
                handler = Sub(s, e)
                              RemoveHandler Me.HandleCreated, handler
                              If Not Me.IsDisposed AndAlso Me.IsHandleCreated Then
                                  ' marshal to UI thread if needed
                                  If Me.InvokeRequired Then
                                      Me.BeginInvoke(New MethodInvoker(Sub()
                                                                           If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                                                                               Me.Size = New Size(newW, newH)
                                                                           End If
                                                                       End Sub))
                                  Else
                                      If Not Me.IsDisposed AndAlso Not Me.Disposing Then
                                          Me.Size = New Size(newW, newH)
                                      End If
                                  End If
                              End If
                          End Sub
                AddHandler Me.HandleCreated, handler
            End If
        End If
        ' If placed inside a scrollable parent (Panel with AutoScroll=True), update the parent's AutoScrollMinSize
        Try
            If Me.Parent IsNot Nothing AndAlso TypeOf Me.Parent Is ScrollableControl Then
                Dim sc = DirectCast(Me.Parent, ScrollableControl)
                If sc.AutoScroll Then
                    Dim newSize = New Size(newW, newH)
                    If sc.IsHandleCreated Then
                        If sc.InvokeRequired Then
                            sc.BeginInvoke(New MethodInvoker(Sub() sc.AutoScrollMinSize = newSize))
                        Else
                            sc.AutoScrollMinSize = newSize
                        End If
                    Else
                        Dim pHandler As EventHandler = Nothing
                        pHandler = Sub(s, e)
                                       RemoveHandler sc.HandleCreated, pHandler
                                       If Not sc.IsDisposed AndAlso sc.IsHandleCreated Then
                                           If sc.InvokeRequired Then
                                               sc.BeginInvoke(New MethodInvoker(Sub()
                                                                                    If Not sc.IsDisposed Then sc.AutoScrollMinSize = newSize
                                                                                End Sub))
                                           Else
                                               If Not sc.IsDisposed Then sc.AutoScrollMinSize = newSize
                                           End If
                                       End If
                                   End Sub
                        AddHandler sc.HandleCreated, pHandler
                    End If
                End If
            End If
        Catch
            ' swallow any cross-thread/layout errors - they shouldn't be fatal
        End Try
    End Sub
End Class
