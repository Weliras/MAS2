using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_GUI_Pta0054;

public partial class WattsStrogatzPage : Page
{
    public static double progress = 0.0f;
    private static ProgressBar progressBar;
    public static string progressActualProcedureName = "";
    private static TextBlock progressActualProcedure;
    private Graph g;
    public WattsStrogatzPage()
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
    private void GenerateGraph_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        // Make not visible export option
        ExportContainer.Visibility = Visibility.Hidden;
        ExportContainer.IsEnabled = false;
        ExportResult.Text = "";

       // Validate and parse out inputs
       // P
       double? p = ValidateInputP(Input3TextBox.Text);
       if (p == null)
       {
           MessageBox.Show("Invalid input for P! Please enter a valid double in the range <0.0, 1.0>.");
           return;
       }
       // K
       int? k = ValidateInputK(Input2TextBox.Text, low: 2, high: 1_000);
       if (k == null)
       {
           MessageBox.Show("Invalid input for K! Please enter a valid even positive integer in the range <2, 1000>.");
           return;
       }
       // N
       int? n = ValidateInputN(Input1TextBox.Text, high: 1_000_000);
       if (n == null)
       {
           MessageBox.Show("Invalid input for N! Please enter a valid positive integer in the range <0, 1000000>.");
           return;
       }
       // Create progress bar
       AddProgressBar();
       
       // Generate random graph
       g = new Graph();
       g.ProgressChangedValue += UpdateProgressBar;
       g.ProgressChangedName += UpdateProgressLabel;
       
       
       Task.Run(() => g.CreateWattsStrogatzModel((int)n, (int)k, (double)p)).ContinueWith(task =>
       {
           // Make visible export option after 
           ExportContainer.Visibility = Visibility.Visible;
           ExportContainer.IsEnabled = true;
       }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private double? ValidateInputP(string text, double low = 0.0f, double high = 1.0f)
    {
        if (double.TryParse(text, out double result))
            if (result >= low && result <= high)
                return result;
            else
                return null;
        else
            return null;
    }

    private int? ValidateInputN(string text, int low = 0, int high = 1_000_000)
    {
        if (int.TryParse(text, out int result))
            if (result >= low && result <= high)
                return result;
            else
                return null;
        else
            return null;
    }
    private int? ValidateInputK(string text, int low = 0, int high = 1_000_000)
    {
        if (int.TryParse(text, out int result))
            if (result >= low && result <= high && result % 2 == 0)
                return result;
            else
                return null;
        else
            return null;
    }

    private void ExportGraph_Click(object sender, RoutedEventArgs e)
    {
        // Export generated graph into csv
        var exportOption = myComboBox.Text;

        if (exportOption == "Adjacency List")
        {
            string adjacencyListPath = Path.Combine("..", "..", "..", "Outputs", "WS_adjacencylist.csv");
            g.ExportToAdjacencyList(adjacencyListPath);
            ExportResult.Text = $"Adjacency List was saved to {adjacencyListPath}";
            return;
        }
        if (exportOption == "Edge List")
        {
            string edgeListPath = Path.Combine("..", "..", "..", "Outputs", "WS_edgelist.csv");
            g.ExportToEdgeList(edgeListPath);
            ExportResult.Text = $"Edge List was saved to {edgeListPath}";
            return;
        }
        
    }
}