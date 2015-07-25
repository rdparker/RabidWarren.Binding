// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceObjectTests.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Contains tests that pass a source object to the BindingObject constructor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RabidWarren.Binding.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// Contains tests that pass a source object to the BindingObject constructor.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class SourceObjectTests
    {
        View _view;
        ViewModel _viewModel;

        public string Text { get; set; }

        [SetUp]
        public void Setup()
        {
            _viewModel = new ViewModel();
            _view = new View(_viewModel);
        }

        /// <summary>
        /// Tests that a three argument Bind call returns without exception, when the source property is initialized.
        /// </summary>
        [Test]
        public void BindReturns3Arg()
        {
            _viewModel.Text = "Some Text";
            _view.Bind("Text", "Text");

            Assert.True(true);
        }

        /// <summary>
        /// Tests that a three argument Bind call returns without exception, when the source property is not initialized.
        /// </summary>
        [Test]
        public void BindReturns3ArgUninitialized()
        {
            _view.Bind(_view, "Text", "Text");

            Assert.True(true);
        }

        /// <summary>
        /// Tests that a three argument Bind call returns without exception, when the target property is initialized.
        /// </summary>
        [Test]
        public void BindReturns3ArgInitialized()
        {
            _view.Text = "Original Value";
            _view.Bind(_view, "Text", "Text");
        }
    }
}
