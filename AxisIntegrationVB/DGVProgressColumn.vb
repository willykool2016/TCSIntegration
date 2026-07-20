Imports System.Drawing
Imports System.ComponentModel

Public Class DGVProgressColumn
    Inherits DataGridViewImageColumn

    Public Sub New()
        Me.CellTemplate = New DataGridViewProgressCell
    End Sub
End Class

Public Class DataGridViewProgressCell
    Inherits DataGridViewImageCell

    Sub New()
        ValueType = Type.GetType("Integer")
    End Sub

    ' Method required to make the Progress Cell consistent with the default Image Cell. 
    ' The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an Integer.
    Protected Overrides Function GetFormattedValue( _
        ByVal value As Object, _
        ByVal rowIndex As Integer, _
        ByRef cellStyle As DataGridViewCellStyle, _
        ByVal valueTypeConverter As TypeConverter, _
        ByVal formattedValueTypeConverter As TypeConverter, _
        ByVal context As DataGridViewDataErrorContexts _
        ) As Object

        Static emptyImage As Bitmap = New Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        GetFormattedValue = emptyImage
    End Function

    Protected Overrides Sub Paint(ByVal g As System.Drawing.Graphics, ByVal clipBounds As System.Drawing.Rectangle, ByVal cellBounds As System.Drawing.Rectangle, ByVal rowIndex As Integer, ByVal cellState As System.Windows.Forms.DataGridViewElementStates, ByVal value As Object, ByVal formattedValue As Object, ByVal errorText As String, ByVal cellStyle As System.Windows.Forms.DataGridViewCellStyle, ByVal advancedBorderStyle As System.Windows.Forms.DataGridViewAdvancedBorderStyle, ByVal paintParts As System.Windows.Forms.DataGridViewPaintParts)
        Dim progressVal As Integer = CType(value, Integer)
        Dim percentage As Single = CType((progressVal / 100), Single)
        Dim backBrush As Brush = New SolidBrush(cellStyle.BackColor)
        Dim foreBrush As Brush = New SolidBrush(cellStyle.ForeColor)
        Dim clrPGB As Color

        ' Call the base class method to paint the default cell appearance.
        MyBase.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts)

        If progressVal = 0 Then
            clrPGB = Color.FromArgb(0, 0, 0)
        ElseIf progressVal < 70 Then
            clrPGB = Color.FromArgb(0, 255, 0)
        ElseIf progressVal < 80 Then
            clrPGB = Color.FromArgb(255, 255, 0)
        ElseIf progressVal < 90 Then
            clrPGB = Color.FromArgb(255, 150, 50)
        Else
            clrPGB = Color.FromArgb(255, 0, 0)
        End If

        If percentage > 1 Then percentage = 1

        g.FillRectangle(New SolidBrush(clrPGB), cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((percentage * cellBounds.Width - 4)), cellBounds.Height - 4)
        g.DrawString(progressVal.ToString() & "%", cellStyle.Font, foreBrush, cellBounds.X + 6, cellBounds.Y + 2)

    End Sub
End Class