import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

### US PATENTS
def plot_nodes_edges(file_path:str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y1 = df["Nodes"].tolist()
    y2 = df["Edges"].tolist()
    ax.plot(x, y1, label='Count of nodes.')
    ax.plot(x, y2, label="Count of edges.")
    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Count of nodes and edges')
    ax.set_title('Count of nodes/edges of temporal network.')
    # set scale
    ax.set_yscale("log")
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

def plot_average_cc(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y = df["AverageClusteringCoefficient"].tolist()
    ax.plot(x, y, label='Average Clustering Coefficient')
    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Average Clustering Coefficient')
    ax.set_title('Average Clustering Coefficient of temporal network.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

def plot_most_cited_patents(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y = df["NumberOfCitations"].tolist()
    #labels = df["MostCitedPatentId"].tolist()

    ax.plot(x, y, label='Most cited patents.')
    # Add labels to each point
    for i, (x, y) in enumerate(zip(x, y)):
        ax.annotate(f'{df["MostCitedPatentId"].iloc[i]}', (x, y), fontsize=6, rotation=90,
                    textcoords="offset points", xytext=(0, 1), ha='center')

    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Number of citations.')
    ax.set_title('Most cited patents in each year.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

def plot_average_degree(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y = df["AverageOutDegree"].tolist()
    ax.plot(x, y, label='Average In/Out degree.')
    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Average In/Out Degree')
    ax.set_title('Average IN/OUT degree of temporal network.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

# Yahoo
def plot_average_degree_w_degree(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y1 = df["AverageDegree"].tolist()
    y2 = df["AverageWeightedDegree"].tolist()
    ax.plot(x, y1, label='Average degree.')
    ax.plot(x, y2, label="Average weighted degree.")
    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Average Degree')
    ax.set_title('Average degree of temporal network.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

def plot_nodes_with_max_degree(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y = df["Degree"].tolist()
    # labels = df["MostCitedPatentId"].tolist()

    ax.plot(x, y, label='Nodes with biggest degree.')
    # Add labels to each point
    for i, (x, y) in enumerate(zip(x, y)):
        ax.annotate(f'{df["NodeWithMostDegree"].iloc[i]}', (x, y), fontsize=6, rotation=90,
                    textcoords="offset points", xytext=(0, 1), ha='center')

    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Degree')
    ax.set_title('Nodes with biggest degree in each year.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()
def plot_density(file_path: str):
    # Load data
    df = pd.read_csv(file_path, sep=';')
    # Sort by year
    df = df.sort_values(by="Year")
    fig, ax = plt.subplots()
    # Plot the data
    x = df["Year"].tolist()
    y = df["Density"].tolist()
    ax.plot(x, y, label='Density of network.')
    # Set labels and title
    ax.set_xlabel('Year')
    ax.set_ylabel('Density')
    ax.set_title('Density of temporal network.')
    # Add grid
    ax.grid(lw=0.2)
    # Add a legend
    ax.legend()
    # Save the plot to a PDF file
    plt.savefig(file_path.replace(".csv", ".pdf"))
    # close plt
    plt.close()

def plot_boxplots(input_filepath: str):
    df = pd.read_csv(input_filepath, sep=';')
    #df = df.loc[(df.k == k)]
    #df['combined'] = "k=" + df['k'].astype(str) + ', ' + "c=" + df['c'].astype(str)

    # Box plot
    plt.figure(figsize=(8, 6))
    sns.boxplot(data=df, x="MaxCandidates", y="Ratio", hue="MaxCandidates")
    plt.title("Ratio of \"infected\" portion of nodes.")
    plt.grid(True)
    # Save the plot to a PDF file
    plt.savefig(input_filepath.replace(".csv", ".pdf"))
    plt.close()


if __name__ == '__main__':
    # US Patents.
    plot_average_degree(r"C:\Users\ptaku\vsb_fei\MAS2\Project_USPatents\Project_USPatents\bin\Data\Outputs\averageOutDegrees.csv")
    plot_nodes_edges(r"C:\Users\ptaku\vsb_fei\MAS2\Project_USPatents\Project_USPatents\bin\Data\Outputs\nodesAndEdges.csv")
    plot_average_cc(r"C:\Users\ptaku\vsb_fei\MAS2\Project_USPatents\Project_USPatents\bin\Data\Outputs\averageClusteringCoefficient.csv")
    plot_most_cited_patents(r"C:\Users\ptaku\vsb_fei\MAS2\Project_USPatents\Project_USPatents\bin\Data\Outputs\mostCitedPatents_old.csv")

    # Yahooo.
    plot_average_degree_w_degree(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\averageOutDegrees.csv")
    plot_nodes_edges(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\nodesAndEdges.csv")
    plot_average_cc(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\averageClusteringCoefficient.csv")
    plot_nodes_with_max_degree(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\maxOutDegrees.csv")
    plot_density(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\density.csv")
    plot_boxplots(r"C:\Users\ptaku\vsb_fei\MAS2\Project_Yahoo\Project_Yahoo\bin\Data\Outputs\influence.csv")
