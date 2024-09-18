using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.EmailTemplates;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Mapper;
using zamren_management_system.Areas.Common.Services;
using zamren_management_system.Areas.Common.ViewModels;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisition;
using zamren_management_system.Areas.Procurement.Services.PurchaseRequisition;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;
using zamren_management_system.Areas.Security.Services;
using zamren_management_system.Areas.Workflow.Interfaces;
using zamren_management_system.Areas.Workflow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using zamren_management_system.Areas.Procurement.Interfaces.PurchaseRequisitions;
using zamren_management_system.Areas.Procurement.Services.PurchaseRequisitions;
using Privilege = zamren_management_system.Areas.Security.Models.Privilege;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add database context
var preConnectionString = configuration.GetConnectionString("DbContextConnection") ??
                          throw new InvalidOperationException(
                              "Connection string 'DbContextConnection' not found.");

// Set secret UserID and Password properties of the connection string (from secrets.json)
var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(preConnectionString)
{
    Server = configuration["DbContextCredentials:Server"] ?? string.Empty,
    UserID = configuration["DbContextCredentials:Username"] ?? string.Empty,
    Password = configuration["DbContextCredentials:Password"] ?? string.Empty,
    Database = configuration["DbContextCredentials:Database"] ?? string.Empty,
    Port = Convert.ToUInt32(configuration["DbContextCredentials:Port"])
};

var postConnectionString = mySqlConnectionStringBuilder.ConnectionString;

// Link connection string to database context
builder.Services.AddDbContext<AuthContext>(options =>
    options.UseMySql(postConnectionString, ServerVersion.AutoDetect(postConnectionString)));

//Map email secrets to a POCO 'EmailConfiguration' class (from secrets.json)
builder.Services.Configure<EmailConfigurationViewModel>(configuration.GetSection("EmailConfiguration"));

// Add Identity services
builder.Services.AddDefaultIdentity<ApplicationUser>(config =>
    {
        config.SignIn.RequireConfirmedAccount = true;
        config.SignIn.RequireConfirmedEmail = true;
        // config.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AuthContext>();

// Configure Data Protection
builder.Services.AddDataProtection()
    .SetApplicationName("zamren_management_system")
    .PersistKeysToFileSystem(new DirectoryInfo("./keys/"));

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IDatatableService, DatatableService>();
builder.Services.AddTransient<IPrivilegeService, PrivilegeService>();
builder.Services.AddTransient<IAuthorizationHandler, PrivilegeAuthorizationHandler>();
builder.Services.AddTransient<IAuthorizationPolicyProvider, PrivilegeAuthorizationPolicyProvider>();
builder.Services.AddTransient<ICypherService, CypherService>();
builder.Services.AddTransient<IModuleService, ModuleService>();
builder.Services.AddTransient<IUserRoleService, UserRoleService>();
builder.Services.AddTransient<IOrganizationService, OrganizationService>();
builder.Services.AddTransient<IBranchService, BranchService>();
builder.Services.AddTransient<IDepartmentService, DepartmentService>();
builder.Services.AddTransient<IOfficeService, OfficeService>();
builder.Services.AddTransient<IUtil, Util>();
builder.Services.AddTransient<ISystemAttachmentManager, SystemAttachmentManager>();
builder.Services.AddTransient<ISystemAttachmentService, SystemAttachmentService>();
builder.Services.AddTransient<IPasswordHistoryService, PasswordHistoryService>();
builder.Services.AddTransient<IUserDetailService, UserDetailService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<Util>();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<IOtpService, OtpService>();
builder.Services.AddTransient<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddTransient<IEmailTemplate, EmailTemplate>();
builder.Services.AddTransient<IWorkflowService, WorkflowService>();
builder.Services.AddTransient<IProcessService, ProcessService>();
builder.Services.AddTransient<IStepService, StepService>();
builder.Services.AddTransient<IStepConditionService, StepConditionService>();
builder.Services.AddTransient<IRoleService, RoleService>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<ITaskLogService, TaskLogService>();
builder.Services.AddTransient<ITaskCommentService, TaskCommentService>();
builder.Services.AddTransient<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddTransient<ISystemNotificationService, SystemNotificationService>();
builder.Services.AddTransient<ISystemNotificationAttachmentService, SystemNotificationAttachmentService>();
builder.Services.Configure<SmsOptions>(configuration); //2fa sms not working, try scaffolding
builder.Services.AddTransient<ISmsSender, SmsSender>();
builder.Services.AddTransient<ISmsService, SmsService>();
builder.Services
    .AddTransient<IPurchaseRequisitionAttachmentService, PurchaseRequisitionAttachmentService>();
builder.Services.AddTransient<IPurchaseRequisitionGoodService, PurchaseRequisitionGoodService>();
builder.Services.AddTransient<IPurchaseRequisitionService, PurchaseRequisitionService>();
builder.Services.AddTransient<IPurchaseRequisitionServiceService, PurchaseRequisitionServiceService>();

// Add controllers with Razor runtime compilation
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    );

//Add Razor Pages with Razor runtime compilation
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Configure password requirements
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(Convert.ToDouble(configuration["SystemVariables:LockoutTimeInMinutes"]));
    options.Lockout.MaxFailedAccessAttempts = Convert.ToInt32(configuration["SystemVariables:MaxFailedAccessAttempts"]);
    options.Lockout.AllowedForNewUsers = true;
});

