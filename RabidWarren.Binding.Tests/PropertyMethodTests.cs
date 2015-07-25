// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyMethodTests.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Tests Properties that exist as explicit get and set methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;


    /// <summary>
    /// Tests Properties that exist as explicit get and set methods.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class PropertyMethodTests
    {
		PropertiedClass _view;
        
        class Settable : BindingObject
		{
			internal string _text;
			
			public void set_Text(string value)
			{
				_text = value;
#pragma warning disable 0618
                OnPropertyChangedEvent("Number");
#pragma warning restore 0618
            }
		}

		class PropertiedClass : Settable
        {
            public string get_Text()
            {
                return _text;
            }
        }

		[SetUp]
		public void Setup()
		{
			_view = new PropertiedClass();
		}

		[Test]
        public void PropertyMethods()
        {
            var exampleText = "Some Text";
			var viewModel = new PropertiedClass();

            viewModel.set_Text(exampleText);
            _view.Bind(_view, "Text", viewModel, "Text");

            Assert.AreEqual(exampleText, _view.get_Text());
        }

		/// <summary>
		/// Binding to unreadable properties is not permitted, so a get_* method is required.
		/// </summary>
		[Test]
		[ExpectedException(typeof(ArgumentException),
			ExpectedMessage = "A source property must be readable.\r\nParameter name: sourceProperty")]
		public void CannotBindSetOnlyMethod()
		{
			var viewModel = new Settable();

			_view.Bind(_view, "Text", viewModel, "Text");
		}
	}
}
