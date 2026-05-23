using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanlyDiemAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng đăng ký môn học
            migrationBuilder.CreateTable(
                name: "DangKyMonHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    MonHocId   = table.Column<int>(type: "int", nullable: false),
                    TrangThai  = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NgayDangKy = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyMonHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKyMonHocs_MonHocs_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyMonHocs_SinhViens_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyMonHocs_MonHocId",
                table: "DangKyMonHocs",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyMonHocs_SinhVienId_MonHocId",
                table: "DangKyMonHocs",
                columns: new[] { "SinhVienId", "MonHocId" },
                unique: true);

            // Đổi cột điểm sang nullable để hỗ trợ nhập điểm từng phần
            migrationBuilder.AlterColumn<float>(
                name: "CC", table: "Diems", type: "float", nullable: true,
                oldClrType: typeof(float), oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "KT1", table: "Diems", type: "float", nullable: true,
                oldClrType: typeof(float), oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "KT2", table: "Diems", type: "float", nullable: true,
                oldClrType: typeof(float), oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "KT3", table: "Diems", type: "float", nullable: true,
                oldClrType: typeof(float), oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "Exam", table: "Diems", type: "float", nullable: true,
                oldClrType: typeof(float), oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DangKyMonHocs");

            migrationBuilder.AlterColumn<float>(
                name: "CC", table: "Diems", type: "float", nullable: false,
                defaultValue: 0f, oldClrType: typeof(float), oldType: "float", oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "KT1", table: "Diems", type: "float", nullable: false,
                defaultValue: 0f, oldClrType: typeof(float), oldType: "float", oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "KT2", table: "Diems", type: "float", nullable: false,
                defaultValue: 0f, oldClrType: typeof(float), oldType: "float", oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "KT3", table: "Diems", type: "float", nullable: false,
                defaultValue: 0f, oldClrType: typeof(float), oldType: "float", oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Exam", table: "Diems", type: "float", nullable: false,
                defaultValue: 0f, oldClrType: typeof(float), oldType: "float", oldNullable: true);
        }
    }
}
