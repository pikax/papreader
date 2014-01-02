using papReader.Common;
using papReader.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace papReader
{
	/// <summary>
	/// A page that displays details for a single item within a group.
	/// </summary>
	public sealed partial class ItemDetailPage : Page
	{
		private NavigationHelper navigationHelper;
		private ObservableDictionary defaultViewModel = new ObservableDictionary();


		string _number = "";

		private CancellationTokenSource cts;
		private DownloadOperation _downloadOp = null;

		/// <summary>
		/// NavigationHelper is used on each page to aid in navigation and 
		/// process lifetime management
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}

		/// <summary>
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public ObservableDictionary DefaultViewModel
		{
			get { return this.defaultViewModel; }
		}

		public ItemDetailPage()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;

			cts = new CancellationTokenSource();
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event data that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			// TODO: Create an appropriate data model for your problem domain to replace the sample data
			_number = e.NavigationParameter.ToString();


			_flipView.ItemTemplate = _flipView.Resources["SinglePageWithZoom"] as DataTemplate;

			StorageFile file = await revista.GetPDF(_number);
			if (file == null)
			{
				if (!await RequestFileDownload())
					return;

				_downloadOp = await revista.GetDownloadFile(_number);

				await HandleDownloadAsync(_downloadOp, true);

				return;
			}

			await SetPDF(file);
		}

		private async Task HandleDownloadAsync(DownloadOperation download, bool start)
		{
			string notifica = "";
			try
			{
				Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
				if (start)
				{
					// Start the download and attach a progress handler.
					await download.StartAsync().AsTask(cts.Token, progressCallback);
				}
				else
				{
					// The download was already running when the application started, re-attach the progress handler.
					await download.AttachAsync().AsTask(cts.Token, progressCallback);
				}

				ResponseInformation response = download.GetResponseInformation();

				//LogStatus(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
				//	download.Guid, response.StatusCode), NotifyType.StatusMessage);
			}
			catch (TaskCanceledException)
			{
				notifica = "Download Cancelado, tente novamente mais tarde";
				//LogStatus("Canceled: " + download.Guid, NotifyType.StatusMessage);
			}
			catch (Exception ex)
			{
				notifica = "Download Cancelado, tente novamente mais tarde";



				//if (!IsExceptionHandled("Execution error", ex, download))
				//{
				//	throw;
				//}
			}
			finally
			{
				_downloadOp = null;
			}
			if (!string.IsNullOrEmpty(notifica))
			{
				await MessageBox(notifica);
				return;
			}
			await SetPDF(await revista.GetPDF(_number));
		}

		private void DownloadProgress(DownloadOperation download)
		{
			//MarshalLog(String.Format(CultureInfo.CurrentCulture, "Progress: {0}, Status: {1}", download.Guid,
			//   download.Progress.Status));

			double percent = 100;
			if (download.Progress.TotalBytesToReceive > 0)
			{
				percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
			}

			//MarshalLog(String.Format(CultureInfo.CurrentCulture, " - Transfered bytes: {0} of {1}, {2}%",
			//	download.Progress.BytesReceived, download.Progress.TotalBytesToReceive, percent));

			//if (download.Progress.HasRestarted)
			//{
			//	MarshalLog(" - Download restarted");
			//}

			//if (download.Progress.HasResponseChanged)
			//{
			//	// We've received new response headers from the server.
			//	MarshalLog(" - Response updated; Header count: " + download.GetResponseInformation().Headers.Count);

			//	// If you want to stream the response data this is a good time to start.
			//	// download.GetResultStreamAt(0);
			//}
		}

		private async Task SetPDF(StorageFile file)
		{
			var _pdfDocument = await PdfDocument.LoadFromFileAsync(file); ;

			if (_pdfDocument != null && _pdfDocument.PageCount > 0)
			{
				uint numpg = _pdfDocument.PageCount;



				var list = new List<Lazy<BitmapImage>>();

				for (uint i = 0; i < numpg; i++)
				{
					var xx = i;
					//list.Add(new Lazy<BitmapImage>AsyncHelpers.RunSync(()=>{ return GetBitmapFrom(_pdfDocument.GetPage(i)); })));
					Func<BitmapImage> funcs = () => { return AsyncHelpers.RunSync(() => { return GetBitmapFromAsync(_pdfDocument.GetPage(xx)); }); };
					list.Add(new Lazy<BitmapImage>(funcs));
				}

				_flipView.ItemsSource = list;
			}
		}

		private Uri GetBitmapFrom(PdfPage pg)
		{
			StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

			var picName = Guid.NewGuid().ToString() + ".png";
			StorageFile jpgFile = AsyncHelpers.RunSync(() => { return tempFolder.CreateFileAsync(picName, CreationCollisionOption.ReplaceExisting).AsTask(); });

			if (jpgFile != null)
			{
				using (IRandomAccessStream randomStream = AsyncHelpers.RunSync(() => { return jpgFile.OpenAsync(FileAccessMode.ReadWrite).AsTask(); }))
				{
					PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();

					AsyncHelpers.RunSync(() => { return pg.RenderToStreamAsync(randomStream).AsTask(); });
					AsyncHelpers.RunSync(() => { return randomStream.FlushAsync().AsTask(); });

					pg.Dispose();

					return new Uri(string.Format(@"ms-appdata:///temp/{0}", picName));
				}
			}
			return new Uri("");
		}

		private async Task<BitmapImage> GetBitmapFromAsync(PdfPage pg)
		{
			BitmapImage bitmap = new BitmapImage();

			PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();
			MemoryStream stream = new MemoryStream();

			

			await pg.RenderToStreamAsync(stream.AsRandomAccessStream());
			stream.Seek(0, SeekOrigin.Begin);
			bitmap.SetSource(stream.AsRandomAccessStream());
			return bitmap;
			
			//StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;



			//var picName = Guid.NewGuid().ToString() + ".png";
			//StorageFile jpgFile = await tempFolder.CreateFileAsync(picName, CreationCollisionOption.ReplaceExisting);

			//if (jpgFile != null)
			//{
			//	using (IRandomAccessStream randomStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite))
			//	{
			//		PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();

			//		await pg.RenderToStreamAsync(randomStream);
			//		await randomStream.FlushAsync();

			//		pg.Dispose();
			//		var bit = new BitmapImage();
			//		bit.SetSource(await jpgFile.OpenAsync(FileAccessMode.Read));
			//		return bit;

			//	}
			//}
			//return new BitmapImage();
		}

		public async Task DisplayImageFileAsync(StorageFile file)
		{
			// Display the image in the UI.
			BitmapImage src = new BitmapImage();
			src.SetSource(await file.OpenAsync(FileAccessMode.Read));
			//Image1.Source = src;
		}

		private async Task MessageBox(string message)
		{
			var messageDialog = new MessageDialog(message, "Notificação");
			messageDialog.Commands.Add(new UICommand("OK", (command) => { }));
			await messageDialog.ShowAsync();
		}

		private async Task<bool> RequestFileDownload()
		{
			// Create the message dialog and set its content and title
			var messageDialog = new MessageDialog("Ficheiro não encontrado, deseja fazer download?", "Uppsss");

			var yesCommand = new UICommand("Sim", (command) =>
			{
			});
			// Add commands and set their callbacks
			messageDialog.Commands.Add(yesCommand);

			messageDialog.Commands.Add(new UICommand("Não", (command) =>
			{
				//rootPage.NotifyUser("The 'Install updates' command has been selected.", NotifyType.StatusMessage);
			}));

			// Set the command that will be invoked by default
			messageDialog.DefaultCommandIndex = 1;

			// Show the message dialog
			return (await messageDialog.ShowAsync()) == yesCommand;
		}

		#region NavigationHelper registration

		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// 
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
		/// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.


		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedFrom(e);
		}

		#endregion
	}
}