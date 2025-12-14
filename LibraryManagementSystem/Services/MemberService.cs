using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class MemberService : Service<Member>, IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository) : base(memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public async Task<Member?> GetByEmailAsync(string email)
        {
            return await _memberRepository.GetByEmailAsync(email);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        {
            return await _memberRepository.IsEmailUniqueAsync(email, excludeId);
        }

        public async Task<(IEnumerable<Member> members, int total)> GetMembersAsync(int page, int size)
        {
            return await _memberRepository.GetMembersAsync(page, size);
        }
    }
}