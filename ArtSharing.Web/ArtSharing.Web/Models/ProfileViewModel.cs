namespace ArtSharing.Web.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public string? ProfileDescription { get; set; }

        public DateTime DateRegistered { get; set; }
    }
}
