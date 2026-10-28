using SkillHub.Domain.Entities;

namespace SkillHub.Application.Interfaces.Services;

public interface IPdfCertificateGenerator
{
    byte[] GenerateEnrollmentCertificate(Enrollment enrollment);
}
