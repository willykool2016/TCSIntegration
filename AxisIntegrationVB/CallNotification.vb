Public Class CallNotification
    Public Event AnswerRequested()
    Public Event HangupRequested()
    Private Sub AnswerButton_Click(sender As Object, e As EventArgs) Handles AnswerButton.Click
        RaiseEvent AnswerRequested()
    End Sub

    Private Sub DeclineButton_Click(sender As Object, e As EventArgs) Handles DeclineButton.Click
        RaiseEvent HangupRequested()
    End Sub
End Class