namespace Data.Enums
{
    public enum GlobalEvent
    {
        // NETWORK EVENT
        ALL_PLAYERS_CONECTED_TO_GAME,
        
        // GAME STATE EVENT
        WIN_GAMESTATE,
        LOST_GAMESTATE,
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