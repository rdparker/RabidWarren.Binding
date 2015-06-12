using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    class ViewModel : NotifyingObject
    {
        int _number;
        string _text;
        string _text2;
        readonly NestedNotifiable _nested = new NestedNotifiable();
        readonly OuterClass _outer = new OuterClass();

        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
#pragma warning disable 0618
                OnPropertyChangedEvent("Text");
#pragma warning restore 0618
            }
        }

        public string Text2
        {
            get
            {
                return _text2;
            }

            set
            {
                _text2 = value;
#pragma warning disable 0618
                OnPropertyChangedEvent("Text2");
#pragma warning restore 0618
            }
        }

        public string UnwritableText
        {
            get { return "Some Text"; }
        }

        public string UnreadableText
        {
            private get { return "Some Text"; }

#pragma warning disable 0618
            set { OnPropertyChangedEvent("UnreadableText"); }
#pragma warning restore 0618
        }

        public OuterClass Outer
        {
            get
            {
                return _outer;
            }
        }

        public int Number
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

        public NestedNotifiable Nested
        {
            get
            {
                return _nested;
            }
        }

        public class OuterClass
        {
            public string Inner;
        }
    }
}
