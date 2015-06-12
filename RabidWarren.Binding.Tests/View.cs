using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    class View : BindingObject
    {
        int _number;
        string _text;
        readonly OuterClass _outer = new OuterClass();

        public View()
        {
        }

        public View(NotifyingObject sourceObject) : base(sourceObject)
        {
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

        public string Text2 { get; set; }

        public OuterClass Outer
        {
            get
            {
                return _outer;
            }
        }

        public class OuterClass
        {
            public string Inner;
        }
    }
}
