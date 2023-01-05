using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Physalia.Flexi.GraphDataFixer
{
    public class ValidationResult
    {
        internal List<GraphAsset> invalidAssets = new();
        internal List<string> invalidTypeNames = new();
    }

    internal static class GraphDataFixer
    {
        internal static ValidationResult ValidateGraphAssets(List<GraphAsset> assets)
        {
            var result = new ValidationResult();

            for (var i = 0; i < assets.Count; i++)
            {
                string graphJson;
                if (assets[i] is MacroAsset macroAsset)
                {
                    graphJson = macroAsset.Text;
                }
                else if (assets[i] is AbilityAsset abilityAsset)
                {
                    graphJson = abilityAsset.GraphJsons[0];
                }
                else
                {
                    continue;
                }

                bool success = Validate(graphJson, result);
                if (!success)
                {
                    result.invalidAssets.Add(assets[i]);
                }
            }

            return result;
        }

        private static bool Validate(string graphJson, ValidationResult result)
        {
            JObject jObject = JObject.Parse(graphJson);
            var hasAnyInvalidType = false;

            IterateNodes(jObject, RecordInvalidType);

            return !hasAnyInvalidType;

            void RecordInvalidType(JToken typeToken)
            {
                string typeName = typeToken.ToString();
                Type type = ReflectionUtilities.GetTypeByName(typeName);
                if (type == null)
                {
                    hasAnyInvalidType = true;
                    if (!result.invalidTypeNames.Contains(typeName))
                    {
                        result.invalidTypeNames.Add(typeName);
                    }
                }
            }
        }

        internal static void FixGraphAssets(List<GraphAsset> assets, Dictionary<string, string> fixTable)
        {
            for (var i = 0; i < assets.Count; i++)
            {
                if (assets[i] is MacroAsset macroAsset)
                {
                    macroAsset.Text = Fix(macroAsset.Text, fixTable);
                }
                else if (assets[i] is AbilityAsset abilityAsset)
                {
                    abilityAsset.GraphJsons[0] = Fix(abilityAsset.GraphJsons[0], fixTable);
                }
                else
                {
                    continue;
                }
            }
        }

        private static string Fix(string graphJson, Dictionary<string, string> fixTable)
        {
            JObject jObject = JObject.Parse(graphJson);
            IterateNodes(jObject, FixInvalidType);

            string result = jObject.ToString(Formatting.None);
            return result;

            void FixInvalidType(JToken typeToken)
            {
                string typeName = typeToken.ToString();
                Type type = ReflectionUtilities.GetTypeByName(typeName);
                if (type == null)
                {
                    bool success = fixTable.TryGetValue(typeName, out string newName);
                    if (success)
                    {
                        typeToken.Replace(newName);
                    }
                }
            }
        }

        private static void IterateNodes(JObject jObject, Action<JToken> actionForTypeToken)
        {
            JArray nodes = (JArray)jObject[TokenKeys.GRAPH_NODES];
            if (nodes == null)
            {
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                JToken typeToken = nodes[i][TokenKeys.NODE_TYPE];
                if (typeToken == null)  // The type field doesn't exist, which would not probably happened.
                {
                    continue;
                }

                actionForTypeToken?.Invoke(typeToken);
            }
        }
    }
}
