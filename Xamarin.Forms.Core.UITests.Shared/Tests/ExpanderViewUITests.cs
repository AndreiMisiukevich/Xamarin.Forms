using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	[Category(UITestCategories.ExpanderView)]
	internal class ExpanderViewUITests : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ExpanderGallery);
		}

		[TestCase]
		public void ExpanderView()
		{
			App.WaitForElement("The Second");
			App.Tap("Expander Level 2");
			App.WaitForElement("Hi, I am Red");
			App.Tap("The Fourth");

			App.WaitForNoElement("Hi, I am Red");

			App.WaitForElement("Expander Level 2");
			App.Tap("Expander Level 2");
			App.WaitForElement("Hi, I am Red");
			App.Tap("Expander Level 2");

			App.WaitForNoElement("Hi, I am Red");

			App.Tap("The Fourth");

			App.WaitForNoElement("Expander Level 2");

			App.Back();
		}
	}
}
