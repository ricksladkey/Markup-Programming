using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Markup.Programming.Core
{
    public class TokenQueue : IEnumerable<string>
    {
        private List<string> list = new List<string>();
        private int current = 0;
        public void Enqueue(string item) { list.Add(item); }
        public string Dequeue() { return list[current++]; }
        public void Undequeue() { --current; }
        public string Peek() { return current < list.Count ? list[current] : null; }
        public int Count { get { return list.Count - current; } }
        public IEnumerator<string> GetEnumerator() { return list.Skip(current).GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
