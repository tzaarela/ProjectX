namespace Data.Enums
{
    public enum GlobalEvent
    {
        // NETWORK EVENT
        ALL_PLAYERS_CONNECTED_TO_GAME,
        
        // GAME STATE EVENT
        END_GAMESTATE,
        PAUSED_GAMESTATE,
        PLAY_GAMESTATE,
        
        // DEBUG
        DEBUG_ON,
        DEBUG_OFF,
        SHOW_FPS,
        HIDE_FPS,
        
        // CINEMACHINE CAMERA
        SET_FOLLOW_TARGET,
    }
}