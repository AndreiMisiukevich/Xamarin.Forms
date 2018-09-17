﻿using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3809, "SetUseSafeArea is wiping out Page Padding ")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue3809 : TestMasterDetailPage
	{
		const string _setPagePadding = "Set Page Padding";
		const string _safeAreaText = "Safe Area Enabled: ";
		const string _paddingLabel = "paddingLabel";
		const string _safeAreaAutomationId = "SafeAreaAutomation";

		Label label = null;
		protected override void Init()
		{
			label = new Label()
			{
				AutomationId = _paddingLabel
			};

			Master = new ContentPage() { Title = "Master" };
			Button button = null;
			button = new Button()
			{
				Text = $"{_safeAreaText} true",
				AutomationId = _safeAreaAutomationId,
				Command = new Command(() =>
				{
					bool safeArea = !Detail.On<Xamarin.Forms.PlatformConfiguration.iOS>().UsingSafeArea();
					Detail.On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(safeArea);
					button.Text = $"{_safeAreaText} {safeArea}";
					Device.BeginInvokeOnMainThread(() =>
					{
						label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}";
					});
				})
			};

			Detail = new ContentPage()
			{
				Title = "Details",
				Content = new StackLayout()
				{
					Children =
					{
						new ListView(ListViewCachingStrategy.RecycleElement)
						{
							ItemsSource = Enumerable.Range(0,200).Select(x=> x.ToString()).ToList()
						},
						label,
						button,
						new Button()
						{
							Text = _setPagePadding,
							Command = new Command(() =>
							{
								Detail.Padding = new Thickness(25, 25, 25, 25);
							})
						}
					}
				}
			};

			Detail.Padding = new Thickness(25, 25, 25, 25);
			Detail.On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}";
		}

#if UITEST
		[Test]
		public void SafeAreaInsetsBreaksAndroidPadding()
		{
			// ensure initial paddings are honored
			var element = RunningApp.WaitForElement(_paddingLabel).First();
			Assert.AreNotEqual(element.Text, "0, 0, 0, 0");
#if !__IOS__
			Assert.AreEqual(element.Text, "25, 25, 25, 25");
#endif

			// disable Safe Area Insets
			RunningApp.Tap(_safeAreaAutomationId);
			element = RunningApp.WaitForElement(_paddingLabel).First();

#if __IOS__
			Assert.AreEqual(element.Text, "0, 0, 0, 0");
#else
			Assert.AreEqual(element.Text, "25, 25, 25, 25");
#endif

			// enable Safe Area insets
			RunningApp.Tap(_safeAreaAutomationId);
			element = RunningApp.WaitForElement(_paddingLabel).First();
			Assert.AreNotEqual(element.Text, "0, 0, 0, 0");
#if !__IOS__
			Assert.AreEqual(element.Text, "25, 25, 25, 25");
#endif


			// Set Padding and then disable safe area insets
			RunningApp.Tap(_setPagePadding);
			RunningApp.Tap(_safeAreaAutomationId);
			element = RunningApp.WaitForElement(_paddingLabel).First();
			Assert.AreEqual(element.Text, "25, 25, 25, 25");

		}
#endif
	}
}
