using PaPReaderLib.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PaPReaderLib
{
	public class RevistaController : INotifyPropertyChanged
	{
		public static readonly RevistaController Instance = new RevistaController();
		private const string SITE = "http://revista-programar.info/";
		private Uri _GetRevistaURL = new Uri(string.Format("{0}{1}", SITE, "?action=editions"), UriKind.Absolute);
		private List<Revista> _lista;
		private Uri _site = new Uri(SITE, UriKind.Absolute);


		private RevistaController()
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public static Uri PaPSite { get { return Instance._site; } }

		public static Uri RevistaUrl { get { return Instance._GetRevistaURL; } }

		public List<Revista> Lista
		{
			get { return _lista; }
			set
			{
				_lista = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Lista"));
			}
		}

		public bool Load(Stream stre)
		{
			DataContractJsonSerializer des = new DataContractJsonSerializer(typeof(List<Revista>));
			Lista = des.ReadObject(stre) as List<Revista>;
			return Lista != null;
		}

		public bool Save(out Stream str)
		{
			DataContractJsonSerializer x = new DataContractJsonSerializer(typeof(List<Revista>));
			str = new MemoryStream();
			x.WriteObject(str, Lista);
			return str.CanRead;
		}

		public bool SaveToStream(Stream str)
		{
			DataContractJsonSerializer x = new DataContractJsonSerializer(typeof(List<Revista>));
			x.WriteObject(str, Lista);
			return str.CanRead;
		}

		public Uri GetDownloadFile(Revista rev)
		{
			const string FRM = @"http://www.portugal-a-programar.pt/revista-programar/edicoes/download.php?t=site&e={0}";
			return new Uri(string.Format(FRM, rev.ID), UriKind.Absolute);
		}

		public Uri GetDownloadFile(Edicao edi)
		{
			return new Uri(edi.pdf);
		}

		public bool RefreshLista(string pagHtml)
		{
			const string IMAGEHD = @"http://www.portugal-a-programar.pt/revista-programar/images/ed{0}.{1}";
			const string REGEX = @"(?:<a href="")(?<url>[^""]*)(?:""><img border=""0"" style=""padding: 10px; margin: 10px;"" alt="")(?<nome>[^""]*)(?:"" src="")(?<img>[^""]*)";

			if (string.IsNullOrEmpty(pagHtml))
				return false;

			List<Revista> list = new List<Revista>();

			foreach (Match item in Regex.Matches(pagHtml, REGEX, RegexOptions.Multiline))
			{
				var grp = item.Groups;
				var rev = new Revista()
				{
					Url = grp["url"].Value
					,
					//ImgUrl = grp["img"].Value //mudar para uma imagem com melhor qualidade
					//,
					Name = grp["nome"].Value
					,
					ID = grp["url"].Value.Split('=').LastOrDefault()//um pouco errado, mas funciona :)
				};
				rev.ImgUrl = new Uri(string.Format(IMAGEHD, rev.ID, grp["img"].Value.Split('.').LastOrDefault())); //outra vez, nao muito correcto mas funciona
				list.Add(rev);
			}
			if (list.Count > 0)
				Lista = list;

			return list.Count > 0;
		}

		
	}
}