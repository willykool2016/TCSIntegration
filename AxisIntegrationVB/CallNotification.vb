Public Class CallNotification
    Public Event AnswerRequested()
    Public Event HangupRequested()
    Public Property CallerName As String
    Private Sub AnswerButton_Click(sender As Object, e As EventArgs) Handles AnswerButton.Click
        RaiseEvent AnswerRequested()
        Me.Hide()
    End Sub

    Private Sub DeclineButton_Click(sender As Object, e As EventArgs) Handles DeclineButton.Click
        RaiseEvent HangupRequested()
        Me.Hide()
    End Sub

    Private Sub CallNotification_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CallerName IsNot Nothing Then
            IncomingLabel.Text = $"Incoming call from {CallerName}"
        Else
            IncomingLabel.Text = "Incoming call from [Unknown]"
        End If
    End Sub
End Class