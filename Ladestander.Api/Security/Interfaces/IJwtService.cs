using Ladestander.Api.Entities;

namespace Ladestander.Api.Security.Interfaces;

public interface IJwtService
{
    string GenerateToken(AdminUser adminUser);
}