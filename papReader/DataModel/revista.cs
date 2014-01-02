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

namespace papReader.DataModel
{
	public sealed class revista
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public string ImgUrl { get; set; }
		public string ID { get; set; }


		const string PAPSITE = "http://revista-programar.info/";

		static async Task<string> GetHttpPageAsyncHelper(string url)
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

		public static async Task<List<revista>> GetRevistas()
		{
			const string GETOPERATION = "?action=editions";
			const string REGEX = @"(?:<a href="")(?<url>[^""]*)(?:""><img border=""0"" style=""padding: 10px; margin: 10px;"" alt="")(?<nome>[^""]*)(?:"" src="")(?<img>[^""]*)";

			var str = await GetHttpPageAsyncHelper(string.Format("{0}{1}", PAPSITE, GETOPERATION));

			List<revista> list = new List<revista>();

			foreach (Match item in Regex.Matches(str, REGEX, RegexOptions.Multiline))
			{
				var grp = item.Groups;
				list.Add(new revista()
							{
								Url = grp["url"].Value
								,
								ImgUrl = grp["img"].Value
								,
								Name = grp["nome"].Value
								,
								ID = grp["url"].Value.Split('=').LastOrDefault()//um pouco errado, mas funciona :)
							}
					);
			}
			return list;
		}

		public static async Task<StorageFile> GetPDF(string id)
		{
			try
			{
				return await ApplicationData.Current.LocalFolder.GetFileAsync(string.Format("{0}.pdf", id));
			}
			catch
			{
				return null;
			}
		}

		public static async Task<DownloadOperation> GetDownloadFile(string id)
		{
			const string FRM = @"http://www.portugal-a-programar.pt/revista-programar/edicoes/download.php?t=site&e={0}";
			BackgroundDownloader bg = new BackgroundDownloader();

			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(string.Format("{0}.pdf", id), CreationCollisionOption.ReplaceExisting);
			return bg.CreateDownload(new Uri(string.Format(FRM, id)), file);
		}
	}
}
