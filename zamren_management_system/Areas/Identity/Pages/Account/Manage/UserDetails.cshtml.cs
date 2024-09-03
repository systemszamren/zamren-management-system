// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace zamren_management_system.Areas.Identity.Pages.Account.Manage;

public class UserDetailsModel : PageModel
{
    [BindProperty] public string UserId { get; set; }

    public PageResult OnGet()
    {
        return Page();
    }
}