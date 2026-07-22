Imports Microsoft.VisualBasic
Imports RemoteViewing.Windows.Forms

Public Class GridControls
    Public CtrlList As New List(Of Control)

    'Public Function CreateCtrl(Of T As {Control, New})() As T
    '    Dim ctrl As New T()
    '    ctrl.Dock = DockStyle.None
    '    ctrl.Anchor = AnchorStyles.None
    '    ctrl.BackColor = SystemColors.ControlDark


    '    ResizeCtrl(T, TotalPanel)

    '    If (Not ctrl.IsHandleCreated) Then
    '        ctrl.CreateControl()
    '    End If
    '    Return ctrl
    'End Function

    Public Function ResizeCtrl(ctrl As Control, grid As TableLayoutPanel) As TableLayoutPanel
        grid.SuspendLayout()
        Dim count As Integer = grid.Controls.Count + 1
        If count < 2 Then
            grid.ColumnCount = 1
            grid.RowCount = 1
        ElseIf count < 3 Then
            grid.ColumnCount = 2
            grid.RowCount = 1
        ElseIf count < 5 Then
            grid.ColumnCount = 2
            grid.RowCount = 2
        ElseIf count < 7 Then
            grid.ColumnCount = 3
            grid.RowCount = 2
        ElseIf count < 10 Then
            grid.ColumnCount = 3
            grid.RowCount = 3
        ElseIf count < 13 Then
            grid.ColumnCount = 4
            grid.RowCount = 3
        ElseIf count < 17 Then
            grid.ColumnCount = 4
            grid.RowCount = 4
        Else
            grid.ColumnCount = 5
            grid.RowCount = 5
        End If
        If grid Is Nothing Then Return grid
        If grid.IsDisposed Then Return grid
        If Not grid.IsHandleCreated Then Return grid
        Try
            grid.Invoke(Sub()
                            grid.GrowStyle = TableLayoutPanelGrowStyle.FixedSize
                            grid.ColumnStyles.Clear()
                            grid.RowStyles.Clear()
                            For i As Integer = 1 To grid.ColumnCount
                                grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0! / grid.ColumnCount))
                            Next
                            For i As Integer = 1 To grid.RowCount
                                grid.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0! / grid.RowCount))
                            Next
                            grid.ResumeLayout()
                            grid.Controls.Add(ctrl, -1, -1)
                        End Sub)
        Catch ex As InvalidOperationException
            Debug.WriteLine("grid handle is gone.")
        Catch ex As ObjectDisposedException
            Debug.WriteLine("grid disposed.")
        End Try
        Return grid
    End Function
End Class
