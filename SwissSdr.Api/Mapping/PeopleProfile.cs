using AutoMapper;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Mapping
{
    public class PeopleProfile : Profile
    {
		public PeopleProfile()
		{
			CreateMap<Person, Resources.PersonResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedPersonSummary, Resources.PersonSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.ProfileImage?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedEntitySummary, Resources.PersonSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<PersonUpdateInputModel, Person>()
				.ForMember(r => r.InterestAreas, opt => opt.ResolveUsing(x => x.InterestAreas.Select(s => s.ToTitleCase())))
				.ForMember(r => r.Library, opt => opt.Ignore())
				.ForMember(r => r.Publications, opt => opt.Ignore())
				.ForMember(r => r.Associations, opt => opt.Ignore())
				.ForMember(r => r.Id, opt => opt.Ignore())
				.ForMember(r => r.Permissions, opt => opt.Ignore())
				.ForMember(r => r.CreatedAt, opt => opt.Ignore())
				.ForMember(r => r.UpdatedAt, opt => opt.Ignore());
		}
	}
}
