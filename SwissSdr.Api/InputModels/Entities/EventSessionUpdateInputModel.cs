using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
	public class EventSessionUpdateInputModel
	{
		public Multilingual<string> Name { get; set; }
		public Multilingual<Richtext> Content { get; set; }

		public string Venue { get; set; }

		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
	}

	public class EventSessionUpdateInputModelValidator : AbstractValidator<EventSessionUpdateInputModel>
	{
		public EventSessionUpdateInputModelValidator()
		{
			RuleFor(x => x.Name)
				.NotNull()
				.ValidateMultilingualString();
		}
	}
}
