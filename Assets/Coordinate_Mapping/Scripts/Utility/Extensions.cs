using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

namespace CoordinateMapper {
    public static class JObject_Extensions {
        public static IEnumerable<JToken> AllTokens(this JObject obj) {
            var toSearch = new Stack<JToken>(obj.Children());
            while (toSearch.Count > 0) {
                var inspected = toSearch.Pop();
                yield return inspected;
                foreach (var child in inspected) {
                    toSearch.Push(child);
                }
            }
        }
    }
}
