using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    class ViewModel : NotifyingObject
    {
        int _number;
        string _text;
        string _text2;
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
                OnPropertyChangedEvent("Text");
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
                OnPropertyChangedEvent("Text2");
            }
        }

        public string UnwritableText
        {
            get { return "Some Text"; }
        }

        public string UnreadableText
        {
            private get { return "Some Text"; }

            set { OnPropertyChangedEvent("UnreadableText"); }
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
                OnPropertyChangedEvent("Number");
            }
        }

        public class OuterClass
        {
            public string Inner;
        }
    }
}
