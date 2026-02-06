using Newtonsoft.Json;
using OVRSharp;
using OVRSharp.Math;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SrvSurvey.game;
using System.Drawing.Imaging;
using System.Numerics;
using Valve.VR;
using Device = SharpDX.Direct3D11.Device;

namespace SrvSurvey.plotters
{
    internal class VR : OVRSharp.Application, IDisposable
    {
        public static bool enabled => PlotAdjustVR.force || Game.settings.displayVR;

        public static VR? app;
        public static string vrMode = "";
        public static Dictionary<string, Dictionary<string, PlotPos.VR>> overrides = new();

        public static void init()
        {
            Game.log($"VR.init: existing: {app != null}");
            try
            {
                if (app != null || !VR.enabled) return;

                app = new VR();

                // load any vehicle overlay position overrides
                loadOverlayOverrides();
            }
            catch (Exception ex)
            {
                Game.log($"VR.init failed: {ex.Message}");
            }
        }

        public static void shutdown()
        {
            Game.log($"VR.shutdown: existing: {app != null}");
            if (app != null)
            {
                app.Dispose();
                app = null;
            }
        }

        private static void loadOverlayOverrides()
        {
            var newOverrides = new Dictionary<string, Dictionary<string, PlotPos.VR>>();

            var folder = Path.Combine(Program.dataFolder, "vr");
            if (!Directory.Exists(folder)) return;

            var files = Directory.GetFiles(folder, "*.json");
            Game.log($"Reading {files.Length} VR overlay overrides files");
            foreach (var filename in files)
            {
                var name = Path.GetFileNameWithoutExtension(filename);
                var json = File.ReadAllText(filename);

                var overrides = newOverrides.init(name);
                var raw = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
                foreach (var _ in raw)
                {
                    var parsed = PlotPos.VR.parse(_.Value);
                    if (parsed != null)
                        overrides[_.Key] = parsed;
                }
            }

            overrides = newOverrides;
        }

        public static void saveOverlayOverrides()
        {
            var folder = Path.Combine(Program.dataFolder, "vr");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            foreach (var key in overrides.Keys)
            {
                var raw = overrides[key].ToDictionary(x => x.Key, x => x.Value.ToString());
                var json = JsonConvert.SerializeObject(raw, Formatting.Indented);
                var filepath = Path.Combine(folder, $"{key}.json");
                Game.log($"Writing: {filepath}");
                Data.saveWithRetry(filepath, json);
            }
        }

        public Device device { get; private set; }
        public DeviceContext context { get; private set; }

        private VR() : base(ApplicationType.Overlay)
        {
            device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            context = device.ImmediateContext;
            Game.update += Game_update;
        }

        private void Game_update(GameMode newMode, bool force)
        {
            var game = Game.activeGame;
            if (game == null) return;
            switch (newMode)
            {
                // These modes are ship agnostic
                case GameMode.GalaxyMap:
                case GameMode.SystemMap:
                case GameMode.Orrery:
                case GameMode.FSS:
                case GameMode.SAA:
                case GameMode.OnFoot:
                    VR.vrMode = newMode.ToString();
                    break;

                case GameMode.OnFootInStation:
                    VR.vrMode = GameMode.OnFoot.ToString();
                    break;

                // TODO: support SRV turrets as their own vrMode?
                //case GameMode.InSrv:
                //    VR.vrMode = game.status.UsingSrvTurret ? "SrvTurret" : game.currentVehicleType;
                //    break;

                default:
                    // otherwise be the type of vehicle we are in
                    VR.vrMode = game.currentVehicleType;
                    break;
            }
            //Game.log($"!>> >> >> >> >> VR.vrMode: {newMode} => {VR.vrMode}");
        }

        public void Dispose()
        {
            this.Shutdown();

            Game.update -= Game_update;

            if (context != null)
            {
                context.Dispose();
                context = null!;
            }

            if (device != null)
            {
                device.Dispose();
                device = null!;
            }
        }
    }

    internal class VROverlay : IDisposable
    {
        private PlotDef def;
        private Overlay overlay;
        private Texture2D? texture;
        private Texture_t overlayTex;
        private PlotPos.VR? lastPP;
        private bool projecting = false;

        public VROverlay(PlotDef def)
        {
            Game.log($"VR.VROverlay.ctor: {def.name}");
            this.def = def;
            this.overlay = new Overlay(def.name, def.name)
            {
                WidthInMeters = 1f,
                Alpha = Game.settings.Opacity,
            };
        }

        public void Dispose()
        {
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }

