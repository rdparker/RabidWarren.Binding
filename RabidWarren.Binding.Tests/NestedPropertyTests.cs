using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    using System.ComponentModel;
    using NUnit.Framework;

    class NestedPropertyTests : BindingObject
    {
        public string Text { get; set; }

        [Test]
        public void BasicNestedProperty()
        {
            var nested = new Nested();

            Bind(this, "Text", nested, "OuterProperty.Inner");
        }

        class Nested : INotifyingObject
        {
            public Outer OuterProperty { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public Nested()
            {
                OuterProperty = new Outer();
            }

            public void OnPropertyChangedEvent(string propertyName)
            {
                throw new NotImplementedException();
            }

            public class Outer
            {
                public string Inner;
            }
        }
    }
}
