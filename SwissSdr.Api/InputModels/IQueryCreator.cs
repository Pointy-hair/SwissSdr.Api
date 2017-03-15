using Raven.Client;
using Raven.Client.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public interface IQueryCreator<T>
	{
		IRavenQueryable<T> CreateQuery(IAsyncDocumentSession session);
	}

	public interface IAsyncQueryCreator<T>
	{
		Task<IRavenQueryable<T>> CreateQueryAsync(IAsyncDocumentSession session);
	}
}