namespace Athena.UI
{
    public enum UpdateType
    {
        /// <summary>
        /// Undefined update type.
        /// </summary>
        None,

        /// <summary>
        /// Data is initialized. Usually processed directly.
        /// </summary>
        Initialize,

        /// <summary>
        /// Data is added. Usually processed directly or skipped if data is unrelated to processing entity.
        /// </summary>
        Add,

        /// <summary>
        /// Data is deleted. Usually processed directly or skipped if data is unrelated to processing entity.
        /// </summary>
        Delete,

        /// <summary>
        /// Data is edited. Usually processed directly or skipped if data is unrelated to processing entity.
        /// </summary>
        Edit,

        /// <summary>
        /// Data is moved. Usually not processed directly, processing is delegated to related entity or entity is flagged as unloaded.
        /// </summary>
        Move
    }
}
