// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionExceptions.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Tests exceptions related to property expressions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RabidWarren.Binding.Tests.Expressions
{
    using System;
    using NUnit.Framework;

    class ExpressionExceptions
    {
        View _view;
        ViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _view = new View();
            _viewModel = new ViewModel();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException),
            ExpectedMessage = "Unsupported method call 'Dummy' in expression")]
        public void UnsupportedMethodCall()
        {
            _view.Bind(v => v.Text, _viewModel, vm => Dummy());
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException),
            ExpectedMessage = "Unsupported expression type: 'Add'")]
        public void UnsupportedExpression()
        {
            _view.Bind(v => v.Text, _viewModel, vm => "Hi, " + vm.Text);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException),
            ExpectedMessage = "The CanWrite method should never be called directly.")]
        public void CannotCallCanWrite()
        {
            _view.Text.CanWrite();
        }

        static string Dummy()
        {
            return "Dummy";
        }
    }
}
