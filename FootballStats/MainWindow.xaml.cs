using FootballStats.Exceptions;
using FootballStats.GridModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

            // Following section could probably be done with interfaces and methods that use them
            // But for sake of time I chose to do this part manually.

            // Views for table
            TeamStandingsOverview teamOverview = new TeamStandingsOverview(dbContext);
            BestPlayersOverview bestPlayersOverview = new BestPlayersOverview(dbContext);
            TeamAveragesOverview teamAveragesOverview = new TeamAveragesOverview(dbContext);
            GameOverview gameOverview = new GameOverview(dbContext);
            BestAttackerOverview attackerOverview = new BestAttackerOverview(dbContext);

            // Binds items to item source
            TeamGrid.ItemsSource = teamOverview.Stats;
            TopPlayerGrid.ItemsSource = bestPlayersOverview.Stats;
            TeamAveragesGrid.ItemsSource = teamAveragesOverview.Stats;
            PopularGameGrid.ItemsSource = gameOverview.Stats;
            BestAttackerGrid.ItemsSource = attackerOverview.Stats;

            // Add event handlers.
            DatabaseChange += teamOverview.HandleDatabaseUpdate;
            DatabaseChange += bestPlayersOverview.HandleDatabaseUpdate;
            DatabaseChange += teamAveragesOverview.HandleDatabaseUpdate;
            DatabaseChange += gameOverview.HandleDatabaseUpdate;
            DatabaseChange += attackerOverview.HandleDatabaseUpdate;

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
                addGameWindow.Show();

                bool existChanges = false;

                foreach (string name in openFileDialog.FileNames)
                {
                    using (StreamReader reader = new StreamReader(name, Encoding.UTF8))
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            dbContext.DeleteAllData();
            DatabaseChange?.Invoke(this, e);
        }
    }
}
