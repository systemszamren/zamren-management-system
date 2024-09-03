using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using zamren_management_system.Areas.Common.Dto;
using zamren_management_system.Areas.Common.Interfaces;

namespace zamren_management_system.Areas.Common.Services;

public class SystemAttachmentManager : ISystemAttachmentManager
{
    private readonly ILogger<SystemAttachmentManager> _logger;
    private readonly IConfiguration _configuration;

    public SystemAttachmentManager(ILogger<SystemAttachmentManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public SystemAttachmentDto? UploadFile(IFormFile formFile, string? customFileName, string? folderName)
    {
        try
        {
            var baseFilePath = _configuration["SystemVariables:BaseFilePath"] ?? string.Empty;
            if (string.IsNullOrEmpty(folderName)) folderName = "common";
            var dirPath = Path.Combine(baseFilePath, folderName);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var guid = Guid.NewGuid().ToString();
            var timeStamp = DateTime.Now.Ticks.ToString();
            var systemFileName = guid + "_" + timeStamp + "_" + formFile.FileName;
            var filePath = Path.Combine(dirPath, systemFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            formFile.CopyToAsync(fileStream);

            // if filePath contains 'wwwroot' remove it
            if (filePath.Contains("wwwroot"))
                filePath = filePath.Replace("wwwroot", string.Empty);

            return new SystemAttachmentDto
            {
                FilePath = filePath,
                SystemFileName = systemFileName,
                CustomFileName = string.IsNullOrEmpty(customFileName) ? formFile.FileName : customFileName,
                OriginalFileName = formFile.FileName,
                FileSize = formFile.Length,
                ContentType = formFile.ContentType,
                FileExtension = formFile.FileName.Split(".").Last()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return null;
        }
    }

    public async Task<SystemAttachmentDto?> UploadFile(IFormFile formFile, string? folderName, bool compressImage = false)
    {
        try
        {
            var baseFilePath = _configuration["SystemVariables:BaseFilePath"] ?? string.Empty;
            if (string.IsNullOrEmpty(folderName)) folderName = "common";
            var dirPath = Path.Combine(baseFilePath, folderName);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            var guid = Guid.NewGuid().ToString();
            var timeStamp = DateTime.Now.Ticks.ToString();
            var systemFileName = guid + "_" + timeStamp +
                                 formFile.FileName[formFile.FileName.LastIndexOf(".", StringComparison.Ordinal)..];
            var filePath = Path.Combine(dirPath, systemFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);

            // Check if the file is an image and its size is greater than 100KB
            if (compressImage && formFile.ContentType.StartsWith("image/") && formFile.Length > 100 * 1024)
            {
                // Load the image
                var image = await Image.LoadAsync(formFile.OpenReadStream());

                // Compress the image
                image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

                // Save the compressed image
                await image.SaveAsync(fileStream, new JpegEncoder { Quality = 75 });

                // Update the file size after compression
                fileStream.SetLength(fileStream.Length);
            }
            else
            {
                await formFile.CopyToAsync(fileStream);
            }

            // if filePath contains 'wwwroot' remove it
            if (filePath.Contains("wwwroot"))
                filePath = filePath.Replace("wwwroot", string.Empty);

            filePath = filePath.Replace("\\", "/");

            if (fileStream.Length == 0) return null;

            return new SystemAttachmentDto
            {
                FilePath = filePath,
                SystemFileName = systemFileName,
                CustomFileName = formFile.FileName,
                OriginalFileName = formFile.FileName,
                FileSize = fileStream.Length, // Update the file size after compression
                ContentType = formFile.ContentType,
                FileExtension = formFile.FileName.Split(".").Last()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return null;
        }
    }

    public List<SystemAttachmentDto> UploadFiles(IEnumerable<IFormFile> formFiles, string folderName)
    {
        try
        {
            return formFiles.Select(formFile => UploadFile(formFile, folderName).Result!).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return new List<SystemAttachmentDto>();
        }
    }


    public IdentityResult IsValid(IFormFile? file)
    {
        try
        {
            if (file == null)
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "IsValid",
                    Description = "Attachment is required"
                });

            if (file.Length <= 0)
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "IsValid",
                    Description = "Attachment is empty"
                });

            var maxFileSizeInMb = Convert.ToInt32(_configuration["SystemVariables:MaxFileSizeInMB"]);
            if (file.Length > maxFileSizeInMb * 1024 * 1024)
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "IsValid",
                    Description = "Attachment size exceeds the maximum allowed size. Maximum size is " +
                                  maxFileSizeInMb + "MB"
                });

            var fNames = file.FileName.Split(".");
            if (!fNames[^1].Equals("pdf") && !fNames[^1].Equals("jpg") && !fNames[^1].Equals("png") &&
                !fNames[^1].Equals("jpeg"))
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "IsValid",
                    Description = "Invalid file format. Supported formats are pdf, jpg, png and jpeg"
                });

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "IsValid",
                Description = "File not valid"
            });
        }
    }

    public IdentityResult IsValid(IEnumerable<IFormFile?> files)
    {
        try
        {
            foreach (var file in files)
            {
                var isValid = IsValid(file);
                if (!isValid.Succeeded) return isValid;
            }

            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return IdentityResult.Failed(new IdentityError
            {
                Code = "IsValid",
                Description = "File not valid"
            });
        }
    }

    public bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            var baseFilePath = _configuration["SystemVariables:BaseFilePath"] ?? string.Empty;

            if (!File.Exists(baseFilePath + filePath)) return false;
            File.Delete(baseFilePath + filePath);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the request");
            return false;
        }
    }

    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }
}