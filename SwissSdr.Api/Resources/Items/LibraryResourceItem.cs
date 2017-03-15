using SwissSdr.Api.QueryModels;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
	public class LibraryResourceItem
	{
		public LibraryResourceItemType Type { get; set; }
		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public Multilingual<string> Reference { get; set; }
		public string Url { get; set; }
		public string FileId { get; set; }

		public static LibraryResourceItem Create(LibraryItem item, IEnumerable<DenormalizedFileSummary> files)
		{
			if (item.GetItemType() == LibraryResourceItemType.File)
			{
				var file = files.SingleOrDefault(f => f.Id == item.FileId);
				return new LibraryResourceItem()
				{
					Type = LibraryResourceItemType.File,
					FileId = item.FileId,
					Name = file?.Name,
					Description = file?.Description,
					Url = file?.Url,
					Reference = item.Reference
				};
			}
			else
			{
				return new LibraryResourceItem()
				{
					Type = LibraryResourceItemType.Url,
					FileId = item.FileId,
					Name = item.Name,
					Description = item.Description,
					Url = item.Url,
					Reference = item.Reference
				};
			}
		}
	}

	public enum LibraryResourceItemType
	{
		File,
		Url
	}

	public static class LibraryItemTypeExtensions
	{
		public static LibraryResourceItemType GetItemType(this LibraryItem libraryItem)
		{
			if (string.IsNullOrEmpty(libraryItem.FileId))
			{
				return LibraryResourceItemType.Url;
			}
			else
			{
				return LibraryResourceItemType.File;
			}
		}
	}
}
