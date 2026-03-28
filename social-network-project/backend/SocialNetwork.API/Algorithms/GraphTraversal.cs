namespace SocialNetwork.API.Algorithms
{
    /// <summary>
    /// Graph traversal algorithms (BFS/DFS) for the social network.
    /// Used for friend recommendations, second-degree connections,
    /// and social graph analysis.
    ///
    /// Time complexity: O(V + E) where V = users, E = connections.
    /// </summary>
    public class SocialGraph
    {
        private readonly Dictionary<int, HashSet<int>> _adjacencyList = new();

        /// <summary>
        /// Build the undirected adjacency list from connection pairs.
        /// </summary>
        public void BuildGraph(IEnumerable<(int UserId1, int UserId2)> connections)
        {
            _adjacencyList.Clear();
            foreach (var (u1, u2) in connections)
            {
                if (!_adjacencyList.ContainsKey(u1))
                    _adjacencyList[u1] = new HashSet<int>();
                if (!_adjacencyList.ContainsKey(u2))
                    _adjacencyList[u2] = new HashSet<int>();
                _adjacencyList[u1].Add(u2);
                _adjacencyList[u2].Add(u1);
            }
        }

        /// <summary>
        /// BFS to find all users within a given depth (degrees of separation).
        /// Used for second-degree connection recommendations.
        /// Time: O(V + E)
        /// </summary>
        public Dictionary<int, int> BfsGetUsersWithinDepth(int startUserId, int maxDepth)
        {
            var visited = new Dictionary<int, int>(); // userId -> depth
            var queue = new Queue<(int UserId, int Depth)>();

            visited[startUserId] = 0;
            queue.Enqueue((startUserId, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();
                if (depth >= maxDepth) continue;

                if (!_adjacencyList.ContainsKey(current)) continue;
                foreach (int neighbor in _adjacencyList[current])
                {
                    if (!visited.ContainsKey(neighbor))
                    {
                        visited[neighbor] = depth + 1;
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }

            visited.Remove(startUserId);
            return visited;
        }

        /// <summary>
        /// Get direct connections (first degree) for a user.
        /// </summary>
        public HashSet<int> GetDirectConnections(int userId)
        {
            return _adjacencyList.TryGetValue(userId, out var neighbors)
                ? neighbors
                : new HashSet<int>();
        }

        /// <summary>
        /// Count mutual connections between two users.
        /// </summary>
        public int CountMutualConnections(int userId1, int userId2)
        {
            var friends1 = GetDirectConnections(userId1);
            var friends2 = GetDirectConnections(userId2);
            return friends1.Intersect(friends2).Count();
        }

        /// <summary>
        /// Get all mutual connections between two users.
        /// </summary>
        public List<int> GetMutualConnections(int userId1, int userId2)
        {
            var friends1 = GetDirectConnections(userId1);
            var friends2 = GetDirectConnections(userId2);
            return friends1.Intersect(friends2).ToList();
        }

        /// <summary>
        /// DFS to find if two users are connected (path exists).
        /// </summary>
        public bool DfsAreConnected(int startUserId, int targetUserId, int maxDepth = 6)
        {
            var visited = new HashSet<int>();
            return DfsHelper(startUserId, targetUserId, 0, maxDepth, visited);
        }

        private bool DfsHelper(int current, int target, int depth, int maxDepth, HashSet<int> visited)
        {
            if (current == target) return true;
            if (depth >= maxDepth) return false;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            if (!_adjacencyList.ContainsKey(current)) return false;

            foreach (int neighbor in _adjacencyList[current])
            {
                if (DfsHelper(neighbor, target, depth + 1, maxDepth, visited))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// BFS shortest path between two users (degrees of separation).
        /// </summary>
        public int ShortestPath(int startUserId, int targetUserId)
        {
            if (startUserId == targetUserId) return 0;
            var visited = new Dictionary<int, int> { [startUserId] = 0 };
            var queue = new Queue<int>();
            queue.Enqueue(startUserId);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (!_adjacencyList.ContainsKey(current)) continue;

                foreach (int neighbor in _adjacencyList[current])
                {
                    if (!visited.ContainsKey(neighbor))
                    {
                        visited[neighbor] = visited[current] + 1;
                        if (neighbor == targetUserId) return visited[neighbor];
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return -1; // Not connected
        }
    }
}