            if (overlay != null)
            {
                try
                {
                    overlay.Destroy();
                }
                catch { }
                overlay = null!;
            }
        }

        public float alpha
        {
            get => overlay.Alpha;
            set => overlay.Alpha = value;
        }

        public Boolean isVisible => OpenVR.Overlay?.IsOverlayVisible(overlay.Handle) == true;

        public void hide()
        {
            if (!isVisible) return;

            Game.log($"VR.hide: {overlay.Name}");
            overlay.Hide();
        }

        public void show()
        {
            if (isVisible) return;

            Game.log($"VR.show: {overlay.Name}");
            overlay.Show();
        }

        public void project()
        {
            // prevent overlapping renderings
            if (projecting) return;

            try
            {
                projecting = true;
                if (overlay == null || def.instance == null || VR.app == null) return;
                var visible = isVisible;
                //Game.log($"VR.project: {overlay.Name}, visible: {visible}, size: {def.instance.size}");

                // use current or re-render the frame
                var bitmap = def.instance.frame == null || def.instance.stale
                    ? def.instance.render()
                    : def.instance.frame;

                // adjust if size has changed
                if (texture == null || texture.Description.Width != bitmap.Width || texture.Description.Height != bitmap.Height)
                    texture = updateTextureSize(bitmap.Size);

                updateTextureImage(bitmap, VR.app.context);

                // reposition if necessary
                var pp = PlotPos.get(overlay.Name)?.vr;
                // do we have an override?
                var vrMode = PlotAdjustVR.overrideName ?? VR.vrMode;
                if (VR.overrides.GetValueOrDefault(vrMode)?.TryGetValue(overlay.Name, out var alternatePP) == true)
                    pp = alternatePP;

                if (pp != null && pp.ToString() != lastPP?.ToString())
                    repositionOverlay(pp);

                // ensure we are visible
                if (!visible && !def.instance.hidden) show();
            }
            catch (Exception ex)
            {
                Game.log($"VR.project: error on '{def.name}': {ex.Message}\r\n\t{ex.StackTrace}");
                Game.log($"VR.project: error on '{def.name}': handle: {overlay?.Handle}");
                PlotBase2.remove(def);
            }
            finally
            {
                projecting = false;
            }
        }

        private Texture2D updateTextureSize(Size sz)
        {
            Game.log($"VR.updateTextureSize: {overlay.Name}: {texture?.Description.Width}, {texture?.Description.Height} => {sz.Width}, {sz.Height}");

            var textureDesc = new Texture2DDescription
            {
                Width = sz.Width,
                Height = sz.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                //Usage = ResourceUsage.Immutable,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            return new Texture2D(VR.app!.device, textureDesc);
        }

        private void updateTextureImage(Bitmap bitmap, DeviceContext context)
        {
            if (VR.app?.context == null) return;
            //Game.log($"VR.updateTextureImage: {overlay.Name} ({def.instance?.stale})");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var region = new ResourceRegion
                {
                    Left = 0,
                    Top = 0,
                    Front = 0,
                    Right = bitmap.Width,
                    Bottom = bitmap.Height,
                    Back = 1
                };
                DataBox dataBox = new DataBox(data.Scan0, data.Stride, 0);
                context.UpdateSubresource(dataBox, texture, 0, region);

                overlayTex = new Texture_t()
                {
                    eColorSpace = EColorSpace.Auto,
                    eType = ETextureType.DirectX,
                    handle = texture!.NativePointer
                };
                overlay.SetTexture(overlayTex);

                context.Flush();
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private void repositionOverlay(PlotPos.VR pp)
        {
            //Game.log($"VR.repositionOverlay: {overlay.Name}: {pp}");

            // yaw = Y, pitch = X, roll = Z - the Y and X really should be swapped like this
            var rot = Matrix4x4.CreateFromYawPitchRoll(
                MathF.PI / 180 * pp.r.Y,
                MathF.PI / 180 * pp.r.X,
                MathF.PI / 180 * pp.r.Z
            );

            // divide by 10 to avoid decimals on the fomrs
            var pos = Matrix4x4.CreateTranslation(new Vector3(
                pp.p.X / 10,
                pp.p.Y / 10,
                -pp.p.Z / 10
            ));

            var sc = Matrix4x4.CreateScale(pp.s / 10); // yes 10

            var tr = Matrix4x4.Multiply(rot, sc);
            tr = Matrix4x4.Multiply(tr, pos);

            overlay.Transform = tr.ToHmdMatrix34_t();
            lastPP = PlotPos.VR.parse(pp.ToString());
        }
    }
}
