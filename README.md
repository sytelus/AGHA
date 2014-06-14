AGHA
====

Asymmetric Generalized Hebbian algorithm for calculating components of a given matrix. When matrix is not square this algorithm still yields vector components that can be used to rebuild approximate input matrix unlike SVD. The beauty of the algorithm is in its ability to process extremely large sparce matrix where traditional methods comes to a grind. The algorithm yields most significant eignvectors first.

##Implementation Note##
This implementation is tuned towards performance and optimizing memory requirement. There is Java version available elsewhere on Internet which may be more suitable if you want to learn.

##More Information##
The algorithm is described at http://www.dcs.shef.ac.uk/~genevieve/gorrell_thesis.pdf

