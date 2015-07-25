using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabidWarren.Binding;
using NUnit.Framework;
using System.Linq.Expressions;
using System.ComponentModel;

namespace RabidWarren.Binding.Tests.Expressions
{
    class ExpressionCompositionTests : NotifyingObject
    {
        ViewModel _viewModel;
        View _view;

        public bool IsWritable { get; set; }

        [SetUp]
        public void Setup()
        {
            _viewModel = new ViewModel();
            _view = new View();
        }

        [Test]
        public void BindComposition()
        {
            Expression<Func<View, View.OuterClass>> target = v => v.Outer;

            _view.Bind(target.Compose(o => o.Inner), _viewModel, vm => vm.Text);

            _viewModel.Text = "Nested";
            Assert.AreEqual("Nested", _view.Outer.Inner);
        }

        [Test]
        public void BindCompositionCall()
        {
            Expression<Func<ViewModel, string>> source = vm => vm.Text;

            _view.Bind(this, o => o.IsWritable, _viewModel, source.Compose(s => s.CanWrite()));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException),
            ExpectedMessage = "Unsupported expression type: 'Equal'")]
        public void BindCompositionException()
        {
            Expression<Func<ViewModel, string>> source = vm => vm.Text;

            _view.Bind(this, o => o.IsWritable, _viewModel, source.Compose(s => s.CanWrite() == true));
        }

        /// <summary>
        /// This test exposed a bug in the composition logic, where composing with a child specification of:
        /// 
        ///   x => x.y.z
        /// 
        /// failed
        /// </summary>
        [Test]
        public void ComplexChildCompositionBug()
        {
            Expression<Func<ViewModel, NestedNotifiable>> source = vm => vm.Nested;

            _view.Bind(
                this,
                o => o.IsWritable,
                _viewModel,
                source.Compose(s => s.Text.CanWrite()));
        }
    }
}
