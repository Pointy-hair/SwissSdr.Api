using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client;
using SwissSdr.Api.InputModels;
using SwissSdr.Api.Services;
using Raven.Client.Indexes;
using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using SwissSdr.Api.Resources;
using AutoMapper;

namespace SwissSdr.Api.Endpoints
{
	public class JobsEndpoint<TController> : IJobsEndpoint
		where TController : ControllerBase, IHasJobsEndpoint
	{
		private readonly IAuthorizationService _authService;
		private readonly IMapper _mapper;
		private readonly IAsyncDocumentSession _session;

		public JobsEndpoint(IAsyncDocumentSession session,
			IMapper mapper,
			IAuthorizationService authService)
		{
			_session = session;
			_mapper = mapper;
			_authService = authService;
		}

		public async Task<IActionResult> GetJobs<T>(TController controller, int id)
			where T : IHasJobs
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);
			var resource = CreateJobsResource(model);

			var representation = resource
				.CreateRepresentation(controller, _ => _.GetJobs(id));

			return controller.Ok(representation);
		}

		public async Task<IActionResult> UpdateJobs<T>(TController controller, int id, JobsUpdateInputModel updateModel)
			where T : EntityBase, IHasJobs
		{
			var model = await _session.LoadAsyncAndThrowIfNull<T>(id);

			if (await _authService.AuthorizeEditAsync(controller.User, model))
			{
				model.Jobs = _mapper.Map<ICollection<JobAdvertisement>>(updateModel.Items);
				await _session.SaveChangesAsync();

				return await controller.GetJobs(id);
			}

			return controller.Forbid();
		}

		public ItemsResource<JobResourceItem> CreateJobsResource(IHasJobs hasJobs)
		{
			return new ItemsResource<JobResourceItem>()
			{
				Items = hasJobs.Jobs.Select(j => _mapper.Map<JobResourceItem>(j))
			};
		}
	}
}
