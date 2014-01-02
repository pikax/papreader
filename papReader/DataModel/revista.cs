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
				var xx = new HttpClient();
				
				
				string tex;

				using (var strea = new StreamReader(await xx.GetStreamAsync(url), Encoding.GetEncoding("iso-8859-1")))
				{
					tex = strea.ReadToEnd();
				}
				return tex;
				
			}
			catch
			{
				return null;
			}
		}

		static string GetConvertedString(Encoding from, Encoding to, string str)
		{
			string unicodeString = str;
			// Create two different encodings.
			Encoding ascii = from;
			Encoding unicode = to;

			// Convert the string into a byte[].
			byte[] unicodeBytes = unicode.GetBytes(unicodeString);

			// Perform the conversion from one encoding to the other.
			byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

			// Convert the new byte[] into a char[] and then into a string.
			// This is a slightly different approach to converting to illustrate
			// the use of GetCharCount/GetChars.
			char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
			ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
			string asciiString = new string(asciiChars);

			//// Display the strings created before and after the conversion.
			//Console.WriteLine("Original string: {0}", unicodeString);
			//Console.WriteLine("Ascii converted string: {0}", asciiString);

			return asciiString;
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
