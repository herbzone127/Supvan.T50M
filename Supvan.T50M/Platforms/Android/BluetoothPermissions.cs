using Microsoft.Maui.ApplicationModel;
namespace Supvan.T50M;

public class BluetoothPermissions : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new[]
    {
    ("android.permission.BLUETOOTH_SCAN", true),
    ("android.permission.BLUETOOTH_CONNECT", true)
};
}
