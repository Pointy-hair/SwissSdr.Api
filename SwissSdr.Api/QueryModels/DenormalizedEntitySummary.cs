using SwissSdr.Datamodel;
using SwissSdr.Datamodel.Authorization;
using SwissSdr.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.QueryModels
{
	public class DenormalizedEntitySummary
	{
		public string EntityName { get; set; }

		public string Id { get; set; }
		public IDictionary<string, IEnumerable<ObjectPermission>> Permissions { get; set; }

		public Multilingual<string> Name { get; set; }
		public Multilingual<string> Description { get; set; }
		public DenormalizedFileSummary Image { get; set; }

		public string Title { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public ICollection<string> InterestAreas { get; set; }
		public IEnumerable<Multilingual<string>> AssociatedOrganisationNames { get; set; }

		public DateTime? Begin { get; set; }
		public DateTime? End { get; set; }

		public ContactInfo ContactInfo { get; set; }
		public GeoCoordinate? Coordinates => ContactInfo?.Addresses.FirstOrDefault()?.Coordinates;

		public string GetDisplayName()
		{
			var entityType = this.GetEntityType();
			switch (entityType)
			{
				case EntityType.Person:
					return $"{Firstname} {Lastname}";
				default:
					return Name.ValueByBestMatch();
			}
		}

		public string GetDisplayImageUrl()
		{
			return Image?.GetImageUrl(ImageSize.Thumbnail);
		}

		public Type GetTypedSummaryType()
		{
			var entityType = this.GetEntityType();
			switch (entityType)
			{
				case EntityType.Topic:
					return typeof(DenormalizedTopicSummary);
				case EntityType.Project:
					return typeof(DenormalizedProjectSummary);
				case EntityType.Person:
					return typeof(DenormalizedPersonSummary);
				case EntityType.Organisation:
					return typeof(DenormalizedOrganisationSummary);
				case EntityType.Event:
					return typeof(DenormalizedEventSummary);
				case EntityType.File:
					return typeof(DenormalizedFileSummary);
				default:
					throw new InvalidOperationException($"Could not determine summary type for entity type '{entityType}'.");
			}
		}
	}
}
