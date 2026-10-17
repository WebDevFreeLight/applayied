using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Inventory
{
    /// <summary>
    /// Warehouse - المخزن
    /// </summary>
    [SugarTable("erp_warehouses")]
    [SqlDict(
        dictType: "erp_warehouse",
        dictName: "Warehouse - المخزن",
        valueColumn: "warehouseid",
        labelColumns: "warehousename",
        RemarkColumn = "warehousenamear",
        OrderBy = "warehousecode ASC",
        Description = "Auto-generated from ErpWarehouse table"
    )]
    public class ErpWarehouse : SysBase
    {
        /// <summary>
        /// Warehouse ID - معرف المخزن
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WarehouseId { get; set; }

        /// <summary>
        /// Warehouse Code - كود المخزن
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// Warehouse Name - اسم المخزن
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// Warehouse Name Arabic - اسم المخزن بالعربي
        /// </summary>
        public string WarehouseNameAr { get; set; }

        /// <summary>
        /// Warehouse Type - نوع المخزن (main/branch/virtual)
        /// </summary>
        public string WarehouseType { get; set; } = "main";

        /// <summary>
        /// Location - الموقع
        /// </summary>
        public string Location { get; set; } 

        /// <summary>
        /// Manager ID - معرف المسؤول
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ManagerId { get; set; }

        /// <summary>
        /// Phone - الهاتف
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Address - العنوان
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Is Default - افتراضي
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Allow Sales - يسمح بالمبيعات
        /// </summary>
        public bool AllowSales { get; set; } = true;

        /// <summary>
        /// Allow Purchases - يسمح بالمشتريات
        /// </summary>
        public bool AllowPurchases { get; set; } = true;

        /// <summary>
        /// Allow Negative Stock - يسمح بالرصيد السالب
        /// </summary>
        public bool AllowNegativeStock { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

