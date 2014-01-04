using papReader.Common;
using papReader.Helper;
using PaPReaderLib;
using System;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace papReader
{
	/// <summary>
	/// A page that displays a grouped collection of items.
	/// </summary>
	public sealed partial class GroupedItemsPage : Page
	{
		private string _number;
		private NavigationHelper navigationHelper;

		public GroupedItemsPage()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;
		}

		/// <summary>
		/// NavigationHelper is used on each page to aid in navigation and
		/// process lifetime management
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
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

		/// <summary>
		/// Invoked when a group header is clicked.
		/// </summary>
		/// <param name="sender">The Button used as a group header for the selected group.</param>
		/// <param name="e">Event data that describes how the click was initiated.</param>
		private void Header_Click(object sender, RoutedEventArgs e)
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
		private async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var rev = ((Revista)e.ClickedItem);
			var file = await RevistaControllerWrapper.Instance.GetPDF(rev);
			if (file == null)
			{
				if (!await RequestFileDownload())
					return;
				try
				{
					BarProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;

					await DownloadHelper.Download(rev, ExecFile, ShowError, (x) => { BarProgress.Value = x; if (x == 100)BarProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed; });
					return;
				}
				finally
				{
					BarProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				}
			}
			await ExecFile(file);
		}

		async Task ExecFile(IStorageFile file)
		{
			await Windows.System.Launcher.LaunchFileAsync(file);
		}

		private async Task MessageBox(string message)
		{
			var messageDialog = new MessageDialog(message, "Notificação");
			messageDialog.Commands.Add(new UICommand("OK", (command) => { }));
			await messageDialog.ShowAsync();
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

			if(!await RevistaControllerWrapper.Instance.Refresh())
				await RevistaControllerWrapper.Instance.Load();
			_grd.DataContext = RevistaControllerWrapper.Instance;
			//itemGridView.ItemsSource = RevistaControllerWrapper.Instance.;
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

		private async void ShowError(string x)
		{
			await MessageBox(x);
			BarProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedFrom(e);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedTo(e);
		}

		#endregion NavigationHelper registration
	}
}