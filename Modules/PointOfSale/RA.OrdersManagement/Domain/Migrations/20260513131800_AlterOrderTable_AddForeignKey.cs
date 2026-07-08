//using FluentMigrator;
//using RA.OrdersManagement.Domain.Orders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RA.OrdersManagement.Domain.Migrations
//{
//    [Migration(20260513131800, "Order, OrderItem: AlterOrderTable AddForeignKey")]
//    public class _20260513131800_AlterOrderTable_AddForeignKey : Migration
//    {
//        public override void Down()
//        {
//            throw new NotImplementedException();
//        }

//        public override void Up()
//        {
//            Alter.Table(nameof(OrderItem))
//                .AlterColumn(nameof(OrderItem.FormId)).AsGuid().NotNullable()
//                .ForeignKey("FK_OrderItem_Order", nameof(Order), nameof(Order.Id));
//        }
//    }
//}
