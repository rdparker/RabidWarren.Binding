namespace RabidWarren.Binding.Tests.Expressions
{
    using System.ComponentModel;
    using NUnit.Framework;

    /// <summary>
    /// Contains tests that do not pass a source object to the BindingObject constructor.
    /// </summary>
    public class SimpleExpressionTests : INotifyPropertyChanged
    {
        View _view;
        ViewModel _viewModel;

        // The event 'SimpleTests.PropertyChanged' is never used
        // Actually it is used by Bind, when CanRead binds to IsReadable.
#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

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
        /// Tests that a 4-argument Bind call returns without exception, when the source property is initialized.
        /// </summary>
        [Test]
        public void BindReturns4Arg()
        {
            _viewModel.Text = "Some Text";
            _view.Bind(_view, v => v.Text, _viewModel, vm => vm.Text);

            Assert.True(true);
        }

        /// <summary>
        /// Tests that a 4 argument Bind call returns without exception, when the source property is
        /// not initialized.
        /// </summary>
        [Test]
        public void BindReturns4ArgUninitialized()
        {
            _view.Bind(_view, v => v.Text, _viewModel, vm => vm.Text);

            Assert.True(true);
        }
        
        // Test the CanWrite pseudo-property.
        [Test]
        public void CanWrite()
        {
            _view.Bind(this, v => v.IsWritable, _viewModel, vm => vm.Text.CanWrite());

            Assert.True(IsWritable);
        }

        // Test the CanWrite pseudo-property returning false.
        [Test]
        public void CannotWrite()
        {
            _view.Bind(this, v => v.IsWritable, _viewModel, vm => vm.UnwritableText.CanWrite());

            Assert.False(IsWritable);
        }

        [Test]
        public void BindToNestedProperty()
        {
            _view.Bind(v => v.Outer.Inner, _viewModel, vm => vm.Text);
            _viewModel.Text = "Nested";
            Assert.AreEqual("Nested", _view.Outer.Inner);
        }

        /// <summary>
        /// Improve code coverage by testing binding a null property to a null nested property.
        /// </summary>
        [Test]
        public void BindNullToNullNestedProperty()
        {
            _view.Bind(this, o => o.Text, _viewModel, vm => vm.Outer.Inner);
        }

        /// <summary>
        /// This tests binding multiple properties within the same objects.  This also helps with code
        /// coverage in <see cref="M:BindingObject::Bind{TTarget,TSource}"/>.
        /// </summary>
        [Test]
        public void BindMultipleProperties()
        {
            _view.Bind(v => v.Text, _viewModel, vm => vm.Text);
            _view.Bind(v => v.Text2, _viewModel, vm => vm.Text2);

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
            _view.Bind(_view, v => v.Text, _viewModel, vm => vm.Number);

            _viewModel.Number = 314;
            Assert.AreEqual("314", _view.Text);

            _view.Text = string.Empty;
            Assert.AreEqual(314, _viewModel.Number);
        }

        [Test]
        public void BindValueTypeProperties()
        {
            _view.Bind(_view, v => v.Number, _viewModel, vm => vm.Number);

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
            _view.Bind(_view, v => v.Text, _viewModel, vm => vm.Nested.Text);

            _viewModel.Nested.Text = "Nested";
            Assert.AreEqual("Nested", _view.Text);
        }
    }
}
