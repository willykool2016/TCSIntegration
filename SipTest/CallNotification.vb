Public Class CallNotification
    Private Sub AnswerButton_Click(sender As Object, e As EventArgs) Handles AnswerButton.Click
        IntercomForm.btnAnswer.PerformClick()
    End Sub

    Private Sub DeclineButton_Click(sender As Object, e As EventArgs) Handles DeclineButton.Click
        IntercomForm.HangUp()
    End Sub
End Class