﻿namespace zamren_management_system.Areas.Common.ViewModels;

public class EmailConfigurationViewModel
{
    public string From { get; set; }
    public string To { get; set; }
    public string CcBackup { get; set; }
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; }
}