namespace SocialNetwork.API.Algorithms
{
    /// <summary>
    /// Trie data structure for O(L) username/hashtag lookup and autocomplete,
    /// where L is the length of the search prefix.
    /// Supports 100K+ records with efficient prefix-based search.
    /// </summary>
    public class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; set; } = new();
        public bool IsEndOfWord { get; set; } = false;
        public List<int> UserIds { get; set; } = new();
        public string? Word { get; set; }
    }

    public class Trie
    {
        private readonly TrieNode _root = new();

        /// <summary>
        /// Insert a word into the Trie. Time: O(L), Space: O(L)
        /// </summary>
        public void Insert(string word, int userId)
        {
            if (string.IsNullOrEmpty(word)) return;
            word = word.ToLowerInvariant();

            var current = _root;
            foreach (char ch in word)
            {
                if (!current.Children.ContainsKey(ch))
                    current.Children[ch] = new TrieNode();
                current = current.Children[ch];
                current.UserIds.Add(userId);
            }
            current.IsEndOfWord = true;
            current.Word = word;
        }

        /// <summary>
        /// Search for all userIds matching a prefix. Time: O(L + K) where K = results.
        /// </summary>
        public List<int> SearchByPrefix(string prefix, int maxResults = 20)
        {
            if (string.IsNullOrEmpty(prefix)) return new List<int>();
            prefix = prefix.ToLowerInvariant();

            var current = _root;
            foreach (char ch in prefix)
            {
                if (!current.Children.ContainsKey(ch))
                    return new List<int>();
                current = current.Children[ch];
            }

            return current.UserIds.Distinct().Take(maxResults).ToList();
        }

        /// <summary>
        /// Search for all words matching a prefix for autocomplete suggestions.
        /// </summary>
        public List<string> AutoComplete(string prefix, int maxSuggestions = 10)
        {
            if (string.IsNullOrEmpty(prefix)) return new List<string>();
            prefix = prefix.ToLowerInvariant();

            var current = _root;
            foreach (char ch in prefix)
            {
                if (!current.Children.ContainsKey(ch))
                    return new List<string>();
                current = current.Children[ch];
            }

            var results = new List<string>();
            DfsCollectWords(current, prefix, results, maxSuggestions);
            return results;
        }

        private void DfsCollectWords(TrieNode node, string current, List<string> results, int max)
        {
            if (results.Count >= max) return;
            if (node.IsEndOfWord && node.Word != null)
                results.Add(node.Word);
            foreach (var child in node.Children)
                DfsCollectWords(child.Value, current + child.Key, results, max);
        }

        /// <summary>
        /// Delete a word from the Trie. Time: O(L)
        /// </summary>
        public bool Delete(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            word = word.ToLowerInvariant();
            return DeleteHelper(_root, word, 0);
        }

        private bool DeleteHelper(TrieNode node, string word, int depth)
        {
            if (depth == word.Length)
            {
                if (!node.IsEndOfWord) return false;
                node.IsEndOfWord = false;
                return node.Children.Count == 0;
            }
            char ch = word[depth];
            if (!node.Children.ContainsKey(ch)) return false;
            bool shouldDelete = DeleteHelper(node.Children[ch], word, depth + 1);
            if (shouldDelete)
            {
                node.Children.Remove(ch);
                return !node.IsEndOfWord && node.Children.Count == 0;
            }
            return false;
        }
    }
}
