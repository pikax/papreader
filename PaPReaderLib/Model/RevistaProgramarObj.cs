using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PaPReaderLib.Model
{
	public class Info
	{
		public string versao { get; set; }
		public string totaledicoes { get; set; }
		public string ultimaedicao { get; set; }
		public string url { get; set; }
	}

	public class Capa
	{
		public string tamanho { get; set; }
		public string text { get; set; }
	}

	public class Capas
	{
		public List<Capa> capa { get; set; }
	}

	public class Paginas
	{
		public List<string> pagina { get; set; }
	}

	public class Imagens
	{
		public Capas capas { get; set; }
		public Paginas paginas { get; set; }
	}

	public class Categoria
	{
		public string designacao { get; set; }
		public object artigo { get; set; }
	}

	public class Artigos
	{
		public List<Categoria> categoria { get; set; }
	}

	public class Edicao
	{
		public string num { get; set; }
		public string data { get; set; }
		public Imagens imagens { get; set; }
		public Artigos artigos { get; set; }
		public object equipa { get; set; }
		public string pdf { get; set; }
	}

	public class Edicoes
	{
		public List<Edicao> edicao { get; set; }
	}

	public class Programar
	{
		public Info info { get; set; }
		public Edicoes edicoes { get; set; }
	}

	public class RevistaProgramarObj
	{
		public Programar programar { get; set; }
	}

	public class RevistaProgramar
	{
		public RevistaProgramarObj RevistaPro { get; private set; }
		public bool Load(Stream str)
		{
			using (StreamReader reader = new StreamReader(str))
			{
				var contents = reader.ReadToEnd();
				contents = RevistaEdicoesInfoHelper.CleanAndTreatJson(contents);

				DataContractJsonSerializer des = new DataContractJsonSerializer(typeof(RevistaProgramarObj));
				using (var mem = new MemoryStream(GetBytes(contents)))
				{
					RevistaPro = des.ReadObject(mem) as RevistaProgramarObj;
					return RevistaPro != null;
				}
			}

		}

		public static byte[] GetBytes(string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}
	}
}
