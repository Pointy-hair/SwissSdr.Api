using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Infrastructure
{
	public class CommaDelimitedCollectionModelBinder : IModelBinder
	{
        public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var key = bindingContext.ModelName;
			var value = bindingContext.ValueProvider.GetValue(key);
			if (value != null)
			{
				var collectionValues = value.Values
					.Where(s => !string.IsNullOrEmpty(s))
					.SelectMany(s => s.Split(new[] { "," }, StringSplitOptions.None))
					.ToList();

				if (collectionValues.Any())
				{
					bindingContext.Result = ModelBindingResult.Success(collectionValues);
				}
			}

			return Task.CompletedTask;
		}
	}
}
