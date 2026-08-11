namespace MerxoSell.API.DTOs.Admin;

public class AdminStatsDto
{
    public int     TotalProducts      { get; set; }
    public int     TotalUsers         { get; set; }
    public int     TotalManufacturers { get; set; }
    public int     TotalOrders        { get; set; }
    public decimal TotalRevenueCAD    { get; set; }
    public int     ActiveProducts     { get; set; }
    public int     LowStockProducts   { get; set; }
}
