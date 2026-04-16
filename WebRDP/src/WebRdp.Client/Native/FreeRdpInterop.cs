using System;
using System.Runtime.InteropServices;

namespace WebRdp.Client.Native
{
    /// <summary>
    /// FreeRDP P/Invoke 互操作声明
    /// 支持 Windows 和 Linux/UOS 跨平台
    /// </summary>
    internal static class FreerdpInterop
    {
        // 根据平台选择正确的库名称
        private static readonly string FreeRdpLibrary = 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? "freerdp3.dll" 
                : "libfreerdp3.so";

        #region 上下文管理

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr freerdp_context_new();

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freerdp_context_free(IntPtr context);

        #endregion

        #region 连接控制

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern int freerdp_context_connect(IntPtr context);

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern int freerdp_context_disconnect(IntPtr context);

        #endregion

        #region 配置管理

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freerdp_context_set_settings(IntPtr context, FREERDP_SETTINGS settings);

        #endregion

        #region 事件回调

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freerdp_context_set_frame_callback(IntPtr context, FrameCallback callback);

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freerdp_context_set_state_callback(IntPtr context, StateCallback callback);

        #endregion

        #region 输入事件

        [DllImport(FreeRdpLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void freerdp_context_send_input(IntPtr context, FREERDP_INPUT_EVENT input);

        #endregion

        #region 委托类型

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FrameCallback(IntPtr frameData, int width, int height, int stride);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StateCallback(int oldState, int newState);

        #endregion
    }

    /// <summary>
    /// FreeRDP 连接配置结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FREERDP_SETTINGS
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string ServerHostname;
        
        public int ServerPort;
        
        [MarshalAs(UnmanagedType.LPStr)]
        public string Username;
        
        [MarshalAs(UnmanagedType.LPStr)]
        public string Password;
        
        public int Width;
        
        public int Height;
        
        public int ColorDepth;
        
        [MarshalAs(UnmanagedType.LPStr)]
        public string LocalSessionId;
    }

    /// <summary>
    /// FreeRDP 输入事件结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FREERDP_INPUT_EVENT
    {
        public int EventType;
        public int KeyCode;
        public int MouseX;
        public int MouseY;
        public int Flags;
    }

    /// <summary>
    /// 输入事件类型枚举
    /// </summary>
    public enum InputEventType
    {
        Keyboard = 0,
        Mouse = 1,
        MouseWheel = 2
    }

    /// <summary>
    /// 连接状态枚举
    /// </summary>
    public enum ConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Reconnecting = 3,
        Disconnecting = 4,
        Error = 5
    }
}
