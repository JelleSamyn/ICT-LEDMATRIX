using System;
using System.IO.Ports;
using System.Windows;

namespace Project_ICT
{
    public partial class MainWindow : Window
    {
        SerialPort _serialPort;

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();

            // Zorg ervoor dat deze elementen uitgeschakeld beginnen.
            TextBoxMessage.IsEnabled = false;
            ButtonSend.IsEnabled = false;

        }

        private void LoadAvailablePorts()
        {
            _serialPort = new SerialPort();

            // Voeg standaardoptie toe
            ComboBoxPorts.Items.Add("None");

            // Voeg beschikbare poorten toe
            foreach (string s in SerialPort.GetPortNames())
            {
                ComboBoxPorts.Items.Add(s);
            }

            ComboBoxPorts.SelectedIndex = 0; // Standaard "None" selecteren
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    string selectedPort = ComboBoxPorts.SelectedItem?.ToString();

                    if (string.IsNullOrEmpty(selectedPort) || selectedPort == "None")
                    {
                        MessageBox.Show("Selecteer een geldige poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _serialPort = new SerialPort(selectedPort, 57600);
                    _serialPort.Open();

                    // Update UI
                    ButtonConnect.Content = "Disconnect";
                    EnableMessageControls(true);
                }
                else
                {
                    _serialPort.Close();

                    // Update UI
                    ButtonConnect.Content = "Connect";
                    EnableMessageControls(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij verbinden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    string message = TextBoxMessage.Text;
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        _serialPort.WriteLine(message);
                        MessageBox.Show("Bericht verzonden!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Bericht mag niet leeg zijn.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij verzenden: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(string.Empty); // Stuur een reset-commando
                    MessageBox.Show("Reset uitgevoerd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij resetten: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnableMessageControls(bool isEnabled)
        {
            TextBoxMessage.IsEnabled = isEnabled;
            ButtonSend.IsEnabled = isEnabled;
            ButtonReset.IsEnabled = isEnabled; // Voeg deze regel toe om de Reset-knop te beheren
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void TabControlModes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int selectedIndex = TabControlModes.SelectedIndex;

            switch (selectedIndex)
            {
                case 0: // Connectie-tab
                    ButtonConnect.IsEnabled = true;
                    EnableMessageControls(false); // Alle invoerknoppen uit
                    break;

                case 1: // Tekstverzender-tab
                    ButtonConnect.IsEnabled = false;
                    EnableMessageControls(_serialPort != null && _serialPort.IsOpen); // Controleer of verbinding actief is
                    break;

                case 2: // Lege tab
                    ButtonConnect.IsEnabled = false;
                    EnableMessageControls(false); // Alles uitschakelen
                    break;

                default:
                    break;
            }
        }

        private void CheckBoxEQControl_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("EQ_ON");
                    MessageBox.Show("Scroller ingeschakeld.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij inschakelen Scroller: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBoxEQControl_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("EQ_OFF");
                    MessageBox.Show("Scroller uitgeschakeld.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij uitschakelen Scroller: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Text scroll functionaliteit
        private void CheckBoxScrollText_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("START");
                    MessageBox.Show("Tekstscrollen ingeschakeld.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij inschakelen tekstscrollen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBoxScrollText_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("STOP");
                    MessageBox.Show("Tekstscrollen uitgeschakeld.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Verbind eerst met een poort.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij uitschakelen tekstscrollen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
