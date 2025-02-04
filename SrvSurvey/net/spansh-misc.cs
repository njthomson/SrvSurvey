using Newtonsoft.Json;
using SrvSurvey.game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.net
{
    partial class Spansh
    {
        public async Task getFactionSystems()
        {
            var targetFaction = "Raven Colonial Corporation";
            Game.log($"getFactionSystems:");

            // get controlled systems 
            var q1 = new SystemQuery
            {
                page = 0,
                size = 50,
                sort = new() { new("name", SortOrder.asc) },
                filters = new() { { "minor_faction_presences", new SystemQuery.Value(targetFaction) } }
            };
            var response = await this.querySystems(q1);
            var factionSystems = response.results.Select(s => s.name);

            var txt = string.Join("\n", factionSystems);
            Clipboard.SetText(txt);
            Game.log(txt);
        }

        public async Task getRavenColonizeTargets()
        {
            Game.log($"getRavenColonizeTargets:");

            // get controlled systems 
            var q1 = new SystemQuery
            {
                page = 0,
                size = 50,
                sort = new() { new("name", SortOrder.asc) },
                filters = new() { { "controlling_minor_faction", new SystemQuery.Value("Raven Colonial Corporation") } }
            };
            var controlledSystems = await this.querySystems(q1);
            var factionSystems = controlledSystems.results.ToDictionary(r => r.name, r => r);
            Game.log(string.Join('\n', factionSystems));

            /* {
             * "filters":{
             * "distance":{"min":"0","max":"10"},
             * "population":{"comparison":"<=>","value":[0,0]}
             * },
             * "sort":[
             * {"distance":{"direction":"asc"}}],"size":10,"page":0,"reference_system":"Kwatyri"}
             */
            var q2 = new SystemQuery
            {
                page= 0,
                size = 50,
                sort = new() { new("name", SortOrder.asc) },
                reference_system = "Kwatyri", // <-- !!
                filters = new() {
                    { "distance", new SystemQuery.MinMax("0", "10") },
                    { "population", new SystemQuery.Comparison(0, 0) },
                }
            };

            var colonizationTargets = new Dictionary<string, string>();
            foreach (var name in factionSystems.Keys)
            {
                q2.reference_system = name;
                var response = await this.querySystems(q2);
                foreach(var r in response.results)
                {
                    var key = $"`{r.name}` ({r.bodies?.Count} bodies)";
                    var dist = factionSystems[name].getDistanceFrom(r);
                    var txtDist = $"{dist:N2} ly from `{name}`";

                    if (!colonizationTargets.ContainsKey(key))
                        colonizationTargets[key] = txtDist;
                    else
                        colonizationTargets[key] += ", " + txtDist;
                }
            }

            var txt = $"{colonizationTargets.Count} colonization targets:\n" + string.Join("\n", colonizationTargets.Select(_ => $"- {_.Key} : {_.Value}"));
            Clipboard.SetText(txt);
            Game.log(txt); // JsonConvert.SerializeObject(colonizationTargets, Formatting.Indented));
        }

        public async Task getMinorFactionSystems()
        {
            Game.log($"Requesting api/systems/search for factions by body");

            var factions = new List<string>()
            {
                "Raven Colonial Corporation",
                "Steven Gordon Jolliffe",
                "Elite Secret Service",
                "The Blue Brotherhood",
                "Guardians of Cygnus"
            };

            var factionsTxt = string.Join(",", factions.Select(_ => $"\"{_}\""));
            var json = "{\"filters\":{\"minor_faction_presences\":{\"value\":[" + factionsTxt + "]}},\"sort\":[{\"distance\":{\"direction\":\"asc\"}}],\"size\":200,\"page\":0,\"reference_system\":\"Banka\"}";

            var body = new StringContent(json, Encoding.ASCII, "application/json");

            var response = await Spansh.client.PostAsync($"https://spansh.co.uk/api/systems/search", body);
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == "{\"error\":\"Invalid request\"}")
            {
                Debugger.Break();
                throw new Exception("Bad Spansh request: " + responseText);
            }

            var results = JsonConvert.DeserializeObject<SystemsSearchResults>(responseText)!;

            var foos = new List<MinorFactionSystem>();
            foreach (var system in results.results)
            {
                var foo = new MinorFactionSystem
                {
                    name = system.name,
                    coords = new MinorFactionSystem.Coords { x = system.x, y = system.y, z = system.z },
                    cat = new List<int> { },
                    infos = $"Population: {system.population.ToString("N0")}%3Cbr%20%3E"
                };

                foreach (var fac in system.minor_faction_presences)
                {
                    var idx = factions.IndexOf(fac.name);
                    if (idx != -1)
                    {
                        foo.cat.Add(idx);
                        foo.infos += "%3Cbr%20%3E" + $"{fac.name}: {fac.influence.ToString("N2")}%";
                    }
                }

                switch (system.name)
                {
                    case "Banka": // Raven Colonial Corporation
                    case "Hungalun": //Steven Gordon Jolliffe
                    case "Loperada": // Elite Secret Service
                    case "Bhumians": // The Blue Brotherhood
                    case "Dyavansana": // Guardians of Cygnus
                        foo.cat[0] = 99;
                        break;
                }

                foos.Add(foo);
            }

            var txt = JsonConvert.SerializeObject(foos, Formatting.None)
                .Replace("]},", "]},\r\n");
            txt = txt.Insert(1, "\r\n");
            txt = txt.Insert(txt.Length - 1, "\r\n");
            Clipboard.SetText(txt);

            Game.log($"Processed {results.size} of {results.count} results");
        }


    }
}
