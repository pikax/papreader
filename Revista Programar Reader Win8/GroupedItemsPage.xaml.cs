using RevProgramarWin8.Common;
using RevProgramarWin8.Helper;
using PaPReaderLib;
using System;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace RevProgramarWin8
{
	/// <summary>
	/// A page that displays a grouped collection of items.
	/// </summary>
	public sealed partial class GroupedItemsPage : Page
	{
		public static GroupedItemsPage CurrentPage;
		private NavigationHelper navigationHelper;

		public GroupedItemsPage()
		{
			this.InitializeComponent();
			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;
			this.itemGridView.ItemTemplateSelector = new RevistaDataTemplateSelector();
			CurrentPage = this;
		}

		/// <summary>
		/// NavigationHelper is used on each page to aid in navigation and
		/// process lifetime management
		/// </summary>
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
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
			if (!RevistaControllerWrapper.ListaFicheiros.Contains(file.Name))
				RevistaControllerWrapper.ListaFicheiros.Add(file.Name);
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

			if (!await RevistaControllerWrapper.Instance.Refresh())
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

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
			base.OnNavigatedTo(e);
			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			navigationHelper.OnNavigatedFrom(e);
			SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
		}

		/// <summary>
		/// Handler for the CommandsRequested event. Add custom SettingsCommands here.
		/// </summary>
		/// <param name="e">Event data that includes a vector of commands (ApplicationCommands)</param>
		void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
		{
			SettingsCommand generalCommand = new SettingsCommand("PoliticalPrivacy", "Política de Privacidade ",
				async (handler) =>
				{
					await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.portugal-a-programar.pt/RevistaProgramarReaderEula.html"));
				});
			e.Request.ApplicationCommands.Add(generalCommand);
		}

	}
}