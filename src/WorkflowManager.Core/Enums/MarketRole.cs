namespace WorkflowManager.Core.Enums;

/// <summary>
/// Market participant roles in the Belgian energy market
/// </summary>
public enum MarketRole
{
    /// <summary>
    /// Balance Responsible Party - Responsible for energy balance
    /// </summary>
    BRP,

    /// <summary>
    /// Balancing Service Provider - Provides balancing services to TSO
    /// </summary>
    BSP,

    /// <summary>
    /// Grid User - Uses the transmission/distribution grid
    /// </summary>
    GU,

    /// <summary>
    /// Access Holder - Holds access rights to delivery points
    /// </summary>
    ACH,

    /// <summary>
    /// Consumer Representative Manager - Manages consumer data
    /// </summary>
    CRM,

    /// <summary>
    /// Energy Service Provider - Provides energy services to end customers
    /// </summary>
    ESP,

    /// <summary>
    /// Distribution System Operator - Operates distribution grid
    /// </summary>
    DSO,

    /// <summary>
    /// Transmission System Operator - Operates transmission grid (e.g., Elia)
    /// </summary>
    TSO,

    /// <summary>
    /// Supplier Agent - Acts on behalf of supplier
    /// </summary>
    SA,

    /// <summary>
    /// Off-taker Portfolio Administrator - Manages off-take portfolio
    /// </summary>
    OPA,

    /// <summary>
    /// Virtual Storage Provider - Provides virtual storage services
    /// </summary>
    VSP
}
