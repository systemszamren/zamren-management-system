using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Identity.Data;
using zamren_management_system.Areas.Security.Configurations.PrivilegeAuthentication;
using zamren_management_system.Areas.Security.Dto;
using zamren_management_system.Areas.Security.Enums;
using zamren_management_system.Areas.Security.Interfaces;
using zamren_management_system.Areas.Security.Models;

namespace zamren_management_system.Areas.Security.ApiControllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Area("Security")]
[Route("api/security/office")]
public class OfficeApiController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IOfficeService _officeService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<OfficeApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUtil _util;
    private readonly ICypherService _cypherService;

    public OfficeApiController(IOfficeService officeService, IDepartmentService departmentService,
        IDatatableService datatableService, ILogger<OfficeApiController> logger,
        UserManager<ApplicationUser> userManager, IUtil util, ICypherService cypherService)
    {
        _officeService = officeService;
        _departmentService = departmentService;
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _util = util;
        _cypherService = cypherService;
    }

    //create office
    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> CreateOffice()
    {
        try
        {
            var officeDto = new OfficeDto
            {
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                DepartmentId = Request.Form["departmentId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(officeDto.Description))
                return Ok(new { success = false, message = "Office description is required" });

            if (string.IsNullOrEmpty(officeDto.Name))
                return Ok(new { success = false, message = "Office name is required" });

            //trim and remove extra spaces
            officeDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(officeDto.Name);

            if (!string.IsNullOrEmpty(officeDto.DepartmentId))
            {
                officeDto.DepartmentId = _cypherService.Decrypt(officeDto.DepartmentId);
                var department = await _departmentService.FindByIdAsync(officeDto.DepartmentId);
                if (department == null)
                {
                    return Ok(new { success = false, message = "Selected department does not exist" });
                }
            }

            if (await _officeService.OfficeNameExistsAsync(officeDto.Name))
            {
                return Ok(new { success = false, message = "Office with the same name already exists" });
            }

            await _officeService.CreateAsync(new Office
            {
                Name = officeDto.Name!,
                Description = officeDto.Description,
                DepartmentId = officeDto.DepartmentId,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = _userManager.GetUserId(User)!
            });
            return Ok(new { success = true, message = "Office created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> EditOffice()
    {
        try
        {
            var officeDto = new OfficeDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                DepartmentId = Request.Form["departmentId"].FirstOrDefault()!
            };

            if (string.IsNullOrEmpty(officeDto.Description))
                return Ok(new { success = false, message = "Office description is required" });

            if (string.IsNullOrEmpty(officeDto.Name))
                return Ok(new { success = false, message = "Office name is required" });

            if (string.IsNullOrEmpty(officeDto.Id))
                return Ok(new { success = false, message = "Office not found" });

            //trim and remove extra spaces
            officeDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(officeDto.Name);

            if (!string.IsNullOrEmpty(officeDto.DepartmentId))
            {
                officeDto.DepartmentId = _cypherService.Decrypt(officeDto.DepartmentId);
                if (!await _departmentService.DepartmentIdExistsAsync(officeDto.DepartmentId))
                {
                    return Ok(new { success = false, message = "Selected department does not exist" });
                }
            }

            officeDto.Id = _cypherService.Decrypt(officeDto.Id!);
            if (await _officeService.OfficeNameExistsExceptAsync(officeDto.Name, officeDto.Id!))
            {
                return Ok(new { success = false, message = "Office with the same name already exists" });
            }

            var office = await _officeService.FindByIdAsync(officeDto.Id!);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            if (!string.IsNullOrEmpty(officeDto.DepartmentId))
                office.DepartmentId = officeDto.DepartmentId;

            office.Description = officeDto.Description;
            office.Name = officeDto.Name!;
            office.ModifiedDate = DateTimeOffset.UtcNow;
            office.ModifiedByUserId = _userManager.GetUserId(User)!;
            await _officeService.UpdateAsync(office);
            return Ok(new { success = true, message = "Office updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //edit-user-office-tenure
    [HttpPost("edit-user-office-tenure")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> EditUserOfficeTenure()
    {
        try
        {
            var userOfficeDto = new UserOfficeDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                StartDateString = Request.Form["startDate"].FirstOrDefault(),
                EndDateString = Request.Form["endDate"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(userOfficeDto.Id))
                return Ok(new { success = false, message = "User office not found" });

            if (string.IsNullOrEmpty(userOfficeDto.StartDateString))
                return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(userOfficeDto.EndDateString))
                return Ok(new { success = false, message = "End date is required" });

            //if end date is before start date
            if (_util.ConvertStringToDateTimeOffset(userOfficeDto.StartDateString) >
                _util.ConvertStringToDateTimeOffset(userOfficeDto.EndDateString, true))
                return Ok(new { success = false, message = "End date cannot be before start date" });

            userOfficeDto.Id = _cypherService.Decrypt(userOfficeDto.Id!);
            var userOffice = await _officeService.FindUserOfficeByIdAsync(userOfficeDto.Id!);
            if (userOffice == null)
                return Ok(new { success = false, message = "User office not found" });

            userOffice.StartDate = _util.ConvertStringToDateTimeOffset(userOfficeDto.StartDateString);
            userOffice.EndDate = _util.ConvertStringToDateTimeOffset(userOfficeDto.EndDateString, true);
            userOffice.ModifiedDate = DateTimeOffset.UtcNow;
            userOffice.ModifiedByUserId = _userManager.GetUserId(User)!;
            await _officeService.UpdateUserOfficeAsync(userOffice);
            return Ok(new
            {
                success = true, officeId = _cypherService.Encrypt(userOffice.OfficeId),
                message = "User office tenure updated successfully"
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> DeleteOffice()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id)) return Ok(new { success = false, message = "Office not found" });

            id = _cypherService.Decrypt(id);
            var office = await _officeService.FindByIdAsync(id);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            //check if office has users
            var officeUsers = await _officeService.FindUsersAsync(id);
            if (officeUsers.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete office. It has " + officeUsers.Count + " user(s)"
                });

            //delete office
            await _officeService.DeleteAsync(office);
            return Ok(new { success = true, message = "Office deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }


    [HttpPost("add-user-to-office")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> AddUserToOffice()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var officeId = Request.Form["currentOfficeId"].FirstOrDefault() ?? "";
            var startDate = Request.Form["startDate"].FirstOrDefault() ?? "";
            var endDate = Request.Form["endDate"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId)) return Ok(new { success = false, message = "User is required" });

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office is required" });

            if (string.IsNullOrEmpty(startDate)) return Ok(new { success = false, message = "Start date is required" });

            if (string.IsNullOrEmpty(endDate)) return Ok(new { success = false, message = "End date is required" });

            //if end date is before start date
            if (_util.ConvertStringToDateTimeOffset(startDate) > _util.ConvertStringToDateTimeOffset(endDate, true))
                return Ok(new { success = false, message = "End date cannot be before start date" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            officeId = _cypherService.Decrypt(officeId);
            var office = await _officeService.FindByIdAsync(officeId);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            //check if user is already in office
            var userExistsInOffice = await _officeService.IsInOfficeAsync(user.Id, office.Id);
            if (userExistsInOffice)
                return Ok(new { success = false, message = "User already in office" });

            var result = await _officeService.AddUserAsync(new UserOffice
            {
                OfficeId = office.Id,
                UserId = user.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow,
                StartDate = _util.ConvertStringToDateTimeOffset(startDate),
                EndDate = _util.ConvertStringToDateTimeOffset(endDate, true)
            });

            return Ok(result.Succeeded
                ? new { success = true, message = "User added to office successfully" }
                : new { success = false, message = "Unable to add user to office" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("count-users-by-office-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> CountUsersByOfficeId([FromForm] string officeId)
    {
        try
        {
            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            officeId = _cypherService.Decrypt(officeId);
            var office = await _officeService.FindByIdAsync(officeId);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            var usersWithOffice = await _officeService.FindUsersAsync(officeId);
            var totalCount = usersWithOffice.Count;
            return Ok(new { success = true, totalCount, officeName = office.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-offices-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetOfficesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value"].FirstOrDefault() ?? "";
            var offices = await _officeService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                offices.ToList().Select((office, index) => new OfficeDto
                {
                    Id = _cypherService.Encrypt(office.Id),
                    Name = office.Name,
                    Counter = index + 1,
                    Description = office.Description,
                    Department = new DepartmentDto
                    {
                        Id = _cypherService.Encrypt(office.DepartmentId),
                        Name = office.Department.Name,
                        Description = office.Department.Description
                    }
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-office-data
    [HttpPost("get-office-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetOfficeData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id)) return Ok(new { success = false, message = "Office not found" });

            if (string.IsNullOrEmpty(id)) return Ok(new { success = false, message = "Office not found" });

            id = _cypherService.Decrypt(id);
            var office = await _officeService.FindByIdAsync(id);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            return Ok(new
            {
                success = true,
                office = new OfficeDto
                {
                    Id = _cypherService.Encrypt(office.Id),
                    Name = office.Name,
                    Description = office.Description,
                    Department = new DepartmentDto
                    {
                        Id = _cypherService.Encrypt(office.DepartmentId),
                        Name = office.Department.Name,
                        Description = office.Department.Description
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-office-user-data
    [HttpPost("get-office-user-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetOfficeUserData()
    {
        try
        {
            var userOfficeId = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userOfficeId))
                return Ok(new { success = false, message = "User office not found" });

            userOfficeId = _cypherService.Decrypt(userOfficeId);
            var userOffice = await _officeService.FindUserOfficeByIdAsync(userOfficeId);

            if (userOffice == null)
                return Ok(new { success = false, message = "User office not found" });

            return Ok(new
            {
                success = true, userOffice = new UserOfficeDto
                {
                    Id = _cypherService.Encrypt(userOffice.Id),
                    UserId = _cypherService.Encrypt(userOffice.UserId),
                    OfficeId = _cypherService.Encrypt(userOffice.OfficeId),
                    StartDate = userOffice.StartDate.ToLocalTime(),
                    EndDate = userOffice.EndDate.ToLocalTime(),
                    FirstName = userOffice.User.FirstName,
                    LastName = userOffice.User.LastName,
                    Email = userOffice.User.Email,
                    OfficeName = userOffice.Office.Name
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //get-users-in-office-dt2
    [HttpPost("get-users-in-office-dt2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetUsersInOfficeDt2()
    {
        try
        {
            var officeId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            officeId = _cypherService.Decrypt(officeId);
            var officeUsers = await _officeService.FindUsersAsync(officeId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.FirstName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.LastName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Email!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                officeUsers.ToList().Select((userOffice, index) => new UserOfficeDto
                {
                    Id = _cypherService.Encrypt(userOffice.Id),
                    Counter = index + 1,
                    UserId = _cypherService.Encrypt(userOffice.User.Id),
                    OfficeId = _cypherService.Encrypt(userOffice.Office.Id),
                    FirstName = userOffice.User.FirstName,
                    LastName = userOffice.User.LastName,
                    Email = userOffice.User.Email,
                    StartDate = userOffice.StartDate.ToLocalTime(),
                    EndDate = userOffice.EndDate.ToLocalTime()
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-users-not-in-office-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetUsersNotInOfficeSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;
            var officeId = Request.Form["officeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            officeId = _cypherService.Decrypt(officeId);
            var office = await _officeService.FindByIdAsync(officeId);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            //get all users not in the office by office id
            var usersNotInOffice = await _officeService.FindUsersNotInOfficeAsync(officeId);

            // Filter the users based on the search term ignoring case
            var usersQuery = usersNotInOffice
                .Where(user => user.Email != null && (user.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                                      user.LastName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                                      user.Email.Contains(q, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var totalCount = usersQuery.Count;

            var users = usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserDto
                {
                    Id = _cypherService.Encrypt(user.Id),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                })
                .ToList();

            return Ok(new { results = users, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //remove-user-from-office
    [HttpPost("remove-user-from-office")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> RemoveUserFromOffice()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var officeId = Request.Form["officeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId)) return Ok(new { success = false, message = "User not found" });

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            officeId = _cypherService.Decrypt(officeId);
            var office = await _officeService.FindByIdAsync(officeId);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            //find user office record by user id and office id
            var userOffice = await _officeService.GetUserOfficeAsync(userId, officeId);
            if (userOffice == null)
                return Ok(new { success = false, message = "User not in office" });

            var result = await _officeService.RemoveUserAsync(userOffice);
            return result.Succeeded
                ? Ok(new { success = true, message = "User removed from office successfully" })
                : Ok(new { success = false, message = result.Errors.First().Description });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    //check-user-exists-in-office
    [HttpPost("check-user-exists-in-office")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> CheckUserExistsInOffice()
    {
        try
        {
            var userId = Request.Form["userId"].FirstOrDefault() ?? "";
            var officeId = Request.Form["officeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(userId)) return Ok(new { success = false, message = "User not found" });

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            userId = _cypherService.Decrypt(userId);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Ok(new { success = false, message = "User not found" });

            officeId = _cypherService.Decrypt(officeId);
            var office = await _officeService.FindByIdAsync(officeId);
            if (office == null)
                return Ok(new { success = false, message = "Office not found" });

            //check if user is in office
            var userExistsInOffice = await _officeService.IsInOfficeAsync(userId, officeId);
            return Ok(new { success = true, exists = userExistsInOffice });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-departments-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetDepartmentsSelect2()
    {
        try
        {
            var q = Request.Form["q"].FirstOrDefault() ?? "";
            var pageNumber = int.TryParse(Request.Form["pageNumber"].FirstOrDefault(), out var pageNumber1)
                ? pageNumber1
                : 1;
            var pageSize = int.TryParse(Request.Form["pageSize"].FirstOrDefault(), out var pageSize1)
                ? pageSize1
                : 10;

            var departments = await _departmentService.FindAllAsync();

            // Filter the departments based on the search term ignoring case
            var departmentsQuery = departments
                .Where(department => department.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = departmentsQuery.Count;

            var departmentsList = departmentsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(department => new DepartmentDto
                {
                    Id = _cypherService.Encrypt(department.Id),
                    Name = department.Name,
                    Description = department.Description
                })
                .ToList();

            return Ok(new { results = departmentsList, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-users-in-office-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetUsersInOfficeDt()
    {
        try
        {
            var officeId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            officeId = _cypherService.Decrypt(officeId);
            var users = await _officeService.FindUsersAsync(officeId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.FirstName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.LastName!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Email!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                users.ToList().Select((userOffice, index) => new UserOfficeDto
                {
                    Counter = index + 1,
                    UserId = _cypherService.Encrypt(userOffice.User.Id),
                    OfficeId = _cypherService.Encrypt(userOffice.Office.Id),
                    FirstName = userOffice.User.FirstName,
                    LastName = userOffice.User.LastName,
                    Email = userOffice.User.Email,
                    StartDate = userOffice.StartDate.ToLocalTime(),
                    EndDate = userOffice.EndDate.ToLocalTime()
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-offices-except")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetOfficesExcept()
    {
        try
        {
            var officeId = Request.Form["officeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(officeId)) return Ok(new { success = false, message = "Office not found" });

            officeId = _cypherService.Decrypt(officeId);
            var offices = await _officeService.FindAllExceptAsync(officeId);

            return Ok(new
            {
                success = true,
                offices = offices.Select(office => new OfficeDto
                {
                    Id = _cypherService.Encrypt(office.Id),
                    Name = office.Name,
                    Description = office.Description
                }).ToList()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-departments-except")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_OFFICES)]
    public async Task<IActionResult> GetDepartmentsExcept()
    {
        try
        {
            var departmentId = Request.Form["departmentId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(departmentId))
                return Ok(new { success = false, message = "Department not found" });

            departmentId = _cypherService.Decrypt(departmentId);
            var departments = await _departmentService.FindAllExceptAsync(departmentId);

            return Ok(new
            {
                success = true,
                departments = departments.Select(department => new DepartmentDto
                {
                    Id = _cypherService.Encrypt(department.Id),
                    Name = department.Name,
                    Description = department.Description
                }).ToList()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}