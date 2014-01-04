using PaPReaderLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace papReader.Helper
{
	public class RevistaControllerWrapper : INotifyPropertyChanged
	{
		public static readonly RevistaControllerWrapper Instance = new RevistaControllerWrapper();

		private const string LISTAFILENAME = "listRevista.json";

		private const string PATH = "Revistas";

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

		public async Task<bool> Load()
		{
			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LISTAFILENAME, CreationCollisionOption.OpenIfExists);

			return RevistaController.Instance.Load(await file.OpenStreamForReadAsync());
		}

		public async Task<bool> Refresh()
		{
			return RevistaController.Instance.RefreshLista(
				await LibHelper.GetHttpPageAsyncHelper(RevistaController.RevistaUrl));
		}

		public async Task<bool> Save()
		{
			var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(LISTAFILENAME, CreationCollisionOption.ReplaceExisting);

			return RevistaController.Instance.SaveToStream(await file.OpenStreamForWriteAsync());
		}
	}
}