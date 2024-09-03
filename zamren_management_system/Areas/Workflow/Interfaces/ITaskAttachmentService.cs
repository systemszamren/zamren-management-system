using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Workflow.Models;

namespace zamren_management_system.Areas.Workflow.Interfaces;

public interface ITaskAttachmentService
{
    Task<IdentityResult> CreateAsync(WkfTaskAttachment taskAttachment);

    Task<IdentityResult> CreateAsync(IEnumerable<WkfTaskAttachment> taskAttachments);

    Task<IdentityResult> UpdateAsync(WkfTaskAttachment taskAttachment);

    Task<IdentityResult> DeleteAsync(WkfTaskAttachment taskAttachment);
}