// Configure Token Lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan =
        TimeSpan.FromHours(Convert.ToDouble(configuration["SystemVariables:ExpireVerificationTokenInHours"]));
});

// Configure Google authentication (from secrets.json)
/*builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Google:ClientId"] ?? string.Empty;
    googleOptions.ClientSecret = configuration["Google:ClientSecret"] ?? string.Empty;
});*/

var maxAge = Convert.ToInt32(configuration["SystemVariables:HSTSMaxAgeDays"]);

// Set HSTS to 365 days
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(maxAge);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

builder.Services.AddDataProtection();
builder.Services.AddTransient<CypherService>();

builder.Services.AddHttpClient();

//Register your mapping profile
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithRedirects("/Home/SystemError/{0}");

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Client",
    areaName: "Client",
    pattern: "Client/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Security",
    areaName: "Security",
    pattern: "Security/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Workflow",
    areaName: "Workflow",
    pattern: "Workflow/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Common",
    areaName: "Common",
    pattern: "Common/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Procurement",
    areaName: "Procurement",
    pattern: "Procurement/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Backoffice",
    areaName: "Backoffice",
    pattern: "Backoffice/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

SeedDatabase(app, configuration);

app.Run();
return;

//seed the database with system variables
async void SeedDatabase(IHost app1, IConfiguration configuration1)
{
    var seedDatabaseOnInitialSystemDeploy =
        Convert.ToBoolean(configuration1["SystemVariables:SeedDatabaseOnInitialSystemDeploy"]);

    //Seed the database with System variables
    if (seedDatabaseOnInitialSystemDeploy == false) return;

    //Seed the database with System Admin and Client roles
    using var scope = app1.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var userDetailService = scope.ServiceProvider.GetRequiredService<IUserDetailService>();

    var adminEmail = configuration1["SystemAdminAccount:Email"] ?? string.Empty;
    var systemAdminUserId = configuration["SystemAdminAccount:UserId"] ?? string.Empty;

    //Get Default User Roles
    var adminRoleName = configuration1["DefaultUserRoles:AdminRole"] ?? string.Empty;
    var adminRoleDescription = configuration1["DefaultUserRoles:AdminRoleDescription"] ?? string.Empty;
    var clientRoleName = configuration1["DefaultUserRoles:ClientRole"] ?? string.Empty;
    var clientRoleDescription = configuration1["DefaultUserRoles:ClientRoleDescription"] ?? string.Empty;
    var employeeRoleName = configuration1["DefaultUserRoles:EmployeeRole"] ?? string.Empty;
    var employeeRoleDescription = configuration1["DefaultUserRoles:EmployeeRoleDescription"] ?? string.Empty;

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    //Get System Admin account details
    var adminPassword = configuration1["SystemAdminAccount:Password"] ?? string.Empty;
    var adminFirstName = configuration["SystemAdminAccount:FirstName"] ?? string.Empty;
    var adminLastName = configuration["SystemAdminAccount:LastName"] ?? string.Empty;

    var monthsUntilPasswordExpires = Convert.ToInt32(configuration1["SystemVariables:MonthsUntilPasswordExpires"]);

    var currentDateTime = DateTimeOffset.UtcNow;

    //Check if System Admin user exists, if not create
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        user = new ApplicationUser
        {
            Id = systemAdminUserId,
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = adminFirstName,
            LastName = adminLastName,
            CanActionWkfTasks = true,
            IsEmployee = true,
            AccountCreatedDate = currentDateTime,
            CreatedByUserId = systemAdminUserId,
            CreatedDate = currentDateTime,
            RecentActivity = Status.AccountCreated.ToString(),
            IsScheduledForDeletion = false,
            PasswordExpiryDate = currentDateTime.AddMonths(monthsUntilPasswordExpires)
        };
        await userManager.CreateAsync(user, adminPassword);

        //create user detail
        await userDetailService.CreateAsync(new UserDetail
        {
            UserId = user.Id,
            ProfileCompletionPercentage = 13,
            CreatedDate = currentDateTime,
            CreatedByUserId = user.Id
        });
    }

    await userManager.UnlockAccountAsync(user, systemAdminUserId);

    //passwordHistoryService
    var passwordHistoryService = scope.ServiceProvider.GetRequiredService<IPasswordHistoryService>();

    //save password history
    if (!string.IsNullOrEmpty(user.PasswordHash))
        await passwordHistoryService.CreateAsync(new PasswordHistory
        {
            UserId = user.Id,
            PasswordHash = user.PasswordHash,
            PasswordCreatedDate = currentDateTime,
            PasswordExpiryDate = (DateTimeOffset)user.PasswordExpiryDate!,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime
        });

    //Create System Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(adminRoleName))
        await roleManager.CreateAsync(new ApplicationRole(
            adminRoleName,
            adminRoleDescription,
            user.Id,
            currentDateTime
        ));


    //Create Client role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(clientRoleName))
        await roleManager.CreateAsync(new ApplicationRole(
            clientRoleName,
            clientRoleDescription,
            user.Id,
            currentDateTime
        ));

    //Create Employee role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(employeeRoleName))
        await roleManager.CreateAsync(new ApplicationRole(
            employeeRoleName,
            employeeRoleDescription,
            user.Id,
            currentDateTime
        ));

    //scope IUserRoleService
    var userRoleService = scope.ServiceProvider.GetRequiredService<IUserRoleService>();

    //get admin role by name
    var adminRole = await roleManager.FindByNameAsync(adminRoleName);

    //check if System Admin user is already in System Admin role
    var hasAdminRole = await userManager.IsInRoleAsync(user, adminRole!.Name!);
    if (!hasAdminRole)
        await userRoleService.CreateAsync(new ApplicationUserRole
        {
            RoleId = adminRole.Id,
            UserId = user.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime,
            StartDate = currentDateTime,
            EndDate = DateTimeOffset.MaxValue
        });

    //get client role by name
    var clientRole = await roleManager.FindByNameAsync(clientRoleName);

    //check if System Admin user is already in Client role
    var hasClientRole = await userManager.IsInRoleAsync(user, clientRole!.Name!);
    if (!hasClientRole)
        await userRoleService.CreateAsync(new ApplicationUserRole
        {
            RoleId = clientRole.Id,
            UserId = user.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime,
            StartDate = currentDateTime,
            EndDate = DateTimeOffset.MaxValue
        });

    //get employee role by name
    var employeeRole = await roleManager.FindByNameAsync(employeeRoleName);

    //check if System Admin user is already in Employee role
    var hasEmployeeRole = await userManager.IsInRoleAsync(user, employeeRole!.Name!);
    if (!hasEmployeeRole)
        await userRoleService.CreateAsync(new ApplicationUserRole
        {
            RoleId = employeeRole.Id,
            UserId = user.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime,
            StartDate = currentDateTime,
            EndDate = DateTimeOffset.MaxValue
        });

    //get list of privileges from privilege enum and add to database
    var privileges = Enum.GetValues<PrivilegeConstant>().Select(p => new Privilege
    {
        Name = p.ToString(),
        Description = p.GetDescription(),
        CreatedDate = currentDateTime,
        CreatedByUserId = user.Id
    }).ToList();

    var privilegeService = scope.ServiceProvider.GetRequiredService<IPrivilegeService>();

    foreach (var privilege in privileges)
    {
        if (!await privilegeService.PrivilegeNameExistsAsync(privilege.Name))
            await privilegeService.CreateAsync(privilege);
    }

    //Assign all privileges to the System Admin role
    foreach (var privilege in privileges)
    {
        //check if privilege is already assigned to role
        var isInRole = await privilegeService.IsInRoleAsync(adminRole.Id, privilege.Id);
        if (isInRole) continue;
        await privilegeService.AddToRoleAsync(new RolePrivilege
        {
            RoleId = adminRole.Id,
            PrivilegeId = privilege.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime
        });
    }

    var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();

    //get the list of modules from module enum and add to the database
    var modules = Enum.GetValues<ModuleConstant>().Select(m => new Module
    {
        Name = m.ToString(),
        Description = m.GetDescription(),
        Code = m.GetModuleCode()!,
        CreatedDate = currentDateTime,
        CreatedByUserId = user.Id
    }).ToList();

    //check if module exists in the database, if not create
    foreach (var module in modules)
    {
        if (await moduleService.ModuleNameExistsAsync(module.Name) ||
            await moduleService.ModuleCodeExistsAsync(module.Code)) continue;
        await moduleService.CreateAsync(module);
    }

    //get all privileges that begin with 'SEC_' and assign them module 'SECURITY'
    var securityModule = modules.FirstOrDefault(m => m.Name == ModuleConstant.SECURITY.ToString());
    var securityPrivileges = privileges.Where(p => p.Name.StartsWith("SEC_")).ToList();

    foreach (var securityPrivilege in securityPrivileges)
    {
        //check if privilege is already assigned to module
        var isInModule = await moduleService.IsInModuleAsync(securityPrivilege.Id, securityModule!.Id);
        if (isInModule) continue;
        await moduleService.AddPrivilegeAsync(new ModulePrivilege
        {
            ModuleId = securityModule.Id,
            PrivilegeId = securityPrivilege.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime
        });
    }

    //create organization
    var organizationService = scope.ServiceProvider.GetRequiredService<IOrganizationService>();
    var organization = new Organization
    {
        Name = configuration1["Organization:Name"] ?? string.Empty,
        Description = configuration1["Organization:Description"] ?? string.Empty,
        CreatedByUserId = user.Id,
        CreatedDate = currentDateTime
    };
    //check if organization exists in database, if not create
    if (!await organizationService.OrganizationNameExistsAsync(organization.Name))
        await organizationService.CreateAsync(organization);

    //create branch
    var branchService = scope.ServiceProvider.GetRequiredService<IBranchService>();
    var branch = new Branch
    {
        Name = configuration1["Branch:Name"] ?? string.Empty,
        Description = configuration1["Branch:Description"] ?? string.Empty,
        OrganizationId = organization.Id,
        CreatedByUserId = user.Id,
        CreatedDate = currentDateTime
    };
    //check if branch exists in database, if not create
    if (!await branchService.BranchNameExistsAsync(branch.Name))
        await branchService.CreateAsync(branch);

    //create department
    var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
    var department = new Department
    {
        Name = configuration1["Department:Name"] ?? string.Empty,
        Description = configuration1["Department:Description"] ?? string.Empty,
        BranchId = branch.Id,
        CreatedByUserId = user.Id,
        CreatedDate = currentDateTime
    };
    //check if department exists in database, if not create
    if (!await departmentService.DepartmentNameExistsAsync(department.Name))
        await departmentService.CreateAsync(department);

    //create office
    var officeService = scope.ServiceProvider.GetRequiredService<IOfficeService>();
    var office = new Office
    {
        Name = configuration1["Office:Name"] ?? string.Empty,
        Description = configuration1["Office:Description"] ?? string.Empty,
        DepartmentId = department.Id,
        CreatedByUserId = user.Id,
        CreatedDate = currentDateTime
    };
    //check if office exists in database, if not create
    if (!await officeService.OfficeNameExistsAsync(office.Name))
        await officeService.CreateAsync(office);

    //check if System Admin user is already in the office
    var isInOffice = await officeService.IsInOfficeAsync(user.Id, office.Id);
    if (!isInOffice)
        await officeService.AddUserAsync(new UserOffice
        {
            OfficeId = office.Id,
            UserId = user.Id,
            CreatedByUserId = user.Id,
            CreatedDate = currentDateTime,
            StartDate = currentDateTime,
            EndDate = DateTimeOffset.MaxValue
        });
}