using Ladestander.Api.Data;
using Ladestander.Api.Entities;
using Ladestander.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _context;

    public AdminUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUser?> GetByUsernameAsync(string username)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(x => x.Username == username && x.IsActive);
    }
}