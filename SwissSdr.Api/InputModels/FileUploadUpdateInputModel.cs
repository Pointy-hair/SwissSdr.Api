using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
	public class FileUploadUpdateInputModel
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
	}

	public class FileUploadUpdateInputModelValidator : AbstractValidator<FileUploadUpdateInputModel>
	{
		public FileUploadUpdateInputModelValidator()
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
