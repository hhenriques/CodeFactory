using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CodeGeneration {
    public static class ExtensionMethods {

        public static string EscapeStringLiteral(this string s) {
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\'", "\\\'")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }

        public static void Apply<Type>(this IEnumerable<Type> collection, Action<Type> action) {
            foreach (Type item in collection) {
                action(item);
            }
        }
        public static IEnumerable<T> ToEnumerable<T>(this T obj) {
            if (obj != null) {
                yield return obj;
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
            var collection = enumerable as ICollection<T>;
            if (collection != null) {
                return collection.Count == 0;
            } else {
                using (var iter = enumerable.GetEnumerator()) {
                    return !iter.MoveNext();
                }
            }
        }

        public static int AddRange<Type>(this ICollection<Type> collection, IEnumerable<Type> otherCollection) {
            int count = 0;

            var set = collection as HashSet<Type>;
            if (set != null) {
                foreach (Type item in otherCollection) {
                    if (set.Add(item)) {
                        count += 1;
                    }
                }
            } else if (collection != null) {
                foreach (Type item in otherCollection) {
                    collection.Add(item);
                    count += 1;
                }
            }

            return count;
        }
        public static T GetValueOrDefault<K, T>(this IDictionary<K, T> collection, K key) {
            return GetValueOrDefault(collection, key, default(T));
        }

        public static T GetValueOrDefault<K, T>(this IDictionary<K, T> collection, K key, T defaultValue) {
            T item;
            if (collection.TryGetValue(key, out item)) {
                return item;
            } else {
                return defaultValue;
            }
        }


        public static bool IsNullOrEmpty(this string s) {
            return string.IsNullOrEmpty(s);
        }

        public static string StrCat(this IEnumerable<string> source, string separator) {
            return string.Join(separator, source);
        }

        public static string WithoutInvalidChars(this string s) {
            return WithoutInvalidChars(s, '_');
        }

        public static string WithoutInvalidChars(this string s, char? replaceChar) {
            return WithoutInvalidChars(s, replaceChar, IsValidChar);
        }

        public static string WithoutInvalidChars(this string s, char? replaceChar, Func<char, bool> isValidChar) {
            var result = new StringBuilder();

            foreach (char c in s.WithoutDiacritics()) {
                if (isValidChar(c)) {
                    result.Append(c);
                } else if (replaceChar.HasValue && !CharsToRemoveInsteadOfReplacing.Contains(c)) {
                    result.Append(replaceChar.Value);
                }
            }

            return result.ToString();
        }

        public static string WithoutDiacritics(this string value) {
            value = value.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value) {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static readonly HashSet<char> CharsToRemoveInsteadOfReplacing = new HashSet<char>() { ' ', '-', '&' };

        public static bool IsValidChar(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
        }




    }
}
