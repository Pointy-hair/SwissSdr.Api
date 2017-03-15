using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
	public interface IHasLibraryEndpoint
	{
		ILibraryEndpoint LibraryEndpoint { get; }

		Task<IActionResult> GetLibrary(int id);
		Task<IActionResult> UpdateLibrary(int id, LibraryUpdateInputModel updateModel);
	}
}