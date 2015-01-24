namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;

    class ExceptionTests
    {
        class PropertiedClass : BindingObject
        {
            string _text;

            public string Text
            {
                get
                {
                    return _text;
                }

                set
                {
                    _text = value;
                    OnPropertyChangedEvent("Text");
                }
            }
        }

        PropertiedClass _view;
        PropertiedClass _viewModel;

        [SetUp]
        public void Setup()
        {
            _view = new PropertiedClass();
            _viewModel = new PropertiedClass();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),
            ExpectedMessage = "Cannot bind to an unregistered property.\r\nParameter name: targetProperty")]
        public void InvalidTargetProperty()
        {
            _view.Bind(_view, "BadProperty", _viewModel, "Text");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),
            ExpectedMessage = "Cannot bind to an unregistered property.\r\nParameter name: sourceProperty")]
        public void InvalidSourceProperty()
        {
            _view.Bind(_view, "Text", _viewModel, "BadProperty");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),
            ExpectedMessage = "A target property can only be bound once.\r\nParameter name: targetProperty")]
        public void CannotBindTargetTwice()
        {
            _viewModel.Text = "Not null";

            _view.Bind(_view, "Text", _viewModel, "Text");
            _view.Bind(_view, "Text", _viewModel, "Text");
        }
    }
}
