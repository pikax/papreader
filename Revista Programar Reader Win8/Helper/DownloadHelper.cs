﻿using PaPReaderLib;
using PaPReaderLib.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;

namespace RevProgramarWin8.Helper
{
	public class DownloadHelper : IDisposable
	{
		private Action<double> _progressCall;

		private CancellationTokenSource cts = new CancellationTokenSource();

		~DownloadHelper()
		{
			this.Dispose(false);
		}

		public static async Task<bool> Download(Revista rev, Func<IStorageFile, Task> onSucess, Action<string> callBack, Action<double> progressHandler)
		{
			if (!HaveInternetConnection())
			{
				callBack("Sem conexão à Internet");
				return false;
			}

			BackgroundDownloader bg = new BackgroundDownloader();

			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(RevistaControllerWrapper.FolderName, CreationCollisionOption.OpenIfExists);
			var file = await folder.CreateFileAsync(RevistaControllerWrapper.GetPdfName(rev), CreationCollisionOption.ReplaceExisting);

			using (DownloadHelper down = new DownloadHelper())
			{
				try
				{
					var d = bg.CreateDownload(RevistaController.Instance.GetDownloadFile(rev), file);

					await down.HandleDownloadAsync(d, true, onSucess, callBack, progressHandler);
				}
				catch
				{
					return false;
				}
				return true;
			}
		}

		public static async Task<bool> Download(Edicao edicao, Func<IStorageFile, Task> onSucess, Action<string> callBack, Action<double> progressHandler)
		{
			if (!HaveInternetConnection())
			{
				callBack("Sem conexão à Internet");
				return false;
			}

			BackgroundDownloader bg = new BackgroundDownloader();
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(RevistaControllerWrapper.FolderName, CreationCollisionOption.OpenIfExists);
			var file = await folder.CreateFileAsync(RevistaControllerWrapper.GetPdfName(edicao), CreationCollisionOption.ReplaceExisting);

			using (DownloadHelper down = new DownloadHelper())
			{
				try
				{
					var d = bg.CreateDownload(RevistaController.Instance.GetDownloadFile(edicao), file);

					await down.HandleDownloadAsync(d, true, onSucess, callBack, progressHandler);
				}
				catch
				{
					return false;
				}
				return true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// The bulk of the clean-up code is implemented in Dispose(bool)
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				if (cts != null)
				{
					cts.Dispose();
					cts = null;
				}
			}
		}

		private static bool HaveInternetConnection()
		{
			string connectionProfileList = string.Empty;
			try
			{
				var ConnectionProfiles = NetworkInformation.GetConnectionProfiles();
				foreach (var connectionProfile in ConnectionProfiles)
				{
					if (connectionProfile != null)
					{
						var lvl = connectionProfile.GetNetworkConnectivityLevel();
						if (lvl == NetworkConnectivityLevel.InternetAccess || lvl == NetworkConnectivityLevel.ConstrainedInternetAccess)
							return true;
					}
				}
			}
			catch
			{
				return false;
			}
			return false;
		}
		private void DownloadProgress(DownloadOperation download)
		{
			double percent = 100;
			if (download.Progress.TotalBytesToReceive > 0)
			{
				percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
			}

			_progressCall(percent);
		}

		private async Task HandleDownloadAsync(DownloadOperation download, bool start, Func<IStorageFile, Task> finish, Action<string> callBack, Action<double> progressHandler)
		{
			_progressCall = progressHandler;
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
			}
			catch (TaskCanceledException)
			{
				notifica = "Download Cancelado, tente novamente mais tarde";
				//LogStatus("Canceled: " + download.Guid, NotifyType.StatusMessage);
			}
			catch (Exception ex)
			{
				notifica = "-- Erro desconhecido --";
				notifica = string.Format("{0}\n{1}", notifica, ex.ToString());
			}

			if (notifica == "")
			{
				await finish(download.ResultFile);
			}
			else
			{
				await download.ResultFile.DeleteAsync();
				callBack(notifica);
			}
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
	}
}