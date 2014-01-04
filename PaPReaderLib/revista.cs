using System.Runtime.Serialization;
namespace PaPReaderLib
{
	[KnownType(typeof(Revista))]
	public class Revista
	{
		public string ID {  get; set; }
		public string ImgUrl { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
	}
}