using System;
using System.Runtime.InteropServices;
using System.Threading;
using Timer = System.Threading.Timer;   // <-- add this line

namespace Zelenskyj
{
    /// <summary>
    /// Forces system master volume to 100% and unmuted by polling CoreAudio.
    /// No external libs needed. Handles default-device changes implicitly by reconnecting on failure.
    /// </summary>
    public sealed class AudioEnforcer : IDisposable
    {
        private Timer _timer;
        private IAudioEndpointVolume? _epVol;

        // Poll every 250 ms (tweak if you want)
        private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(250);

        public void Start()
        {
            _timer ??= new Timer(_ => Tick(), null, TimeSpan.Zero, Interval);
        }

        private void Tick()
        {
            try
            {
                if (_epVol == null)
                    _epVol = GetEndpointVolume();

                // Force unmute
                int isMuted;
                Marshal.ThrowExceptionForHR(_epVol.GetMute(out isMuted));
                if (isMuted != 0)
                    Marshal.ThrowExceptionForHR(_epVol.SetMute(0, IntPtr.Zero));

                // Force 100%
                float vol;
                Marshal.ThrowExceptionForHR(_epVol.GetMasterVolumeLevelScalar(out vol));
                if (vol < 0.999f) // avoid redundant writes
                    Marshal.ThrowExceptionForHR(_epVol.SetMasterVolumeLevelScalar(1.0f, IntPtr.Zero));
            }
            catch
            {
                // On any failure (device removed, etc.), rebuild on next tick
                SafeRelease(ref _epVol);
            }
        }

        private static IAudioEndpointVolume GetEndpointVolume()
        {
            var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            IMMDevice device = null;
            try
            {
                Marshal.ThrowExceptionForHR(enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out device));
                var iid = typeof(IAudioEndpointVolume).GUID;
                object obj;
                Marshal.ThrowExceptionForHR(device.Activate(ref iid, CLSCTX.CLSCTX_INPROC_SERVER, IntPtr.Zero, out obj));
                return (IAudioEndpointVolume)obj;
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
                Marshal.ReleaseComObject(enumerator);
            }
        }

        private static void SafeRelease<T>(ref T? comObj) where T : class
        {
            if (comObj != null)
            {
                try { Marshal.ReleaseComObject(comObj); } catch { /*ignore*/ }
                comObj = null;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
            SafeRelease(ref _epVol);
            GC.SuppressFinalize(this);
        }

        #region CoreAudio COM interop

        // CLSID and interfaces
        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [Flags]
        private enum CLSCTX : uint
        {
            CLSCTX_INPROC_SERVER = 0x1,
        }

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, uint dwStateMask, out object ppDevices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            int OpenPropertyStore(int stgmAccess, out object ppProperties);
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            int GetState(out int pdwState);
        }

        [ComImport]
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            // Only the members we need
            int RegisterControlChangeNotify(IntPtr pNotify);
            int UnregisterControlChangeNotify(IntPtr pNotify);
            int GetChannelCount(out uint pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, IntPtr pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, IntPtr pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, IntPtr pguidEventContext);
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, IntPtr pguidEventContext);
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            int SetMute(int bMute, IntPtr pguidEventContext);
            int GetMute(out int pbMute);
            int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            int VolumeStepUp(IntPtr pguidEventContext);
            int VolumeStepDown(IntPtr pguidEventContext);
            int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        #endregion
    }
}
