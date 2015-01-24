namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;


    /// <summary>
    /// Tests Properties that exist as explicit get and set methods.
    /// </summary>
    class PropertyMethodTests
    {
        class PropertiedClass : BindingObject
        {
            string _text;

            public string get_Text()
            {
                return _text;
            }

            public void set_Text(string value)
            {
                _text = value;
                OnPropertyChangedEvent("Number");
            }
        }

        [Test]
        public void PropertyMethods()
        {
            var exampleText = "Some Text";

            var view = new PropertiedClass();
            var viewModel = new PropertiedClass();

            viewModel.set_Text(exampleText);
            view.Bind(view, "Text", viewModel, "Text");

            Assert.AreEqual(exampleText, view.get_Text());
        }
    }
}
