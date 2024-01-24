import pandas as pd
import seaborn as sns
import matplotlib
import os
import matplotlib.pyplot as plt
from matplotlib.gridspec import GridSpec


def plot_data(input_filepath: str, output_filepath: str):

    df = pd.read_csv(input_filepath, header=0, delimiter=";")
    df = df.sort_values(by="Year", ascending=True)

    fig = plt.figure(figsize=(8, 10))
    gs = GridSpec(nrows=1, ncols=1)
    # gs_in = matplotlib.gridspec.GridSpecFromSubplotSpec(2, 1, subplot_spec=gs[0], hspace=0, height_ratios=[1.2, 1.4])
    gs_in = matplotlib.gridspec.GridSpecFromSubplotSpec(4, 1, subplot_spec=gs[0], hspace=0,
                                                        height_ratios=[1.4, 1.4, 1.2, 1.0])

    ax1 = plt.Subplot(fig, gs_in[0], title=f"Analysis of observed metrics of network DBLP")
    bx = plt.Subplot(fig, gs_in[1], sharex=ax1)
    cx = plt.Subplot(fig, gs_in[2], sharex=bx)
    ex = plt.Subplot(fig, gs_in[3], sharex=cx)
    # dx = cx.twinx()

    # Count Of nodes
    x_values = df.Year.to_list()
    y_values = df.CountOfNodes.tolist()
    ax1.plot(x_values, y_values, label=f'Count Of Nodes', lw=1.00)
    # Count Of Edges
    y_values = df.CountOfEdges.tolist()
    ax1.plot(x_values, y_values, label=f'Count Of Edges', lw=1.00)  # linestyle='--'

    # Average Degree
    y_values = df.AverageDegree.tolist()
    bx.plot(x_values, y_values, label=f'Average Degree', lw=1.00)  # linestyle='--'
    # Average Weighted degree
    y_values = df.AverageWeightedDegree.tolist()
    bx.plot(x_values, y_values, label=f'Average Weighted Degree', lw=1.00)

    # Average Clustering Coeff
    y_values = df.AverageClusteringCoefficient.tolist()
    cx.plot(x_values, y_values, label=f'Average Clustering Coefficient', lw=1.00,)

    # Max Average weighted degree of simplex
    y_values = df.WDegreeOfSimplexWithMaxAvgWDegree.tolist()
    ex.plot(x_values, y_values, label=f'Simplex with max avg weighted degree', lw=1.00, )
    # Add labels to each point
    for i, (x, y) in enumerate(zip(x_values, y_values)):
        ex.annotate(f'{df.IdOfSimplexWithMaxAvgWDegree.iloc[i]}', (x, y),fontsize=6, rotation=90, textcoords="offset points", xytext=(0, 1), ha='center')

    fig.add_subplot(ax1)
    fig.add_subplot(bx)

    fig.add_subplot(cx)
    fig.add_subplot(ex)
    #dx = cx.twinx()

    # Mem BW
    #x_values = cpu_is.index.tolist()
    #y_values = cpu_is['MemBW'].tolist()
    #dx.plot(x_values, y_values, label=f'{IS}-Mem BW', linestyle='--', lw=1.00, color=color)

    # Freq
    ax1.set_xlabel("Years")
    ax1.set_ylabel("Count of nodes/edges")
    ax1.tick_params(axis='x', which='minor', labelsize=8)
    ax1.yaxis.set_major_formatter(matplotlib.ticker.FuncFormatter(lambda x, y: f"{int(x)}"))
    # ax1.grid(which = 'minor', alpha = 0.5, lw=0.2)
    ax1.grid(lw=0.2)

    bx.set_ylabel("Average Degree")  # Average power
    bx.grid(lw=0.2)

    cx.set_ylabel("Average Clustering Coefficient")  # Gflips
    cx.grid(lw=0.2)

    #dx.set_ylabel("Memory BW [GBps]")  # Mem BW

    ex.set_ylabel("Weighted Degree")  # Gflops
    ex.grid(lw=0.2)

    lines, labels = ax1.get_legend_handles_labels()
    lines2, labels2 = bx.get_legend_handles_labels()
    lines3, labels3 = cx.get_legend_handles_labels()
    #lines4, labels4 = dx.get_legend_handles_labels()
    lines5, labels5 = ex.get_legend_handles_labels()

    ax1.legend(loc="upper left")
    bx.legend(loc="upper left")

    # cx.legend(handles=[cp, dp])
    #cx.legend(lines3 + lines4, labels3 + labels4, loc="lower center", ncol=4, fancybox=True, framealpha=0.6,fontsize="smaller", markerscale=0.850)  # fontsize = "smaller", markerscale = 0.850
    cx.legend(loc="upper left")
    ex.legend(loc="upper left")

    fig.savefig(output_filepath, dpi=300, bbox_inches='tight')

    plt.plot()

    #plt.plot()


if __name__ == '__main__':
    input_filepaths = [r"C:\Users\ptaku\vsb_fei\MAS2\cv2\cv2\cv2\bin\Release\net7.0\outputs\outputsPerOneYears.csv",
                       r"C:\Users\ptaku\vsb_fei\MAS2\cv2\cv2\cv2\bin\Release\net7.0\outputs\outputsPerTenYears.csv"]
    output_filepaths = [r"C:\Users\ptaku\vsb_fei\MAS2\cv2\cv2\cv2\bin\Release\net7.0\outputs\outputsPerOneYears.pdf",
                        r"C:\Users\ptaku\vsb_fei\MAS2\cv2\cv2\cv2\bin\Release\net7.0\outputs\outputsPerTenYears.pdf"]

    for i in range(0, len(input_filepaths)):
        plot_data(input_filepaths[i], output_filepaths[i])
