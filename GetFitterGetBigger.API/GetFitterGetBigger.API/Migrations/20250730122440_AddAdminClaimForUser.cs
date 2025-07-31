using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminClaimForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Admin-Tier claim for the user aboimpinto@gmail.com
            // Note: This assumes the user already exists in the database
            // The user would be created automatically when they first authenticate
            migrationBuilder.Sql(@"
                INSERT INTO ""Claims"" (""Id"", ""UserId"", ""ClaimType"", ""ExpirationDate"", ""Resource"")
                SELECT 
                    gen_random_uuid(),
                    u.""Id"",
                    'Admin-Tier',
                    NULL,
                    NULL
                FROM ""Users"" u
                WHERE u.""Email"" = 'aboimpinto@gmail.com'
                AND NOT EXISTS (
                    SELECT 1 
                    FROM ""Claims"" c 
                    WHERE c.""UserId"" = u.""Id"" 
                    AND c.""ClaimType"" = 'Admin-Tier'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the Admin-Tier claim for the user
            migrationBuilder.Sql(@"
                DELETE FROM ""Claims"" c
                USING ""Users"" u
                WHERE c.""UserId"" = u.""Id""
                AND u.""Email"" = 'aboimpinto@gmail.com'
                AND c.""ClaimType"" = 'Admin-Tier';
            ");
        }
    }
}
