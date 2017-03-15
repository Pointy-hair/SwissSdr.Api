using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System.Collections.Generic;

namespace SwissSdr.Api.InputModels
{
	public class JobsUpdateInputModel
	{
		public IList<Item> Items { get; set; }

		public class Item
		{
			public Multilingual<string> Name { get; set; }
			public Multilingual<Richtext> Content { get; set; }
			public string Function { get; set; }
			public string Url { get; set; }
		}
	}

	public class JobsUpdateInputModelValidator : AbstractValidator<JobsUpdateInputModel>
	{
		public JobsUpdateInputModelValidator()
		{
			RuleForEach(x => x.Items)
				.NotNull();

			RuleFor(x => x.Items)
				.SetCollectionValidator(new JobsUpdateInputModelItemValidator());
		}
	}

	public class JobsUpdateInputModelItemValidator : AbstractValidator<JobsUpdateInputModel.Item>
	{
		public JobsUpdateInputModelItemValidator()
		{
			RuleFor(x => x.Name)
				.NotNull()
				.ValidateMultilingualString();

			RuleFor(x => x.Function)
				.NotEmpty();
		}
	}
}
