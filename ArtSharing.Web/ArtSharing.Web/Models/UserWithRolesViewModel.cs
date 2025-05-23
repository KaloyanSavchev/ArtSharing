﻿namespace ArtSharing.Web.Models
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public bool IsBanned { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

}
