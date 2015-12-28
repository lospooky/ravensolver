using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }

        public Tuple(T1 first, T2 second) {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");

            First = first;
            Second = second;
        }

        public override int GetHashCode() {
            return First.GetHashCode() ^ Second.GetHashCode();
        }

        public static bool operator ==(Tuple<T1, T2> a, Tuple<T1, T2> b) {
            return object.ReferenceEquals(a, b) ||
                (object)a != null && a.Equals(b);
        }

        public static bool operator !=(Tuple<T1, T2> a, Tuple<T1, T2> b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            var t = obj as Tuple<T1, T2>;

            return
                t != null &&
                t.First.Equals(this.First) &&
                t.Second.Equals(this.Second);
        }

        public override string ToString() {
            return string.Format("First: {0}  Second: {1}", First, Second);
        }
    }
}
