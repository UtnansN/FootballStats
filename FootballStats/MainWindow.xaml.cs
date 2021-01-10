using FootballStats.Entities;
using FootballStats.Exceptions;
using FootballStats.GridModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FootballStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StatsContext dbContext = new StatsContext();

        private EntityAssembler entityAssembler;

        public event EventHandler DatabaseChange;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Checks if DB created. If not, creates it.
            dbContext.Database.EnsureCreated();

            // Following section could technically be done with interfaces and methods that use them

            // Views for table
            TeamStandingsOverview teamOverview = new TeamStandingsOverview(dbContext);
            BestPlayersOverview bestPlayersOverview = new BestPlayersOverview(dbContext);
            TeamAveragesOverview teamAveragesOverview = new TeamAveragesOverview(dbContext);

            // Binds items to item source
            TeamGrid.ItemsSource = teamOverview.Stats;
            TopPlayerGrid.ItemsSource = bestPlayersOverview.Stats;
            TeamAveragesGrid.ItemsSource = teamAveragesOverview.Stats;

            // Add event handlers.
            DatabaseChange += teamOverview.HandleDatabaseUpdate;
            DatabaseChange += bestPlayersOverview.HandleDatabaseUpdate;
            DatabaseChange += teamAveragesOverview.HandleDatabaseUpdate;

            // Invoke initial data loading
            OnDatabaseChange(EventArgs.Empty);

            // Create an entity assembler to handle data input
            entityAssembler = new EntityAssembler(dbContext);
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "JSON files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddGameWindow addGameWindow = new AddGameWindow();
                addGameWindow.GameNamesBox.Items.Clear();
                addGameWindow.Show();

                bool existChanges = false;
                foreach (string name in openFileDialog.FileNames)
                {
                    using (StreamReader reader = File.OpenText(name))
                    {
                        JObject obj = (JObject)JToken.ReadFrom(new JsonTextReader(reader))["Spele"];
                        try
                        {
                            entityAssembler.AssembleAndAddEntry(obj);
                        }
                        catch (GamePlayedException)
                        {
                            addGameWindow.GameNamesBox.Items.Add(name + " NEPIEVIENOTS");
                            continue;
                        }
                    }

                    existChanges = true;
                    addGameWindow.GameNamesBox.Items.Add(name + " PIEVIENOTS");
                }
                if (existChanges)
                {
                    dbContext.SaveChanges();
                    OnDatabaseChange(EventArgs.Empty);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            dbContext.Dispose();
            base.OnClosing(e);
        }

        protected virtual void OnDatabaseChange(EventArgs e)
        {
            DatabaseChange?.Invoke(this, e);
        }
    }
}
