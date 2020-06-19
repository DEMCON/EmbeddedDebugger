using EmbeddedDebugger.Connectors.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace EmbeddedDebugger.View.UserControls.ObjectDisplayers
{
    /// <summary>
    /// Interaction logic for ConnectionSettingDisplayer.xaml
    /// </summary>
    public partial class ConnectionSettingDisplayerUserControl
    {
        public object Value;

        public ConnectionSettingDisplayerUserControl()
        {
            InitializeComponent();
        }

        private void ConnectionSettingDisplayerUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ConnectionSetting setting)
            {
                this.TheGrid.Children.Add(this.GetFrameworkElement(setting));
            }
        }

        private FrameworkElement GetFrameworkElement(ConnectionSetting setting)
        {
            FrameworkElement element;
            Binding binding = new Binding("Value") { NotifyOnSourceUpdated = true };

            // Check if the setting has possibilities
            if (setting.Possibilities == null)
            {
                switch (setting.Value)
                {
                    case bool _:
                        element = this.GenerateValueCheckBoxElement(setting.Value, binding);
                        break;
                    default:
                        element = this.GenerateValueTextBoxElement(setting.Value, binding);
                        break;
                }
            }
            else
            {
                element = this.GenerateValueComboBoxElement(setting.Value, binding, setting.Possibilities, setting.EnforcePossibilities);
            }
            element.Margin = new Thickness(3);
            Grid.SetRow(element, 0);
            Grid.SetColumn(element, 1);
            if (setting.PossibilitiesRefresher == null)
            {
                Grid.SetColumnSpan(element, 2);
            }
            else
            {
                this.RefreshButton.Visibility = Visibility.Visible;
                this.RefreshButton.Click += delegate { setting.PossibilitiesRefresher(); };
                Grid.SetColumnSpan(element, 1);
            }
            return element;
        }


        private FrameworkElement GenerateValueTextBoxElement(object value, BindingBase b)
        {
            this.Value = value;
            FrameworkElement returnable = new TextBox();
            returnable.SetBinding(TextBox.TextProperty, b);
            return returnable;
        }
        private FrameworkElement GenerateValueComboBoxElement(object value, BindingBase b, IEnumerable<object> possibilities, bool enforcePossibilities)
        {
            this.Value = value;
            FrameworkElement returnable = new ComboBox() { ItemsSource = possibilities, IsEditable = !enforcePossibilities };
            returnable.SetBinding(Selector.SelectedValueProperty, b);
            return returnable;
        }

        private FrameworkElement GenerateValueCheckBoxElement(object value, BindingBase b)
        {
            this.Value = value;
            FrameworkElement returnable = new CheckBox();
            returnable.SetBinding(Selector.SelectedValueProperty, b);
            return returnable;
        }
    }
}
