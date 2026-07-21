Public Class CallNotification
    Private sipService = New SIPService
    Private Sub AnswerButton_Click(sender As Object, e As EventArgs) Handles AnswerButton.Click
        sipService.AnswerCall()
    End Sub

    Private Sub DeclineButton_Click(sender As Object, e As EventArgs) Handles DeclineButton.Click
        sipService.HangUp()
    End Sub
End Class