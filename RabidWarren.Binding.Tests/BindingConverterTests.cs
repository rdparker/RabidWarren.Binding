namespace RabidWarren.Binding.Tests
{
    using System;
    using NUnit.Framework;
    using RabidWarren.Binding.Tests.ConverterAssembly;


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
#pragma warning disable 0618
                    OnPropertyChangedEvent("Number");
#pragma warning restore 0618
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
#pragma warning disable 0618
                    OnPropertyChangedEvent("Number");
#pragma warning restore 0618
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
        }

        /// <summary>
        /// There was a bug with a chicken and egg issue, where a call to
        /// <see cref="M:ConverterRegistry.RegisterAllConverters"/> would fail when invoked directly for an assembly.
        /// This is because the RegisterAllConverters method relied upon _registry having been constructed and by
        /// default it was only constructed when the Registry property was gotten, which also caused
        /// RegisterAllConverters to be invoked on the assembly that contains ConverterRegistry.
        /// </summary>
        [Test]
        public void ChickenAndEggBug()
        {
            ConverterRegistry.RegisterAllConverters(typeof(TestConverter).Assembly); 
        }

        [Test]
        public void ConvertToTarget()
        {
            _view.Bind(_view, "Number", _viewModel, "Number");
            Assert.AreEqual("314", _view.Number);
        }

        [Test]
        public void ConvertBackToSource()
        {
            _view.Bind(_view, "Number", _viewModel, "Number");
            _view.Number = "278";

            Assert.AreEqual(278, _viewModel.Number);
        }
    }
}
