using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSharing.Data.Models.Constants
{
    public class EntityConstants
    {
        public static class UserConstants
        {        
            public const int ProfileDescriptionMaxLength = 300;
        }

        public static class PostConstants
        {
            public const int TitleMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
        }

        public static class CommentConstants
        {
            public const int ContentMaxLength = 500;
        }

        public static class ChatRoomConstants
        {
            public const int NameMaxLength = 50;
            public const int DescriptionMaxLength = 300;
        }

        public static class MessageConstants
        {
            public const int ContentMaxLength = 1000;
        }
    }
}
