using Sandbox.ModAPI;
using Sisk.BuildColors.Settings;
using System;
using VRage.Utils;

namespace Sisk.BuildColors {

    public static class FileHandler {

        public static T Load<T>(string fileName) where T : class {
            T data = null;
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(fileName)) {
                try {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(fileName)) {
                        data = MyAPIGateway.Utilities.SerializeFromXML<T>(reader.ReadToEnd());
                    }
                } catch (Exception exception) {
                    MyLog.Default.Error($"Error loading data from file '{fileName}': {exception.Message}\n{exception.StackTrace}");
                }
            }

            return data;
        }

        public static void Save<T>(string fileName, T data) where T : class {
            try {
                using (var writer = MyAPIGateway.Utilities.WriteFileInGlobalStorage(fileName)) {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(data));
                }
            } catch (Exception exception) {
                MyLog.Default.Error($"Error saving data to file '{fileName}': {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}