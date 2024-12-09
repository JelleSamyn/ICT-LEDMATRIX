using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace Project_ICT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailablePorts();
        }

        private void LoadAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ComboBoxPorts.ItemsSource = ports;

            if (ports.Length > 0)
                ComboBoxPorts.SelectedIndex = 0;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    string selectedPort = ComboBoxPorts.SelectedItem.ToString();
                    _serialPort = new SerialPort(selectedPort, 57600);
                    _serialPort.Open();
                    ButtonConnect.Content = "Disconnect";
                    TextBoxMessage.IsEnabled = true;
                    ButtonSend.IsEnabled = true;
                }
                else
                {
                    _serialPort.Close();
                    ButtonConnect.Content = "Connect";
                    TextBoxMessage.IsEnabled = false;
                    ButtonSend.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    string message = TextBoxMessage.Text;
                    _serialPort.WriteLine(message);
                }
                else
                {
                    MessageBox.Show("Please connect to a port first.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine(string.Empty);
                }
                else
                {
                    MessageBox.Show("Please connect to a port first.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}