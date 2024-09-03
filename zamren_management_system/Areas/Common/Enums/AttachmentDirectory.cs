using zamren_management_system.Areas.Common.Attributes;

namespace zamren_management_system.Areas.Common.Enums;

public enum AttachmentDirectory
{
    [DirectoryName("common")] Common,
    [DirectoryName("profile-pictures")] ProfilePictures,
    [DirectoryName("identity-documents")] IdentityDocuments
}