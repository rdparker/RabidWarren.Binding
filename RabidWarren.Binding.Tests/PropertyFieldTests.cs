namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;


    /// <summary>
    /// Tests Properties that exist as explicit get and set methods.
    /// </summary>
    class PropertyFieldTests
    {
        class PropertiedClass : BindingObject
        {
            public string Text;
        }

        [Test]
        public void PropertyFields()
        {
            var exampleText = "Some Text";

            var view = new PropertiedClass();
            var viewModel = new PropertiedClass();

            viewModel.Text = exampleText;
            view.Bind(view, "Text", viewModel, "Text");

            Assert.AreEqual(exampleText, view.Text);
        }
    }
}
