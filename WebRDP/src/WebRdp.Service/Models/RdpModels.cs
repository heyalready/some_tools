namespace WebRdp.Service.Models
{
    /// <summary>
    /// RDP 会话状态枚举
    /// </summary>
    public enum RdpSessionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Error
    }

    /// <summary>
    /// RDP 会话信息
    /// </summary>
    public class RdpSession
    {
        public string Id { get; set; } = string.Empty;
        public RdpSessionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public Client.RdpConnectionConfig Config { get; set; } = new();
    }

    /// <summary>
    /// RDP 设置配置
    /// </summary>
    public class RdpSettings
    {
        public string DefaultHost { get; set; } = "127.0.0.2";
        public int DefaultPort { get; set; } = 3389;
        public int MaxSessionCount { get; set; } = 1;
        public int SessionTimeout { get; set; } = 3600;
        public int ReconnectDelay { get; set; } = 2000;
        public int MaxReconnectAttempts { get; set; } = 3;
    }

    /// <summary>
    /// 输入事件模型
    /// </summary>
    public class InputEvent
    {
        public string Type { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public bool Pressed { get; set; }
        public int Button { get; set; }
    }
}
