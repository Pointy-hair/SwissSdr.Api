using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel;
using SwissSdr.Shared;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Mapping
{
    public class EventsProfile : Profile
    {
		public EventsProfile()
		{
			CreateMap<Event, Resources.EventResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedEventSummary, Resources.EventSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedEntitySummary, Resources.EventSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<EventUpdateInputModel, Datamodel.Event>()
				.ForMember(t => t.Tags, opt => opt.ResolveUsing(x => x.Tags.Select(s => s.ToTitleCase())))
				.ForMember(t => t.Library, opt => opt.Ignore())
				.ForMember(t => t.ImageIds, opt => opt.Ignore())
				.ForMember(t => t.Associations, opt => opt.Ignore())
				.ForMember(t => t.Id, opt => opt.Ignore())
				.ForMember(t => t.Permissions, opt => opt.Ignore())
				.ForMember(r => r.CreatedAt, opt => opt.Ignore())
				.ForMember(r => r.UpdatedAt, opt => opt.Ignore());

			// sessions
			CreateMap<EventSession, Resources.EventSessionResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<EventSessionUpdateInputModel, Datamodel.EventSession>()
				.ForMember(s => s.Id, opt => opt.Ignore())
				.ForMember(s => s.Permissions, opt => opt.Ignore())
				.ForMember(r => r.CreatedAt, opt => opt.Ignore())
				.ForMember(r => r.UpdatedAt, opt => opt.Ignore());
		}
	}
}
