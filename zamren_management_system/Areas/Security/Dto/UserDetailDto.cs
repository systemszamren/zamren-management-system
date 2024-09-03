namespace zamren_management_system.Areas.Security.Dto;

public class UserDetailDto
{
    public string? Id { get; set; }
    public string? PlainId { get; set; }
    public string? UserId { get; set; }
    public UserDto? User { get; set; }
    public string? Gender { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? DateOfBirthString { get; set; }
    public string? ProfilePictureAttachmentId { get; set; }
    public string? IdentityType { get; set; }
    public string? IdentityNumber { get; set; }
    public string? IdentityAttachmentId { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? AlternativePhoneNumber { get; set; }
    public bool? AlternativePhoneNumberConfirmed { get; set; }
    public string? AlternativePhoneNumberCountryCode { get; set; }
    public string? AlternativeEmailAddress { get; set; }
    public bool? AlternativeEmailAddressConfirmed { get; set; }
    public string? PhysicalAddress { get; set; }
    public  string? IsEmployee { get; set; }
    public string? IsEmployeeString { get; set; }
    public bool? TermsOfUseAccepted { get; set; }
    public string? TermsOfUseAcceptedString { get; set; }
    public bool? PrivacyPolicyAccepted { get; set; }
    public string? PrivacyPolicyAcceptedString { get; set; }
    public string? ProofOfResidencyAttachmentId { get; set; }
    public string? NextOfKinFirstName { get; set; }
    public string? NextOfKinLastName { get; set; }
    public string? NextOfKinIdentityType { get; set; }
    public string? NextOfKinIdentityNumber { get; set; }
    public string? NextOfKinIdentityAttachmentId { get; set; }
    public string? NextOfKinPhysicalAddress { get; set; }
    public string? NextOfKinGender { get; set; }
    public string? NextOfKinPhoneNumber { get; set; }
    public bool? NextOfKinPhoneNumberConfirmed { get; set; }
    public string? NextOfKinPhoneNumberCountryCode { get; set; }
    public string? NextOfKinCountryCode { get; set; }
    public string? NextOfKinCity { get; set; }
    public string? NextOfKinEmailAddress { get; set; }
    public bool? NextOfKinEmailAddressConfirmed { get; set; }
    public string? NextOfKinProofOfResidencyAttachmentId { get; set; }
    public int ProfileCompletionPercentage { get; set; }
}