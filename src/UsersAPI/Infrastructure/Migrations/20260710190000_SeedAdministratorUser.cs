using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsersAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdministratorUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @AdminId uniqueidentifier = '7F44D546-5DC7-4C2E-9C9B-1F3B357EC0F1';

                IF NOT EXISTS (
                    SELECT 1
                    FROM Users
                    WHERE Email = 'admin@email.com'
                )
                BEGIN
                    INSERT INTO Users (
                        Id,
                        Name,
                        Email,
                        Cpf,
                        BirthDate,
                        PasswordHash,
                        Role,
                        CreatedAt,
                        CreatedBy,
                        UpdatedAt,
                        UpdatedBy,
                        IsActive
                    )
                    VALUES (
                        @AdminId,
                        'admin',
                        'admin@email.com',
                        '52998224725',
                        '1990-01-01',
                        '$2a$11$.6IfXw/hL5TqNcrntSudCuxKlxLrym5Tuz.QaTPOH5KCuwLKbZlhy',
                        'Administrator',
                        SYSUTCDATETIME(),
                        @AdminId,
                        NULL,
                        NULL,
                        1
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Users
                WHERE Id = '7F44D546-5DC7-4C2E-9C9B-1F3B357EC0F1'
                  AND Email = 'admin@email.com';
                """);
        }
    }
}
