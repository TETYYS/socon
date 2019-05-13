using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.WMI
{
	static class Utils
	{
		private static bool PropertyExists(string Property, ManagementBaseObject Obj)
		{
			foreach (var prop in Obj.Properties) {
				if (prop.Name == Property)
					return true;
			}

			return false;
		}

		public static async Task<List<T>> MapWMIToStructArray<T>(string Query, CancellationToken Cancel) where T : new()
		{
			var searcher = new ManagementObjectSearcher(Query);
			var items = new List<T>();
			var typ = typeof(T);
			TaskCompletionSource<List<T>> tcs = new TaskCompletionSource<List<T>>();
			var results = new ManagementOperationObserver();

			results.ObjectReady += (sender, obj) => {
				var item = new T();
				foreach (var field in typ.GetFields()) {
					if (PropertyExists(field.Name, obj.NewObject))
						field.SetValue(item, obj.NewObject[field.Name]);
				}
				items.Add(item);
			};
			results.Completed += (sender, obj) => {
				tcs.SetResult(items);
			};
			searcher.Get(results);

			return await Task.Run(() => tcs.Task, Cancel);
		}

	}
}
