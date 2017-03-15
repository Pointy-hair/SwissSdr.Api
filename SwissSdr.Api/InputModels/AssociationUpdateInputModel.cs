using FluentValidation;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Http;
using Raven.Client;
using SwissSdr.Api.Resources;
using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SwissSdr.Api.InputModels
{
    public class AssociationUpdateInputModel
	{
		public IList<Item> Items { get; set; }

		public class Item
		{
			public AssociationResourceItemType AssociationType { get; set; }
			public string AssociationDescription { get; set; }

			public EntityType SourceType { get; set; }
			public EntityType TargetType { get; set; }

			// entity ref
			public string TargetId { get; set; }

			// stub ref
			public Multilingual<string> Name { get; set; }
			public Multilingual<string> Description { get; set; }
			public string Url { get; set; }
		}
	}

	public class AssociationUpdateInputModelValidator : AbstractValidator<AssociationUpdateInputModel>
	{
		public AssociationUpdateInputModelValidator(IHttpContextAccessor contextAccessor)
		{
			RuleForEach(x => x.Items)
				.NotNull();

			RuleFor(x => x.Items)
				.SetCollectionValidator(new AssociationUpdateInputModelItemValidator(contextAccessor));
		}
	}

	public class AssociationUpdateInputModelItemValidator : AbstractValidator<AssociationUpdateInputModel.Item>
	{
		public AssociationUpdateInputModelItemValidator(IHttpContextAccessor contextAccessor)
		{
			RuleFor(x => x.AssociationDescription)
				.NotEmpty()
				.MustAsync(async (item, value, cancellationToken) =>
				{
					var session = contextAccessor.HttpContext.RequestServices.GetService<IAsyncDocumentSession>();
					var settings = await session.LoadAsync<AppSettings>(AppSettings.AppSettingsId);
					var definition = settings.AssociationDescriptionDefinitions.SingleOrDefault(d => d.Name == value);
					if (definition == null)
					{
						return false;
					}

					return definition.AllowedEntityAssociations.Any(p => p.Source == item.SourceType && p.Target == item.TargetType);
				})
				.WithMessage(x => $"No association with description '{x.AssociationDescription}' found that allows linking '{x.SourceType}' with '{x.TargetType}'.");

			When(x => x.AssociationType == AssociationResourceItemType.Entity, () =>
			{
				RuleFor(x => x.TargetId)
					.NotEmpty();
			});

			When(x => x.AssociationType == AssociationResourceItemType.Stub, () =>
			{
				RuleFor(x => x.Name)
					.ValidateMultilingualString();

				RuleFor(x => x.Description)
					.ValidateMultilingualString();
			});
		}
	}
}
