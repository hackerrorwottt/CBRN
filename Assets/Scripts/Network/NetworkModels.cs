using System;
using System.Collections.Generic;

namespace CBRN.Network
{
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class UserDTO
    {
        public int id;
        public string username;
        public string email;
        public string role;
    }

    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public string message;
        public string token;
        public UserDTO user;
    }

    [Serializable]
    public class StepLogPayload
    {
        public int step_number;
        public string step_name;
        public float duration_seconds;
        public int mistakes_count;
        public string status;
    }

    [Serializable]
    public class SessionUploadPayload
    {
        public int user_id;
        public int scenario_id;
        public int score;
        public float total_duration_seconds;
        public int total_mistakes;
        public string status; // "COMPLETED" or "FAILED"
        public List<StepLogPayload> step_logs;
    }

    [Serializable]
    public class SessionUploadResponse
    {
        public bool success;
        public string message;
        public int session_id;
    }

    [Serializable]
    public class DifficultyResponse
    {
        public bool success;
        public float adaptive_difficulty_multiplier; // e.g. 1.25 for harder, 0.75 for easier
        public float average_mistakes_last_5;
        public string recommendation;
    }

    [Serializable]
    public class GenericErrorResponse
    {
        public bool success;
        public string error;
        public string message;
    }
}

