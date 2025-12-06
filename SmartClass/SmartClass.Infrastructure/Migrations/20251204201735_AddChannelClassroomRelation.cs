using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartClass.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelClassroomRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Channels",
                newName: "Description");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassroomId",
                table: "Channels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Channels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ClassroomId",
                table: "Channels",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMembers_ChannelId",
                table: "ChannelMembers",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                table: "ChannelMembers",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Classrooms_ClassroomId",
                table: "Channels",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChannelMembers_Channels_ChannelId",
                table: "ChannelMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Classrooms_ClassroomId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Channels_ClassroomId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_ChannelMembers_ChannelId",
                table: "ChannelMembers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Channels",
                newName: "Title");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClassroomId",
                table: "Channels",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Channels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
