using System.ComponentModel.DataAnnotations;

namespace zamren_management_system.Areas.Security.ViewModels;

public class CreateRoleViewModel
{
    [Required] 
    [Display(Name = "Role Name", Prompt = "Enter Role Name")]
    public string RoleName { get; set; }
    
    [Required]
    [Display(Name = "Description", Prompt = "Enter Role Description")]
    public string Description { get; set; }
}