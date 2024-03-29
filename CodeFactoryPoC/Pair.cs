using System;

namespace CodeFactory {

    public interface IPair {
        object First { get; }
        object Second { get; }
    }

    [Serializable]
    public class Pair<T1, T2> : IPair {

        object IPair.First { get { return First; } }
        object IPair.Second { get { return Second; } }

        public T1 First { get; set; }
        public T2 Second { get; set; }

        public Pair() { }

        public Pair(T1 first, T2 second) {
            First = first;
            Second = second;
        }

        public override int GetHashCode() {
            return (First == null ? 0 : First.GetHashCode()) ^
                   (Second == null ? 0 : Second.GetHashCode());
        }

        public override bool Equals(object obj) {
            var other = obj as Pair<T1, T2>;
            return this == other;
        }

        public static bool operator ==(Pair<T1, T2> pair1, Pair<T1, T2> pair2) {
            if (ReferenceEquals(pair1, null)) {
                return ReferenceEquals(pair2, null);
            }
            if (ReferenceEquals(pair2, null)) {
                return false;
            }

            if (pair1.First == null) {
                if (pair2.First != null) {
                    return false;
                }
            } else if (!pair1.First.Equals(pair2.First)) {
                return false;
            }

            if (pair1.Second == null) {
                if (pair2.Second != null) {
                    return false;
                }
            } else if (!pair1.Second.Equals(pair2.Second)) {
                return false;
            }

            return true;
        }

        public static bool operator !=(Pair<T1, T2> pair1, Pair<T1, T2> pair2) {
            if (ReferenceEquals(pair1, null)) {
                return !ReferenceEquals(pair2, null);
            }
            if (ReferenceEquals(pair2, null)) {
                return true;
            }

            if (pair1.First == null) {
                if (pair2.First != null) {
                    return true;
                }
            } else if (!pair1.First.Equals(pair2.First)) {
                return true;
            }

            if (pair1.Second == null) {
                if (pair2.Second != null) {
                    return true;
                }
            } else if (!pair1.Second.Equals(pair2.Second)) {
                return true;
            }

            return false;
        }

        public override string ToString() {
            return string.Format("<{0},{1}>", First, Second);
        }
    }

    public static class Pair {

        public static Pair<T1, T2> Create<T1, T2>(T1 first, T2 second) {
            return new Pair<T1, T2>(first, second);
        }
    }
}