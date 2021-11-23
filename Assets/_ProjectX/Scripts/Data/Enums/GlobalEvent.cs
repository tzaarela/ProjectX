namespace Data.Enums
{
    public enum GlobalEvent
    {
        // NETWORK EVENT
        LOCAL_PLAYER_CONNECTED_TO_GAME,
        ALL_PLAYERS_CONNECTED_TO_GAME,
        
        // GAME EVENT
        END_OF_COUNTDOWN,
        FLAG_TAKEN,
        FLAG_DROPPED,
        SET_FOLLOW_TARGET,
        CAMERA_SHAKE,

        // GAME STATE EVENT
        END_GAMESTATE,
        PAUSED_GAMESTATE,
        PLAY_GAMESTATE,

        // DEBUG
        DEBUG_ON,
        DEBUG_OFF,
        SHOW_FPS,
        HIDE_FPS,
    }
}