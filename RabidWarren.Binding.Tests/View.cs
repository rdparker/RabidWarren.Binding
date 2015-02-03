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

        public string Text { get; set; }
        public string Text2 { get; set; }
    }
}
