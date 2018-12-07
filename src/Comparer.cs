using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thon.Hotels.PactVerifier
{
    public class Comparer
    {
        public static IEnumerable<string> Compare(object expectedResponse, object actualResponse)
        {
            if (actualResponse == null)
            {
                if (expectedResponse != null) 
                    yield return $"Actual response is null, expected is {expectedResponse.ToString()}";
                yield break;
            }
            if (actualResponse is JObject)
            {
                foreach(var r in Comparer.CompareObjects(expectedResponse as JObject, actualResponse as JObject)) yield return r;
            }
            else if (actualResponse is JArray)
            {
                foreach(var r in Comparer.CompareArrays(expectedResponse as JArray, actualResponse as JArray)) yield return r;
            }
            else if (actualResponse is System.Object)
            {
                foreach(var r in Comparer.CompareValues(expectedResponse, actualResponse)) yield return r;
            }
            else
                yield return $"Unsupported response type - {actualResponse.GetType()}";
        }
        
        private static IEnumerable<string> CompareObjects(JObject source, JObject target)
        {
            foreach (KeyValuePair<string, JToken> sourcePair in source)
            {
                if (sourcePair.Value.Type == JTokenType.Object)
                {
                    if (target.GetValue(sourcePair.Key, StringComparison.InvariantCultureIgnoreCase) == null)
                    {
                        yield return "Key " + sourcePair.Key + " not found" + Environment.NewLine;
                    }
                    else if (target.GetValue(sourcePair.Key, StringComparison.InvariantCultureIgnoreCase).Type != JTokenType.Object)
                    {
                        yield return "Key " + sourcePair.Key + " is not an object in target" + Environment.NewLine;
                    }
                    else
                    {
                        foreach (var errorMessage in CompareObjects(sourcePair.Value.ToObject<JObject>(), target.GetValue(sourcePair.Key, StringComparison.InvariantCultureIgnoreCase).ToObject<JObject>()))
                        {
                            yield return errorMessage;
                        }
                    }
                }
                else if (sourcePair.Value.Type == JTokenType.Array)
                {
                    if (target.GetValue(sourcePair.Key, StringComparison.InvariantCultureIgnoreCase) == null)
                    {
                        yield return "Key " + sourcePair.Key + " not found" + Environment.NewLine;
                    }
                    else
                    {
                        foreach (var errorMessage in CompareArrays(sourcePair.Value.ToObject<JArray>(), target.GetValue(sourcePair.Key, StringComparison.InvariantCultureIgnoreCase).ToObject<JArray>(), sourcePair.Key))
                        {
                            yield return errorMessage;
                        }
                    }
                }
                else
                {
                    JToken expected = sourcePair.Value;
                    var actual = SelectToken(target, sourcePair.Key);
                    if (actual.Equals(default(KeyValuePair<string, JToken>)))
                    {
                        yield return "Key " + sourcePair.Key + " not found" + Environment.NewLine;
                    }
                    else
                    {
                        if (!JToken.DeepEquals(expected, actual.Value))
                        {
                            yield return "Key " + sourcePair.Key + ": "
                                                + sourcePair.Value + " !=  "
                                                + actual.Value
                                                + Environment.NewLine;
                        }
                    }
                }
            }
        }

        private static KeyValuePair<string, JToken> SelectToken(JObject jObject, string key)
        {
            foreach (var keyValuePair in jObject)
            {
                if (string.Equals(keyValuePair.Key, key, StringComparison.OrdinalIgnoreCase))
                    return keyValuePair;
            }
            return default(KeyValuePair<string, JToken>);
        }

        private static IEnumerable<string> CompareArrays(JArray source, JArray target, string arrayName = "")
        {
            var returnString = new StringBuilder();
            for (var index = 0; index < source.Count; index++)
            {
                var expected = source[index];
                if (expected.Type == JTokenType.Object)
                {
                    var actual = (index >= target.Count) ? new JObject() : target[index];
                    foreach (var errorMessage in CompareObjects(expected.ToObject<JObject>(), actual.ToObject<JObject>()))
                    {
                        yield return errorMessage;
                    }
                }
                else
                {
                    var actual = (index >= target.Count) ? "" : target[index];
                    if (!JToken.DeepEquals(expected, actual))
                    {
                        if (String.IsNullOrEmpty(arrayName))
                        {
                            yield return "Index " + index + ": " + expected + " != " + actual + Environment.NewLine;
                        }
                        else
                        {
                            yield return "Key " + arrayName + "[" + index + "]: " + expected + " != " + actual + Environment.NewLine;
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> CompareValues(object expectedResponse, object actualResponse)
        {
            if (expectedResponse == null) yield break;
            var expectedValue = Convert.ChangeType(expectedResponse, actualResponse.GetType());
            if (!Object.Equals(expectedValue, actualResponse))
            {
                yield return $"{expectedValue} != {actualResponse}";
            }
        }
    }
}