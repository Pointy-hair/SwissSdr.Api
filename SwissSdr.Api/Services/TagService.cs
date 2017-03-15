using Raven.Client;
using Raven.Client.Linq;
using SwissSdr.Api.Indexes;
using SwissSdr.Datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api.Services
{
    public class TagService
    {
		private readonly IAsyncDocumentSession _session;

		public TagService(
			IAsyncDocumentSession session)
		{
			_session = session;
		}

		public async Task<IEnumerable<TagCluster>> GetClusters()
		{
			var query = _session.Query<TagCluster_Tag.Result, TagCluster_Tag>()
				.OrderBy(x => x.Tag)
				.As<TagCluster>();
			var stream = await _session.Advanced.StreamAsync(query);

			var tags = new Collection<TagCluster>();
			while (await stream.MoveNextAsync())
			{
				tags.Add(stream.Current.Document);
			}

			return tags;
		}

		public async Task<IEnumerable<TagCluster>> GetClusters(string search)
		{
			var query = _session.Query<TagCluster_Tag.Result, TagCluster_Tag>()
				.Where(r => r.Tag.StartsWith(search))
				.OrderBy(x => x.Tag)
				.As<TagCluster>();
			var stream = await _session.Advanced.StreamAsync(query);

			var tags = new Collection<TagCluster>();
			while (await stream.MoveNextAsync())
			{
				tags.Add(stream.Current.Document);
			}

			return tags;
		}

		public async Task<IEnumerable<TagCluster>> CreateOrUpdateClusters(IEnumerable<string> tags)
		{
			if (tags == null || !tags.Any())
			{
				return Enumerable.Empty<TagCluster>();
			}

			var clusters = await _session.Query<TagCluster_Tag.Result, TagCluster_Tag>()
				.Where(r => r.Tag.In(tags))
				.As<TagCluster>()
				.ToListAsync();

			foreach (var tag in tags)
			{
				if (!clusters.SelectMany(c => c.Tags).Select(m => m.Value).Contains(tag))
				{
					var cluster = new TagCluster()
					{
						Tags = new Multilingual<string>(new LocalizedValue<string>(tag))
					};

					await _session.StoreAsync(cluster);
					clusters.Add(cluster);
				}
			}

			await _session.SaveChangesAsync();
			return clusters;
		}

		public async Task<TagCluster> CreateCluster(string tag)
		{
			var cluster = await _session.Query<TagCluster_Tag.Result, TagCluster_Tag>()
				.Where(r => r.Tag == tag)
				.As<TagCluster>()
				.FirstOrDefaultAsync();

			if (cluster == null)
			{
				cluster = new TagCluster()
				{
					Tags = new Multilingual<string>(new LocalizedValue<string>(tag))
				};

				await _session.StoreAsync(cluster);
				await _session.SaveChangesAsync();
			}

			return cluster;
		}

	}
}
