namespace SkillHub.Application.DTOs.Enrollments;

public record CertificatePdfDto(
    byte[] Content,
    string FileName);
