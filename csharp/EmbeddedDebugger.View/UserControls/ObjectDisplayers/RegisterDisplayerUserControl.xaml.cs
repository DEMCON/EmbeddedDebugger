using EmbeddedDebugger.DebugProtocol.RegisterValues;
using EmbeddedDebugger.Model;
using EmbeddedDebugger.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EmbeddedDebugger.View.UserControls.ObjectDisplayers
{
    /// <summary>
    /// Interaction logic for RegisterDisplayerUserControl.xaml
    /// </summary>
    public partial class RegisterDisplayerUserControl : UserControl
    {
        private SystemViewModel systemViewModel;
        private RegisterValue currentValue;

        public static readonly DependencyProperty RegisterProperty = DependencyProperty.Register(
            "Register",
            typeof(Register),
            typeof(RegisterDisplayerUserControl),
            new FrameworkPropertyMetadata(null)
            );
        public static readonly DependencyProperty CollapsedProperty = DependencyProperty.Register(
            "Collapsed",
            typeof(bool),
            typeof(RegisterDisplayerUserControl),
            new FrameworkPropertyMetadata(null)
            );

        public bool Collapsed
        {
            get => (bool)this.GetValue(CollapsedProperty);
            set => this.SetValue(CollapsedProperty, value);
        }

        public Register Register
        {
            get => (Register)this.GetValue(RegisterProperty);
            set => this.SetValue(RegisterProperty, value);
        }

        public RegisterDisplayerUserControl()
        {
            this.InitializeComponent();
        }

        private void PlotCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PlotCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.systemViewModel.RequestNewValue(this.Register);
        }

        private void CollapseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Collapsed = !this.Collapsed;
            if (this.Register.HasChildren && this.ChildrenStackPanel.Children.Count == 0)
            {
                foreach (Register r in this.Register.ChildRegisters)
                {
                    this.ChildrenStackPanel.Children.Add(new RegisterDisplayerUserControl() { Register = r });
                }
            }
        }

        public void Refresh()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.Register != null && this.Register.RegisterValue != this.currentValue)
                {
                    this.currentValue = this.Register.RegisterValue;
                    this.ValueTextBox.Text = this.Register.Value;
                }
            });
        }

        public void Update(object o, EventArgs e)
        {
            this.Refresh();
        }

        private void RegisterDisplayerUserControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModelManager vmmOld)
            {
                vmmOld.RefreshLow -= this.Update;
            }
            if (e.NewValue is ViewModelManager vmm)
            {
                this.systemViewModel = vmm.SystemViewModel;
                vmm.RefreshLow += this.Update;
            }
        }

    }
}
