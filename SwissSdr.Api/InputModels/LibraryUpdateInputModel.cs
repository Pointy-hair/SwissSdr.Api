using FluentValidation;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Http;
using Raven.Client;
using SwissSdr.Api.Resources;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SwissSdr.Api.InputModels
{
	public class LibraryUpdateInputModel
	{
		public IList<Item> Items { get; set; }

		public IList<LibraryItem> CreateLibraryItems()
		{
			return Items.Select(i => i.CreateLibraryItem()).ToList();
		}

		public class Item
		{
			public LibraryResourceItemType Type { get; set; }

			public Multilingual<string> Name { get; set; }
			public Multilingual<string> Description { get; set; }
			public Multilingual<string> Reference { get; set; }
			public string Url { get; set; }
			public string FileId { get; set; }

			public LibraryItem CreateLibraryItem()
			{
				switch (Type)
				{
					case LibraryResourceItemType.File:
						return new LibraryItem()
						{
							FileId = FileId
						};

					case LibraryResourceItemType.Url:
						return new LibraryItem()
						{
							Name = Name,
							Description = Description,
							Url = Url,
							Reference = Reference
						};

					default:
						throw new InvalidOperationException($"Can not create a library item with type '{Type}'.");
				}
			}
		}
	}

	public class LibraryUpdateInputModelValidator : AbstractValidator<LibraryUpdateInputModel>
	{
		public LibraryUpdateInputModelValidator(IHttpContextAccessor contextAccessor)
		{
			RuleForEach(x => x.Items)
				.NotNull();

			RuleFor(x => x.Items)
				.Cascade(CascadeMode.StopOnFirstFailure)
				.SetCollectionValidator(new LibraryUpdateInputModelItemValidator())
				.MustAsync(async (item, items, cancellationToken) =>
				{
					var session = contextAccessor.HttpContext.RequestServices.GetService<IAsyncDocumentSession>();
					var files = await session.LoadAsync<File>(items.Where(i => i.Type == LibraryResourceItemType.File).Select(i => i.FileId));
					var filesNotFound = files.Zip(items, (f, i) => new { Id = i.FileId, Exists = f != null }).Where(x => !x.Exists);
					return !filesNotFound.Any();
				})
				.WithMessage($"Could not find all referenced files.");
		}
	}

	public class LibraryUpdateInputModelItemValidator : AbstractValidator<LibraryUpdateInputModel.Item>
	{
		public LibraryUpdateInputModelItemValidator()
		{
			When(x => x.Type == LibraryResourceItemType.Url, () =>
			{
				RuleFor(x => x.Name)
					.NotNull()
					.ValidateMultilingualString()
					.WithMessage("Items of type 'Url' must specify 'Name' in at least one language.");
				RuleFor(x => x.Url)
					.NotEmpty()
					.WithMessage("Items of type 'Url' must specify 'Url'.");
				RuleFor(x => x.FileId)
					.Null()
					.WithMessage("Items of type 'Url' must not contain 'FileId'.");
			});

			When(x => x.Type == LibraryResourceItemType.File, () =>
			{
				RuleFor(x => x.FileId)
					.NotEmpty()
					.WithMessage("Items of type 'File' must specify 'FileId'.");
			});
		}
	}
}
