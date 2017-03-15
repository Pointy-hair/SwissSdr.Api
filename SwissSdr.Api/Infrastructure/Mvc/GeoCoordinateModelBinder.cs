using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SwissSdr.Datamodel;

namespace SwissSdr.Api.Infrastructure
{
	public class GeoCoordinateModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var key = bindingContext.ModelName;
			var value = bindingContext.ValueProvider.GetValue(key);
			if (value != null)
			{
				GeoCoordinate coordinates;
				if (GeoCoordinate.TryParse(value.FirstValue, out coordinates))
				{
					bindingContext.Result = ModelBindingResult.Success(coordinates);
				}
			}

			return Task.CompletedTask;
		}
	}
}