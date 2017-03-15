using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public enum SortOptions
	{
		Default,
		Name,
		Random,
		Created,
		Updated
	}
}
