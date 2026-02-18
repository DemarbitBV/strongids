namespace Demarbit.StrongIds.Generator;

/// <summary>
/// The type of the backing fields for the StrongId
/// </summary>
public enum BackingType
{
    /// <summary>
    /// Guid backing field (DEFAULT)
    /// </summary>
    Guid,
        
    /// <summary>
    /// Int backing field 
    /// </summary>
    Int,
        
    /// <summary>
    /// Long backing field
    /// </summary>
    Long,
        
    /// <summary>
    /// String backing field
    /// </summary>
    String
}