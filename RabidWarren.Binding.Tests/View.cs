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
                OnPropertyChangedEvent("Number");
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
                OnPropertyChangedEvent("Text");
            }
        }

        public string Text2 { get; set; }
    }
}
