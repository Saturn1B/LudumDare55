using Proyecto26;
using RSG;
using System.Threading.Tasks;

public static class RestExtensions
{
    public static Task<ResponseHelper> AsTask(this IPromise<ResponseHelper> promise)
	{
		var tcs = new TaskCompletionSource<ResponseHelper>();

		promise
			.Then(response => tcs.SetResult(response))
			.Catch(error => tcs.SetException(error));

		return tcs.Task;
	}
}
