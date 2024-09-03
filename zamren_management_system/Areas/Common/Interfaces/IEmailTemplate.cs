namespace zamren_management_system.Areas.Common.Interfaces;

public interface IEmailTemplate
{
    /// <summary>
    ///   This method is used to generate the email template for confirming the next of kin email.
    /// </summary>
    /// <param name="nextOfKinFullName"> The full name of the next of kin </param>
    /// <param name="fullName"> The full name of the user requesting the next of kin </param>
    /// <param name="phoneNumber"> The phone number of the user requesting the next of kin </param>
    /// <param name="htmlEncodedCallbackUrl"> The callback url to confirm the next of kin </param>
    /// <param name="expireTokenInHours"> The expiration time of the token in hours </param>
    /// <returns> html body of the email template </returns>
    string ConfirmNextOfKinEmail(string nextOfKinFullName, string fullName, string? phoneNumber,
        string htmlEncodedCallbackUrl, int expireTokenInHours);

    /// <summary>
    ///     This method is used to generate the email template for confirming the email.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <param name="htmlEncodedCallbackUrl"></param>
    /// <param name="expireTokenInHours"></param>
    /// <returns> html body of the email template </returns>
    string RegisterConfirmEmail(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours);

    /// <summary>
    ///     This method is used to generate the email template for registering and resetting the password.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <param name="htmlEncodedCallbackUrl"></param>
    /// <param name="expireTokenInHours"></param>
    /// <returns> html body of the email template </returns>
    string ConfirmEmail(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours);

    /// <summary>
    ///     This method is used to generate the email template for resetting the password.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <param name="htmlEncodedCallbackUrl"></param>
    /// <param name="expireTokenInHours"></param>
    /// <returns> html body of the email template </returns>
    string RegisterAndResetPassword(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours);

    /// <summary>
    ///     This method is used to generate the email template for resetting the password.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <param name="htmlEncodedCallbackUrl"></param>
    /// <param name="expireTokenInHours"></param>
    /// <returns> html body of the email template </returns>
    string ResetPassword(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours);

    /// <summary>
    ///     This method is used to generate the email template for deleting the account.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <returns></returns>
    public string DeleteAccount(string firstName, string hiddenEmailAddress);

    /// <summary>
    ///     This method is used to generate the email template for creating a new workflow task.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="referenceNumber"></param>
    /// <param name="taskName"></param>
    /// <param name="htmlEncodedCallbackUrl"></param>
    /// <returns></returns>
    public string AssignedWorkflowTask(string firstName, string referenceNumber, string taskName,
        string htmlEncodedCallbackUrl);

    /// <summary>
    ///     This method is used to generate the email template for unassigning a workflow task.
    /// </summary>
    /// <param name="oldUserFirstName"></param>
    /// <param name="newUserFullName"></param>
    /// <param name="referenceNumber"></param>
    /// <param name="taskName"></param>
    /// <returns></returns>
    public string UnassignedWorkflowTask(string oldUserFirstName, string newUserFullName, string referenceNumber,
        string taskName);

    /// <summary>
    ///     This method is used to generate the email template for notifying the user that a workflow task has been closed.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="referenceNumber"></param>
    /// <param name="processName"></param>
    /// <returns></returns>
    public string WorkflowTaskClosed(string firstName, string referenceNumber, string processName);

    /// <summary>
    ///     This method is used to generate the email template for notifying the user that a workflow task has been reopened.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="referenceNumber"></param>
    /// <param name="taskName"></param>
    /// <returns></returns>
    public string WorkflowTaskReopened(string firstName, string referenceNumber, string taskName);

    /// <summary>
    ///     This method is used to generate the email template for notifying the user the password has been changed.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="hiddenEmailAddress"></param>
    /// <returns></returns>
    public string PasswordChanged(string firstName, string hiddenEmailAddress);
}