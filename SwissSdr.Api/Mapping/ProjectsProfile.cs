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
    public class ProjectsProfile : Profile
    {
		public ProjectsProfile()
		{
			CreateMap<Project, Resources.ProjectResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedProjectSummary, Resources.ProjectSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedEntitySummary, Resources.ProjectSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e => e.Image?.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<ProjectUpdateInputModel, Datamodel.Project>()
				.ForMember(r => r.Tags, opt => opt.ResolveUsing(x => x.Tags.Select(s => s.ToTitleCase())))
				.ForMember(r => r.ImageIds, opt => opt.Ignore())
				.ForMember(r => r.Jobs, opt => opt.Ignore())
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
