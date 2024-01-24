using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_GUI_Pta0054;

public partial class BarabasiAlbertPage : Page
{
    public static double progress = 0.0f;
    private static ProgressBar progressBar;
    public static string progressActualProcedureName = "";
    private static TextBlock progressActualProcedure;
    private Graph g;

    public BarabasiAlbertPage()
    {
        InitializeComponent();
    }
    public static void UpdateProgressLabel(string value)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            progressActualProcedure.Text = value;
        });
    }
    public static void UpdateProgressBar(double value)
    {
        //Console.WriteLine($"Updating: {value}");
        // Ensure that the UI update happens on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            progressBar.Value = value;
        });
    }
    
    private async void AddProgressBar()
    {
        progress = 0.0f;
        if (progressBarsContainer.Children.Count <= 0)
        {
            progressBar = new ProgressBar
            {
                Width = 200,
                Height = 25,
                Maximum = 100
            };
            progressBarsContainer.Children.Add(progressBar);
        }

        if (progressActualProcedureContainer.Children.Count <= 0)
        {
            progressActualProcedure = new TextBlock
            {
                Style = (Style)FindResource("FancyLabelStyle"),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
            };
            progressActualProcedureContainer.Children.Add(progressActualProcedure);
        }
    }
    private int? ValidateInputM(string text, int low = 0, int high = 1_000_000)
    {
        if (int.TryParse(text, out int result))
            if (result >= low && result <= high)
                return result;
            else
                return null;
        else
            return null;
    }
    private void GenerateGraph_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        // Make not visible export option
        ExportContainer.Visibility = Visibility.Hidden;
        ExportContainer.IsEnabled = false;
        ExportResult.Text = "";
        OccupationCentralityContainer.Visibility = Visibility.Hidden;
        OccupationCentralityContainer.IsEnabled = false;
        OccupationCentralityResult.Text = "";
        DegreeCentralityContainer.Visibility = Visibility.Hidden;
        DegreeCentralityContainer.IsEnabled = false;
        DegreeCentralityResult.Text = "";

       // Validate and parse out inputs
       // N
       int? n = ValidateInputM(Input1TextBox.Text, low: 1, high: 1_000_000);
       if (n == null)
       {
           MessageBox.Show("Invalid input for N! Please enter a valid positive integer in the range <1, 1000000>.");
           return;
       }
       // M
       int? m = ValidateInputM(Input2TextBox.Text, low:1, high: 1_000_000);
       if (m == null)
       {
           MessageBox.Show("Invalid input for M! Please enter a valid positive integer in the range <1, 1000000>.");
           return;
       }
       // M
       int? d = ValidateInputM(Input3TextBox.Text, low:1, high: 1_000_000);
       if (d == null)
       {
           MessageBox.Show("Invalid input for D! Please enter a valid positive integer in the range <1, 1000000>.");
           return;
       }
       // Create progress bar
       AddProgressBar();
       
       // Generate random graph
       g = new Graph();
       g.ProgressChangedValue += UpdateProgressBar;
       g.ProgressChangedName += UpdateProgressLabel;
       
       
       Task.Run(() => g.CreateBarabasiAlbertModel((int)n, (int)m, (int)d)).ContinueWith(task =>
       {
           // Make visible export option after 
           ExportContainer.Visibility = Visibility.Visible;
           ExportContainer.IsEnabled = true;
           OccupationCentralityContainer.Visibility = Visibility.Visible;
           OccupationCentralityContainer.IsEnabled = true;
           DegreeCentralityContainer.Visibility = Visibility.Visible;
           DegreeCentralityContainer.IsEnabled = true;
       }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void ExportGraph_Click(object sender, RoutedEventArgs e)
    {
        // Export generated graph into csv
        var exportOption = myComboBox.Text;

        if (exportOption == "Adjacency List")
        {
            string adjacencyListPath = Path.Combine("..", "..", "..", "Outputs", "BA_adjacencylist.csv");
            g.ExportToAdjacencyList(adjacencyListPath);
            ExportResult.Text = $"Adjacency List was saved to {adjacencyListPath}";
            return;
        }
        if (exportOption == "Edge List")
        {
            string edgeListPath = Path.Combine("..", "..", "..", "Outputs", "BA_edgelist.csv");
            g.ExportToEdgeList(edgeListPath);
            ExportResult.Text = $"Edge List was saved to {edgeListPath}";
            return;
        }
        
    }

    private void CaclculateOccupationCentrality_Click(object sender, RoutedEventArgs e)
    {
        ExportContainer.IsEnabled = false;
        OccupationCentralityContainer.IsEnabled = false;
        DegreeCentralityContainer.IsEnabled = false;
        GenerateGraph.IsEnabled = false;
        
        // Validate and parse out inputs
        // Number of walks - n
        int? n = ValidateInputM(Input5TextBox.Text, low: 1, high: 1_000_000);
        if (n == null)
        {
            MessageBox.Show("Invalid input for n! Please enter a valid positive integer in the range <1, 1000000>.");
            return;
        }
        // Length of walk - k
        int? k = ValidateInputM(Input4TextBox.Text, low:1, high: 1_000_000);
        if (k == null)
        {
            MessageBox.Show("Invalid input for k! Please enter a valid positive integer in the range <1, 1000000>.");
            return;
        }
        
        Task.Run(() => g.GetAverageOccupationCentrality((int)k, (int)n)).ContinueWith(task =>
        {
            // Make visible export option after 
            ExportContainer.IsEnabled = true;
            OccupationCentralityContainer.IsEnabled = true;
            DegreeCentralityContainer.IsEnabled = true;
            GenerateGraph.IsEnabled = true;
            
            OccupationCentralityResult.Text = (Math.Round(task.Result, 3)).ToString();
            
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void CaclculateDegreeCentrality_Click(object sender, RoutedEventArgs e)
    {
        ExportContainer.IsEnabled = false;
        OccupationCentralityContainer.IsEnabled = false;
        DegreeCentralityContainer.IsEnabled = false;
        GenerateGraph.IsEnabled = false;
        
        Task.Run(() => g.GetAverageDegreeCentrality()).ContinueWith(task =>
        {
            // Make visible export option after 
            ExportContainer.IsEnabled = true;
            OccupationCentralityContainer.IsEnabled = true;
            DegreeCentralityContainer.IsEnabled = true;
            GenerateGraph.IsEnabled = true;
            
            DegreeCentralityResult.Text = (Math.Round(task.Result, 3)).ToString();
            
        }, TaskScheduler.FromCurrentSynchronizationContext());    }
}