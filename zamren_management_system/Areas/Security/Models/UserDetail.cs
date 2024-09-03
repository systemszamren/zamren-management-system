using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Models;
using zamren_management_system.Areas.Identity.Data;

namespace zamren_management_system.Areas.Security.Models;

public class UserDetail
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required] [StringLength(255)] public string UserId { get; set; }

    public ApplicationUser User { get; set; }

    [PersonalData] [StringLength(255)] public string? Gender { get; set; }

    [PersonalData] public DateTimeOffset? DateOfBirth { get; set; }

    public string? ProfilePictureAttachmentId { get; set; }

    public SystemAttachment? ProfilePictureAttachment { get; set; }

    [PersonalData] [StringLength(255)] public string? IdentityType { get; set; }

    [PersonalData] [StringLength(255)] public string? IdentityNumber { get; set; }

    public string? IdentityAttachmentId { get; set; }

    public SystemAttachment? IdentityAttachment { get; set; }

    [PersonalData] [StringLength(255)] public string? Country { get; set; }

    [PersonalData] [StringLength(255)] public string? City { get; set; }

    [PersonalData]
    [Phone]
    [StringLength(255)]
    public string? AlternativePhoneNumber { get; set; }

    public bool? AlternativePhoneNumberConfirmed { get; set; }

    [EmailAddress]
    [PersonalData]
    [StringLength(255)]
    public string? AlternativeEmailAddress { get; set; }

    public bool? AlternativeEmailAddressConfirmed { get; set; }

    [PersonalData]
    [DataType(DataType.Text)]
    public string? PhysicalAddress { get; set; }

    public bool? TermsOfUseAccepted { get; set; }

    public bool? PrivacyPolicyAccepted { get; set; }

    public string? ProofOfResidencyAttachmentId { get; set; }

    public SystemAttachment? ProofOfResidencyAttachment { get; set; }

    [Required] public int ProfileCompletionPercentage { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinFirstName { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinLastName { get; set; }

    [NotMapped] public string NextOfKinFullName => $"{NextOfKinFirstName} {NextOfKinLastName}";

    [PersonalData] [StringLength(255)] public string? NextOfKinIdentityType { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinIdentityNumber { get; set; }

    public string? NextOfKinIdentityAttachmentId { get; set; }

    public SystemAttachment? NextOfKinIdentityAttachment { get; set; }

    [PersonalData]
    [DataType(DataType.Text)]
    public string? NextOfKinPhysicalAddress { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinGender { get; set; }

    [PersonalData]
    [Phone]
    [StringLength(255)]
    public string? NextOfKinPhoneNumber { get; set; }

    public bool? NextOfKinPhoneNumberConfirmed { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinCountry { get; set; }

    [PersonalData] [StringLength(255)] public string? NextOfKinCity { get; set; }

    [EmailAddress]
    [PersonalData]
    [StringLength(255)]
    public string? NextOfKinEmailAddress { get; set; }

    public bool? NextOfKinEmailAddressConfirmed { get; set; }

    public string? NextOfKinProofOfResidencyAttachmentId { get; set; }

    public SystemAttachment? NextOfKinProofOfResidencyAttachment { get; set; }

    [Required] [StringLength(255)] public string Status { get; set; } = Common.Enums.Status.Active.ToString();

    [Required] public string CreatedByUserId { get; set; }
    public ApplicationUser CreatedBy { get; set; }
    [Required] public DateTimeOffset CreatedDate { get; set; }
    public string? ModifiedByUserId { get; set; }
    public ApplicationUser? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
}