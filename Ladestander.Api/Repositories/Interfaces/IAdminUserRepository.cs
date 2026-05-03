using Ladestander.Api.Entities;

namespace Ladestander.Api.Repositories.Interfaces;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByUsernameAsync(string username);
}