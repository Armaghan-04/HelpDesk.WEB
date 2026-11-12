using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.ViewModels;
using Riok.Mapperly.Abstractions;

namespace HelpDesk.WEB.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class TicketMapper
    {
        [MapProperty(nameof(Ticket.CreatedByUser) + "." + nameof(AppUser.FullName), nameof(TicketListItemDto.CreatedByName))]
        [MapProperty(nameof(Ticket.AssignedTo) + "." + nameof(AppUser.FullName), nameof(TicketListItemDto.AssignedToName))]
        public partial TicketListItemDto ToListItem(Ticket source);

        public partial IQueryable<TicketListItemDto> ProjectList(IQueryable<Ticket> source);

        [MapProperty(nameof(Ticket.CreatedByUser) + "." + nameof(AppUser.FullName), nameof(TicketDetailsDto.CreatedByName))]
        [MapProperty(nameof(Ticket.AssignedTo) + "." + nameof(AppUser.FullName), nameof(TicketDetailsDto.AssignedToName))]
        [MapperIgnoreTarget(nameof(TicketDetailsDto.Comments))]
        [MapperIgnoreTarget(nameof(TicketDetailsDto.History))]
        public partial TicketDetailsDto ToDetails(Ticket source);

        public partial IQueryable<TicketDetailsDto> ProjectDetails(IQueryable<Ticket> source);

        [MapProperty(nameof(TicketComment.Author) + "." + nameof(AppUser.FullName), nameof(TicketCommentDto.AuthorName))]
        public partial TicketCommentDto ToComment(TicketComment source);

        public partial IQueryable<TicketCommentDto> ProjectComments(IQueryable<TicketComment> source);

        [MapProperty(nameof(TicketHistory.ChangedBy) + "." + nameof(AppUser.FullName), nameof(TicketHistoryDto.ChangedByName))]
        public partial TicketHistoryDto ToHistory(TicketHistory source);

        public partial IQueryable<TicketHistoryDto> ProjectHistory(IQueryable<TicketHistory> source);

        [MapperIgnoreTarget(nameof(TicketFormViewModel.Departments))]
        [MapperIgnoreTarget(nameof(TicketFormViewModel.IsEdit))]
        public partial TicketFormViewModel ToForm(TicketDetailsDto source);

        public partial CreateTicketRequest ToCreateRequest(TicketFormViewModel source);

        public partial UpdateTicketRequest ToUpdateRequest(TicketFormViewModel source);
    }
}
