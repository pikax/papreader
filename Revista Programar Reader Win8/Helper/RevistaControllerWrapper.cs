using PaPReaderLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using PaPReaderLib.Model;

namespace RevProgramarWin8.Helper
{
	public class RevistaControllerWrapper : INotifyPropertyChanged
	{
		public static readonly RevistaControllerWrapper Instance = new RevistaControllerWrapper();

		public static readonly List<string> ListaFicheiros = new List<string>();

		private const string LISTAFILENAME = "listRevista.json";

		private const string PATH = "Revistas";
		private const string PATHIMAGENS = "Capas";

		private RevistaControllerWrapper()
		{
			RevistaController.Instance.PropertyChanged += (x, y) =>
					{
						if (PropertyChanged != null)
							PropertyChanged(this, new PropertyChangedEventArgs("Lista"));
					};
		}
		public event PropertyChangedEventHandler PropertyChanged;

		public List<Revista> Lista { get { return RevistaController.Instance.Lista; } }

		public static string FolderName { get { return PATH; } }

		public static string GetPdfName(Revista rev)
		{
			return string.Format("{0}.pdf", rev.ID);
		}

		public static string GetPdfName(Edicao edi)
		{
			return string.Format("{0}.pdf", edi.num);
		}

		public async Task<StorageFile> GetPDF(Revista rev)
		{
			try
			{
				var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists);
				return await folder.GetFileAsync(GetPdfName(rev));
			}
			catch
			{
				return null;
			}
		}
		public async Task<StorageFile> GetPDF(Edicao edi)
		{
			try
			{
				var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists);
				return await folder.GetFileAsync(GetPdfName(edi));
			}
			catch
			{
				return null;
			}
		}


		public async Task<bool> Load()
		{
			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LISTAFILENAME, CreationCollisionOption.OpenIfExists);

			return RevistaController.Instance.Load(await file.OpenStreamForReadAsync());
		}

		public async Task<bool> Refresh()
		{
			if (!RevistaController.Instance.RefreshLista(
				await LibHelper.GetHttpPageAsyncHelper(RevistaController.RevistaUrl)))
				return false;

			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(PATHIMAGENS, CreationCollisionOption.OpenIfExists);
			if (folder == null)
				return false;
			using (var http = new System.Net.Http.HttpClient())
			{
				var ll = await folder.GetFilesAsync();
				foreach (var item in Lista.Where(x => ll.FirstOrDefault(y => y.Name.Equals(Path.GetFileName(x.ImgUrl.AbsolutePath))) == null))
				{
					var file = await folder.CreateFileAsync(Path.GetFileName(item.ImgUrl.AbsolutePath), CreationCollisionOption.ReplaceExisting);

					using (var str = await http.GetStreamAsync(item.ImgUrl))
					{
						str.CopyTo(await file.OpenStreamForWriteAsync());
					}
					item.ImgUrl = new Uri(file.Path);
				}
			}
			return true;
		}

		public async Task<bool> Save()
		{
			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LISTAFILENAME, CreationCollisionOption.ReplaceExisting);

			return RevistaController.Instance.SaveToStream(await file.OpenStreamForWriteAsync());
		}
	}
}