using Microsoft.AspNetCore.Identity;

namespace SkillHub.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
    }

    public ApplicationUser(Guid id, string email)
    {
        Id = id;
        UserName = email;
        Email = email;
    }
}
