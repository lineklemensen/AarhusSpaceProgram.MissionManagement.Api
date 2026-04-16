using Microsoft.AspNetCore.Identity;

namespace AarhusSpaceProgram.MissionManagement.Api.Models
{
    public class AspUser : IdentityUser<int>
    {
        public Astronaut? Astronaut { get; set; }
        public Scientist? Scientist { get; set; }
        public Manager? Manager { get; set; }
    }
}
