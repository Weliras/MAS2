import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd

def plot_boxplots(input_filepath: str, output_filepath: str, k: int):
    df = pd.read_csv(input_filepath, sep=';', names=["ratio", "k", "c"])
    df = df.loc[(df.k == k)]
    df['combined'] = "k=" + df['k'].astype(str) + ', ' + "c=" + df['c'].astype(str)

    # Box plot
    plt.figure(figsize=(8, 6))
    sns.boxplot(data=df, x="combined", y="ratio", hue="combined")
    plt.title("Ratio of \"infected\" portion of nodes.")
    plt.grid(True)
    plt.savefig(output_filepath, format="pdf")
    plt.close()



plot_boxplots(r"Outputs/log.txt", r"Outputs/plot_k2.pdf", 2)
plot_boxplots(r"Outputs/log.txt", r"Outputs/plot_k4.pdf", 4)
plot_boxplots(r"Outputs/log.txt", r"Outputs/plot_k10.pdf", 10)