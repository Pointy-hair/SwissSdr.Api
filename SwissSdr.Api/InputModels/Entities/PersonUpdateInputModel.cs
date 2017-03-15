using FluentValidation;
using FluentValidation.Attributes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class PersonUpdateInputModel
    {
        public string Salutation { get; set; }
        public string Title { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ProfileImageId { get; set; }
        public Multilingual<Richtext> Profile { get; set; }

        public IList<string> Languages { get; set; }
        public ICollection<string> InterestAreas { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }

    public class PersonUpdateInputModelValidator : AbstractValidator<PersonUpdateInputModel>
    {
        public PersonUpdateInputModelValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty();
            RuleFor(x => x.Lastname)
                .NotEmpty();
        }
    }
}
