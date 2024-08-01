namespace Mineant.Event
{
    /// <summary>
    /// 事件清单类，定义了游戏中各种事件的描述字段
    /// 命名规则，注释后需要加上（参数a类型，参数b类型），方便调用的时候查看参数是否匹配
    /// </summary>
    public partial class Events
    {
        /// <summary>
        /// 不知名、程序未定义事件
        /// </summary>
        public const string Unkown = "Unkown";

        /// <summary>
        /// 退出整个程序/游戏
        /// </summary>
        public const string QuitAppcation = "退出整个程序";

        public class FrameworkEvent
        {
            /// <summary>
            /// 创建实体通知(int)
            /// </summary>
            public const string EntityCreateEvent = "FrameworkEvent.EntityCreateEvent";
        }

        /// <summary>
        /// 角色状态事件
        /// </summary>
        public class CharacterStateEvent
        {
            /// <summary>
            /// 移动状态事件 (vector3)
            /// </summary>
            public const string OnPlayerMove = "PlayerStateEvent.OnPlayerMove";
            /// <summary>
            /// 待机状态事件
            /// </summary>
            public const string OnPlayerIdle = "PlayerStateEvent.OnPlayerIdle";
        }

        /// <summary>
        /// loading 界面事件
        /// </summary>
        public class LoadingEvent
        {
            /// <summary>
            /// loading 界面加载进度事件（int）
            /// </summary>
            public const string OnLoadingProcess = "LoadingEvent.OnLoadingProcess";
            /// <summary>
            /// loading 界面数据加载完成通知事件（bool）
            /// </summary>
            public const string OnLoadingFinished = "LoadingEvent.OnLoadingFinished";
        }

        /// <summary>
        /// 网络事件
        /// </summary>
        public class NetworkEvent
        {
            ///// <summary>
            ///// 连接请求事件
            ///// </summary>
            //public const string Connect = "NetworkEvent.Connect"; 
            //public const string OnClose = "NetworkEvent.OnClose";
            //public const string OnDataRecv = "NetworkEvent.OnDataRecv";
            //public const string OnConnected = "NetworkEvent.OnConnected";

            ///// <summary>
            ///// 服务器心跳检测
            ///// </summary>
            //public const string OnEchoRspEvent = "NetworkEvent.OnEchoRspEvent";


            /// <summary>
            /// WebSocket服务器给客户端发送心跳检测事件(int)
            /// </summary>
            public const string OnS2C_EchoRspEvent_WS = "WebSocketEvent.OnS2C_EchoRspEvent_WS";
        }
    }
}


