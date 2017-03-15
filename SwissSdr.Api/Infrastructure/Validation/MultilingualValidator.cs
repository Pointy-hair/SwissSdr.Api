using FluentValidation;
using FluentValidation.Validators;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
    public class MultilingualStringValidator : AbstractValidator<Multilingual<string>>
    {
        public MultilingualStringValidator()
        {
            RuleFor(x => x.Count).GreaterThan(0);
            RuleForEach(x => x).Must(x => !string.IsNullOrEmpty(x.LanguageCode) && !string.IsNullOrEmpty(x.Value));
        }
	}

	public static class MultilingualStringValidatorExtensions
	{
		public static IRuleBuilderOptions<T, Multilingual<string>> ValidateMultilingualString<T>(this IRuleBuilder<T, Multilingual<string>> ruleBuilder)
		{
			return ruleBuilder.SetValidator(new MultilingualStringValidator());
		}
	}
}
