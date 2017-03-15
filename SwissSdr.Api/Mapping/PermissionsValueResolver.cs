using AutoMapper;
using SwissSdr.Api.QueryModels;
using SwissSdr.Api.Services;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SwissSdr.Api.Mapping
{
	public class PermissionsValueResolver : IMemberValueResolver<object, object, IDictionary<string, IEnumerable<ObjectPermission>>, IEnumerable<ObjectPermission>>
	{
		private readonly PermissionService _permissionService;

		public PermissionsValueResolver(PermissionService permissionService)
		{
			_permissionService = permissionService;
		}

		public IEnumerable<ObjectPermission> Resolve(object source, object destination, IDictionary<string, IEnumerable<ObjectPermission>> sourceMember, IEnumerable<ObjectPermission> destMember, ResolutionContext context)
		{
			return _permissionService.GetPermissionsForCurrentUser(sourceMember);
		}
	}

	public static class PermissionsValueResolverExtensions
	{
		public static void UsePermissionsResolver<T>(this IMemberConfigurationExpression<T, object, IEnumerable<ObjectPermission>> config, Expression<Func<T, IDictionary<string, IEnumerable<ObjectPermission>>>> sourceMember) where T : class
		{
			config.ResolveUsing<PermissionsValueResolver, IDictionary<string, IEnumerable<ObjectPermission>>>(sourceMember);
		}
	}
}
