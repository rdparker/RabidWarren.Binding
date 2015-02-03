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
		
        [Test]
        [ExpectedException(typeof(ArgumentException),
            ExpectedMessage = "Cannot bind to an unregistered property.\r\nParameter name: sourceProperty")]
        public void CannotBindToNestedProperty()
        {
            _viewModel.Outer.Inner = "Nested";
            _view.Bind(this, "Text", _viewModel, "Outer.Inner");
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
            _view.Bind("Text", _viewModel, "Text");
            _view.Bind("Text2", _viewModel, "Text2");

            _viewModel.Text = "Text";
            _viewModel.Text2 = "Text2";

            // TODO:  Fix the order of all Assert arguments.
            Assert.AreEqual(_view.Text, "Text");
            Assert.AreEqual(_view.Text2, "Text2");
        }

        /// <summary>
        /// This provides code coverage for the integer converter case, where the target value becomes
        /// unconvertable to the source property type, such as clearing a text field bound to a numeric
        /// property.
        /// </summary>
        [Test]
        public void UnconvertableTargetProperty()
        {
            _view.Bind(_view, "Text", _viewModel, "Number");

            _viewModel.Number = 314;
            Assert.AreEqual("314", _view.Text);

            _view.Text = string.Empty;
            Assert.AreEqual(314, _viewModel.Number);
        }

        [Test]
        public void BindValueTypeProperties()
        {
            _view.Bind(_view, "Number", _viewModel, "Number");

            _viewModel.Number = 314;
            Assert.AreEqual(314, _view.Number);

            _view.Number = 278;
            Assert.AreEqual(278, _viewModel.Number);
        }

        /// <summary>
        /// Test that properties of a nested INotifyPropertyChanged object may be bound to.
        /// </summary>
        [Test]
        public void NestedNotifiable()
        {
            _view.Bind(_view, "Text", _viewModel, "Nested.Text");

            _viewModel.Nested.Text = "Nested";
            Assert.AreEqual("Nested", _view.Text);
        }
    }
}
