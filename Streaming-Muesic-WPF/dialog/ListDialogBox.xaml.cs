using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Streaming_Muesic_WPF
{
    /// <summary>
    /// Interaction logic for ListDialogBox.xaml
    /// </summary>
    public partial class ListDialogBox : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //Local copies of all the properties. (with default values)
        private string prompt = "Select a bridge from the list.";
        private string selectText = "Select";
        private ObservableCollection<BridgeItem> items;
        private BridgeItem selectedItem = null;
        
        public ListDialogBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        /* Handles when an item is double-clicked.
     * The ListDialogBox.SelectedItem property is set and the dialog is closed.
     */
        private void LstItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedItem = ((FrameworkElement)sender).DataContext as BridgeItem;
            Close();
        }

        /* Handles when the confirm selection button is pressed.
     * The ListDialogBox.SelectedItem property is set and the dialog is closed.
     */
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = LstItems.SelectedItem as BridgeItem;
            Close();
        }

        /* Handles when the cancel button is pressed.
     * The lsitDialogBox.SelectedItem remains null, and the dialog is closed.
     */
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /* Handles when any key is pressed.  Here we determine when the user presses
     * the ESC key.  If that happens, the result is the same as cancelling.
     */
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {   //If the user presses escape, close this window.
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        //Below are the customizable properties.

        /* This property specifies the prompt that displays at the top of the dialog. */
        public string Prompt
        {
            get { return prompt; }
            set
            {
                if (prompt != value)
                {
                    prompt = value;
                    RaisePropertyChanged("Prompt");
                }
            }
        }

        /* This property specifies the text on the confirm selection button. */
        public string SelectText
        {
            get { return selectText; }
            set
            {
                if (selectText != value)
                {
                    selectText = value;
                    RaisePropertyChanged("SelectText");
                }
            }
        }

        /* This property specifies the collection of items that the user can select from.
     * Note that this uses the INamedItem interface.  The caller must comply with that
     * interface in order to use the ListDialogBox.
     */
        public ObservableCollection<BridgeItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        //Below are the read only properties that the caller uses after
        //prompting for a selection.

        /* This property contains either the selected INamedItem, or null if
         * no selection is made.
         */
        public BridgeItem SelectedItem
        {
            get { return selectedItem; }
            private set
            {
                selectedItem = value;
            }
        }

        /* This property indicates if a selection was made.
     * The caller should check this property before trying to use the selected item.
     */
        public bool IsCancelled
        {   //A simple null-check is performed (the caller can do this too).
            get { return (SelectedItem == null); }
        }
    }
}
