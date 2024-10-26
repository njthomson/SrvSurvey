using Newtonsoft.Json;
using SrvSurvey.game;
using SrvSurvey.net;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace SrvSurvey
{
    internal static class Localize
    {
        private static string? key = Environment.GetEnvironmentVariable("trans-srv");
        private static string endpoint = "https://api.cognitive.microsofttranslator.com";
        private static string location = "westus";

        public static Dictionary<string, string> supportedLanguages = new()
        {
            { "English",  "en" },
            { "Deutsch",  "de" },
            { "Russisch", "ru" },
            { "Pseudo",   "ps" },
        };

        public static string? nameFromCode(string? code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            return supportedLanguages.FirstOrDefault(p => p.Value == code).Key;
        }

        public static string codeFromName(string name)
        {
            return Localize.supportedLanguages.GetValueOrDefault(name) ?? "";
        }

        public static void localize()
        {
            if (key == null) throw new NotSupportedException("Secret not available");

            var targetLangs = supportedLanguages.Values
                .Where(lang => lang != "en")
                .ToList();

            var resxFiles = new[]
            {
                @"SrvSurvey\Properties\Misc.resx",
                @"SrvSurvey\FormSphereLimit.resx",
                @"SrvSurvey\FormGroundTarget.resx",
            }
            .Select(f => Path.GetFullPath(Path.Combine(Git.srcRootFolder, f)))
            .ToList();

            Game.log($"\tLocalizing {resxFiles.Count} resx files across {targetLangs.Count} languages:\r\n> " + string.Join("\r\n> ", targetLangs));
            foreach (var targetLang in targetLangs)
                foreach (var filepath in resxFiles)
                    Localize.translateResx(filepath, targetLang).ContinueWith(t => { Game.log("Done"); });
        }

        public static async Task<string?> translateString(string textToTranslate, string targetLang)
        {
            // Input and output languages are defined as parameters.
            var uri = new Uri($"{endpoint}/translate?api-version=3.0&from=en&to={targetLang}");

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);

            var requestBody = JsonConvert.SerializeObject(new object[] { new { Text = textToTranslate } });
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            var response = await client.SendAsync(request).ConfigureAwait(false);

            string json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<Translators>>(json);
            var translation = result?.FirstOrDefault()?.translations.FirstOrDefault();

            Debug.WriteLine($"\t\t'{textToTranslate}' => {targetLang} => '{translation?.text}'");
            return translation?.text;
        }

        class Translators { public List<Translation> translations; }

        class Translation
        {
            public string text;
            public string to;

            public override string ToString()
            {
                return $"({to}) {text}";
            }
        }

        public static async Task translateResx(string sourceFilepath, string targetLang)
        {
            if (!File.Exists(sourceFilepath)) throw new FileNotFoundException($"File not found: {sourceFilepath}");

            // prep target .resx file
            var targetFilename = Path.GetFileNameWithoutExtension(sourceFilepath) + $".{targetLang}.resx";
            var targetFilepath = Path.Combine(Path.GetDirectoryName(sourceFilepath)!, targetFilename);

            var oldTargetDoc = File.Exists(targetFilepath) ? XDocument.Load(targetFilepath) : null;
            var sourceDoc = XDocument.Load(sourceFilepath);
            var newTargetDoc = XDocument.Load(sourceFilepath);
            //var newTargetDoc = XDocument.Load(File.Exists(targetFilepath) ? targetFilepath : sourceFilepath) // or ... start from the current localized .resx

            var sourceNodes = sourceDoc.Root?.Elements().Where(_ => _.Name.LocalName == "data")!;
            if (sourceNodes == null) return;

            var count = 0;
            foreach (var element in sourceNodes)
            {
                // extract resource
                var resourceName = element.Attribute("name")?.Value;
                var sourceText = element.Element("value")?.Value;
                //Game.log($">{resourceName}: '{sourceText}'");

                // skip any empty strings or non string resources
                if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(sourceText)) continue;
                if (resourceName.Contains(".") && !resourceName.EndsWith(".Text")) continue;

                // find target node + set the source text as a comment on the target element -  so we can detect when the source changes (saving on translation costs)
                var targetNode = newTargetDoc.Root!.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).First();
                targetNode.SetAttributeValue("comment", sourceText);

                // default to prior translation
                var oldTargetNode = oldTargetDoc?.Root?.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).FirstOrDefault();
                var oldTranslation = oldTargetNode?.Element("value")?.Value;
                if (!string.IsNullOrEmpty(oldTranslation))
                    targetNode.SetElementValue("value", oldTranslation);

                // always pseudo-localize when we're first creating the .resx file
                if (oldTargetDoc == null)
                {
                    targetLang = "ps";
                    targetNode.SetAttributeValue("comment", "-");
                }

                // skip anything that hasn't changed (unless we're doing PS)
                if (oldTargetNode?.Attribute("comment")?.Value == sourceText && targetLang != "ps") continue;

                if (targetLang == "ps")
                {
                    // pseudo translate it
                    var translation = $"*{sourceText.ToUpperInvariant()}→→→!";
                    targetNode.SetElementValue("value", translation);
                }
                else
                {
                    // translate into a real language
                    var translation = await translateString(sourceText, targetLang);
                    if (string.IsNullOrEmpty(translation))
                    {
                        Debugger.Break();
                        continue;
                    }
                    targetNode.SetElementValue("value", translation);
                }

                count++;
            }

            // Save and sort it
            Game.log($"> '{targetLang}' updated {count} of {sourceNodes.Count()} resources");
            alphaSortResx(newTargetDoc, targetFilepath);
        }

        public static void alphaSortResx(string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException($"File not found: {filepath}");

            var doc = XDocument.Load(filepath);
            alphaSortResx(doc, filepath);
        }

        public static void alphaSortResx(XDocument doc, string filepath)
        {
            // collect elements to be sorted
            var sorted = doc.Root!.Elements()
                .Where(_ => _.Name.LocalName == "data")
                .OrderBy(n => n.FirstAttribute?.Value.Replace("$", "").Replace("&gt;&gt;", ""))
                .ToList();

            // remove then re-add them
            foreach (var element in sorted) element.Remove();
            foreach (var element in sorted) doc.Root.Add(element);

            doc.Save(filepath);
        }
    }
}
