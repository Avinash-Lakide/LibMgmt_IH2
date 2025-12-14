using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly ILogger<MembersController> _log;

        public MembersController(IMemberService memberService, ILogger<MembersController> logger)
        {
            _memberService = memberService;
            _log = logger;
        }

        // List all members with paging
        [HttpGet]
        public async Task<IActionResult> GetMembers(int page = 1, int size = 10)
        {
            if (page <= 0)
                return BadRequest("Page must be greater than 0.");
            if (size <= 0 || size > 100)
                return BadRequest("Size must be between 1 and 100.");

            try
            {
                var (members, total) = await _memberService.GetMembersAsync(page, size);
                _log.LogInformation($"Fetched {members.Count()} members for page {page}");
                return Ok(new { members, total, page, size });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to get members");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Get one member by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(Guid id)
        {
            try
            {
                var member = await _memberService.GetByIdAsync(id);
                if (member == null)
                    return NotFound("Member not found");
                _log.LogInformation($"Retrieved member {id}");
                return Ok(member);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error getting member {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Add a new member
        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] Member member)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if email is unique
                if (!await _memberService.IsEmailUniqueAsync(member.EmailId))
                {
                    return BadRequest("Email already exists");
                }

                member.MemberId = Guid.NewGuid();
                member.JoinedDate = DateTime.UtcNow;
                var createdMember = await _memberService.CreateAsync(member);
                _log.LogInformation($"Member created with ID {createdMember.MemberId}");
                return CreatedAtAction(nameof(GetMember), new { id = createdMember.MemberId }, createdMember);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to create member");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Edit a member
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, [FromBody] Member member)
        {
            if (id != member.MemberId)
                return BadRequest("ID mismatch");
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if email is unique excluding current member
                if (!await _memberService.IsEmailUniqueAsync(member.EmailId, id))
                {
                    return BadRequest("Email already exists");
                }

                var existingMember = await _memberService.GetByIdAsync(id);
                if (existingMember == null)
                    return NotFound("Member not found");

                existingMember.FullName = member.FullName;
                existingMember.EmailId = member.EmailId;

                await _memberService.UpdateAsync(existingMember);
                _log.LogInformation($"Updated member {id}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _memberService.GetByIdAsync(id) == null)
                    return NotFound("Member not found");
                else
                    return Conflict("Member was changed by someone else");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error updating member {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Delete a member
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            try
            {
                var member = await _memberService.GetByIdAsync(id);
                if (member == null)
                    return NotFound("Member not found");
                await _memberService.DeleteAsync(id);
                _log.LogInformation($"Deleted member {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error deleting member {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        private async Task<bool> MemberExists(Guid id) => await _memberService.GetByIdAsync(id) != null;
    }
}