using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.IO;
using System.Linq.Expressions;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;

namespace RevProgramarWin8.Helper
{
	internal static class LibHelper
	{
		public static async Task<string> GetHttpPageAsyncHelper(string url)
		{
			try
			{
				using (var xx = new HttpClient())
				{
					using (var strea = new StreamReader(await xx.GetStreamAsync(url), Encoding.GetEncoding("iso-8859-1")))
					{
						return strea.ReadToEnd();
					}
				}	
			}
			catch
			{
				return null;
			}
		}

		public static async Task<string> GetHttpPageAsyncHelper(Uri url)
		{
			try
			{
				using (var xx = new HttpClient())
				{
					using (var strea = new StreamReader(await xx.GetStreamAsync(url), Encoding.GetEncoding("iso-8859-1")))
					{
						return strea.ReadToEnd();
					}
				}
			}
			catch
			{
				return null;
			}
		}

	}
}
