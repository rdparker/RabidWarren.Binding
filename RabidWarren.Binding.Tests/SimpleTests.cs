namespace RabidWarren.Binding.Tests
{
	using NUnit.Framework;
	using System;
	using System.ComponentModel;


	/// <summary>
	/// Contains tests that do not pass a source object to the BindingObject constructor.
	/// </summary>
	public class SimpleTests : INotifyPropertyChanged
    {
        View _view;
        ViewModel _viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
        public string Text { get; set; }

        [SetUp]
        public void Setup()
        {
            _viewModel = new ViewModel();
            _view = new View();
        }

        /// <summary>
        /// Tests that a 4 argument Bind call returns without exception, when the source property is initialized.
        /// </summary>
        [Test]
        public void BindReturns4Arg()
        {
            _viewModel.Text = "Some Text";
            _view.Bind(_view, "Text", _viewModel, "Text");

            Assert.True(true);
        }

        /// <summary>
        /// Tests that a 4 argument Bind call returns without exception, when the source property is
        /// not initialized.
        /// </summary>
        [Test]
        public void BindReturns4ArgUninitialized()
        {
            _view.Bind(_view, "Text", _viewModel, "Text");

            Assert.True(true);
        }

        // Test the CanRead pseudo-property.
        [Test]
        public void CanRead()
        {
            _view.Bind(this, "IsReadable", _viewModel, "Text.CanRead");

            Assert.True(IsReadable);
        }

        // Test the CanWrite pseudo-property.
        [Test]
        public void CanWrite()
        {
            _view.Bind(this, "IsWritable", _viewModel, "Text.CanWrite");

            Assert.True(IsWritable);
        }

        // Test the CanRead pseudo-property returning false.
        [Test]
        public void CannotRead()
        {
            _view.Bind(this, "IsReadable", _viewModel, "UnreadableText.CanRead");

            Assert.False(IsReadable);
        }

		// Test the CanWrite pseudo-property returning false.
        [Test]
        public void CannotWrite()
        {
            _view.Bind(this, "IsWritable", _viewModel, "UnwritableText.CanWrite");

            Assert.False(IsWritable);
        }
		
		/// <summary>
		/// Binding to unreadable properties is not permitted.
		/// </summary>
		[Test]
		[ExpectedException(typeof(ArgumentException),
			ExpectedMessage = "A source property must be readable.\r\nParameter name: sourceProperty")]
		public void CannotBindUnreadableProperty()
		{
			_view.Bind(this, "Text", _viewModel, "UnreadableText");
		}

        /// <summary>
        /// This tests binding multiple properties within the same objects.  This also helps with code
        /// coverage in <see cref="M:BindingObject::Bind{TTarget,TSource}"/>.
        /// </summary>
        [Test]
        public void BindMultipleProperties()
        {
            _view.Bind(_view, "Text", _viewModel, "Text");
            _view.Bind(_view, "Text2", _viewModel, "Text2");

            _viewModel.Text = "Text";
            _viewModel.Text2 = "Text2";

            Assert.AreEqual(_view.Text, "Text");
            Assert.AreEqual(_view.Text2, "Text2");
        }
    }
}
