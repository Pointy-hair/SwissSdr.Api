using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class OrganisationUpdateInputModel
    {
        public Multilingual<string> Name { get; set; }
        public Multilingual<string> Description { get; set; }
        public Multilingual<string> HierarchyInformation { get; set; }
        public string RootOrganisationId { get; set; }
        public OrganisationType Type { get; set; }
        public ICollection<char> IsicClassification { get; set; }
		public ICollection<SnfDiscipline> Disciplines { get; set; }
		public string ProfileImageId { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public Multilingual<Richtext> Profile { get; set; }
        public ICollection<string> Tags { get; set; }
    }

    public class OrganisationUpdateInputModelValidator : AbstractValidator<OrganisationUpdateInputModel>
    {
		public OrganisationUpdateInputModelValidator()
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
