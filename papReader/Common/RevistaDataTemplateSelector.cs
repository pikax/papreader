using papReader.Helper;
using PaPReaderLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace papReader.Common
{
	class RevistaDataTemplateSelector : DataTemplateSelector
	{
		protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
		{
			Revista rev = item as Revista;
			if(rev == null)
				base.SelectTemplateCore(item, container);

			if(RevistaControllerWrapper.ListaFicheiros.Contains(rev.ID))
				return GroupedItemsPage.CurrentPage.Resources["TemplateDownloaded"] as DataTemplate;
			else
				return GroupedItemsPage.CurrentPage.Resources["TemplateNormal"] as DataTemplate;
		}
	}
}
