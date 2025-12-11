using CounterStrikeSharp.API.Core;
using Newtonsoft.Json.Linq;
using CounterStrikeSharp.API.Modules.Utils;

namespace HolidaysManager_GoldKingZ;

public class Globals_Static
{
    public const float MoveSpeed = 1.0f;
    public const float RotateSpeed = 2.0f;
    public const float HighlightDistance = 200.0f;
    public class PersonData
    {
        public ulong PlayerSteamID { get; set; }
        public int Toggle_Snow { get; set; }
        public int Toggle_Props { get; set; }
        public DateTime DateAndTime { get; set; }
    }
}

public class Globals
{
    public CDynamicProp Server_Highlighted_Prop = null!;
    public bool OnTakeDamage_Hooked = false;
    public bool Server_EditMode = false;
    public CEnvSky Server_SkyBox = null!;
    public CCSPlayerController Server_EditMode_Player = null!;
    public CDynamicProp Server_Prop_Attached = null!;
    public float RightLeft { get; set; } = 0.0f;
    public float ForwardBack { get; set; } = 0.0f;
    public float UpDown { get; set; } = 0.0f;
    public QAngle RotationOffset { get; set; } = new QAngle(0, 0, 0);

    public class DroppedPropInfo
    {
        public string ModelPath { get; set; } = string.Empty;
        public int ModelType { get; set; } = 0;
        public Vector? Position { get; set; }
        public QAngle? Rotation { get; set; }
        public CDynamicProp? PropEntity { get; set; }
    }
    public List<DroppedPropInfo> DroppedProps_Data = new List<DroppedPropInfo>();

    public class PlayerDataClass
    {
        public CCSPlayerController Player { get; set; }
        public CCSPlayerPawn PlayerPawnLocated { get; set; }
        public ulong SteamId { get; set; }
        public int Toggle_Snow { get; set; }
        public int Toggle_Props { get; set; }
        public bool Decoy_Throwed { get; set; }
        public CParticleSystem Snow_Fall { get; set; }
        public DateTime Refill_CoolDown { get; set; }
        public DateTime EventPlayerChat { get; set; }
        public PlayerDataClass(CCSPlayerController Playerr, CCSPlayerPawn PlayerPawnLocatedd, ulong SteamIdd, int Toggle_Snoww, int Toggle_Propss, bool Decoy_Throwedd, CParticleSystem Snow_Falll, DateTime Refill_CoolDownn, DateTime EventPlayerChatt)
        {
            Player = Playerr;
            PlayerPawnLocated = PlayerPawnLocatedd;
            SteamId = SteamIdd;
            Toggle_Snow = Toggle_Snoww;
            Toggle_Props = Toggle_Propss;
            Decoy_Throwed = Decoy_Throwedd;
            Snow_Fall = Snow_Falll;
            Refill_CoolDown = Refill_CoolDownn;
            EventPlayerChat = EventPlayerChatt;
        }
    }
    public Dictionary<int, PlayerDataClass> Player_Data = new Dictionary<int, PlayerDataClass>();
    public CounterStrikeSharp.API.Modules.Timers.Timer? TimerChecker;


    public void Clear(bool clear_data = false)
    {
        Server_Highlighted_Prop = null!;        
        Server_EditMode = false;
        Server_SkyBox = null!;
        Server_EditMode_Player = null!;
        Server_Prop_Attached = null!;
        RightLeft = 0.0f;
        ForwardBack = 0.0f;
        UpDown = 0.0f;
        RotationOffset = new QAngle(0, 0, 0);
        DroppedProps_Data?.Clear();
        TimerChecker?.Kill();
        TimerChecker = null;

        if(clear_data)
        {
            Player_Data?.Clear();
        }
    }
}