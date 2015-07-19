// -----------------------------------------------------------------------
// <copyright file="NotifyingObjectTests.cs" company="Ron Parker">
//  Copyright 2015 Ron Parker
// </copyright>
// <summary>
//   Tests the NotifyingObject implementation.
// </summary>
// -----------------------------------------------------------------------

using System;
using NUnit.Framework;

namespace RabidWarren.Binding.Tests
{
    public class NotifyingObjectTests
    {
        private class ContainingObject : NotifyingObject
        {
            private string _name;

            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }

            public string Address
            {
                get { return _address; }
                set
                {
                    _address = value;
                    RaisePropertyChanged("Address");
                }
            }

            private string _address;
        }

        private readonly ContainingObject _testObject = new ContainingObject();

        [Test]
        public void RaisePropertyChangedViaPropertyExpressionTest()
        {
            TestPropertyModification("Name", "Ron", () => _testObject.Name, v => { _testObject.Name = v; });
        }

        [Test]
        public void RaisePropertyChangedViaStringTest()
        {
            TestPropertyModification("Address", "Somewhere", () => _testObject.Address,
                v => { _testObject.Address = v; });
        }

        private void TestPropertyModification(string propertyName, string newValue, Func<string> getter,
            Action<string> setter)
        {
            var raised = false;

            _testObject.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyName)
                    raised = true;
            };

            setter(newValue);

            Assert.True(raised);
            Assert.AreEqual(newValue, getter());
        }
    }
}