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
[Route("api/security/module")]
public class ModuleApiController : ControllerBase
{
    private readonly IModuleService _moduleService;
    private readonly IPrivilegeService _privilegeService;
    private readonly IDatatableService _datatableService;
    private readonly ILogger<ModuleApiController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ICypherService _cypherService;
    private readonly IUtil _util;

    public ModuleApiController(IPrivilegeService privilegeService,
        IDatatableService datatableService,
        ILogger<ModuleApiController> logger, UserManager<ApplicationUser> userManager,
        IModuleService moduleService, IUtil util, ICypherService cypherService, IConfiguration configuration)
    {
        _privilegeService = privilegeService;
        _datatableService = datatableService;
        _logger = logger;
        _userManager = userManager;
        _moduleService = moduleService;
        _util = util;
        _cypherService = cypherService;
        _configuration = configuration;
    }

    [HttpPost("get-modules-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> GetModulesDt()
    {
        try
        {
            var searchValue = Request.Form["search[value"].FirstOrDefault() ?? "";
            var modules = await _moduleService.FindAllAsync();

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                modules.ToList().Select((module, index) => new ModuleDto
                {
                    Id = _cypherService.Encrypt(module.Id),
                    Name = module.Name,
                    Code = module.Code,
                    Counter = index + 1,
                    Description = module.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("count-privileges-by-module-id")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> CountPrivilegesByModuleId([FromForm] string moduleId)
    {
        try
        {
            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module id is required" });

            moduleId = _cypherService.Decrypt(moduleId);
            var module = await _moduleService.FindByIdAsync(moduleId);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            var privilegesWithModule = await _moduleService.FindPrivilegesAsync(moduleId);
            var totalCount = privilegesWithModule.Count();
            return Ok(new { success = true, totalCount, moduleName = module.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("create")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> CreateModule()
    {
        try
        {
            var moduleDto = new ModuleDto
            {
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Code = Request.Form["code"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(moduleDto.Description))
                return Ok(new { success = false, message = "Module description is required" });

            if (string.IsNullOrEmpty(moduleDto.Name))
                return Ok(new { success = false, message = "Module name is required" });

            if (string.IsNullOrEmpty(moduleDto.Code))
                return Ok(new { success = false, message = "Module code is required" });

            //trim and remove extra spaces
            moduleDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(moduleDto.Name);
            moduleDto.Code = _util.TrimAndRemoveExtraSpacesAndToUpperCase(moduleDto.Code);

            if (await _moduleService.ModuleNameExistsAsync(moduleDto.Name))
            {
                return Ok(new { success = false, message = "Module with the same name already exists" });
            }

            if (await _moduleService.ModuleCodeExistsAsync(moduleDto.Code))
            {
                return Ok(new { success = false, message = "Module with the same code already exists" });
            }

            // var refNoPrefixLength = Convert.ToInt32(_configuration["WorkflowVariables:ReferenceNumberPrefixLength"]);
            
            
            await _moduleService.CreateAsync(new Module
            {
                Name = moduleDto.Name!,
                Code = moduleDto.Code!,
                Description = moduleDto.Description,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByUserId = _userManager.GetUserId(User)!
            });
            return Ok(new { success = true, message = "Module created successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("delete")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> DeleteModule([FromForm] string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Module id is required" });

            id = _cypherService.Decrypt(id);
            var module = await _moduleService.FindByIdAsync(id);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            //check if module has privileges
            var privilegesWithModule = await _moduleService.FindPrivilegesAsync(id);
            var privileges = privilegesWithModule.ToList();
            if (privileges.Any())
                return Ok(new
                {
                    success = false,
                    message = "Cannot delete module. It has " + privileges.Count + " privilege(s)"
                });

            //delete module
            await _moduleService.DeleteAsync(module);
            return Ok(new { success = true, message = "Module deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("edit")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> EditModule()
    {
        try
        {
            var moduleDto = new ModuleDto
            {
                Id = Request.Form["id"].FirstOrDefault(),
                Description = Request.Form["description"].FirstOrDefault(),
                Name = Request.Form["name"].FirstOrDefault(),
                Code = Request.Form["code"].FirstOrDefault()
            };

            if (string.IsNullOrEmpty(moduleDto.Description))
                return Ok(new { success = false, message = "Module description is required" });

            if (string.IsNullOrEmpty(moduleDto.Name))
                return Ok(new { success = false, message = "Module name is required" });

            if (string.IsNullOrEmpty(moduleDto.Code))
                return Ok(new { success = false, message = "Module code is required" });

            if (string.IsNullOrEmpty(moduleDto.Id))
                return Ok(new { success = false, message = "An error occurred while processing the request" });

            //trim and remove extra spaces
            moduleDto.Name = _util.TrimAndRemoveExtraSpacesAndToUpperCase(moduleDto.Name);
            moduleDto.Code = _util.TrimAndRemoveExtraSpacesAndToUpperCase(moduleDto.Code);

            moduleDto.Id = _cypherService.Decrypt(moduleDto.Id!);

            //get module by id
            var module = await _moduleService.FindByIdAsync(moduleDto.Id!);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            if (await _moduleService.ModuleNameExistsExceptAsync(moduleDto.Name, moduleDto.Id!))
            {
                return Ok(new { success = false, message = "Module  with the same name already exists" });
            }

            if (await _moduleService.ModuleCodeExistsExceptAsync(moduleDto.Code, moduleDto.Id!))
            {
                return Ok(new { success = false, message = "Module with the same code already exists" });
            }

            //update module
            module.Name = moduleDto.Name;
            module.Code = moduleDto.Code!;
            module.Description = moduleDto.Description;
            module.ModifiedByUserId = _userManager.GetUserId(User);
            module.ModifiedDate = DateTimeOffset.UtcNow;
            await _moduleService.UpdateAsync(module);
            return Ok(new { success = true, message = "Module updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-module-data")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> GetModuleData()
    {
        try
        {
            var id = Request.Form["id"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(id))
                return Ok(new { success = false, message = "Module id is required" });

            id = _cypherService.Decrypt(id);
            var module = await _moduleService.FindByIdAsync(id);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            return Ok(new
            {
                success = true,
                module = new ModuleDto
                {
                    Id = _cypherService.Encrypt(module.Id),
                    Name = module.Name,
                    Code = module.Code,
                    Description = module.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-privileges-in-module-dt")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> GetPrivilegesInModuleDt()
    {
        try
        {
            var moduleId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            moduleId = _cypherService.Decrypt(moduleId);
            var privileges = await _moduleService.FindPrivilegesAsync(moduleId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                privileges.ToList().Select((privilege, index) => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege!.Privilege.Id),
                    Name = privilege.Privilege.Name,
                    Counter = index + 1,
                    Description = privilege.Privilege.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-privileges-not-in-module-select2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> GetPrivilegesNotInModuleSelect2()
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
            var moduleId = Request.Form["moduleId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module id is required" });

            moduleId = _cypherService.Decrypt(moduleId);

            //get module by id
            var module = await _moduleService.FindByIdAsync(moduleId);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            //get all privileges not in the module by module id
            var privilegesNotInModule = await _moduleService.FindNotInModuleAsync(moduleId);

            // Filter the privileges based on the search term ignoring case
            var privilegesQuery = privilegesNotInModule
                .Where(privilege => privilege.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

            var totalCount = privilegesQuery.Count;

            var privileges = privilegesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(privilege => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege.Id),
                    Name = privilege.Name,
                    Description = privilege.Description
                })
                .ToList();

            return Ok(new { results = privileges, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("check-privilege-exists-in-module")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> CheckPrivilegeExistsInModule()
    {
        try
        {
            var moduleId = Request.Form["moduleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module is required" });

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "Privilege is required" });

            moduleId = _cypherService.Decrypt(moduleId);
            var module = await _moduleService.FindByIdAsync(moduleId);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege is in module
            var privilegeExistsInModule = await _moduleService.IsInModuleAsync(privilegeId, moduleId);
            return Ok(new { success = true, exists = privilegeExistsInModule });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("get-privileges-in-module-dt2")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> GetPrivilegesInModuleDt2()
    {
        try
        {
            var moduleId = Request.Form["id"].FirstOrDefault() ?? "";
            var searchValue = Request.Form["search[value]"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module id is required" });

            moduleId = _cypherService.Decrypt(moduleId);
            var privileges = await _moduleService.FindPrivilegesAsync(moduleId);

            return _datatableService.GetEntitiesForDatatable(
                Request.Form,
                p => p.Name!.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                     p.Description!.Contains(searchValue, StringComparison.OrdinalIgnoreCase),
                p => p,
                privileges.ToList().Select((privilege, index) => new PrivilegeDto
                {
                    Id = _cypherService.Encrypt(privilege!.Privilege.Id),
                    Name = privilege.Privilege.Name,
                    Counter = index + 1,
                    Description = privilege.Privilege.Description
                }).ToList()
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("remove-privilege-from-module")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> RemovePrivilegeFromModule()
    {
        try
        {
            var moduleId = Request.Form["moduleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module is required" });

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "Privilege is required" });

            moduleId = _cypherService.Decrypt(moduleId);
            var module = await _moduleService.FindByIdAsync(moduleId);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege is in module
            var privilegeExistsInModule = await _moduleService.IsInModuleAsync(privilegeId, moduleId);
            if (!privilegeExistsInModule)
                return Ok(new { success = false, message = "Privilege not in module" });

            var result = await _moduleService.RemovePrivilegeAsync(moduleId, privilegeId);
            return result.Succeeded
                ? Ok(new { success = true, message = "Privilege removed from module successfully" })
                : Ok(new { success = false, message = result.Errors.First().Description });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }

    [HttpPost("add-privilege-to-module")]
    [HasPrivilege(PrivilegeConstant.SEC_MANAGE_SYSTEM_MODULES)]
    public async Task<IActionResult> AddPrivilegeToModule()
    {
        try
        {
            var moduleId = Request.Form["currentModuleId"].FirstOrDefault() ?? "";
            var privilegeId = Request.Form["privilegeId"].FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(moduleId))
                return Ok(new { success = false, message = "Module is required" });

            if (string.IsNullOrEmpty(privilegeId))
                return Ok(new { success = false, message = "Privilege is required" });


            moduleId = _cypherService.Decrypt(moduleId);
            var module = await _moduleService.FindByIdAsync(moduleId);
            if (module == null)
                return Ok(new { success = false, message = "Module not found" });

            privilegeId = _cypherService.Decrypt(privilegeId);
            var privilege = await _privilegeService.FindByIdAsync(privilegeId);
            if (privilege == null)
                return Ok(new { success = false, message = "Privilege not found" });

            //check if privilege is in module
            var privilegeExistsInModule = await _moduleService.IsInModuleAsync(privilegeId, moduleId);
            if (privilegeExistsInModule)
                return Ok(new { success = false, message = "Privilege already in module" });

            var result = await _moduleService.AddPrivilegeAsync(new ModulePrivilege
            {
                ModuleId = module.Id,
                PrivilegeId = privilege.Id,
                CreatedByUserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTimeOffset.UtcNow
            });

            return Ok(result.Succeeded
                ? new { success = true, message = "Privilege added to module successfully" }
                : new { success = false, message = "Unable to add privilege to module" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request");
            return Ok(new { success = false, message = "An error occurred while processing the request" });
        }
    }
}