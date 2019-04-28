// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.Utilities
{
    public static class CompletionUtility
    {
        public static IEnumerable<string> GetRelevantCompletionItems(
            string words,
            IDictionary<string, string[]> completionItems)
        {
            completionItems = new Dictionary<string, string[]>(completionItems, StringComparer.OrdinalIgnoreCase);
            var suggestedItems = new List<string>();

            var parts = words.Split(separator: ' ');
            var currentWord = parts.Last() != string.Empty ? parts.Last() : null;
            var parameters = parts.Where(ParameterService.IsParameter).Select(ParameterService.GetParameterMemberName).ToList();
            var lastParameter = parameters.LastOrDefault();

            void AddSubItems(string parameter)
            {
                var passedItems = parts.Reverse().TakeWhile(x => !ParameterService.IsParameter(x));
                var items = completionItems.GetValueOrDefault(parameter)?.Except(passedItems, StringComparer.OrdinalIgnoreCase) ??
                            new string[0];
                foreach (var item in items)
                {
                    if (currentWord == null)
                        suggestedItems.Add(item);
                    if (currentWord != null && item.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                        suggestedItems.Add(item.ReplaceCurrentWord(currentWord));
                }
            }

            if (lastParameter == null)
                AddSubItems(Constants.InvokedTargetsParameterName);

            if (lastParameter != null && currentWord != lastParameter)
                AddSubItems(lastParameter);

            if (currentWord == null || ParameterService.IsParameter(currentWord))
            {
                foreach (var item in completionItems.Keys)
                {
                    // if (currentWord == null && completionItems.GetValueOrDefault(lastParameter.GetParameterName())?.Length > 0)
                    //     continue;

                    if (parameters.Contains(item, StringComparer.OrdinalIgnoreCase))
                        continue;

                    if (currentWord == null || currentWord.TrimStart("-").Length == 0)
                    {
                        suggestedItems.Add(
                            new[] { "NuGet", "MSBuild", "GitHub" }
                                .Aggregate(
                                    $"--{item.SplitCamelHumpsWithSeparator("-")}",
                                    (i, t) => i.Replace(t.SplitCamelHumpsWithSeparator("-"), t.ToLowerInvariant())));
                    }
                    else if (ParameterService.IsParameter(currentWord) &&
                             item.StartsWithOrdinalIgnoreCase(ParameterService.GetParameterMemberName(currentWord)))
                    {
                        suggestedItems.Add(
                            (currentWord.StartsWith("--")
                                ? $"--{item.SplitCamelHumpsWithSeparator("-")}"
                                : $"-{item}")
                            .ReplaceCurrentWord(currentWord));
                    }
                }
            }

            return suggestedItems;
        }

        private static string ReplaceCurrentWord(this string str, string currentWord)
        {
            return str.ReplaceRegex(currentWord, x => currentWord, RegexOptions.IgnoreCase);
        }
    }
}
