/// <summary>
/// Enumeration of audio categories used by the audio management system.
/// Used for grouping and controlling volume levels.
/// </summary>
namespace AudioManagerLite
{
    /// <summary>
    /// Audio categories used to organize and manage different types of sounds.
    /// </summary>
    public enum AudioCategory
    {
        /// <summary>
        /// Master category. Affects all other categories.
        /// </summary>
        Master,

        /// <summary>
        /// Background music (e.g., ambient tracks, theme songs).
        /// </summary>
        Music,

        /// <summary>
        /// Sound effects (SFX) such as gunshots, explosions, etc.
        /// </summary>
        SFX,

        /// <summary>
        /// User interface sounds (menu clicks, button hovers, etc.).
        /// </summary>
        UI,

        /// <summary>
        /// Character voice lines or dialogue.
        /// </summary>
        Voice,

        /// <summary>
        /// Ambient environmental sounds (wind, rain, background noise).
        /// </summary>
        Ambient
    }
}