namespace SocialNetwork.API.Algorithms
{
    /// <summary>
    /// Generic Max-Heap / Priority Queue implementation.
    /// Used for top-K post retrieval in O(log n) per operation,
    /// trending detection, and personalized feed ranking.
    /// </summary>
    public class MaxHeap<T>
    {
        private readonly List<(double Priority, T Item)> _heap = new();
        private readonly Func<T, double> _prioritySelector;

        public MaxHeap(Func<T, double> prioritySelector)
        {
            _prioritySelector = prioritySelector;
        }

        public int Count => _heap.Count;

        /// <summary>
        /// Insert item. Time: O(log n)
        /// </summary>
        public void Insert(T item)
        {
            double priority = _prioritySelector(item);
            _heap.Add((priority, item));
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        /// Remove and return the max priority item. Time: O(log n)
        /// </summary>
        public T ExtractMax()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Heap is empty.");
            var max = _heap[0].Item;
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);
            if (_heap.Count > 0) HeapifyDown(0);
            return max;
        }

        /// <summary>
        /// Peek at the max priority item without removing. Time: O(1)
        /// </summary>
        public T PeekMax()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Heap is empty.");
            return _heap[0].Item;
        }

        /// <summary>
        /// Extract the top-K items. Time: O(K log n)
        /// </summary>
        public List<T> ExtractTopK(int k)
        {
            var results = new List<T>();
            int count = Math.Min(k, _heap.Count);
            for (int i = 0; i < count; i++)
                results.Add(ExtractMax());
            return results;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[index].Priority > _heap[parent].Priority)
                {
                    (_heap[index], _heap[parent]) = (_heap[parent], _heap[index]);
                    index = parent;
                }
                else break;
            }
        }

        private void HeapifyDown(int index)
        {
            int n = _heap.Count;
            while (true)
            {
                int largest = index;
                int left = 2 * index + 1;
                int right = 2 * index + 2;

                if (left < n && _heap[left].Priority > _heap[largest].Priority)
                    largest = left;
                if (right < n && _heap[right].Priority > _heap[largest].Priority)
                    largest = right;

                if (largest == index) break;
                (_heap[index], _heap[largest]) = (_heap[largest], _heap[index]);
                index = largest;
            }
        }
    }

    /// <summary>
    /// Post ranking using weighted score combining likes, comments, views, and recency.
    /// </summary>
    public static class PostRankingHelper
    {
        public static double CalculateTrendingScore(int likeCount, int commentCount, int viewCount, DateTime createdAt)
        {
            double hoursAge = (DateTime.UtcNow - createdAt).TotalHours + 1;
            double engagementScore = (likeCount * 3.0) + (commentCount * 2.0) + (viewCount * 0.1);
            // Hacker News-style decay: score / (age ^ gravity)
            double gravity = 1.8;
            return engagementScore / Math.Pow(hoursAge, gravity);
        }
    }
}
