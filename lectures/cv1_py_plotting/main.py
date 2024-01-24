import pandas as pd
import seaborn as sns
import matplotlib
import os
import matplotlib.pyplot as plt

def plot_clustering_effect(filepath_input: str, filepath_output: str):
    df = pd.read_csv(filepath_input, header=0, delimiter=";")
    # Scatter Plot
    plt.figure(figsize=(8, 6))
    sns.scatterplot(data=df, x="Degree", y="AvgClusteringCoef", )
    plt.title("Clustering Effect (Log-Log scale)")
    plt.xscale("log")
    plt.yscale("log")
    plt.grid(True)
    plt.savefig(filepath_output, format="pdf")
    plt.close()

def plot_degree_distribution(filepath_input: str, filepath_output: str):
    df = pd.read_csv(filepath_input, header=0, delimiter=";")

    # Scatter Plot
    plt.figure(figsize=(8, 6))
    sns.scatterplot(data=df, x="Degree", y="Count",)
    plt.title("Degree distribution (Log-Log scale)")
    plt.xscale("log")
    plt.yscale("log")
    plt.grid(True)
    plt.savefig(filepath_output, format="pdf")
    plt.close()

# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    plot_degree_distribution(r"C:\Users\ptaku\vsb_fei\MAS2\cv1\cv1\bin\Debug\net7.0\output\degreeDistribution.csv", r"C:\Users\ptaku\vsb_fei\MAS2\cv1\cv1\bin\Debug\net7.0\output\degreeDistribution1.pdf")
    plot_clustering_effect(r"C:\Users\ptaku\vsb_fei\MAS2\cv1\cv1\bin\Debug\net7.0\output\clusteringEffect.csv", r"C:\Users\ptaku\vsb_fei\MAS2\cv1\cv1\bin\Debug\net7.0\output\clusteringEffect1.pdf")
# See PyCharm help at https://www.jetbrains.com/help/pycharm/
