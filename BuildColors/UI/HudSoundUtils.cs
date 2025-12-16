using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;

namespace Sisk.BuildColors.UI {

    public static class HudSoundUtils {

        public static void PlaySound(string name) {
            if (MyAPIGateway.Session?.ControlledObject?.Entity != null) {
                var emitter = new MyEntity3DSoundEmitter((MyEntity)MyAPIGateway.Session.ControlledObject.Entity);
                emitter.PlaySingleSound(new MySoundPair(name));
            }
        }
    }
}