using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SrvSurvey.game
{
    /// <summary>
    /// A base class for various data file classes
    /// </summary>
    internal abstract class Data
    {
        protected string filepath;

        public static T? Load<T>(string filepath) where T : Data
        {
            // read and parse file contents into tmp object
            if (File.Exists(filepath))
            {
                // Game.log($"Reading: {filepath}");
                var json = File.ReadAllText(filepath);
                try
                {
                    var data = JsonConvert.DeserializeObject<T>(json)!;
                    Game.log($"Loaded data from: {filepath}");
                    data.filepath = filepath;
                    return data;
                }
                catch (Exception ex)
                {
                    Game.log($"Failed to read data: {ex.Message}");
                    Game.log(json);
                }
            }
            else
            {
                Game.log($"Data file not found: {filepath}");
            }

            return null;
        }

        public void Save()
        {
            if (Game.activeGame?.journals == null)
                throw new Exception("Why no journals here?");

            var folder = Path.GetDirectoryName(this.filepath)!;
            Directory.CreateDirectory(folder);

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            var success = false;
            var attempts = 0;
            while (!success && attempts < 50)
            {
                try
                {
                    attempts++;
                    File.WriteAllText(this.filepath, json);
                    success = true;
                }
                catch
                {
                    Game.log($"Failed on attempt {attempts} to save: {this.filepath}");
                    // swallow and try again
                    Application.DoEvents();
                }
            }
        }
    }
}
