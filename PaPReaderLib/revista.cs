using System;
using System.Runtime.Serialization;
namespace PaPReaderLib
{
	public class Revista
	{
		public string ID {  get; set; }
		public Uri ImgUrl { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
	}
}