using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using SwissSdr.Datamodel;
using Raven.Abstractions.Indexing;
using SwissSdr.Datamodel.Settings;

namespace SwissSdr.Api.Indexes
{
	public class Users_Filter : AbstractIndexCreationTask<User>
	{
		public class Result
		{
			public IEnumerable<string> Query { get; set; }

			// sorting
			public string Id { get; set; }
			public string Fullname { get; set; }
		}

		public Users_Filter()
		{
			Map = users => from user in users
							select new Result
							{
								Query = new[] { user.Fullname, user.EMail },
								Id = user.Id,
								Fullname = user.Fullname
							};

			Index(nameof(Result.Query), FieldIndexing.Analyzed);
			Analyze(nameof(Result.Query), "StandardAnalyzer");

			Sort(nameof(Result.Id), Raven.Abstractions.Indexing.SortOptions.Int);
		}
	}
}
