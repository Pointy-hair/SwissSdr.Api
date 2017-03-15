using FluentValidation;
using FluentValidation.Attributes;
using Raven.Client;
using SwissSdr.Api.Resources;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
	public class ImagesUpdateInputModel
	{
		public IList<string> FileIds { get; set; }
	}

	public class ImagesUpdateInputModelValidator : AbstractValidator<ImagesUpdateInputModel>
	{
		public ImagesUpdateInputModelValidator()
		{
			RuleFor(x => x.FileIds)
				.Must(l => l.Count == l.Distinct().Count())
				.WithMessage("Can not add the same image twice.");

			RuleForEach(x => x.FileIds)
				.NotEmpty();
		}
	}
}
