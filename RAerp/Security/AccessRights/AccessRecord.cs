
namespace RAerp.Security.AccessRights
{
    /// <summary>
    /// Represents the access record that will be saved to AccessRecordData 
    /// </summary>
    public class AccessRecord
    {
        /// <summary>
        /// Name of the Access Record
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Classification in which it plugin belongs to
        /// </summary>
        public string Classification { get; set; }
        /// <summary>
        /// System name of the that will serve as identity
        /// </summary>
        public string SystemName { get; set; }
        /// <summary>
        /// The type of access
        /// </summary>
        public AccessType AccessRecordType { get; set; }
    }
}
