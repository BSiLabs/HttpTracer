using System;
using System.Linq;

namespace HttpTracer.TestApp.Views
{
	public partial class MainPage
	{
		public MainPage ()
		{
			InitializeComponent ();
		}

		private async void VerbosityButton_OnClicked(object sender, EventArgs e)
		{
			var verbosityOptions = Enum.GetValues(typeof(HttpMessageParts)).Cast<HttpMessageParts>().Select(x => x.ToString()).ToArray();
			string action = await DisplayActionSheet("Select Verbosity:", "Cancel", null, verbosityOptions);
			if (action == "Cancel") return;
			HttpTracerHandler.DefaultVerbosity = (HttpMessageParts)Enum.Parse(typeof(HttpMessageParts), action);
		}
	}
}