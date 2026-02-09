using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace ExamSettings
{
    public partial class MainWindow : Window
    {
        private readonly Pages.Home _pageHome;
        private readonly Pages.GamesPage _pageGames;
        private readonly Pages.ExamEditer _pageApps;
        private readonly Pages.Settings _pageSettings;
        private readonly Pages.About _pageAbout;

        public MainWindow()
        {
            InitializeComponent();

            // Pre-create page instances to improve navigation performance
            _pageHome = new Pages.Home();
            _pageGames = new Pages.GamesPage();
            _pageApps = new Pages.ExamEditer();
            _pageSettings = new Pages.Settings();
            _pageAbout = new Pages.About();
        }

        private void NavigationView_Root_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = sender.SelectedItem;
            Page? page = null;

            if (item == NavigationViewItem_Home)
            {
                page = _pageHome;
            }
            else if (item == NavigationViewItem_Games)
            {
                page = _pageGames;
            }
            else if (item == NavigationViewItem_ExamEditer)
            {
                page = _pageApps;
            }
            else if (item == NavigationViewItem_Settings)
            {
                page = _pageSettings;
            }
            else if (item == NavigationViewItem_About)
            {
                page = _pageAbout;
            }

            if (page != null)
            {
                NavigationView_Root.Header = (item as NavigationViewItem)?.Content;

                // Avoid re-navigating to the same page instance to reduce overhead
                if (Frame_Main.Content != page)
                {
                    Frame_Main.Navigate(page);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView_Root.SelectedItem = NavigationViewItem_Home;
        }
    }
}