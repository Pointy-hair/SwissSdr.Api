using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SwissSdr.Datamodel;
using SwissSdr.Api.QueryModels;
using SwissSdr.Shared;
using SwissSdr.Api.InputModels;

namespace SwissSdr.Api.Mapping
{
	public class UsersProfile : Profile
	{
		public UsersProfile()
		{
			CreateMap<User, Resources.UserResource>()
				.ForMember(r => r.Logins, opt => opt.Ignore());

			CreateMap<User, Resources.UserSummaryResource>()
				.ForMember(r => r.ThumbnailUrl, opt => opt.ResolveUsing(e =>
				{
					if (string.IsNullOrEmpty(e.ProfileImageId) || string.IsNullOrEmpty(e.ProfileImageUrl))
					{
						return null;
					}
					else
					{
						return new DenormalizedFileSummary()
						{
							Id = e.ProfileImageId,
							Url = e.ProfileImageUrl,
							Name = new Multilingual<string>(new LocalizedValue<string>("avatar"))
						}.GetImageUrl(ImageSize.Thumbnail);
					}
				}));

			CreateMap<UserUpdateInputModel, User>()
				.ForMember(r => r.Id, opt => opt.Ignore())
				.ForMember(r => r.ProfileImageUrl, opt => opt.Ignore())
				.ForMember(r => r.ExternalClaims, opt => opt.Ignore())
				.ForMember(r => r.Logins, opt => opt.Ignore());
		}
	}
}
