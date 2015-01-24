using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    class ViewModel : NotifyingObject
    {
        string _text;

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

        public string UnwritableText
        {
            get { return "Some Text"; }
        }

        public string UnreadableText
        {
            private get { return "Some Text"; }

            set { OnPropertyChangedEvent("UnreadableText"); }
        }
    }
}
