using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ddac7.Migrations
{
    public partial class CreateClinicTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinic",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClinicName = table.Column<string>(maxLength: 20, nullable: false),
                    ClinicDesc = table.Column<string>(nullable: false),
                    ContactNum = table.Column<string>(nullable: false),
                    ContactEmail = table.Column<string>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    UserID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinic", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clinic");
        }
    }
}
