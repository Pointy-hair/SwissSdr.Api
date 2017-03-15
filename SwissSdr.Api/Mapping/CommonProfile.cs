using Autofac;
using AutoMapper;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SwissSdr.Shared;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.QueryModels;

namespace SwissSdr.Api.Mapping
{
	public class CommonProfile : Profile
	{
		public CommonProfile()
		{
			CreateMap<TagCluster, Resources.TagClusterResourceItem>();
			CreateMap<Datamodel.Settings.AppSettings, Resources.AppSettingsResource>()
				.ForMember(r => r.EntityTemplates, opt => opt.Ignore());

			CreateMap<EntityStub, QueryModels.DenormalizedStub>()
				.ForMember(t => t.Id, opt => opt.Ignore())
				.ForMember(t => t.Permissions, opt => opt.Ignore());

			CreateMap<DenormalizedEntitySummary, Resources.SearchResultResource>()
				.ForMember(r => r.Type, opt => opt.MapFrom(x => x.GetEntityType()))
				.ForMember(r => r.DisplayName, opt => opt.MapFrom(x => x.GetDisplayName()))
				.ForMember(r => r.DisplayImageUrl, opt => opt.MapFrom(x => x.GetDisplayImageUrl()));

			CreateJobMappings();
			CreateFileMappings();
		}

		private void CreateJobMappings()
		{
			CreateMap<JobAdvertisement, Resources.JobResourceItem>();

			CreateMap<JobsUpdateInputModel.Item, Datamodel.JobAdvertisement>();
		}

		private void CreateFileMappings()
		{
			CreateMap<File, Resources.FileResource>()
				.ForMember(r => r.Urls, opt => opt.ResolveUsing(x => new Dictionary<ImageSize, string>()
				{
					{ ImageSize.Thumbnail, x.GetImageUrl(ImageSize.Thumbnail) },
					{ ImageSize.Large, x.GetImageUrl(ImageSize.Large) },
					{ ImageSize.Original, x.GetImageUrl(ImageSize.Original) },
				}))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<DenormalizedFileSummary, Resources.FileSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(x => x.GetImageUrl(ImageSize.Thumbnail)))
				.ForMember(r => r.Urls, opt => opt.ResolveUsing(x => new Dictionary<ImageSize, string>()
				{
					{ ImageSize.Thumbnail, x.GetImageUrl(ImageSize.Thumbnail) },
					{ ImageSize.Large, x.GetImageUrl(ImageSize.Large) },
					{ ImageSize.Original, x.GetImageUrl(ImageSize.Original) },
				}))
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<FileUpload, Resources.FileUploadResource>()
				.ForMember(r => r.Permissions, opt => opt.UsePermissionsResolver(x => x.Permissions));

			CreateMap<FileUploadUpdateInputModel, Datamodel.FileUpload>()
				.ForMember(r => r.Name, opt => opt.MapFrom(s => s.Name))
				.ForMember(r => r.Description, opt => opt.MapFrom(s => s.Description))
				.ForAllOtherMembers(opt => opt.Ignore());

			CreateMap<FileUploadUpdateInputModel, Datamodel.File>()
				.ForMember(r => r.Name, opt => opt.MapFrom(s => s.Name))
				.ForMember(r => r.Description, opt => opt.MapFrom(s => s.Description))
				.ForAllOtherMembers(opt => opt.Ignore());

			CreateMap<FileUpdateInputModel, Datamodel.File>()
				.ForMember(r => r.Name, opt => opt.MapFrom(s => s.Name))
				.ForMember(r => r.Description, opt => opt.MapFrom(s => s.Description))
				.ForAllOtherMembers(opt => opt.Ignore());
		}
	}
}
