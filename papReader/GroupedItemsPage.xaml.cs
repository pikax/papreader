using papReader.Common;
using papReader.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace papReader
{
	/// <summary>
	/// A page that displays a grouped collection of items.
	/// </summary>
	public sealed partial class GroupedItemsPage : Page
	{
		private NavigationHelper navigationHelper;
		private ObservableDictionary defaultViewModel = new ObservableDictionary();


		private CancellationTokenSource cts;
		private DownloadOperation _downloadOp = null;
		private string _number;


		MessageDialog _dialog;

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

		public GroupedItemsPage()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;
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
			//var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
			//this.DefaultViewModel["Groups"] = sampleDataGroups;

			try
			{
				itemGridView.ItemsSource = await revista.GetRevistas();
				return;
			}
			catch
			{
			}
			itemGridView.ItemsSource = await FillFromCache();
		}

		private async Task<List<revista>> FillFromCache()
		{
			var dir = ApplicationData.Current.LocalFolder;

			var list = new List<revista>();

			var files = await dir.GetFilesAsync();
			foreach (var item in files)
			{
				var xx = item.Name.Replace(".pdf", "");
				list.Add(new revista() { ID = xx, Name = xx });
			}
			return list;
		}

		/// <summary>
		/// Invoked when a group header is clicked.
		/// </summary>
		/// <param name="sender">The Button used as a group header for the selected group.</param>
		/// <param name="e">Event data that describes how the click was initiated.</param>
		void Header_Click(object sender, RoutedEventArgs e)
		{
			//// Determine what group the Button instance represents
			//var group = (sender as FrameworkElement).DataContext;

			//// Navigate to the appropriate destination page, configuring the new page
			//// by passing required information as a navigation parameter
			//this.Frame.Navigate(typeof(GroupDetailPage), ((SampleDataGroup)group).UniqueId);
		}

		/// <summary>
		/// Invoked when an item within a group is clicked.
		/// </summary>
		/// <param name="sender">The GridView (or ListView when the application is snapped)
		/// displaying the item clicked.</param>
		/// <param name="e">Event data that describes the item clicked.</param>
		async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
		{
			cts = new CancellationTokenSource();
			var itemId = ((revista)e.ClickedItem).ID;

			StorageFile file = await revista.GetPDF(itemId);
			if (file == null)
			{
				if (!await RequestFileDownload())
					return;

				BarProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
				_number = itemId;

				_downloadOp = await revista.GetDownloadFile(itemId);

				if (_downloadOp != null)
					await HandleDownloadAsync(_downloadOp, true);
				
				return;
			}
			await Windows.System.Launcher.LaunchFileAsync(file);
			return;

			// Navigate to the appropriate destination page, configuring the new page
			// by passing required information as a navigation parameter
			this.Frame.Navigate(typeof(ItemDetailPage), itemId);
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
				BarProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

			}
			if (!string.IsNullOrEmpty(notifica))
			{
				await MessageBox(notifica);
				return;
			}

			await Windows.System.Launcher.LaunchFileAsync(await revista.GetPDF(_number));
			//await SetPDF(await revista.GetPDF(_number));
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

			BarProgress.Value = percent;
			//_dialog.Content = string.Format("{0:0}%", percent);
			
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