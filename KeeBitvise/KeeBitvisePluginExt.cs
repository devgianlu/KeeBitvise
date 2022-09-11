using System;
using KeePass.Plugins;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using KeePassLib.Security;


namespace KeeBitvise
{
    public sealed class KeeBitviseExt : Plugin
    {
        private IPluginHost _host;

        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
                return false;

            _host = host;
            return true;
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t != PluginMenuType.Entry)
                return null;

            return new ToolStripMenuItem("Open with Bitvise SSH", null, EntryToolStripClick);
        }

        private ProtectedBinary FindSelectedBitviseEntry()
        {
            foreach (var entry in _host.MainWindow.GetSelectedEntries())
            {
                foreach (var item in entry.Binaries)
                {
                    if (!item.Key.EndsWith(".tlp"))
                        continue;

                    return item.Value;
                }
            }

            return null;
        }

        private static string FindBitviseExecutable()
        {
            var bitviseExecutable = new[]
            {
                "C:\\Program Files (x86)\\Bitvise SSH Client\\BvSsh.exe",
                "C:\\Program Files\\Bitvise SSH Client\\BvSsh.exe"
            };
            return bitviseExecutable.FirstOrDefault(File.Exists);
        }

        private void EntryToolStripClick(object sender, EventArgs e)
        {
            var entry = FindSelectedBitviseEntry();
            if (entry == null)
                return;

            var tempFile = _host.TempFilesPool.GetTempFileName();
            File.WriteAllBytes(tempFile, entry.ReadData());

            var bitviseExecutable = FindBitviseExecutable();
            if (bitviseExecutable == null)
                return;

            var cmd = new Process();
            cmd.StartInfo.FileName = bitviseExecutable;
            cmd.StartInfo.Arguments = tempFile;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
        }
    }
}