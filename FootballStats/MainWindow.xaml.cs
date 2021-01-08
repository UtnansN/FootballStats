using FootballStats.Entities;
using FootballStats.Exceptions;
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
        private CollectionViewSource gameViewSource;

        public MainWindow()
        {
            InitializeComponent();
            gameViewSource = (CollectionViewSource)FindResource(nameof(gameViewSource));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Checks if DB created. If not, creates it.
            dbContext.Database.EnsureCreated();

            // Loads entities into EF Core
            dbContext.Games.Load();

            // Binds to source
            gameViewSource.Source = dbContext.Games.Local.ToObservableCollection();

            // Create an entity assembler to handle data input
            entityAssembler = new EntityAssembler(dbContext);
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JSON files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                AddGameWindow addGameWindow = new AddGameWindow();
                addGameWindow.GameNamesBox.Items.Clear();
                addGameWindow.Show();
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
                            UndoChanges();
                            continue;
                        }
                    }
                    addGameWindow.GameNamesBox.Items.Add(name);
                    dbContext.SaveChanges();
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

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            dbContext.Dispose();
            base.OnClosing(e);
        }
    }
}
