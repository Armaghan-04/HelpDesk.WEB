using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.ViewModels;
using Riok.Mapperly.Abstractions;

namespace HelpDesk.WEB.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class TeamMapper
    {
        [MapperIgnoreTarget(nameof(TeamMemberDto.Roles))]
        [MapperIgnoreTarget(nameof(TeamMemberDto.PrimaryRole))]
        public partial TeamMemberDto ToMember(AppUser source);

        public partial IQueryable<TeamMemberDto> ProjectMembers(IQueryable<AppUser> source);

        public partial IQueryable<AssignableUserDto> ProjectAssignableUsers(IQueryable<AppUser> source);
    }
}
