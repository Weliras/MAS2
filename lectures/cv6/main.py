import igraph as ig
import matplotlib.pyplot as plt

# My functions
from functions import *


if __name__ == '__main__':
    filePath = r"aucs.mpx"

    g = load_from_file(filePath)

    plot_layers(g)
    plot_flattened(g)

    plot_communities_layers(g)
    plot_communities_flattened(g)
