using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

namespace FootballStats
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {

        public AddGameWindow()
        {
            InitializeComponent();
        }

        private void ChooseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach(string name in openFileDialog.FileNames)
                {
                    JObject obj;
                    using (StreamReader reader = File.OpenText(name))
                    {
                        obj = (JObject) JToken.ReadFrom(new JsonTextReader(reader))["Spele"];
                    }
                    GameNamesBox.Items.Add(name);
                }
            }

        }
    }
}
