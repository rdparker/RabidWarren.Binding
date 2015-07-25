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
        readonly NotifyingTestClass _testObject = new NotifyingTestClass();

        [Test]
        public void RaisePropertyChangedViaPropertyExpressionTest()
        {
            TestPropertyModification("RaisesByExpression", "Expression", () => _testObject.RaisesByExpression,
                v => { _testObject.RaisesByExpression = v; });
        }

        [Test]
        public void RaisePropertyChangedViaStringTest()
        {
            TestPropertyModification("RaisesByString", "String", () => _testObject.RaisesByString,
                v => { _testObject.RaisesByString = v; });
        }

        void TestPropertyModification(string propertyName, string newValue, Func<string> getter, Action<string> setter)
        {
            var raised = false;

            _testObject.PropertyChanged += (sender, args) =>
            {
                raised |= args.PropertyName == propertyName;
            };

            setter(newValue);

            Assert.True(raised);
            Assert.AreEqual(newValue, getter());
        }
    }
}