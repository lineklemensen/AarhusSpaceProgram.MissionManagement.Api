namespace AarhusSpaceProgram.MissionManagement.Api.Infrastructure
{
    public static class StaffIdGenerator
    {
        public static string GenerateStaffId(string prefix, IEnumerable<string> existingIds)
        {
            // Find existing staff IDs with the same prefix
            var maxId = existingIds
                .Where(id => id.StartsWith(prefix))
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0)
                .Max();

            // Generate the next staff ID
            return $"{prefix}{(maxId + 1).ToString("D6")}";
        }
    }
}
