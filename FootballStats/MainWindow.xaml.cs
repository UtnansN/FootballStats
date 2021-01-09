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

            // Views for table
            TeamOverview teamOverview = new TeamOverview(dbContext);
            BestPlayersOverview bestPlayersOverview = new BestPlayersOverview(dbContext);
            TeamAveragesOverview teamAveragesOverview = new TeamAveragesOverview(dbContext);

            // Adding event subscribers
            DatabaseChange += teamOverview.HandleDatabaseUpdate;
            DatabaseChange += bestPlayersOverview.HandleDatabaseUpdate;
            DatabaseChange += teamAveragesOverview.HandleDatabaseUpdate;

            // Loads entities into EF Core
            dbContext.Games.Load();
            dbContext.Teams.Load();

            // Binds items to item source
            TeamGrid.ItemsSource = teamOverview.Stats;
            TopPlayerGrid.ItemsSource = bestPlayersOverview.Stats;
            TeamAveragesGrid.ItemsSource = teamAveragesOverview.Stats;

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
                            addGameWindow.GameNamesBox.Items.Add(name + " NOT OK");
                            continue;
                        }
                    }

                    existChanges = true;
                    addGameWindow.GameNamesBox.Items.Add(name + " OK");
                }
                if (existChanges)
                {
                    dbContext.SaveChanges();
                    OnDatabaseChange(EventArgs.Empty);
                }
            }
        }

        private void UndoChanges()
        {
            foreach (EntityEntry entry in dbContext.ChangeTracker.Entries().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                    default: break;
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
