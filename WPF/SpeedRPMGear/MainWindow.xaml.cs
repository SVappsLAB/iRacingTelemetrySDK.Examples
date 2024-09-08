/**
 * Copyright (C)2024 Scott Velez
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.using Microsoft.CodeAnalysis;
**/
//using System.Windows.Shapes;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SpeedRPMGear
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.TelemetryViewModel();
        }

        private void ModeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (sender as RadioButton).Content.ToString();
            var viewModel = DataContext as ViewModels.TelemetryViewModel;

            viewModel.IsLiveSession = button == "Live";
        }

        private void IBTFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "telemetry files (.ibt)|*.ibt";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var viewModel = DataContext as ViewModels.TelemetryViewModel;
                viewModel.IBTFileName = dialog.FileName;
            }

        }

        private void Monitor_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            var viewModel = DataContext as ViewModels.TelemetryViewModel;

            viewModel.IsMonitoring = toggleButton.IsChecked ?? false;

        }
    }
}