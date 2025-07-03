using FluentValidation;
using FluentValidation.AspNetCore;
using MiniApi.Application.DTOs.FileUpload;

namespace MiniApi.Application.Validations.UploadFileValidations;

public class FileUploadValidations : AbstractValidator<FileUploadDto>
{
    private static readonly List<string> AllowedContentTypes = new()
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        
        "image/png",
        "image/jpeg",
        
    };

    public FileUploadValidations()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Fayl boş ola bilməz.")
            .Must(file => file != null && file.Length > 0).WithMessage("Fayl boş ola bilməz.")
            .Must(file => file != null && file.Length <= 5 * 1024 * 1024) // 5MB limit
                .WithMessage("Faylın ölçüsü 5MB-dan çox ola bilməz.")
            .Must(file => file != null && AllowedContentTypes.Contains(file.ContentType))
                .WithMessage("Yalnız PDF, DOCX, DOC və şəkil fayllarına icazə verilir.");
    }
}
