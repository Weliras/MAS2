import igraph as ig
import random
import numpy as np


class Graph:
    def __init__(self, directed: bool = False, weighted: bool = True, filepath: str = None, name_of_network: str = "les_miserables"):
        self.g = ig.Graph(directed=directed)
        self.color_palette = {"healthy": "green", "active": "orange", "infected": "red"}
        self.name_of_network = name_of_network
        self.directed = directed
        self.weighted = weighted

        if name_of_network == "les_miserables":
            self.load_from_file(filepath)
        else:
            self.g = self.g.Read(filepath, "edgelist")
            self.g.vs["weight"] = 1
            self.g.vs["state"] = "healthy"
            self.g.vs["number_of_infected"] = 0

    def load_from_file(self, filepath: str):
        with open(filepath, "r") as file:
            # for each line in file
            for line in file:
                # ignore comment lines
                if line.startswith('%') or line.startswith('#'):
                    continue
                # columns are whitespace separated
                parsed_line = line.rstrip('\n').split()

                # parse out columns
                id_from = (parsed_line[0])
                id_to = (parsed_line[1])
                weight = 1
                if self.weighted:
                    weight = int(parsed_line[2])

                # add nodes to graph
                if not any(v["name"] == id_from for v in self.g.vs):
                    self.g.add_vertex(name=id_from, state="healthy", number_of_infected=0)
                if not any(v["name"] == id_to for v in self.g.vs):
                    self.g.add_vertex(name=id_to, state="healthy", number_of_infected=0)

                # add edge
                self.g.add_edge(id_from, id_to, weight=weight)

    def get_number_of_vertices(self):
        return self.g.vcount()

    def get_number_of_edges(self):
        return self.g.ecount()

    def plot(self, timestamp: int = 0):
        # Create a layout for plotting
        layout = self.g.layout("auto")
        # Set the node size based on success of infection
        vertex_sizes = [1.4 * vertex["number_of_infected"] + 10 for vertex in self.g.vs]
        # Set the node color based on its state
        vertex_colors = [self.color_palette[vertex["state"]] for vertex in self.g.vs]
        # Set vertex labels
        vertex_labels = [v["name"] for v in self.g.vs]
        # Set edge width according to its weight
        edge_widths = [0.3 * edge["weight"] + 1 for edge in self.g.es]

        # Plot the multilayer network using matplotlib
        output_filename = rf"Outputs/{self.name_of_network}_{timestamp}.pdf"
        ig.plot(self.g, target=output_filename, layout=layout,
                edge_width=edge_widths, vertex_size=vertex_sizes,
                vertex_color=vertex_colors, vertex_label=vertex_labels)

    def pick_best_candidates(self, max_candidates: int = 1, portion_of_vertices: float = None, number_of_vertices: int = None, degree_percentile: float = 90.0,
                             clustering_coefficient_threshold: float = 0.3):
        # Select only portion of vertices for finding candidates and hubs
        if portion_of_vertices is not None:
            selected_vertix_ids = random.sample(range(self.g.vcount()), int(self.g.vcount() * portion_of_vertices))
        else:
            selected_vertix_ids = random.sample(range(self.g.vcount()), number_of_vertices)

        selected_vertices = [self.g.vs[vertex_id] for vertex_id in selected_vertix_ids]
        del selected_vertix_ids

        # Set degree threshold according to percentile
        degree_threshold = np.percentile([vertex.degree() for vertex in selected_vertices], degree_percentile)

        candidates = []
        for vertex in selected_vertices:
            degree = vertex.degree()
            if degree >= degree_threshold:
                clustering_coefficient = self.g.transitivity_local_undirected(vertices=vertex)
                if clustering_coefficient <= clustering_coefficient_threshold:
                    vertex["clustering_coefficient"] = clustering_coefficient
                    vertex["degree"] = degree
                    candidates.append(vertex)

        candidates.sort(key=lambda vertex: vertex["clustering_coefficient"])

        return candidates[0:max_candidates]

    def reset_states(self):
        self.g.vs["state"] = "healthy"
        self.g.vs["number_of_infected"] = 0

    def IC_simulation(self, p: float = 0.10, timesteps: int = 10, max_candidates: int = 1, portion_of_vertices: float = None,
                      number_of_vertices: int = None, degree_percentile: float = 90.0,
                      clustering_coefficient_threshold: float = 0.3, enable_plotting: bool = False):
        candidates = self.pick_best_candidates(max_candidates, portion_of_vertices, number_of_vertices, degree_percentile, clustering_coefficient_threshold)
        # Set initial candidates infected
        for candidate in candidates:
            candidate["state"] = "active"

        if enable_plotting:
            self.plot(0)
        # for each timestep
        for epoch in range(1, timesteps):
            active_vertices = self.g.vs.select(state="active")
            # If there arent any active vertices. Then end simulation.
            if not active_vertices:
                break

            # Select all healthy neighbours
            for activated_vertix in active_vertices:
                healthy_neighbors = [neighbor for neighbor in activated_vertix.neighbors() if neighbor["state"] =="healthy"]
                for healthy_neighbor in healthy_neighbors:
                    random_float = random.uniform(0.0, 1.0)
                    # infect!
                    if random_float <= p:
                        activated_vertix["number_of_infected"] += 1
                        healthy_neighbor["state"] = "active"

            # change active to infected
            active_vertices["state"] = "infected"
            # plot g for each timestep
            if enable_plotting:
                self.plot(epoch)
        ratio = len(self.g.vs.select(state="infected")) / self.g.vcount()
        print(f"Number of candidates: {len(candidates)}")
        print(f"Simulation ended in: {epoch} timestep")
        print(f"Ratio of infected vertices: {ratio}")
        print()
        return ratio