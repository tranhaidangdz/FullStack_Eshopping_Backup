using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eshopping.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponCodeToOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

        }
    }
}
