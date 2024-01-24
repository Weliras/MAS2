import igraph as ig
import random

import matplotlib.pyplot as plt

def load_from_file(filePath: str):
    actual_header = None
    actor_attributes = []

    g = ig.Graph(directed=False)

    # Open file
    with open(filePath, "r") as file:

        for line in file:
            processed_line = line.rstrip("\n")
            if len(processed_line) <= 0:
                continue

            # headers
            if processed_line.startswith("#"):
                processed_line = processed_line.lstrip("#")
                actual_header = processed_line
            # data
            else:
                if actual_header == "LAYERS":
                    continue
                elif actual_header == "ACTOR ATTRIBUTES":
                    actor_attributes.append(processed_line.split(",")[0])
                elif actual_header == "ACTORS":
                    v = g.add_vertex(name=processed_line.split(",")[0])
                    for i, attribute in enumerate(actor_attributes):
                        v[attribute] = processed_line.split(",")[1:][i]
                elif actual_header == "NODES":
                    continue
                elif actual_header == "EDGES":
                    splitted_line = processed_line.split(",")
                    actorFrom = splitted_line[0]
                    actorTo = splitted_line[1]
                    layer = splitted_line[2]
                    e = g.add_edge(actorFrom, actorTo)
                    e["layer"] = layer

    return g


def plot_flattened(g: ig.Graph):
    # Create a layout for plotting
    layout = g.layout("auto")

    # Set the node size based on its degree
    vertex_sizes = [1.6 * degree + 2 for degree in g.degree()]

    # Plot the multilayer network using matplotlib
    output_filename = rf"Outputs/flattened.pdf"
    ig.plot(g, target=output_filename, layout=layout, vertex_label=[v["name"] for v in g.vs],
            edge_color="green", vertex_size=vertex_sizes)


def plot_layers(g: ig.Graph):
    layers = list(set(g.es.get_attribute_values("layer")))
    # For each layer
    for i, layer in enumerate(layers):
        # Create graph only for certain layer
        g_for_layer = g.copy()
        g_for_layer.delete_edges(g_for_layer.es.select(layer_ne=layer))
        g_for_layer.delete_vertices([v for v in g.vs if len(list(g.es.select(lambda x: (x.source==v.index or x.source==v.index) and x["layer"]==layer))) <= 0])

        #ax = axes[i]
        # Create a layout for plotting
        layout = g_for_layer.layout("auto")

        # Set the node size based on its degree
        vertex_sizes = [1.6 * degree + 2 for degree in g_for_layer.degree()]

        # Plot the multilayer network using matplotlib
        output_filename = rf"Outputs/{layer}.pdf"
        ig.plot(g_for_layer, target=output_filename, layout=layout, vertex_label=[v["name"] for v in g_for_layer.vs],
                edge_color="green", vertex_size=vertex_sizes)


def plot_communities_layers(g: ig.Graph):
    layers = list(set(g.es.get_attribute_values("layer")))
    # For each layer
    for i, layer in enumerate(layers):
        # Create graph only for certain layer
        g_for_layer = g.copy()
        g_for_layer.delete_edges(g_for_layer.es.select(layer_ne=layer))
        g_for_layer.delete_vertices([v for v in g.vs if len(list(
            g.es.select(lambda x: (x.source == v.index or x.source == v.index) and x["layer"] == layer))) <= 0])

        # Perform community detection using the Louvain method
        communities = g_for_layer.community_multilevel()

        # Assign a unique color to each community
        color_dict = {}
        for i, community in enumerate(communities):
            color = "#{:02x}{:02x}{:02x}".format(random.randint(0, 255), random.randint(0, 255), random.randint(0, 255))
            for vertex_id in community:
                color_dict[vertex_id] = color

        # Create a list of colors for each node based on their community
        colors = [color_dict[vertex.index] for vertex in g_for_layer.vs]

        # Set the node size based on its degree
        vertex_sizes = [1.6 * degree + 2 for degree in g_for_layer.degree()]

        # Set the node size based on its degree
        vertex_sizes = [1.6 * degree + 2 for degree in g_for_layer.degree()]

        # ax = axes[i]
        # Create a layout for plotting
        layout = g_for_layer.layout("auto")

        # Plot the multilayer network using matplotlib
        output_filename = rf"Outputs/{layer}_louvain.pdf"
        ig.plot(g_for_layer, target=output_filename, layout=layout, vertex_label=[v["name"] for v in g_for_layer.vs],
                edge_color="green", vertex_color=colors, vertex_size=vertex_sizes)

def plot_communities_flattened(g: ig.Graph):
    # Perform community detection using the Louvain method
    communities = g.community_multilevel()

    # Assign a unique color to each community
    color_dict = {}
    for i, community in enumerate(communities):
        color = "#{:02x}{:02x}{:02x}".format(random.randint(0, 255), random.randint(0, 255), random.randint(0, 255))
        for vertex_id in community:
            color_dict[vertex_id] = color

    # Create a list of colors for each node based on their community
    colors = [color_dict[vertex.index] for vertex in g.vs]

    # Set the node size based on its degree
    vertex_sizes = [1.6 * degree + 2 for degree in g.degree()]

    # ax = axes[i]
    # Create a layout for plotting
    layout = g.layout("auto")

    # Plot the multilayer network using matplotlib
    output_filename = rf"Outputs/flattened_louvain.pdf"
    ig.plot(g, target=output_filename, layout=layout, vertex_label=[v["name"] for v in g.vs],
            edge_color="green", vertex_color=colors, vertex_size=vertex_sizes)