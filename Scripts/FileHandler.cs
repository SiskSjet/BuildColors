using Sandbox.ModAPI;
using Sisk.BuildColors.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace Sisk.BuildColors {

    public static class FileHandler {

        public static ColorSets LoadColorSets(string fileName) {
            ColorSets colorSets = null;
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(fileName)) {
                try {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(fileName)) {
                        colorSets = MyAPIGateway.Utilities.SerializeFromXML<ColorSets>(reader.ReadToEnd());
                    }
                } catch (Exception exception) {
                    MyLog.Default.Error($"Error loading color sets from file '{fileName}': {exception.Message}\n{exception.StackTrace}");
                }
            }

            return colorSets;
        }

        public static void SaveColorSets(string fileName, ColorSets colorSets) {
            try {
                using (var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(fileName)) {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(colorSets));
                }
            } catch (Exception exception) {
                MyLog.Default.Error($"Error saving color sets to file '{fileName}': {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}