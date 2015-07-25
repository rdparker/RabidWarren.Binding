// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyFieldTests.cs" company="Ron Parker">
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
    class PropertyFieldTests
    {
        PropertiedClass _view;
        PropertiedClass _viewModel;

        class PropertiedClass : BindingObject
        {
            public string Text;

            // Field '....IsWritable' is never assigned to, and will always have its default value 'null'
            // IsWritable is accessed by textual name in FieldCanWriteProperty below.
#pragma warning disable 0649
            public bool IsWritable;
#pragma warning restore 0649
        }

        [SetUp]
        public void Setup()
        {
            _view = new PropertiedClass();
            _viewModel = new PropertiedClass();
        }

        [Test]
        public void PropertyFields()
        {
            var exampleText = "Some Text";
            
            _viewModel.Text = exampleText;
            _view.Bind(_view, "Text", _viewModel, "Text");

            Assert.AreEqual(exampleText, _view.Text);
        }

        // Test the CanWrite pseudo-property.
        [Test]
        public void FieldCanWriteProperty()
        {
            _view.Bind(_view, "IsWritable", _viewModel, "Text.CanWrite");

            Assert.True(_view.IsWritable);
        }
    }
}
