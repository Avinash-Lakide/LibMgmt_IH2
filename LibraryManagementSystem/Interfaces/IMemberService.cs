using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface IMemberService : IService<Member>
    {
        Task<Member?> GetByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
        Task<(IEnumerable<Member> members, int total)> GetMembersAsync(int page, int size);
    }
}