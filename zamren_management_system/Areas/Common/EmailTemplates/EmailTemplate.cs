using System.Globalization;
using zamren_management_system.Areas.Common.Interfaces;

namespace zamren_management_system.Areas.Common.EmailTemplates;

public class EmailTemplate : IEmailTemplate
{
    private readonly IConfiguration _configuration;

    public EmailTemplate(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string RegisterConfirmEmail(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "Verify");
        var emailBody = $@"<p>Hey {firstName},</p>
                       <p>Thank you for registering with us. Kindly verify your account email {hiddenEmailAddress} by clicking the button below.</p>
                       <p>{anchorBtn}</p>
                       <p>This link will expire in {expireTokenInHours} hours.</p>
                       <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    public string RegisterAndResetPassword(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "Reset Password");
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>Thank you for registering with us. Kindly setup your account password for <a href='#'>{hiddenEmailAddress}</a> by clicking the button below.</p>
					   <p>{anchorBtn}</p>
					   <p>This link will expire in {expireTokenInHours} hours.</p>
					   <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    public string ResetPassword(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "Reset Password");
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>We received a request to reset your password for {hiddenEmailAddress}. Kindly reset your password by clicking the button below.</p>
					   <p>{anchorBtn}</p>
					   <p>This link will expire in {expireTokenInHours} hours.</p>
					   <p>If you do not recognize this request, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    public string ConfirmEmail(string firstName, string hiddenEmailAddress, string htmlEncodedCallbackUrl,
        int expireTokenInHours)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "Verify");
        var emailBody = $@"<p>Hey {firstName},</p>
                       <p>Kindly verify your email {hiddenEmailAddress} by clicking the button below.</p>
                       <p>{anchorBtn}</p>
                       <p>This link will expire in {expireTokenInHours} hours.</p>
                       <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    public string ConfirmNextOfKinEmail(string nextOfKinFullName, string fullName, string? phoneNumber,
        string htmlEncodedCallbackUrl, int expireTokenInHours)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "Approve");
        var emailBody = $@"<p>Hey {nextOfKinFullName},</p>
                       <p>You have been listed as the next of kin by <strong>{fullName}</strong> with phone number <strong>{phoneNumber}</strong>. If you approve of this, kindly confirm by clicking the button below.</p>
                       <p>{anchorBtn}</p>
                       <p>This link will expire in {expireTokenInHours} hours.</p>
                       <p>If you do not recognize this person, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    //DeleteAccount
    public string DeleteAccount(string firstName, string hiddenEmailAddress)
    {
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>Your account with email {hiddenEmailAddress} has been scheduled for deletion. If you did not request this, please contact us immediately.</p>
					   <p>Thank you for using our services.</p>";
        return BaseTemplate(emailBody);
    }

    //PasswordChanged
    public string PasswordChanged(string firstName, string hiddenEmailAddress)
    {
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>Your password for {hiddenEmailAddress} has been successfully changed.</p>
					   <p>If you do not recognize this action, please contact us immediately at <a href='mailto:{_configuration["SystemContactDetails:SupportEmailAddress"]}'>{_configuration["SystemContactDetails:SupportEmailAddress"]}</a> or <a href='tel: {_configuration["SystemContactDetails:SupportPhoneNumber"]}'>{_configuration["SystemContactDetails:SupportPhoneNumber"]}</a>.</p>";
        return BaseTemplate(emailBody);
    }

    public string AssignedWorkflowTask(string firstName, string referenceNumber, string taskName,
        string htmlEncodedCallbackUrl)
    {
        var anchorBtn = AnchorBtn(htmlEncodedCallbackUrl, "View Task");
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>A task with reference number {referenceNumber} has been assigned to you. Kindly click the button below to view the task.</p>
					   <p>Task Name: {taskName}</p>
					   <p>{anchorBtn}</p>
					   <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    public string UnassignedWorkflowTask(string oldUserFirstName, string newUserFullName, string referenceNumber,
        string taskName)
    {
        var emailBody = $@"<p>Hey {oldUserFirstName},</p>
					   <p>A task with reference number {referenceNumber} has been unassigned from you and re-assigned to {newUserFullName}.</p>
					   <p>Task Name: {taskName}</p>
 					   <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    //TaskClosed
    public string WorkflowTaskClosed(string firstName, string referenceNumber, string taskName)
    {
        var emailBody = $@"<p>Hey {firstName},</p>
					   <p>The task you initiated with reference number {referenceNumber} has been closed.</p>
					   <p>Task Name: {taskName}</p>
					   <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    //TaskReopened
    public string WorkflowTaskReopened(string firstName, string referenceNumber, string taskName)
    {
        var emailBody = $@"<p>Hey {firstName},</p>
	 					   <p>The task you initiated with reference number {referenceNumber} has been reopened.</p>
	 					   <p>Task Name: {taskName}</p>
	 					   <p>If you do not recognize this action, please ignore this email.</p>";
        return BaseTemplate(emailBody);
    }

    private static string AnchorBtn(string url, string btnText)
    {
        var anchorTag = $@"<a href='{url}' class='button button2'
                          style='background-color: #008CBA;
                          border: none;
                          color: white;
                          padding: 8px 25px;
                          text-align: center;
                          text-decoration: none;
                          display: inline-block;
                          font-size: 16px;
                          margin: 4px 2px;
                          cursor: pointer;'>{btnText}</a>";
        return anchorTag;
    }

    private string BaseTemplate(string emailBody)
    {
        var organizationWebsiteUrl = _configuration["Organization:WebsiteUrl"];
        var organizationName = _configuration["Organization:Name"];
        var logoPath = _configuration["Organization:Logo"];
        organizationName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(organizationName!.ToLower());

        return @"
                <table class='full-width-container' border='0' cellpadding='0' cellspacing='0' height='100%' width='100%' bgcolor='#eeeeee' style='width: 100%; height: 100%; padding: 30px 0 30px 0; font-family:Mote,sans-serif;'>
                    <tr>
                        <td align='center' valign='top'>
	                        <table class='container' border='0' cellpadding='0' cellspacing='0' width='700' bgcolor='#ffffff' style='width: 700px;'>
	                          	<tr>
	                          		<td align='center' valign='top'>
	                          			<table class='container header' border='0' cellpadding='0' cellspacing='0' width='620' style='width: 620px;'>
	                          				<tr>
	                          					<td style='padding:30px 0 10px 0;border-bottom:solid 1px #eeeeee;' align='left'>
	                          						<img src='" + logoPath + @"' height='70' width='150'>
	                          					</td>
	                          				</tr>
	                          			</table>
               							<table class='container paragraph-block' border='0' cellpadding='0' cellspacing='0' width='100%'>
               								<tr>
               									<td align='center' valign='top'>
               										<table class='container' border='0' cellpadding='0' cellspacing='0' width='620' style='width: 620px;'>
               											<tr>
               												<td class='paragraph-block__content' style='padding: 20px 0 18px 0; font-size: 16px; line-height: 27px; color: #969696;' align='left'>
               													" + emailBody + @"
               												</td>
               											</tr>
               										</table>
               									</td>
               								</tr>
               							</table>
               							<table class='container' border='0' cellpadding='0' cellspacing='0' width='100%' align='center'>
               								<tr>
               									<td align='center'>
               										<table class='container' border='0' cellpadding='0' cellspacing='0' width='620' align='center' style='border-top: 1px solid #eeeeee; width: 620px;'>
               											<tr>
															<td style='color: #d5d5d5; text-align: center; font-size: 15px; padding: 30px 0 50px 0; line-height: 22px;'>
																Copyright &copy; " + DateTime.Now.Year + @"
																<a href='" + organizationWebsiteUrl + @"' 
																	target='_blank' style='text-decoration: none; border-bottom: 1px solid #d5d5d5; color: #d5d5d5;'>
																	" + organizationName + @"
																</a>. 
																<br/>All rights reserved.
															</td>
               											</tr>
               										</table>
               									</td>
               								</tr>
               							</table>
               						</td>
               					</tr>
               				</table>
               			</td>
               		</tr>
               	</table>
               ";
    }
}