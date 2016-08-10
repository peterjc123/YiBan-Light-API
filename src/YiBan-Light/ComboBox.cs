using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YiBan_Light_Lib
{
    public class ComboBox
    {
        public string Text { get; set; } = "";
        public List<string> Items { get; set; } = new List<string>();

        public event EventHandler SelectedIndexChanged;

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                Text = Items[_selectedIndex];
                SelectedIndexChanged?.Invoke(null, null);
            }
        }

        private int _selectedIndex = -1;
    }
}
