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
    public class TopicsProfile : Profile
    {
		public TopicsProfile()
		{
			CreateMap<Topic, Resources.TopicResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedTopicSummary, Resources.TopicSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedEntitySummary, Resources.TopicSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<TopicUpdateInputModel, Topic>()
				.ForMember(r => r.Tags, opt => opt.ResolveUsing(x => x.Tags.Select(s => s.ToTitleCase())))
				.ForMember(r => r.ImageIds, opt => opt.Ignore())
				.ForMember(r => r.Id, opt => opt.Ignore())
				.ForMember(r => r.Permissions, opt => opt.Ignore())
				.ForMember(r => r.CreatedAt, opt => opt.Ignore())
				.ForMember(r => r.UpdatedAt, opt => opt.Ignore());
		}
	}
}
