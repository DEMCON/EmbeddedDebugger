using EmbeddedDebugger.Connectors.Settings;
using System.Collections.Generic;
using System.Windows;

namespace EmbeddedDebugger.View.Windows
{
    /// <summary>
    /// Interaction logic for ConnectionSettingsWindow.xaml
    /// </summary>
    public partial class ConnectionSettingsWindow : Window
    {
        public List<ConnectionSetting> ConnectionSettings
        {
            get => this.RegistersStackPanel.ItemsSource as List<ConnectionSetting>;
            set => this.RegistersStackPanel.ItemsSource = value;
        }

        public ConnectionSettingsWindow()
        {
            InitializeComponent();
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
