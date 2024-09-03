using Microsoft.AspNetCore.Identity;
using zamren_management_system.Areas.Common.Dto;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface ISystemAttachmentManager
{
    /// <summary>
    ///  Uploads a file to the specified folder.
    /// </summary>
    /// <param name="formFile"></param>
    /// <param name="folderName"></param>
    /// <param name="compressImage"></param>
    /// <returns> AttachmentDto </returns>
    Task<SystemAttachmentDto?> UploadFile(IFormFile formFile, string? folderName, bool compressImage = false);

    /// <summary>
    ///  Uploads a file to the specified folder with a custom file name.
    /// </summary>
    /// <param name="formFile"> Mandatory: IFormFile </param>
    /// <param name="customFileName"> Optional: string </param>
    /// <param name="folderName"> Mandatory: string </param>
    /// <returns> AttachmentDto </returns>
    SystemAttachmentDto? UploadFile(IFormFile formFile, string customFileName, string folderName);

    /// <summary>
    ///  Uploads multiple files to the specified folder.
    /// </summary>
    /// <param name="formFiles"></param>
    /// <param name="folderName"></param>
    /// <returns> IEnumerable<AttachmentDto/> </returns>
    List<SystemAttachmentDto> UploadFiles(IEnumerable<IFormFile> formFiles, string folderName);

    /// <summary>
    ///  Checks if the file is valid.
    /// </summary>
    /// <param name="file"> IFormFile </param>
    /// <returns> IdentityResult </returns>
    IdentityResult IsValid(IFormFile? file);

    /// <summary>
    ///  Checks if the files are valid.
    /// </summary>
    /// <param name="files"> List<IFormFile/> </param>
    /// <returns> IdentityResult </returns>
    IdentityResult IsValid(IEnumerable<IFormFile?> files);

    /// <summary>
    ///  Deletes a file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns> bool </returns>
    bool DeleteFile(string filePath);

    /// <summary>
    ///  Checks if a file exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns> bool </returns>
    public bool Exists(string filePath);
}