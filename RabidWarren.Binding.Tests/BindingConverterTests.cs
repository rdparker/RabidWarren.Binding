namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;
    using System;


    /// <summary>
    /// Tests BindingConverter logic
    /// </summary>
    /// 
    class BindingConverterTests
    {
        class ViewModel : NotifyingObject
        {
            int _number;

            public int Number
            {
                get { return _number; }

                set
                {
                    _number = value;
                    OnPropertyChangedEvent("Number");
                }
            }
        }

        class View : BindingObject
        {
            string _number;

            public string Number
            {
                get
                {
                    return _number;
                }

                set
                {
                    _number = value;
                    OnPropertyChangedEvent("Number");
                }
            }
        }

        View _view;
        ViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _view = new View();
            _viewModel = new ViewModel { Number = 314 };

            _view.Bind(_view, "Number", _viewModel, "Number");
        }

        [Test]
        public void ConvertToTarget()
        {
            Assert.AreEqual("314", _view.Number);
        }

        [Test]
        public void ConvertBackToSource()
        {
            _view.Number = "278";

            Assert.AreEqual(278, _viewModel.Number);
        }
    }
}
