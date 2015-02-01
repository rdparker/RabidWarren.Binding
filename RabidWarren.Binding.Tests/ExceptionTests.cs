namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;

    class ExceptionTests
    {
        class PropertiedClass : BindingObject
        {
            string _text;
            bool _boolean;

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

            public bool Boolean
            {
                get
                {
                    return _boolean;
                }

                set
                {
                    _boolean = value;
                    OnPropertyChangedEvent("Boolean");
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

        [Test]
        [ExpectedException(typeof(ArgumentException),
            ExpectedMessage = "A property can only be registered once.")]
        public void CannotRegisterPropertyTwice()
        {
            Property.Register(GetType(), "Constant", 3.14);
            Property.Register(GetType(), "Constant", 2.78);
        }

        /// <summary>
        /// Make sure that binding fails when there is no appropriate converter.
        /// </summary>
        /// TODO:  Add solution and file system Exceptions directories to RabidWarren.Binding.
        /// TODO:  Add a new exception type and use it in ConversionRegistry.Find.
        [Test]
        [ExpectedException(typeof(InvalidConversionException),
            ExpectedMessage = "No converter exists between System.String and System.Boolean.")]
        public void NoConverter()
        {
            _view.Bind(_view, "Boolean", _viewModel, "Text");
            _viewModel.Text = "True";
        }
    }
}
