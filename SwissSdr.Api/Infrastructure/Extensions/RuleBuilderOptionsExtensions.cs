using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	// from http://stackoverflow.com/a/37802508
	public static class RuleBuilderOptionsExtensions
	{
		public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> builder, Func<T, object> func)
			=> builder.WithMessage("{0}", func);
		public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> builder, Func<T, TProperty, object> func)
			=> builder.WithMessage("{0}", func);
	}
}
