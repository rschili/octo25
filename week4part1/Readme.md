# My Approach:

## Task Overview

You are given an undirected graph where nodes are intersections and edges are roads. Each edge traversal takes one time step. Edges are labeled either "even" or "odd," and can only be traveled along on even or odd timesteps, respectively. This is because of the timing of traffic lights along the road network. The shortest (fastest) path between node AA and node ZZ is not unique: you must count the number of shortest (fastest) paths between AA and ZZ. The graph is given in DOT language (https://www.graphviz.org/doc/info/lang.html).
NOTE: You are allowed to idle at an intersection to wait for the timestep to change.
NOTE: The first step (0) is considered even.

## Approach

I am implementing this in C#.

Without thinking in well known algorithms, this is the approach that came to my mind:

- I want to use channels and worker tasks that will process incoming "tracers" in a consumer pattern.
- Each trace tracks its current timestep, they start at 0. (0 = even, 1= odd, 2 = even ....) and the path it came though (Can use immutable collections for this)
- Since I need to collect all possible paths, I prefer forward-storing tracers over backtracking. (Strictly speaking, I need only track the COUNT of paths, but I want to store them for debugging/validation purposes)
- When I reach a node, I branch into multiple tracers for each connected edge. Through each edge that doesn't match my timestep, I send a new tracer using timestep +2 (I waited 1 step at the node), through each edge that DOES match my timestep I send a new tracer with timestep + 1 (switches from odd to even and vice versa).
- The node gets marked with timestamp + 2 by the first node that leaves it. Any  tracers that arrive here with a higher timestamp will just die because they have no chance of being faster at the destination. (I may consider making this depend on the tracer's even/odd state, but I think just a rough filter is enough here, to stop the infinite cascade)
- Once a tracer arrives at the destination, I store its path and the timestamp (since I'm using threading, it may not be the shortest path)
- Any current tracer that exceeds this set timestamp will immediately discontinue.
- Any other tracers arriving at the target may update the timestamp if they found a shorter path, or just add their possible solution, if their timestamp matches the stored shortest one.
- I see many ways to optimize this, but the goal is to get a correct, debuggable result. Through parallelism and my "discontinue early" approach I'm pretty confident this will scale well enough and probably find the solution within a few milliseconds given the size of the test data.

