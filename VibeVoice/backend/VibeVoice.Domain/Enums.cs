namespace VibeVoice.Domain;

/// <summary>What an attachment (image or audio clip) belongs to.</summary>
public enum OwnerType
{
    Note = 1,
    Memory = 2,
    /// <summary>Images uploaded to the library without a note/memory yet.</summary>
    Library = 3,
    /// <summary>Clips produced in the VibeVoice Studio (standalone TTS / transcription).</summary>
    Studio = 4,
}

public enum AudioSource
{
    Recorded = 1,
    Uploaded = 2,
    Tts = 3,
}

/// <summary>Clips hold audio or video; both can be transcribed.</summary>
public enum MediaKind
{
    Audio = 1,
    Video = 2,
}

public enum Mood
{
    Happy = 1,
    Grateful = 2,
    Calm = 3,
    Excited = 4,
    Nostalgic = 5,
    Proud = 6,
    Sad = 7,
    Loved = 8,
}

public enum UserRole
{
    User = 1,
    Admin = 2,
}
