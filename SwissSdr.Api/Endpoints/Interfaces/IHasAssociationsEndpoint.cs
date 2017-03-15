using Microsoft.AspNetCore.Mvc;
using SwissSdr.Api.InputModels;
using System.Threading.Tasks;

namespace SwissSdr.Api.Endpoints
{
	public interface IHasAssociationsEndpoint
	{
		IAssociationsEndpoint AssociationsEndpoint { get; }

		Task<IActionResult> GetAssociations(int id);
		Task<IActionResult> UpdateAssociations(int id, AssociationUpdateInputModel updateModel);
	}
}