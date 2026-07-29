using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SrvSurvey.net
{
    /// <summary>
    /// Reads the application-wide EDDN consent value without mutating the
    /// process-local Settings instance. Multiple SrvSurvey processes share the
    /// same settings file, so an opt-out in any instance must be observed by
    /// whichever process currently owns the durable outbox.
    /// </summary>
    internal static class EddnConsentFile
    {
        internal static bool tryRead(
            string filepath,
            out bool enabled,
            out string? error)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filepath);
            enabled = false;
            error = null;
            if (!File.Exists(filepath)) return true;

            try
            {
                using var stream = new FileStream(
                    filepath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                var settings = JObject.Load(jsonReader);
                enabled = settings.Value<bool?>("eddnUploadEnabled") == true;
                return true;
            }
            catch (Exception ex) when (
                ex is IOException
                    or UnauthorizedAccessException
                    or JsonException)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
