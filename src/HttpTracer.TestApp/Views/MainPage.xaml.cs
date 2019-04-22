using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HttpTracer.TestApp.Views
{
	public partial class MainPage : ContentPage
	{
		public MainPage ()
		{
			InitializeComponent ();
		}

		private async void VerbosityButton_OnClicked(object sender, EventArgs e)
		{
			var verbosityOptions = new string[]
			{
				HttpMessageParts.None.ToString(),
				HttpMessageParts.All.ToString(),
				HttpMessageParts.RequestAll.ToString(),
				HttpMessageParts.ResponseAll.ToString()
			};
			string action = await DisplayActionSheet("Select Verbosity:", "Cancel", null, verbosityOptions);
			if (action == "Cancel") return;
			HttpTracerHandler.DefaultVerbosity = (HttpMessageParts)Enum.Parse(typeof(HttpMessageParts), action);
		}
	}
}