using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class FileUpdateInputModel
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
	}

	public class FileUpdateInputModelValidator : AbstractValidator<FileUpdateInputModel>
	{
		public FileUpdateInputModelValidator()
		{
			RuleFor(x => x.Name)
				.NotNull()
				.ValidateMultilingualString();
			RuleFor(x => x.Description)
				.NotNull()
				.ValidateMultilingualString();
		}
	}
}
