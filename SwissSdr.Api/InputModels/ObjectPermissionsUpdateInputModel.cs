﻿using SwissSdr.Datamodel.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.InputModels
{
    public class ObjectPermissionsUpdateInputModel
	{
		public IEnumerable<Item> Items { get; set; } = Enumerable.Empty<Item>();

		public class Item
		{
			public string UserId { get; set; }
			public IEnumerable<ObjectPermission> Permissions { get; set; } = Enumerable.Empty<ObjectPermission>();
		}
	}
}
