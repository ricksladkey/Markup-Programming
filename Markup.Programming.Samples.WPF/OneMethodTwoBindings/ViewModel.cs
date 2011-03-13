using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Markup.Programming.Samples.OneMethodTwoBindings
{
    public class ViewModel
    {
        public ViewModel()
        {
            Boys = new ObservableCollection<string> { "Bill", "Mike", "Fred" };
            Girls = new ObservableCollection<string> { "Jill", "Ann", "Jane" };
        }

        public IEnumerable<string> Boys { get; set; }
        public IEnumerable<string> Girls { get; set; }

        public void Match(string boy, string girl)
        {
            // TODO: Put the real matching code here instead of this line.
            MessageBox.Show(string.Format("Match: {0} with {1}", boy, girl));
        }
    }
}
