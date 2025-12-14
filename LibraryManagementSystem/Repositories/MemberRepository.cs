using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories
{
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        public MemberRepository(LibraryDbContext context) : base(context) { }

        public async Task<Member?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.EmailId == email);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        {
            var query = _dbSet.Where(m => m.EmailId == email);
            if (excludeId.HasValue)
            {
                query = query.Where(m => m.MemberId != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<(IEnumerable<Member> members, int total)> GetMembersAsync(int page, int size)
        {
            var query = _dbSet.AsQueryable();
            var total = await query.CountAsync();
            var members = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return (members, total);
        }
    }
}