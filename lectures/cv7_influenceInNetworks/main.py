from Graph import Graph

def log_to_file(filepath: str, avg_ratio: float,  max_candidates: int, number_of_vertices: int):
    with open(filepath, "a") as file:
        file.write(f"{avg_ratio};{max_candidates};{number_of_vertices}\n")


if __name__ == '__main__':
    g = Graph(filepath=r"out.moreno_lesmis_lesmis", name_of_network="les_miserables", weighted=True)
    print(f"Number of vertices: {g.get_number_of_vertices()}")
    print(f"Number of edges: {g.get_number_of_edges()}")

    ratio = g.IC_simulation(p=0.2, timesteps=1000, max_candidates=2,
                            portion_of_vertices=1.0, number_of_vertices=None,
                            degree_percentile=90.0, clustering_coefficient_threshold=0.3,
                            enable_plotting=True)
    print(f"Portion of infected vertices: {ratio}")

    # FB of Pennsylvania
    g = Graph(filepath=r"data/socfb-Penn94.mtx", name_of_network="fb_pen", weighted=False)
    print(f"Number of vertices: {g.get_number_of_vertices()}")
    print(f"Number of edges: {g.get_number_of_edges()}")

    number_of_runs = 1000
    numbers_of_vertices = [100, 200, 500, 1000]
    maxs_candidates = [10, 2, 4]

    # c
    for number_of_vertices in numbers_of_vertices:
        # k
        for max_candidates in maxs_candidates:
            # each run
            for iteration in range(0, number_of_runs):
                # Run simulation
                ratio = g.IC_simulation(p=0.01, timesteps=1000, max_candidates=max_candidates, portion_of_vertices=None,
                                        number_of_vertices=number_of_vertices, degree_percentile=90.0,
                                        clustering_coefficient_threshold=0.5,
                                        enable_plotting=False)
                # Log results of simulation
                log_to_file(r"Outputs/log.txt", ratio, max_candidates, number_of_vertices)

                # Reset states after simulation
                g.reset_states()
