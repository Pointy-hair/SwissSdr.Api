using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Resources
{
    public class ItemsResource<T> : IResource
    {
		public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}
