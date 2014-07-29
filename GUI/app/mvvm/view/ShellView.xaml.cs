using gui.app.mvvm.viewmodel;

namespace gui.app.mvvm.view
{
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}