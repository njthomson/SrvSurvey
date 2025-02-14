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
            { "English",   "en" },
            { "Deutsch",   "de" },
            { "Español",   "es" },
            { "Français",  "fr" },
            { "Português", "pt" },
            { "Русский",   "ru" },
            { "Pseudo",    "ps" },
            { "简体中文",    "zh-Hans" },
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

        public static async Task localize(bool pseudoOnly)
        {
            if (key == null) throw new NotSupportedException("Secret not available");

            var targetLangs = supportedLanguages.Values
                .Where(lang => lang != "en")
                .ToList();

            if (pseudoOnly)
                targetLangs = new() { "ps" };

            // ready loc ready resx file names
            var locReadyResx = File.ReadAllLines(Path.Combine(Git.srcRootFolder, "SrvSurvey", "loc-ready.txt"))
                .Where(f => !f.StartsWith("#") && !f.StartsWith(" "))
                .ToList();
            var resxFiles = Directory.GetFiles(Path.GetFullPath(Path.Combine(Git.srcRootFolder, "SrvSurvey")), "*.resx", SearchOption.AllDirectories)
                .Where(p => locReadyResx.Any(r => p.EndsWith(r)))
                .ToList();

            Game.log($"> Localizing {resxFiles.Count} resx files across {targetLangs.Count} languages:\r\n - " + string.Join("\r\n - ", targetLangs));
            foreach (var filepath in resxFiles)
                foreach (var targetLang in targetLangs)
                    await Localize.translateResx(filepath, targetLang);
        }

        private static async Task<string?> translateString(string textToTranslate, string targetLang)
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

        private class Translators { public List<Translation> translations; }

        private class Translation
        {
            public string text;
            public string to;

            public override string ToString()
            {
                return $"({to}) {text}";
            }
        }

        private static async Task translateResx(string sourceFilepath, string targetLang)
        {
            if (!File.Exists(sourceFilepath)) throw new FileNotFoundException($"File not found: {sourceFilepath}");

            // prep target .resx file
            var targetFilename = Path.GetFileNameWithoutExtension(sourceFilepath) + $".{targetLang}.resx";
            var targetFilepath = Path.Combine(Path.GetDirectoryName(sourceFilepath)!, targetFilename);

            var oldTargetDoc = File.Exists(targetFilepath) ? XDocument.Load(targetFilepath) : null;
            var sourceDoc = XDocument.Load(sourceFilepath);

            // skip certain files that were pre-translated
            var locLevel = sourceDoc.Root?.Elements().FirstOrDefault(e => e.Name == "resheader" && e.Attribute("name")?.Value == "loc-level");
            if (locLevel?.Value == "pseudo-only" && targetLang != "ps")
                return;

            //var newTargetDoc = XDocument.Load(File.Exists(targetFilepath) ? targetFilepath : sourceFilepath) // or ... start from the current localized .resx
            var newTargetDoc = XDocument.Load(sourceFilepath);
            // remove the large annoying comment at the top
            if (newTargetDoc.Root?.FirstNode?.NodeType == System.Xml.XmlNodeType.Comment)
                newTargetDoc.Root.FirstNode.Remove();

            var sourceNodes = sourceDoc.Root?.Elements().Where(_ => _.Name.LocalName == "data")!;
            if (sourceNodes == null) return;

            // TODO: inject element into xsd:schema to make `source` be deemed valid
            //var foo = newTargetDoc.Root!.Descendants().Where(_ => _.Name.LocalName == "element" && _.FirstAttribute?.Value == "comment").First();
            //foo.Parent.Add();

            var count = 0;
            foreach (var element in sourceNodes)
            {
                // extract resource
                var resourceName = element.Attribute("name")?.Value;
                var sourceText = element.Element("value")?.Value;
                var commentText = element.Element("comment")?.Value;
                //Game.log($">{resourceName}: '{sourceText}'");

                // skip any empty strings or non string resources
                if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(sourceText)) continue;
                if (resourceName.Contains(".") && !resourceName.EndsWith(".Text")) continue;

                // find target node + set the source text as a comment on the target element -  so we can detect when the source changes (saving on translation costs)
                var targetNode = newTargetDoc.Root!.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).First();

                // build a replacement node
                var newNode = new XElement("data");
                newNode.SetAttributeValue("name", resourceName);
                newNode.Add(new XElement("value", ""));
                if (commentText!= null)
                    newNode.Add(new XElement("comment", commentText));
                newNode.Add(new XElement("source", sourceText));
                targetNode.ReplaceWith(newNode);
                targetNode = newNode;

                // default to prior translation
                var oldTargetNode = oldTargetDoc?.Root?.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).FirstOrDefault();
                var oldTranslation = oldTargetNode?.Element("value")?.Value;
                var oldSource = oldTargetNode?.Element("source")?.Value ?? oldTargetNode?.Element("comment")?.Value;

                //if (resourceName == "Tussock") Debugger.Break();

                // replace elements when we're first creating the .resx file (and always pseudo-localize)
                if (!string.IsNullOrEmpty(oldTranslation))
                    targetNode.SetElementValue("value", oldTranslation);
                //else if (oldTargetDoc == null && targetLang != "ps") // seed new .resx files untranslated, so we can see the diff's
                //    targetNode.SetElementValue("comment", "-");

                // skip anything that hasn't changed (unless we're doing PS)
                var oldComment = oldTargetNode?.Element("comment")?.Value;
                if (oldSource == sourceText && !string.IsNullOrEmpty(oldTranslation)) continue;

                // presume pseudo translate it
                var translation = $"* {sourceText.ToUpperInvariant()} →→→!";

                // or translate into a real language
                if (targetLang != "ps") // && oldTargetDoc != null)
                {
                    translation = await translateString(sourceText, targetLang);
                    if (string.IsNullOrEmpty(translation))
                    {
                        Debugger.Break();
                        continue;
                    }
                }

                var valueNode = targetNode.Element("value")!;
                //valueNode.SetAttributeValue(XNamespace.Xml.GetName("space"), "preserve");
                valueNode.Value = translation;

                count++;
            }

            // Save and sort it
            Game.log($"Updated {count,3} of {sourceNodes.Count(),3} '{targetLang}' resources in " + Path.GetFileNameWithoutExtension(sourceFilepath));
            alphaSortResx(newTargetDoc, targetFilepath);
        }

        private static void alphaSortResx(string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException($"File not found: {filepath}");

            var doc = XDocument.Load(filepath);
            alphaSortResx(doc, filepath);
        }

        private static void alphaSortResx(XDocument doc, string filepath)
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
