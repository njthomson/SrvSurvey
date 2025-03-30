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
        /// <summary> The time, granular to the hour the process was started, for a more approximate timestamp </summary>
        private static string approxNow = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:00:00Z");

        private static string? key = Environment.GetEnvironmentVariable("trans-srv");
        private static bool canTranslate
        {
            get
            {
                return key != null;
            }
        }

        private static string endpoint = "https://api.cognitive.microsofttranslator.com";
        private static string location = "westus";
        private static XName ordinal = XName.Get("{urn:schemas-microsoft-com:xml-msdata}Ordinal");
        private static Dictionary<string, Dictionary<string, HashSet<string>>> locUpdate = new();
        private static Dictionary<string, Dictionary<string, HashSet<string>>> locMissing = new();

        public static Dictionary<string, string> supportedLanguages = new()
        {
            { "English",   "en" },
            { "Deutsch",   "de" },
            { "Español",   "es" },
            { "Français",  "fr" },
            { "Português (Brasil)", "pt-BR" },
            { "Русский",   "ru" },
            { "简体中文",    "zh-Hans" },
            { "Pseudo",    "ps" },
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

            locUpdate.Clear();
            locMissing.Clear();

            // translate each of the .resx files in each language
            Game.log($"> Localizing {resxFiles.Count} resx files across {targetLangs.Count} languages:\r\n - " + string.Join("\r\n - ", targetLangs));
            foreach (var filepath in resxFiles)
                foreach (var targetLang in targetLangs)
                    await Localize.translateResx(filepath, targetLang);

            // generate reports, per language, of translations that are missing or need updating
            locUpdate.Remove("ps");
            foreach (var (lang, files) in locUpdate)
            {
                var locPath = Path.Combine(Git.srcRootFolder, "SrvSurvey", "Properties", $"_loc-{lang}-needed.txt");
                File.Delete(locPath);
                if (files.Count != 0)
                    File.WriteAllText(
                        locPath,
                        string.Join("\r\n\r\n", files.Select(_ => _.Value.formatWithHeader(_.Key.Replace(".resx", $".{lang}.resx"), "\r\n\t"))) + "\r\n");
            }

            locMissing.Remove("ps");
            foreach (var (lang, files) in locMissing)
            {
                var locPath = Path.Combine(Git.srcRootFolder, "SrvSurvey", "Properties", $"_loc-{lang}-missing.txt");
                File.Delete(locPath);
                if (files.Count != 0)
                    File.WriteAllText(
                        locPath,
                        string.Join("\r\n\r\n", files.Select(_ => _.Value.formatWithHeader(_.Key.Replace(".resx", $".{lang}.resx"), "\r\n\t"))) + "\r\n");
            }
        }

        private static async Task<string?> translateString(string textToTranslate, string targetLang)
        {
            if (!canTranslate) throw new NotSupportedException("Secret not available");

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

            locMissing.init(targetLang);
            locUpdate.init(targetLang);

            // prep target .resx file
            var relativeSourceFilePath = sourceFilepath
                .Replace(Path.GetFullPath(Git.srcRootFolder) + "\\SrvSurvey", "")
                .Replace("\\", "/");
            var targetFilename = Path.GetFileNameWithoutExtension(sourceFilepath) + $".{targetLang}.resx";
            var targetFilepath = Path.Combine(Path.GetDirectoryName(sourceFilepath)!, targetFilename);

            var oldTargetDoc = File.Exists(targetFilepath) ? XDocument.Load(targetFilepath) : null;
            var sourceDoc = XDocument.Load(sourceFilepath);

            //var newTargetDoc = XDocument.Load(File.Exists(targetFilepath) ? targetFilepath : sourceFilepath) // or ... start from the current localized .resx
            var newTargetDoc = XDocument.Load(sourceFilepath);
            // remove the large annoying comment at the top
            if (newTargetDoc.Root?.FirstNode?.NodeType == System.Xml.XmlNodeType.Comment)
                newTargetDoc.Root.FirstNode.Remove();

            // maintain "Pre-loc from" comments at the top
            var preLoc = false;
            var oldFirstNode = oldTargetDoc?.Root?.FirstNode as XComment;
            if (oldFirstNode?.NodeType == System.Xml.XmlNodeType.Comment && oldFirstNode.Value.Contains("Pre-loc from") == true)
            {
                preLoc = true;
                newTargetDoc.Root!.AddFirst(oldFirstNode);
            }

            var sourceNodes = sourceDoc.Root?.Elements().Where(_ => _.Name.LocalName == "data" && _.Attribute("type") == null); // text elements have no type - ignore anything that does
            if (sourceNodes == null || sourceNodes.Count() == 0) return;

            // add to the file's schema otherwise new elements are deemed invalid
            var commentElement = newTargetDoc.Root!.Descendants().Where(_ => _.Name.LocalName == "element" && _.FirstAttribute?.Value == "comment").First();
            var sourceElement = new XElement(commentElement);
            sourceElement.SetAttributeValue("name", "source");
            sourceElement.SetAttributeValue(ordinal, "3");
            commentElement.Parent?.Add(sourceElement);

            var notTranslated = "Not translated";
            var machineTranslated = "Machine translated";
            var translateNeeded = "Update needed: " + targetLang.ToUpperInvariant();
            var count = 0;
            foreach (var element in sourceNodes)
            {
                // extract resource
                var resourceName = element.Attribute("name")?.Value;
                var sourceText = element.Element("value")?.Value;
                var commentText = element.Element("comment")?.Value;

                // skip any empty strings or non string resources
                if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(sourceText)) continue;
                if (resourceName.Contains(".") && !resourceName.EndsWith(".Text")) continue;

                // find target node + set the source text as a comment on the target element -  so we can detect when the source changes (saving on translation costs)
                var targetNode = newTargetDoc.Root!.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).First();

                // build a replacement node
                var newNode = new XElement("data");
                newNode.SetAttributeValue("name", resourceName);
                newNode.SetElementValue("value", "");
                if (commentText != null) newNode.SetElementValue("comment", commentText);
                newNode.SetElementValue("source", sourceText);
                targetNode.ReplaceWith(newNode);
                targetNode = newNode;

                // retrieve prior values
                var oldTargetNode = oldTargetDoc?.Root?.Elements().Where(_ => _.Name.LocalName == "data" && _.FirstAttribute?.Value == resourceName).FirstOrDefault();
                var oldTranslation = oldTargetNode?.Element("value")?.Value;
                var oldSource = oldTargetNode?.Element("source")?.Value;
                var sourceChanged = oldSource != null && oldSource != sourceText;

                var oldFirstChildCommentNode = (oldTargetNode?.FirstNode as XComment);
                var wasMachineTranslated = oldFirstChildCommentNode?.Value == machineTranslated;
                var wasNotTranslated = oldFirstChildCommentNode?.Value == notTranslated;
                var shouldTryTranslate = oldTargetNode == null || wasMachineTranslated || wasNotTranslated;
                var needsUpdating = (oldTargetNode?.FirstNode as XComment)?.Value == translateNeeded;

                //if (resourceName == "Rescue" && preLoc) Debugger.Break();

                if (oldTargetNode?.PreviousNode?.NodeType == System.Xml.XmlNodeType.Comment)
                {
                    // maintain any ad-hoc comment above the <data /> element
                    targetNode.AddBeforeSelf(oldTargetNode.PreviousNode);
                }

                if (!shouldTryTranslate && (sourceChanged || needsUpdating))
                {
                    // it was translated manually at some point, but the source changed either now or sometime in the past. track it as source-updated
                    locUpdate.init(targetLang).init(relativeSourceFilePath).Add(resourceName);
                    targetNode.AddFirst(new XComment(translateNeeded));
                }
                else if (oldFirstChildCommentNode != null && !wasMachineTranslated && !wasNotTranslated)
                {
                    // maintain ad-hoc comment within the <data /> element
                    targetNode.AddFirst(oldFirstChildCommentNode);
                }

                // initialize with the old values
                if (oldTranslation != null) targetNode.SetElementValue("value", oldTranslation);

                // skip anything we should not translate
                if (!shouldTryTranslate) continue;

                // track this as missing a manual translation
                locMissing.init(targetLang).init(relativeSourceFilePath).Add(resourceName);

                // skip anything where source has not changed
                if (oldSource == sourceText && !string.IsNullOrEmpty(oldTranslation))
                {
                    // but keep the comment
                    targetNode.AddFirst(oldFirstChildCommentNode);
                    continue;
                }

                // priority as follows: manual translation (with or without "updated") -> machine translation (even if the source is newer and we can't translate) -> not translated
                // at this point if there ever was manual translation we already continue'd onto the next element, so

                // if there is no suitable machine translation assume we can't translate it and use the source string
                var translation = wasMachineTranslated ? oldTranslation : sourceText;
                var translationComment = wasMachineTranslated ? machineTranslated : notTranslated;

                if (targetLang == "ps")
                {
                    // or "machine" translate it to pseudo
                    translation = $"* {sourceText.ToUpperInvariant()} →→→!";
                    translationComment = machineTranslated;
                }
                else if (canTranslate)
                {
                    // or translate into a real language
                    translation = await translateString(sourceText, targetLang);
                    if (string.IsNullOrEmpty(translation))
                    {
                        Debugger.Break();
                        continue;
                    }
                    else
                        translationComment = machineTranslated;
                }

                // stamp resource as "Machine translated" or "Not translated"
                targetNode.AddFirst(new XComment(translationComment));

                targetNode.SetElementValue("value", translation);

                count++;
            }

            // Save and sort it
            Game.log($"Updated {count,3} of {sourceNodes.Count(),3} '{targetLang}' resources in " + Path.GetFileNameWithoutExtension(sourceFilepath));
            alphaSortResx(newTargetDoc, targetFilepath);
        }

        public static void alphaSortResx(XDocument doc, string filepath)
        {
            var locBelowComment = "Localizable elements are below";

            // remove the comments we don't want
            var badComments = doc.Root!.Nodes().Where(_ => (_ as XComment)?.Value == locBelowComment).ToList();
            foreach (var node in badComments)
                node.Remove();

            // collect free floating comments, we would like to keep these
            var comments = doc.Root!.Nodes()
                .Where(_ => _.Parent == doc.Root && _.NodeType == System.Xml.XmlNodeType.Comment && _.NextNode != null && (_ as XComment)?.Value != locBelowComment)
                .Reverse()
                .ToDictionary(_ => (XComment)_, _ => _.NextNode!);

            // collect elements to be sorted
            var sorted = doc.Root!.Elements()
                .Where(_ => _.Name.LocalName == "data")
                .OrderBy(n => n.FirstAttribute?.Value.Replace("$", "").Replace(">>", ""))
                .ToList();

            // remove then re-add them in the correct order
            foreach (var element in sorted) element.Remove();
            foreach (var element in sorted) doc.Root.Add(element);

            // collect elements to be sorted
            var locNodes = doc.Root!.Elements()
                .Where(_ => _.Name.LocalName == "data"
                    && _.Attribute("type") == null // text elements have no type - ignore anything that does
                    && _.Attribute("name")?.Value.StartsWith(">") != true
                )
                .OrderBy(n => n.FirstAttribute?.Value.Replace("$", ""))
                .ToList();

            // remove then re-add them, with a comment telling where localizable elements will begin
            foreach (var element in locNodes) element.Remove();
            doc.Root.Add(new XComment(locBelowComment));
            foreach (var element in locNodes) doc.Root.Add(element);

            // restore those comments to be ahead of where they were before
            foreach (var (comment, nextNode) in comments)
            {
                comment.Remove();
                nextNode.AddBeforeSelf(comment);
            }

            doc.Save(filepath);
        }
    }
}
