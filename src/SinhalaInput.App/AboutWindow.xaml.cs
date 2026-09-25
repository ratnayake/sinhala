using System.Windows;

namespace SinhalaInput.App;

public partial class AboutWindow : Window
{
    public AboutWindow()
        : this(AboutInfo.Current)
    {
    }

    public AboutWindow(AboutInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        InitializeComponent();
        DataContext = info;

        System.Windows.Media.ImageSource? icon = AppIcon.CreateWindowIcon();
        Icon = icon;
        LogoImage.Source = icon;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => Close();
}
