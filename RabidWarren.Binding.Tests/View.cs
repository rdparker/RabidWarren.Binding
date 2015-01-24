using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabidWarren.Binding.Tests
{
    class View : BindingObject
    {
        public View()
        {
        }

        public View(NotifyingObject sourceObject) : base(sourceObject)
        {
        }

        public string Text { get; set; }
    }
}
