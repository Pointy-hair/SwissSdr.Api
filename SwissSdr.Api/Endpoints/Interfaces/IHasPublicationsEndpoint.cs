using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
	public interface IHasPublicationsEndpoint
	{
		IPublicationsEndpoint PublicationsEndpoint { get; }

		Task<IActionResult> GetPublications(int id);
		Task<IActionResult> UpdatePublications(int id, LibraryUpdateInputModel updateModel);
	}
}