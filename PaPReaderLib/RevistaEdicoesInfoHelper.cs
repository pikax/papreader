using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaPReaderLib
{
	public static class RevistaEdicoesInfoHelper
	{
		static string[] REMOVEMinus = new string[]
			{
					"titulo"
					,"designacao"
					,"tamanho"
					,"num"
					,"data"
			};

		public static string CleanAndTreatJson(string json)
		{
			string res = json;

			foreach (var item in REMOVEMinus)
			{
				res = res.Replace(string.Format("\"-{0}\"", item), item);
			}
			res = res.Replace("\"#text\"", "text");
			return res;
		}
	}
}
